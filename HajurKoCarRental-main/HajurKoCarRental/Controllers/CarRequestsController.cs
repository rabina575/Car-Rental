using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HajurKoCarRental.Context;
using HajurKoCarRental.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Principal;
using Microsoft.AspNetCore.Authorization;
using System.Linq;


namespace HajurKoCarRental.Controllers
{
    //[Authorize]
    public class CarRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;


        public CarRequestsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;

            _userManager = userManager;

        }

        // GET: CarRequests
        public async Task<IActionResult> Index()
        {
            var user = Utils.Utils.user;
            var userId = Utils.Utils.userId;

            var applicationDbContext = _context.CarRequests.Include(c => c.ApprovalUser).Include(c => c.Car).Include(c => c.CustomerUser).Where(c => c.customerId == userId);

            if (User.IsInRole("Customer"))
            {
                // Show only the requests for the current customer user
                applicationDbContext = applicationDbContext.Where(c => c.customerId == userId);
            }
            else
            {
                applicationDbContext = _context.CarRequests.Include(c => c.ApprovalUser).Include(c => c.Car).Include(c => c.CustomerUser);
            }

            // Sort the list by requestedDate in ascending order
            var sortedCarRequests = applicationDbContext.OrderBy(cr => cr.requestedDate);

            return View(await sortedCarRequests.ToListAsync());
        }


        // GET: CarRequests/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.CarRequests == null)
            {
                return NotFound();
            }

            var carRequest = await _context.CarRequests
                .Include(c => c.ApprovalUser)
                .Include(c => c.Car)
                .Include(c => c.CustomerUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (carRequest == null)
            {
                return NotFound();
            }

            return View(carRequest);
        }

        // GET: CarRequests/Create
        public IActionResult Create(Guid? id)
        {
            ViewData["approvedBy"] = new SelectList(_context.Users, "Id", "Name");
            ViewData["carId"] = id;
            var userId = _userManager.GetUserId(User);
            return View();
        }

        // POST: CarRequests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,duration,requestedDate,returnedDate,totalDiscount,totalAmount,isDamaged,rating,isPaid,status,CreatedAt")] CarRequest carRequest, Guid carId)//, int approvedBy)
        {
            var backupRequest = new CarRequest();
            backupRequest.duration = carRequest.duration;
            backupRequest.requestedDate = carRequest.requestedDate;
            backupRequest.CreatedAt = carRequest.CreatedAt;

            // Reload the entity to get the latest version from the database
            _context.Entry(carRequest).Reload();

            var request = _context.CarRequests.Where(c => c.customerId == Utils.Utils.userId).ToList();
            var isPaid = true;
            foreach (var requests in request)
            {
                var damages = await _context.Damage.FirstOrDefaultAsync(m => m.requestId == requests.Id);
                if (damages != null)
                {
                    var repairBills = await _context.RepairBill.FirstOrDefaultAsync(m => m.damageId == damages.Id);
                    if (repairBills != null)
                    {
                        if (repairBills.isPaid == false)
                        {
                            isPaid = false;
                        }
                    }
                }
            }

            var user = Utils.Utils.user;

            if (user.Status == DocumentStatus.VERIFIED)
            {
                if (isPaid == true)
                {
                    if (ModelState.IsValid)
                    {
                        if (isDateAvailable(carId, backupRequest.requestedDate, backupRequest.duration)) 
                        {
                            // Reload the entity to get the latest version from the database
                            _context.Entry(carRequest).Reload();

                            carRequest.Id = Guid.NewGuid();

                            var customerId = carRequest.customerId = Utils.Utils.userId;

                            carRequest.duration = backupRequest.duration;

                            // Set the user's created-at property to the UTC DateTime object
                            carRequest.CreatedAt = Utils.Utils.convertDate(backupRequest.CreatedAt.ToString());
                            carRequest.requestedDate = Utils.Utils.convertDate(backupRequest.requestedDate.ToString());

                            if (carRequest.returnedDate.ToString() != "")
                            {
                                carRequest.returnedDate = Utils.Utils.convertDate(carRequest.returnedDate.ToString());
                            }
                            else
                            {
                                carRequest.returnedDate = null;
                            }

                            // Set the navigation properties
                            carRequest.Car = await _context.Cars.FindAsync(carId);
                            carRequest.CustomerUser = await _userManager.FindByIdAsync(customerId);


                            // Get the roles of the current user
                            var roles = Utils.Utils.role;

                            if (roles.Contains(UserRoles.Admin) || roles.Contains(UserRoles.Staff))
                            {

                                carRequest.totalDiscount = 25;
                            }
                            else
                            {
                                //if user is a regular customer, 10% discount on carRequest + if offer on that car
                                var customerRequests = _context.CarRequests.Where(c => c.CustomerUser.Id == customerId).ToList();
                                var isRegularCustomer = customerRequests.Count > 1;


                                //checking for offers
                                var carOffers = await _context.Offers.FirstOrDefaultAsync(o => o.Car.Id == carId);
                                var carDiscount = 0;
                                if (carOffers != null && carOffers.ExpiresAt > DateTime.UtcNow)//carOffers.ExpiresAt> DateOnly.FromDateTime(DateTime.UtcNow))
                                {
                                    carDiscount = carOffers.Discount;
                                }

                                if (isRegularCustomer)
                                {
                                    carRequest.totalDiscount = 10 + carDiscount;
                                }
                                else
                                {
                                    carRequest.totalDiscount = 0 + carDiscount;
                                }
                            }

                            var total = int.Parse(carRequest.Car.Rate) * carRequest.duration;
                            carRequest.totalAmount = total - (total * carRequest.totalDiscount / 100);

                            //status is pending on request
                            carRequest.status = Status.PENDING;

                            _context.Add(carRequest);
                            await _context.SaveChangesAsync();
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            return BadRequest("The requested date or duration is not available");
                        }
                    }
                }
                else
                {
                    return BadRequest("Your repair bills haven't been paid. Please clear the bills to rent other cars");
                }
            }
            else
            {
                return BadRequest("Your documents are either not uploaded or unverified.");
            }

            ViewData["carId"] = new SelectList(_context.Cars, "Id", "Name", carRequest.carId);
            ViewData["customerId"] = new SelectList(_context.Users, "Id", "Name", carRequest.customerId);
            return View(carRequest);
        }

        //checking the availability of car on requested date and duration
        bool isDateAvailable(Guid carId, DateTimeOffset requestedDate, int durationInDays)
        {
             var overlappingRequests = _context.CarRequests.Where(cr =>
                        cr.carId == carId &&
                        cr.requestedDate <= requestedDate.AddDays(durationInDays) &&
                        cr.requestedDate.AddDays(cr.duration) >= requestedDate &&
                        (cr.status == Status.PENDING || cr.status == Status.APPROVED)).ToList();
            return !overlappingRequests.Any();
        }
       
        private bool CarRequestExists(Guid id)
        {
            return (_context.CarRequests?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        //changing request status for approval or rejection
        public async Task<IActionResult> ChangeStatus(Guid id, Status status)
        {
            var carRequest = await _context.CarRequests.FindAsync(id);
            if (carRequest == null)
            {
                return NotFound();
            }

            carRequest.status = status;
            carRequest.approvedBy = Utils.Utils.userId;

            try
            {
                _context.Update(carRequest);
                await _context.SaveChangesAsync();

                //Change availability status of car
               /* if (carRequest.status == Status.APPROVED)
                {
                    var car = await _context.Cars.FindAsync(carRequest.carId);
                    if (car != null)
                    {
                        car.IsAvailable = false; // set availability status to false
                        _context.Update(car);
                        await _context.SaveChangesAsync();
                    }
                }*/

                // Create an instance of the Notification controller
                NotificationsController notificationController = new NotificationsController(_context);

                // Create a new Notification object with the notification details
                Notification notification = new Notification
                {
                    RecipientId = carRequest.customerId,
                    Title = "Request Status",
                    Description = "Your status has been " + carRequest.status.ToString().ToLower(),
                    RequestId = id
                };

                // Call the Create method with the notification details as a parameter
                await notificationController.Create(notification);

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarRequestExists(carRequest.Id))
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

        //method to report damage by the users
        public IActionResult ReportDamage(Guid id, string approvedBy)
        {
            return RedirectToAction("Create", "Damages", new { requestId = id, approvedBy = approvedBy });
        }


        //give rating after the request has completed
        public async Task<IActionResult> GiveRating(Guid id, float rating)
        {
            var carRequest = await _context.CarRequests.FindAsync(id);
            if (carRequest == null)
            {
                return NotFound();
            }

            carRequest.rating = rating;

            try
            {
                _context.Update(carRequest);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarRequestExists(carRequest.Id))
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

        //initialize the return date by admin
        public async Task<IActionResult> ReturnCar(Guid id)
        {
            var carRequest = await _context.CarRequests.FindAsync(id);
            if (carRequest == null)
            {
                return NotFound();
            }

            carRequest.returnedDate = DateTime.Now;

            try
            {
                _context.Update(carRequest);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarRequestExists(carRequest.Id))
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

        //allowing users to cancel their request
        public async Task<IActionResult> CancelRequest(Guid id)
        {
            var carRequest = await _context.CarRequests.FindAsync(id);
            if (carRequest == null)
            {
                return NotFound();
            }

            carRequest.status = Status.CANCELLED;

            try
            {
                _context.Update(carRequest);
                await _context.SaveChangesAsync();

                // Create an instance of the Notification controller
                NotificationsController notificationController = new NotificationsController(_context);

                // Create a new Notification object with the notification details
                Notification notification = new Notification
                {
                    Title = "Request Cancellations",
                    Description = "The user has cancelled the approved rent request.",
                    RequestId = id
                };

                // Call the Create method with the notification details as a parameter
                await notificationController.Create(notification);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarRequestExists(carRequest.Id))
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

        //allowing admin to input the payment status
        public IActionResult ChangePaymentStatus(Guid Id)
        {
            // Retrieve the RepairBill instance with the given Id from the data store
            var carRequest = _context.CarRequests.FirstOrDefault(c => c.Id == Id);

            if (carRequest == null)
            {
                return NotFound();
            }

            // Update the isPaid attribute of the instance to true
            carRequest.isPaid = true;

            // Save the changes back to the data store
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}