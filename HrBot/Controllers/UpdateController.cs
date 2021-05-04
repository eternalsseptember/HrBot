using System;
using System.Threading.Tasks;
using HrBot.Configuration;
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
        private readonly ChatOptions _options;
        private readonly ILogger<UpdateController> _logger;
        private readonly IVacancyReposter _vacancyReposter;


        public UpdateController(
            ILogger<UpdateController> logger,
            IOptions<ChatOptions> options,
            IVacancyReposter vacancyReposter)
        {
            _options = options.Value;
            _logger = logger;
            _vacancyReposter = vacancyReposter;
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            if (!IsMessageAllowed(update))
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


        private bool IsMessageAllowed(Update update)
        {
            if (!_options.RepostOnlyFromAllowedChats)
                return true;

            var allowedChatIds = _options.AllowedChatIds;

            var isNewMessageAllowed = update.Message != null
                && allowedChatIds.Contains(update.Message.Chat.Id);

            var isEditedMessageAllowed = update.EditedMessage != null
                && allowedChatIds.Contains(update.EditedMessage.Chat.Id);

            return isNewMessageAllowed || isEditedMessageAllowed;
        }
    }
}
