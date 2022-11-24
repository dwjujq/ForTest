using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SilverSoft.Logging;

namespace Demo.Pages
{
    [IgnoreAntiforgeryToken]
    public class ApplicationErrorModel : PageModel
    {
        private IAppLogger<ApplicationErrorModel> _logger;

        public ApplicationErrorModel(IAppLogger<ApplicationErrorModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            try
            {
                var exceptionHandler = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

                _logger.ErrorLog(exceptionHandler.Error);
            }
            catch(Exception ex)
            {
                _logger.ErrorLog(ex);
            }
        }
    }
}
