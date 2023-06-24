using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HajurKoCarRental.Context;
using HajurKoCarRental.Models;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using NuGet.Protocol.Plugins;
using NuGet.Protocol;
using Microsoft.AspNetCore.Identity;

namespace HajurKoCarRental.Controllers
{
    public class DamagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DamagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Damages
        public async Task<IActionResult> Index()
        {
            var user = Utils.Utils.user;
            var userId = Utils.Utils.userId;
            var role = Utils.Utils.role;

            var applicationDbContext = _context.Damage.Include(c => c.User).Include(c => c.RepairBill).Include(c => c.CarRequest).Include(c => c.CarRequest.CustomerUser).Include(c => c.CarRequest.Car).Where(c => c.approvedBy == userId);

            if (User.IsInRole("Staff") || User.IsInRole("Admin"))
            {
                // Show only the requests for the current customer user
                applicationDbContext = applicationDbContext.Where(c => c.approvedBy== userId);
            }
            else
            {
                applicationDbContext = _context.Damage.Include(c => c.User).Include(c => c.RepairBill).Include(c => c.CarRequest).Include(c => c.CarRequest.CustomerUser).Include(c => c.CarRequest.Car).Where(c=> c.CarRequest.customerId == userId);
            }

            return View(await applicationDbContext.ToListAsync());
        }
        
        // GET: Damages/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Damage == null)
            {
                return NotFound();
            }

            var damage = await _context.Damage
                .Include(d => d.CarRequest)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (damage == null)
            {
                return NotFound();
            }

            return View(damage);
        }

        // GET: Damages/Create
        public IActionResult Create(Guid requestId, string approvedBy)
        {
            ViewData["requestId"] = requestId;
            ViewData["approvedBy"] = approvedBy;
            return View();
        }

        // POST: Damages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,approvedBy,requestId,paidAt,CreatedAt")] Damage damage)
        {
            if (ModelState.IsValid)
            {
                damage.Id = Guid.NewGuid();
                try
                {
                    _context.Add(damage);
                    await _context.SaveChangesAsync();

                    //Update damaged status of carRequest
                    var carRequest = await _context.CarRequests.FindAsync(damage.requestId);
                    carRequest.isDamaged = true;
                    _context.Entry(carRequest).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    // Create an instance of the Notification controller
                    NotificationsController notificationController = new NotificationsController(_context);

                    // Create a new Notification object with the notification details
                    Notification notification = new Notification
                    {
                        RecipientId = damage.approvedBy,
                        Title = "Damage Report",
                        Description = damage.Description,
                    };

                    // Call the Create method with the notification details as a parameter
                    await notificationController.Create(notification);

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DamageExists(damage.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["requestId"] = new SelectList(_context.CarRequests, "Id", "customerId", damage.requestId);
            ViewData["approvedBy"] = new SelectList(_context.CarRequests, "approvedBy", damage.approvedBy);
            return View(damage);
        }

        // GET: Damages/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Damage == null)
            {
                return NotFound();
            }

            var damage = await _context.Damage.FindAsync(id);
            if (damage == null)
            {
                return NotFound();
            }
            ViewData["requestId"] = new SelectList(_context.CarRequests, "Id", "customerId", damage.requestId);
            ViewData["approvedBy"] = new SelectList(_context.User, "Id", "Id", damage.approvedBy);
            return View(damage);
        }

        // POST: Damages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Description,requestId,approvedBy,paidAt,CreatedAt")] Damage damage)
        {
            if (id != damage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(damage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DamageExists(damage.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["requestId"] = new SelectList(_context.CarRequests, "Id", "customerId", damage.requestId);
            ViewData["approvedBy"] = new SelectList(_context.User, "Id", "Id", damage.approvedBy);
            return View(damage);
        }

        // GET: Damages/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Damage == null)
            {
                return NotFound();
            }

            var damage = await _context.Damage
                .Include(d => d.CarRequest)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (damage == null)
            {
                return NotFound();
            }

            return View(damage);
        }

        // POST: Damages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Damage == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Damage'  is null.");
            }
            var damage = await _context.Damage.FindAsync(id);
            if (damage != null)
            {
                _context.Damage.Remove(damage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DamageExists(Guid id)
        {
            return (_context.Damage?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // Redirect to the create repair bill page
        public IActionResult GenerateBill(Guid Id)
        {
            return RedirectToAction("Create", "RepairBills", new { damageId = Id });
        }

        // Redirect to the repair bill details page
        public IActionResult ShowRepairBill(Guid Id)
        {
            var repairBill = _context.RepairBill.SingleOrDefault(rb => rb.damageId == Id); ;
            if (repairBill != null)
            {
                return RedirectToAction("Details", "RepairBills", new { Id = repairBill.Id });
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ChangePaymentStatus(Guid Id)
        {
            // Retrieve the RepairBill instance with the given Id from the data store
            var repairBill = _context.RepairBill.FirstOrDefault(rb => rb.Id == Id);

            if (repairBill == null)
            {
                return NotFound();
            }

            // Update the isPaid attribute of the instance to true
            repairBill.isPaid = true;

            // Save the changes back to the data store
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

    }
}
