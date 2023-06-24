using HajurKoCarRental.Context;
using HajurKoCarRental.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HajurKoCarRental.Utils
{
    public static class Utils
    {
        public static User user;
        public static string userId;
        public static IList<string> role;

        //for storing inactive users
        public static List<User> inactiveUsers;

        //for storing all users
        public static Dictionary<User, String> allUsers;

        public static Dictionary<Car, float?> ratings;

        public static Dictionary<Car, int> frequency;

        public static Dictionary<Car, bool?> availability;

        public static ApplicationDbContext context;

        // Creating a single method to create the database context with the connection string and options builder.
        public static ApplicationDbContext GetContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql("Server=dpg-cgpdbf0rddl9mmutjd30-a.oregon-postgres.render.com;Port=5432;Database=hajurkocarrental;User Id=hajurkocarrental_user;Password=7F5kIiWj4EMZQzQEYIAmyTJYhSPM3RZq");
            return new ApplicationDbContext(optionsBuilder.Options);
        }

        public static DateTimeOffset convertDate(string date)
        {
            // Format the datetime with timezone offset
            DateTime dateTime = DateTime.ParseExact(date, "M/d/yyyy h:mm:ss tt zzz", CultureInfo.InvariantCulture);

            // Convert to Nepal time zone
            TimeZoneInfo nepalTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Nepal Standard Time");
            DateTime nepalTime = TimeZoneInfo.ConvertTime(dateTime, nepalTimeZone);

            // Format as string with timezone offset
            string formattedDateTime = nepalTime.ToString("yyyy-MM-dd HH:mm:ss zzz");
            DateTimeOffset finalDateTimeOffset = DateTimeOffset.ParseExact(formattedDateTime, "yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture);
            return finalDateTimeOffset;
        }

        //getting the total times the car has been rented
        public static Dictionary<Car, int> GetRentFrequency()
        {
            var cars = context.Cars.ToList();
            var dictionary = new Dictionary<Car, int>();
            foreach (var car in cars)
            {
                var frequency = context.CarRequests.Count(c => c.carId == car.Id && c.returnedDate != null);
                dictionary.Add((Car)car, frequency);
            }
            return dictionary;
        }

        //getting the average rating of each car
        public static Dictionary<Car, float?> GetAverageRating()
        {
            var cars = context.Cars.ToList();
            var dictionary = new Dictionary<Car, float?>();
            foreach (var car in cars)
            {
                var rating = context.CarRequests.Where(c => c.carId == car.Id && c.returnedDate != null).Average(c => c.rating);
                dictionary.Add(car, rating);
            }
            return dictionary;
        }

        //checking the availability of car 
        public static bool IsCarAvailable(Guid carId)
        {
            var today = DateTimeOffset.Now;
            var overlappingRequests = context.CarRequests.Where(cr => cr.carId == carId
                && cr.requestedDate <= today
                && cr.requestedDate.AddDays(cr.duration) >= today
                && cr.status == Status.APPROVED).ToList();

            return !overlappingRequests.Any();
        }
    }
}