using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartLibrary.Web.Core.Models;

namespace SmartLibrary.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Author> Authors { get; set; }
        public virtual DbSet<Book> Books { get; set; }
        public virtual DbSet<BookCategory> BookCategories { get; set; }
        public virtual DbSet<BookCopy> BookCopies { get; set; }
        public virtual DbSet<Subscriber> Subscribers { get; set; }
        public virtual DbSet<Governorate> Governorates { get; set; }
        public virtual DbSet<Area> Areas { get; set; }
        public virtual DbSet<Subscription> Subscriptions { get; set; }
        public virtual DbSet<Rental> Rentals { get; set; }
        public virtual DbSet<RentalCopy> RentalCopies { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //builder.Entity<Category>()
            //    .Property(c => c.CreatedOn)
            //    .HasDefaultValueSql("GETDATE()");

            builder.Entity<BookCategory>().HasKey(e => new { e.BookId, e.CategoryId});
            builder.Entity<RentalCopy>().HasKey(e => new { e.RentalId, e.BookCopyId});
            builder.Entity<Rental>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<RentalCopy>().HasQueryFilter(e => !e.Rental!.IsDeleted);

            builder.HasSequence<int>("SerialNumber", schema: "shared")
                .StartsAt(1000001);
                //.IncrementsBy(1);
            builder.Entity<BookCopy>()
                .Property(b => b.SerialNumber)
                .HasDefaultValueSql("NEXT VALUE FOR shared.SerialNumber");

            var CascadeFKs = builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in CascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict;

            base.OnModelCreating(builder);
        }
    }
}
