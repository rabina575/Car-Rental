using HajurKoCarRental.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HajurKoCarRental.Context
{
    public class ApplicationDbContext : IdentityDbContext
    {
        //Defining Constructor
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        //Defining Database
        public DbSet<Car> Cars { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<CarRequest> CarRequests { get; set; }
        public DbSet<RepairBill> RepairBill { get; set; }
        public DbSet<Damage> Damage { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Ignore<IdentityUserLogin<string>>();

            //ignoring the files for not saving in the database
            modelBuilder.Entity<Car>()
            .Ignore(c => c.ImageFile);

            modelBuilder.Entity<User>()
           .Ignore(c => c.DocumentFile);
            
            //defining foreign key relations

            modelBuilder.Entity<CarRequest>()
            .HasOne(e => e.ApprovalUser)
            .WithMany(e => e.CarRequestsApproval)
            .HasForeignKey(e => e.approvedBy)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CarRequest>()
                .HasOne(e => e.CustomerUser)
                .WithMany(e => e.CarRequestsCustomer)
                .HasForeignKey(e => e.customerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                    .HasMany(e => e.DamageRequest)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.approvedBy)
            .HasPrincipalKey(e => e.Id);

            modelBuilder.Entity<Damage>()
        .HasOne(e => e.RepairBill)
            .WithOne(e => e.Damage)
        .HasForeignKey<RepairBill>(e => e.damageId)
        .IsRequired();

            modelBuilder.Entity<User>()
                      .HasMany(e => e.Notifications)
                      .WithOne(e => e.User)
                      .HasForeignKey(e => e.RecipientId)
                      .HasPrincipalKey(e => e.Id);

            modelBuilder.Entity<Car>()
            .HasMany(e => e.CarRequests)
            .WithOne(e => e.Car)
            .HasForeignKey(e => e.carId)
            .HasPrincipalKey(e => e.Id);

            modelBuilder.Entity<Car>()
               .HasMany(e => e.Offers)
               .WithOne(e => e.Car)
               .HasForeignKey(e => e.CarId)
               .HasPrincipalKey(e => e.Id);

            modelBuilder.Entity<CarRequest>()
            .HasMany(e => e.Damage)
            .WithOne(e => e.CarRequest)
            .HasForeignKey(e => e.requestId)
            .HasPrincipalKey(e => e.Id);

            modelBuilder.Entity<CarRequest>()
          .HasMany(e => e.Notifications)
          .WithOne(e => e.CarRequest)
          .HasForeignKey(e => e.RequestId)
          .HasPrincipalKey(e => e.Id);

            modelBuilder.Entity<Offer>()
          .HasMany(e => e.Notifications)
          .WithOne(e => e.Offer)
          .HasForeignKey(e => e.OfferId)
          .HasPrincipalKey(e => e.Id);
        }
    }
}