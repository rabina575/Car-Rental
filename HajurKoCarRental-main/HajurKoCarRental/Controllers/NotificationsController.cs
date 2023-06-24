using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HajurKoCarRental.Context;
using HajurKoCarRental.Models;

namespace HajurKoCarRental.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Notifications
        public async Task<IActionResult> Index()
        {
            var user = Utils.Utils.user;
            var userId = Utils.Utils.userId;
            var role = Utils.Utils.role;

            var applicationDbContext = _context.Notifications
                .Include(n => n.User)
                .Include(n => n.CarRequest)
                .Include(n => n.Offer).Where(n => n.RecipientId == null || n.RecipientId == userId);

            //if (role.Contains("Customer"))
                if (User.IsInRole("Customer"))
                {
                applicationDbContext = applicationDbContext
                .Where(n => n.RecipientId == userId || n.Title!="Request Cancellations");
            }
            //else if (role.Contains("Admin") || role.Contains("Staff"))
            else if (User.IsInRole("Staff") || User.IsInRole("Admin"))
            {
                applicationDbContext = _context.Notifications
                .Include(n => n.User)
                .Include(n => n.CarRequest)
                .Include(n => n.Offer)
                .Where(n => n.RecipientId == userId || n.Title == "Damage Report" || n.Title== "Request Cancellations");
            }
            return PartialView("_NotificationListPartial", await applicationDbContext.ToListAsync());
        }

        // GET: Notifications/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Notifications == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.User)
                .Include(n => n.CarRequest)
                .Include(n => n.Offer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // POST: Notifications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RecipientId,Title,Description,RequestId,OfferId,CreatedAt")] Notification notification)
        {
            if (ModelState.IsValid)
            {
                // Set the notification's createdAt date to the UTC DateTime object
                notification.CreatedAt = Utils.Utils.convertDate(notification.CreatedAt.ToString());

                _context.Add(notification);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "Name");
            ViewData["RequestId"] = new SelectList(_context.CarRequests, "Id", "Id", notification.RequestId);
            ViewData["OfferId"] = new SelectList(_context.Offers, "Id", "Description", notification.OfferId);
            return View(notification);
        }

        // POST: Notifications/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Notifications == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Notifications'  is null.");
            }
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
