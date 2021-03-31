using System;
using System.Linq;
using System.Threading.Tasks;
using HrBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HrBot.Controllers
{
    [ApiController]
    [Route("api/hrupdate")]
    public class UpdateController : ControllerBase
    {
        private readonly ILogger<UpdateController> _logger;
        private readonly IVacancyReposter _vacancyReposter;
        private readonly AppSettings _appSettings;

        public UpdateController(
            ILogger<UpdateController> logger,
            IVacancyReposter vacancyReposter,
            AppSettings appSettings)
        {
            _logger = logger;
            _vacancyReposter = vacancyReposter;
            _appSettings = appSettings;
        }

        // POST api/update
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            if (!IsChatAllowed(update))
            {
                return Ok();
            }

            try
            {
                if (update.Type == UpdateType.Message)
                {
                    _logger.LogInformation(
                        "A message from received from {ChatId} {MessageId} {UserId}",
                        update.Message.Chat.Id,
                        update.Message.MessageId,
                        update.Message.From.Id);
                    await _vacancyReposter.TryRepost(update.Message);
                } 
                else if (update.Type == UpdateType.EditedMessage)
                {
                    _logger.LogInformation(
                        "An edited message received from {ChatId} {MessageId} {UserId}",
                        update.EditedMessage.Chat.Id,
                        update.EditedMessage.MessageId,
                        update.EditedMessage.From.Id);
                    await _vacancyReposter.TryEdit(update.EditedMessage);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Проблемы");
            }

            return Ok();
        }

        private bool IsChatAllowed(Update update)
        {
            if (!_appSettings.RepostOnlyFromChatIdsEnabled)
            {
                return true;
            }

            var allowedChatIds = _appSettings.RepostOnlyFromChatIds;

            var isNewMessageAllowed = update.Message != null 
                                      && allowedChatIds.Contains(update.Message.Chat.Id);

            var isEditedMessageAllowed = update.EditedMessage != null 
                                         && allowedChatIds.Contains(update.EditedMessage.Chat.Id);

            return isNewMessageAllowed || isEditedMessageAllowed;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Ok!");
        }
    }
}
