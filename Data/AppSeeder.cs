using LoanApp.Infrastructure;
using LoanApp.Models;
using LoanApp.Services;
using Microsoft.EntityFrameworkCore;

namespace LoanApp.Data
{
    public static class AppSeeder
    {
        private const string SeedAdminCode = "ADMIN-SEED";
        private const string SeedAdminPassword = "Hazemm";

        public static async Task SeedAdminAsync(AppDbContext context, IPasswordService passwordService)
        {
            await context.Database.MigrateAsync();

            var admin = await context.Employees.FirstOrDefaultAsync(employee => employee.EmployeeCode == SeedAdminCode);
            var (hash, salt) = passwordService.HashPassword(SeedAdminPassword);

            if (admin == null)
            {
                context.Employees.Add(new Employee
                {
                    Name = "System Admin",
                    EmployeeCode = SeedAdminCode,
                    Department = "Administration",
                    Phone = "776137120",
                    Role = RoleNames.Admin,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                admin.Role = RoleNames.Admin;
                admin.PasswordHash = hash;
                admin.PasswordSalt = salt;
            }

            await context.SaveChangesAsync();
        }
    }
}