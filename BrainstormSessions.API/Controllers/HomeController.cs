using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BrainstormSessions.Core.Interfaces;
using BrainstormSessions.Core.Model;
using BrainstormSessions.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BrainstormSessions.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBrainstormSessionRepository _sessionRepository;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IBrainstormSessionRepository sessionRepository, ILogger<HomeController> logger)
        {
            _sessionRepository = sessionRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Index action invoked.");

            try
            {
                _logger.LogInformation("Fetching session list from repository.");
                var sessionList = await _sessionRepository.ListAsync();

                _logger.LogInformation("Transforming session list to view model.");
                var model = sessionList.Select(session => new StormSessionViewModel
                {
                    Id = session.Id,
                    DateCreated = session.DateCreated,
                    Name = session.Name,
                    IdeaCount = session.Ideas.Count
                });

                _logger.LogInformation("Returning view with {SessionCount} sessions.", model.Count());
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching or processing the session list.");
                throw; // Re-throw to ensure any higher-level exception handling is triggered
            }
        }

        public class NewSessionModel
        {
            [Required]
            public string SessionName { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Index(NewSessionModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid. Errors: {ModelStateErrors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Adding a new brainstorm session with name: {SessionName}", model.SessionName);

                await _sessionRepository.AddAsync(new BrainstormSession
                {
                    DateCreated = DateTimeOffset.Now,
                    Name = model.SessionName
                });

                _logger.LogInformation("New brainstorm session successfully added: {SessionName}", model.SessionName);
                return RedirectToAction(actionName: nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a new brainstorm session.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
