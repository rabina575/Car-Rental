using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HajurKoCarRental.Context;
using HajurKoCarRental.Models;
using HajurKoCarRental.Utils;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;

namespace HajurKoCarRental.Controllers
{
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public CarsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Cars
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                Utils.Utils.user = await _userManager.GetUserAsync(User);
                Utils.Utils.userId = await _userManager.GetUserIdAsync(Utils.Utils.user);
                Utils.Utils.role = await _userManager.GetRolesAsync(Utils.Utils.user);
            }

            Utils.Utils.context = Utils.Utils.GetContext();
            Utils.Utils.ratings = Utils.Utils.GetAverageRating();
            Utils.Utils.frequency = Utils.Utils.GetRentFrequency();

            return _context.Cars != null ?
                        View(await _context.Cars.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.Cars'  is null.");
        }

        // GET: Cars/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Cars == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // GET: Cars/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cars/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Rate,IsAvailable,CreatedAt")] Car car, IFormFile fileInput)
        {
            if (ModelState.IsValid)
            {
                // Set the user's created-at property to the UTC DateTime object
                car.CreatedAt = Utils.Utils.convertDate(car.CreatedAt.ToString());
                var url = "";

                if (fileInput == null || fileInput.Length == 0)
                {
                    return BadRequest("No file selected");
                }


                if (fileInput.Length > 1.5 * 1024 * 1024) // 1.5MB
                {

                    throw new CannotUnloadAppDomainException("File size exceeds the limit");
                }

                string fileExtension = Path.GetExtension(fileInput.FileName);
                if (fileExtension.ToLower() != ".png" && fileExtension.ToLower() != ".jpg" && fileExtension.ToLower() != ".jpeg")
                {
                    throw new CannotUnloadAppDomainException("Invalid file type");
                }
                else
                {
                    try
                    {
                        //storing image to firebase
                        var firebaseStorageHelper = new FirebaseStorageHelper();
                        var textInput = fileInput.FileName.Substring(fileInput.FileName.LastIndexOf("/") + 1);

                        // Copy the contents of the uploaded file to a memory stream
                        using var memoryStream = new MemoryStream();
                        await fileInput.CopyToAsync(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        url = await firebaseStorageHelper.UploadFileAsync(textInput, memoryStream); // pass the stream to the Firebase storage helper method
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("An error occurred: {0}", ex.Message);
                    }
                }
                car.Image = url;
                _context.Add(car);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(car);
        }

        // GET: Cars/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Cars == null)
            {
                return NotFound();
            }

            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }
            return View(car);
        }

        // POST: Cars/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Rate,IsAvailable,CreatedAt")] Car car, IFormFile? fileInput)
        {
            var carBackup = new Car();
            carBackup.Name = car.Name;
            carBackup.Rate = car.Rate;
            if (id != car.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Reload the entity to get the latest version from the database
                    _context.Entry(car).Reload();

                    // Set the user's created-at property to the UTC DateTime object
                    car.CreatedAt = Utils.Utils.convertDate(car.CreatedAt.ToString());
                    var url = car.Image;

                    if (fileInput != null)
                    {
                        if (fileInput.Length > 1.5 * 1024 * 1024) // 1.5MB
                        {
                            throw new CannotUnloadAppDomainException("File size exceeds the limit");
                        }

                        string fileExtension = Path.GetExtension(fileInput.FileName);
                        if (fileExtension.ToLower() != ".png" && fileExtension.ToLower() != ".jpg" && fileExtension.ToLower() != ".jpeg")
                        {
                            throw new CannotUnloadAppDomainException("Invalid file type");
                        }
                        else
                        {
                            try
                            {
                                //storing image to firebase
                                var firebaseStorageHelper = new FirebaseStorageHelper();
                                var textInput = fileInput.FileName.Substring(fileInput.FileName.LastIndexOf("/") + 1);

                                // Copy the contents of the uploaded file to a memory stream
                                using var memoryStream = new MemoryStream();
                                await fileInput.CopyToAsync(memoryStream);
                                memoryStream.Seek(0, SeekOrigin.Begin);
                                url = await firebaseStorageHelper.UploadFileAsync(textInput, memoryStream); // pass the stream to the Firebase storage helper method
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceError("An error occurred: {0}", ex.Message);
                            }
                        }
                    }
                    car.Image = url;
                    car.Name = carBackup.Name;
                    car.Rate= carBackup.Rate;
                    _context.Update(car);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarExists(car.Id))
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
            return View(car);
        }

        // GET: Cars/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Cars == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // POST: Cars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Cars == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Cars'  is null.");
            }
            var car = await _context.Cars.FindAsync(id);
            if (car != null)
            {
                _context.Cars.Remove(car);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CarExists(Guid id)
        {
            return (_context.Cars?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}