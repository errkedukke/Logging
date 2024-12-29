using System.Threading.Tasks;
using BrainstormSessions.Core.Interfaces;
using BrainstormSessions.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BrainstormSessions.Controllers
{
    public class SessionController : Controller
    {
        private readonly IBrainstormSessionRepository _sessionRepository;
        private readonly ILogger<SessionController> _logger;

        public SessionController(IBrainstormSessionRepository sessionRepository, ILogger<SessionController> logger)
        {
            _sessionRepository = sessionRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? id)
        {
            _logger.LogDebug("Index action invoked with id: {Id}", id);

            if (!id.HasValue)
            {
                _logger.LogDebug("No id provided. Redirecting to Home Index.");
                return RedirectToAction(actionName: nameof(Index), controllerName: "Home");
            }

            var session = await _sessionRepository.GetByIdAsync(id.Value);
            if (session == null)
            {
                _logger.LogDebug("Session with id {Id} not found.", id);
                return Content("Session not found.");
            }

            var viewModel = new StormSessionViewModel
            {
                DateCreated = session.DateCreated,
                Name = session.Name,
                Id = session.Id
            };

            _logger.LogDebug("Session with id {Id} found. Returning view model.", id);
            return View(viewModel);
        }
    }
}
