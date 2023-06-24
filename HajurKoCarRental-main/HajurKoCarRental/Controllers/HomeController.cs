using HajurKoCarRental.Context;
using HajurKoCarRental.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace HajurKoCarRental.Controllers
{
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, UserManager<User> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                Utils.Utils.user = await _userManager.GetUserAsync(User);
                Utils.Utils.userId = await _userManager.GetUserIdAsync(Utils.Utils.user);
                Utils.Utils.role = await _userManager.GetRolesAsync(Utils.Utils.user);
            }
            return _context.Cars != null ?
                        View(await _context.Cars.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.Cars'  is null.");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}