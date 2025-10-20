using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartLibrary.Web.Core.Models;

namespace SmartLibrary.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<Category> Categories { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            //builder.Entity<Category>()
            //    .Property(c => c.CreatedOn)
            //    .HasDefaultValueSql("GETDATE()");
            base.OnModelCreating(builder);
        }
    }
}
