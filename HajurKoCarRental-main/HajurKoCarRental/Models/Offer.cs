using System.ComponentModel.DataAnnotations;

namespace HajurKoCarRental.Models
{
    public class Offer
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public Guid CarId { get; set; }
        public Car? Car { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public int Discount { get; set; }
        [Required]
        public DateTimeOffset CreatedAt { get; set; } = DateTime.Now;
        [Required]
        public DateTimeOffset ExpiresAt { get; set; }

        public ICollection<Notification>? Notifications { get; set; }
    }
}
