using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HajurKoCarRental.Models
{
    public class Car
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Rate must be a number.")]
        public string Rate { get; set; }
        [Required]
        public bool IsAvailable { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? Image { get; set; }
        [DisplayName("Added On")]
        public DateTimeOffset CreatedAt { get; set; } = DateTime.Now;


        //for foreign keys
        public ICollection<CarRequest>? CarRequests { get; set; }
        public ICollection<Offer>? Offers { get; set; }
    }
}
