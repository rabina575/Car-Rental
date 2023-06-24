using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HajurKoCarRental.Context;
using HajurKoCarRental.Models;

namespace HajurKoCarRental.Controllers
{
    public class OffersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OffersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Offers
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Offers.Include(o => o.Car).Where(o => o.ExpiresAt> DateTime.Now);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Offers/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Offers == null)
            {
                return NotFound();
            }

            var offer = await _context.Offers
                .Include(o => o.Car)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (offer == null)
            {
                return NotFound();
            }

            return View(offer);
        }

        // GET: Offers/Create
        public IActionResult Create()
        {
            ViewData["CarId"] = new SelectList(_context.Cars, "Id", "Name");
            return View();
        }

        // POST: Offers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,CarId,Description,Discount,CreatedAt,ExpiresAt")] Offer offer)
        {
            if (ModelState.IsValid)
            {
                // Set the offer's dates to the UTC DateTime object
                offer.CreatedAt = Utils.Utils.convertDate(offer.CreatedAt.ToString());
                offer.ExpiresAt = Utils.Utils.convertDate(offer.ExpiresAt.ToString());


                try
                {
                    _context.Add(offer);
                    await _context.SaveChangesAsync();

                    // Create an instance of the Notification controller
                    NotificationsController notificationController = new NotificationsController(_context);

                    // Create a new Notification object with the notification details
                    Notification notification = new Notification
                    {
                        Title = "Offer",
                        Description = offer.Description,
                        OfferId = offer.Id
                    };

                    // Call the Create method with the notification details as a parameter
                    await notificationController.Create(notification);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OfferExists(offer.Id))
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
            ViewData["CarId"] = new SelectList(_context.Cars, "Id", "Name", offer.CarId);
            return View(offer);
        }

        // GET: Offers/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Offers == null)
            {
                return NotFound();
            }

            var offer = await _context.Offers.FindAsync(id);
            if (offer == null)
            {
                return NotFound();
            }
            ViewData["CarId"] = new SelectList(_context.Cars, "Id", "Name", offer.CarId);
            return View(offer);
        }

        // POST: Offers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,CarId,Description,Discount,CreatedAt,ExpiresAt")] Offer offer)
        {
            if (id != offer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(offer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OfferExists(offer.Id))
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
            ViewData["CarId"] = new SelectList(_context.Cars, "Id", "Name", offer.CarId);
            return View(offer);
        }

        // GET: Offers/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Offers == null)
            {
                return NotFound();
            }

            var offer = await _context.Offers
                .Include(o => o.Car)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (offer == null)
            {
                return NotFound();
            }

            return View(offer);
        }

        // POST: Offers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Offers == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Offers'  is null.");
            }
            var offer = await _context.Offers.FindAsync(id);
            if (offer != null)
            {
                _context.Offers.Remove(offer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OfferExists(Guid id)
        {
            return (_context.Offers?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
