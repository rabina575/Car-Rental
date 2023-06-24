using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

namespace HajurKoCarRental.Models
{
    public class User : IdentityUser
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Phone { get; set; }
        public IFormFile? DocumentFile { get; set; }
        public string? Document { get; set; }
        public DocumentStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTime.Now;


        //for foreign keys
        public ICollection<CarRequest>? CarRequestsCustomer { get; set; }
        public ICollection<CarRequest>? CarRequestsApproval { get; set; }
        public ICollection<Damage>? DamageRequest { get; set; }
        public ICollection<Notification>? Notifications { get; set; }
    }
    public enum DocumentStatus
    {
        PENDING, VERIFIED, NOTVERIFIED
    }
}
