using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using JohnsAuthority.Models;

namespace JohnsAuthority.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<LocationAmenity>()
             .HasKey(t => new { t.LocationId, t.AmenityId });

            builder.Entity<LocationAmenity>()
             .HasOne(pt => pt.Location)
             .WithMany(p => p.LocationAmenities)
             .HasForeignKey(pt => pt.LocationId);

            builder.Entity<LocationAmenity>()
             .HasOne(pt => pt.Amenity)
             .WithMany(t => t.LocationAmenities)
             .HasForeignKey(pt => pt.AmenityId);
        }

        public DbSet<Location> Location { get; set; }

        public DbSet<Image> Image { get; set; }

        public DbSet<Review> Review { get; set; }

        public DbSet<Amenity> Amenity { get; set; }

        public DbSet<ApplicationUser> ApplicationUser { get; set; }

        public DbSet<Report> Report { get; set; }

        public DbSet<ContactMessage> ContactMessage { get; set; }
    }
}
