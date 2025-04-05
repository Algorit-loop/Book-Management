using Microsoft.EntityFrameworkCore;

namespace RazorInMemoryDemo.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed initial books
            modelBuilder.Entity<Book>().HasData(
                new Book { Id = 1, Title = "The Great Gatsby", Author = "F. Scott Fitzgerald" },
                new Book { Id = 2, Title = "To Kill a Mockingbird", Author = "Harper Lee" },
                new Book { Id = 3, Title = "1984", Author = "George Orwell" }
            );

            // Seed admin user 
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Password = "admin", Role = "Admin", IsActive = true },
                new User { Id = 2, Username = "user", Password = "123", Role = "User", IsActive = true }
            );
        }
    }
} 