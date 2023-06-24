using System.ComponentModel.DataAnnotations;

namespace HajurKoCarRental.Models
{
    public class RepairBill
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid damageId { get; set; }
        public Damage? Damage { get; set; }
        [Required]
        public int amount { get; set; }
        public bool isPaid { get; set; }
        public DateTimeOffset? paidAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTime.Now;
    }
}
