using System.ComponentModel.DataAnnotations;

namespace HajurKoCarRental.Models
{
    public class CarRequest
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid carId { get; set; }
        public Car? Car { get; set; }
        public string? customerId { get; set; }
        public User? CustomerUser { get; set; }
        [Required]
        public int duration { get; set; }
        public string? approvedBy { get; set; }
        public User? ApprovalUser { get; set; }
        [Required]
        public DateTimeOffset requestedDate { get; set; }
        public DateTimeOffset? returnedDate { get; set; }
        public int totalDiscount { get; set; }
        public int totalAmount { get; set; }
        public bool isDamaged { get; set; }
        public float? rating { get; set; }
        public bool isPaid { get; set; }
        public Status status { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Damage>? Damage { get; set; }
        public ICollection<Notification>? Notifications { get; set; }
    }

    public enum Status
    {
        PENDING, APPROVED, REJECTED, CANCELLED
    }
}
