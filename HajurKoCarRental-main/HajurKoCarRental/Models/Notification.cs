using System.ComponentModel.DataAnnotations;

namespace HajurKoCarRental.Models
{
    public class Notification
    {
        [Required]
        public Guid Id { get; set; }
        public string? RecipientId { get; set; }
        public User? User { get; set; }
        [Required]
        public String Title { get; set; }
        [Required]
        public string Description { get; set; }
        public Guid? RequestId { get; set; }
        public CarRequest? CarRequest { get; set; }
        public Guid? OfferId { get; set; }
        public Offer? Offer { get; set; }

        [Required]
        public DateTimeOffset CreatedAt { get; set; } = DateTime.Now;
    }
}
