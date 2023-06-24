using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HajurKoCarRental.Context;
using HajurKoCarRental.Models;
using Microsoft.AspNetCore.Identity;
using HajurKoCarRental.Utils;
using System.Diagnostics;

namespace HajurKoCarRental.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public UserController(ApplicationDbContext context, UserManager<User> userManager,
                  RoleManager<IdentityRole> roleManager
   )
        {
            _context = context;

            _userManager = userManager;
            _roleManager = roleManager;

        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            Utils.Utils.inactiveUsers = getInactiveUsers();
            Utils.Utils.allUsers = await GetAllUsersWithRoles();
            //getting inactive users and user roles
            return _context.User != null ?
                        View(await _context.User.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.User'  is null.");
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null || _context.User == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,UserName,Email,Password,Phone,CreatedAt")] User user, IFormFile? fileInput)
        {
            if (ModelState.IsValid)
            {
                // Set the user's created-at property to the UTC DateTime object
                user.CreatedAt = Utils.Utils.convertDate(user.CreatedAt.ToString());

                //status is not verified on user registration
                user.Status = DocumentStatus.NOTVERIFIED;

                var url = "";
                if (fileInput != null)
                {
                    if (fileInput.Length > 1.5 * 1024 * 1024) // 1.5MB
                    {
                        throw new CannotUnloadAppDomainException("File size exceeds the limit");
                    }

                    string fileExtension = Path.GetExtension(fileInput.FileName);
                    if (fileExtension.ToLower() != ".png" && fileExtension.ToLower() != ".pdf")
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
                    //status is pending if user inputs the document
                    user.Status = DocumentStatus.PENDING;
                    user.Document = url;
                }
                var createResp = await _userManager.CreateAsync(user, user.Password);
                if (!createResp.Succeeded)
                {

                }
                var roleResp = await _userManager.AddToRoleAsync(user, UserRoles.Staff);

                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null || _context.User == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: User/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Name,Address,Email,Phone,Password")] User user, IFormFile? Document)
        {
            var backupRequest = new User();
            backupRequest.Name = user.Name;
            backupRequest.Email = user.Email;
            backupRequest.Address = user.Address;
            backupRequest.Phone = user.Phone;
            backupRequest.Password = user.Password;
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Reload the entity to get the latest version from the database
                    _context.Entry(user).Reload();

                    // Set the user's created-at property to the UTC DateTime object
                    user.CreatedAt = Utils.Utils.convertDate(user.CreatedAt.ToString());

                    var url = user.Document;

                    if (Document != null)
                    {
                        if (Document.Length > 1.5 * 1024 * 1024) // 1.5MB
                        {
                            throw new CannotUnloadAppDomainException("File size exceeds the limit");
                        }

                        string fileExtension = Path.GetExtension(Document.FileName);
                        if (fileExtension.ToLower() != ".png" && fileExtension.ToLower() != ".pdf")
                        {
                            throw new CannotUnloadAppDomainException("Invalid file type");
                        }
                        else
                        {
                            try
                            {
                                //storing image to firebase
                                var firebaseStorageHelper = new FirebaseStorageHelper();
                                var textInput = Document.FileName.Substring(Document.FileName.LastIndexOf("/") + 1);

                                // Copy the contents of the uploaded file to a memory stream
                                using var memoryStream = new MemoryStream();
                                await Document.CopyToAsync(memoryStream);
                                memoryStream.Seek(0, SeekOrigin.Begin);
                                url = await firebaseStorageHelper.UploadFileAsync(textInput, memoryStream); // pass the stream to the Firebase storage helper method
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceError("An error occurred: {0}", ex.Message);
                            }
                        }
                    }
                    //status is pending if user inputs the document
                    user.Status = DocumentStatus.PENDING;

                    user.Document = url;
                    user.Name = backupRequest.Name;
                    user.Address = backupRequest.Address;
                    user.Email = backupRequest.Email;
                    user.Phone = backupRequest.Phone;
                    user.Password = backupRequest.Password;
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                if (User.IsInRole("Staff") || User.IsInRole("Admin"))
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return RedirectToAction("Details", "User", new { id = user.Id });
                }
            }
            return View(user);
        }

        public async Task<IActionResult> ChangeDocumentStatus(string id, DocumentStatus status)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Status = status;
            var description = "";
            if (user.Status == DocumentStatus.NOTVERIFIED)
            {
                description = "Your document has been rejected. Please submit new valid ones.";
            }
            else if (user.Status == DocumentStatus.VERIFIED)
            {
                description = "Your document has been verified. You can now request for renting cars.";
            }

            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();

                // Create an instance of the Notification controller
                NotificationsController notificationController = new NotificationsController(_context);

                // Create a new Notification object with the notification details
                Notification notification = new Notification
                {
                    RecipientId = user.Id,
                    Title = "Document Verification Status",
                    Description = description
                };

                // Call the Create method with the notification details as a parameter
                await notificationController.Create(notification);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(user.Id))
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

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null || _context.User == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.User == null)
            {
                return Problem("Entity set 'ApplicationDbContext.User'  is null.");
            }
            var user = await _context.User.FindAsync(id);
            if (user != null)
            {
                _context.User.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(string id)
        {
            return (_context.User?.Any(e => e.Id == id)).GetValueOrDefault();
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