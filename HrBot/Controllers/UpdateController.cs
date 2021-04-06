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
        public UpdateController(
            ILogger<UpdateController> logger,
            IOptions<AppSettings> appSettings,
            IVacancyReposter vacancyReposter)
        {
            _appSettings = appSettings.Value;
            _logger = logger;
            _vacancyReposter = vacancyReposter;
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            if (!IsChatAllowed(update))
                return Ok();

            try
            {
                if (update.Type == UpdateType.Message)
                {
                    _logger.LogInformation(
                        "A message from received from {ChatId} {MessageId} {UserId}",
                        update.Message.Chat.Id,
                        update.Message.MessageId,
                        update.Message.From.Id);
                    await _vacancyReposter.RepostToChannel(update.Message);
                }
                else if (update.Type == UpdateType.EditedMessage)
                {
                    _logger.LogInformation(
                        "An edited message received from {ChatId} {MessageId} {UserId}",
                        update.EditedMessage.Chat.Id,
                        update.EditedMessage.MessageId,
                        update.EditedMessage.From.Id);
                    await _vacancyReposter.Edit(update.EditedMessage);
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
                return true;

            var allowedChatIds = _appSettings.RepostOnlyFromChatIds;

            var isNewMessageAllowed = update.Message != null
                && allowedChatIds.Contains(update.Message.Chat.Id);

            var isEditedMessageAllowed = update.EditedMessage != null
                && allowedChatIds.Contains(update.EditedMessage.Chat.Id);

            return isNewMessageAllowed || isEditedMessageAllowed;
        }


        private readonly AppSettings _appSettings;
        private readonly ILogger<UpdateController> _logger;
        private readonly IVacancyReposter _vacancyReposter;
    }
}
