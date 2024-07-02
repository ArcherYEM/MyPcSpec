using Microsoft.AspNetCore.Mvc;
using MyPCSpec.Models;
using System.Diagnostics;

namespace MyPCSpec.Controllers
{
	public class AccountController : Controller
	{
		private readonly ILogger<AccountController> _logger;

		public AccountController(ILogger<AccountController> logger)
		{
			_logger = logger;
		}

		public IActionResult Login()
		{
			return View();
		}

        public IActionResult Join()
        {
            return View();
        }
    }
}
