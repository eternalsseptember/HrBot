using System;
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

        public UpdateController(
            ITelegramBotClient telegramBot,
            ILogger<UpdateController> logger,
            IVacancyReposter vacancyReposter)
        {
            _telegramBot = telegramBot;
            _logger = logger;
            _vacancyReposter = vacancyReposter;
        }

        // POST api/update
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
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

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Ok!");
        }
    }
}
