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
        protected override void OnModelCreating(ModelBuilder builder)
        {
            //builder.Entity<Category>()
            //    .Property(c => c.CreatedOn)
            //    .HasDefaultValueSql("GETDATE()");

            builder.Entity<BookCategory>()
                .HasKey(e => new { e.BookId, e.CategoryId});
            base.OnModelCreating(builder);

            builder.HasSequence<int>("SerialNumber", schema: "shared")
                .StartsAt(1000001);
                //.IncrementsBy(1);
            builder.Entity<BookCopy>()
                .Property(b => b.SerialNumber)
                .HasDefaultValueSql("NEXT VALUE FOR shared.SerialNumber");
        }
    }
}
