using System.ComponentModel.DataAnnotations;

namespace HajurKoCarRental.Models
{
    public class Damage
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Description { get; set; }
        public Guid requestId { get; set; }
        public CarRequest? CarRequest { get; set; }
        public string? approvedBy { get; set; }
        public User? User { get; set; }
        public DateTimeOffset? paidAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTime.Now;
        public RepairBill? RepairBill { get; set; }
    }
}
