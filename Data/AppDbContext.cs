using Microsoft.EntityFrameworkCore;
using LoanApp.Models;

namespace LoanApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<LoanTransaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Prevent cascade delete issues when an employee is deleted
            modelBuilder.Entity<LoanTransaction>()
                .HasOne(t => t.Lender)
                .WithMany()
                .HasForeignKey(t => t.LenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoanTransaction>()
                .HasOne(t => t.Borrower)
                .WithMany()
                .HasForeignKey(t => t.BorrowerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
