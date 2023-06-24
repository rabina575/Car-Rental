using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HajurKoCarRental.Context;
using HajurKoCarRental.Models;
using Microsoft.AspNetCore.Identity;

namespace HajurKoCarRental.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DashboardController(ApplicationDbContext context, UserManager<User> userManager,
                  RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Data for Dashboard
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                Utils.Utils.user = await _userManager.GetUserAsync(User);
                Utils.Utils.userId = await _userManager.GetUserIdAsync(Utils.Utils.user);
                Utils.Utils.role = await _userManager.GetRolesAsync(Utils.Utils.user);
            }

            //getting inactive users and user roles
            Utils.Utils.inactiveUsers = getInactiveUsers();
            Utils.Utils.allUsers = await GetAllUsersWithRoles();

            var passengersCount = _context.User.Count();
            int inactivePassengersCount = 0;
            foreach (var item in _context.User)
            {
                var isActive = "Active";
                foreach (User i in Utils.Utils.inactiveUsers)
                {
                    if (item.Id == i.Id)
                    {
                        inactivePassengersCount++;
                    }
                }
            }
            int activePassengersCount = passengersCount - inactivePassengersCount;
            var carRequestsCount = _context.CarRequests.Count();
            var carsCount = _context.Cars.Count();
            var offersCount = _context.Offers.Count(o => o.ExpiresAt > DateTime.Now);
            var carRequests = _context.CarRequests.Include(c => c.ApprovalUser).Include(c => c.Car).Include(c => c.CustomerUser).ToList();

            //Get Sales Details
            var sales = carRequests
    .GroupBy(c => c.Car.Name)
        .Select(g => new Sales
        {
            CarName = g.Key,
            SalesCount = g.Count()
        })
        .ToList();


            var model = new DashboardViewModel
            {
                CarsCount = carsCount,
                OffersCount = offersCount,
                PassengersCount = passengersCount,
                InactivePassengersCount = inactivePassengersCount,
                ActivePassengersCount = activePassengersCount,
                CarRequestsCount = carRequestsCount,
                CarRequests = carRequests,
                SalesCounts = sales
        };

            return View(model);
    }

    private List<User> getInactiveUsers()
    {
        var users = _context.User.Where(u => !_context.CarRequests.Any(c => c.customerId == u.Id && c.requestedDate >= DateTime.Now.AddMonths(-3))).ToList();
        return users;
    }

    //users with roles to display in user panel
    private async Task<Dictionary<User, string>> GetAllUsersWithRoles()
    {
        var users = _context.Users.ToList();
        var dictionary = new Dictionary<User, string>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync((User)user);
            var role = roles.FirstOrDefault(); // Get the first role of the user
            dictionary.Add((User)user, role);
        }
        return dictionary;
    }
}
}
