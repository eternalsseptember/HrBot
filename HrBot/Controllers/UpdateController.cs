using System;
using System.Linq;
using System.Threading.Tasks;
using HrBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HrBot.Controllers
{
    [ApiController]
    [Route("api/hrupdate")]
    public class UpdateController : ControllerBase
    {
        private readonly ILogger<UpdateController> _logger;
        private readonly ITelegramBotClient _telegramBot;
        private readonly IVacancyReposter _vacancyReposter;
        private readonly AppSettings _appSettings;

        public UpdateController(
            ITelegramBotClient telegramBot,
            ILogger<UpdateController> logger,
            IVacancyReposter vacancyReposter,
            AppSettings appSettings)
        {
            _telegramBot = telegramBot;
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
                    await _vacancyReposter.TryRepost(update.Message);
                }

                if (update.Type == UpdateType.EditedMessage)
                {
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
