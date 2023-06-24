using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HajurKoCarRental.Context;
using HajurKoCarRental.Models;

namespace HajurKoCarRental.Controllers
{
    public class RepairBillsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RepairBillsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: RepairBills/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.RepairBill == null)
            {
                return NotFound();
            }

            var repairBill = await _context.RepairBill
                .Include(r => r.Damage)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (repairBill == null)
            {
                return NotFound();
            }

            return View(repairBill);
        }

        // GET: RepairBills/Create
        public IActionResult Create(Guid damageId)
        {
            ViewData["damageId"] = damageId;
            return View();
        }


        // POST: RepairBills/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,damageId,amount,isPaid,paidAt")] RepairBill repairBill)
        {
            if (ModelState.IsValid)
            {
                repairBill.Id = Guid.NewGuid();
                repairBill.isPaid = false;
                try
                {
                    _context.Add(repairBill);
                    await _context.SaveChangesAsync();
                    // Create an instance of the Notification controller
                    NotificationsController notificationController = new NotificationsController(_context);

                    var damage = await _context.Damage.FindAsync(repairBill.damageId);
                    var request = await _context.CarRequests.FindAsync(damage.requestId);
                    var user = await _context.User.FindAsync(request.customerId);

                // Create a new Notification object with the notification details
                Notification notification = new Notification
                    {
                        RecipientId = user.Id,
                        Title = "Repair Bill",
                        Description = "Your repair bill is " + repairBill.amount,
                    };

                    // Call the Create method with the notification details as a parameter
                    await notificationController.Create(notification);

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RepairBillExists(repairBill.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Damages");
            }
            ViewData["damageId"] = new SelectList(_context.Damage, "Id", "Description", repairBill.damageId);
            return View(repairBill);
        }

        private bool RepairBillExists(Guid id)
        {
            return (_context.RepairBill?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
