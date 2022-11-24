using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SilverSoft.Logging;

namespace Demo.Pages
{
    [IgnoreAntiforgeryToken]
    public class Error404 : PageModel
    {
        private IAppLogger<Error404> _logger;

        public Error404(IAppLogger<Error404> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}
