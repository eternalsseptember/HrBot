using System;
using System.Linq;
using System.Threading.Tasks;
using HrBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _vacancyReposter = vacancyReposter;
            _appSettings = appSettings.Value;
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
                if (update is { Type: UpdateType.Message, Message: {} message })
                {
                    _logger.LogInformation(
                        "A message from received from {ChatId} {MessageId} {UserId}",
                        message.Chat.Id,
                        message.MessageId,
                        message.From?.Id);
                    await _vacancyReposter.TryRepost(update.Message);
                } 
                else if (update is { Type: UpdateType.EditedMessage, EditedMessage: {} editedMessage })
                {
                    _logger.LogInformation(
                        "An edited message received from {ChatId} {MessageId} {UserId}",
                        editedMessage.Chat.Id,
                        editedMessage.MessageId,
                        editedMessage.From?.Id);
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
