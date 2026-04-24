using LoanApp.Data;
using LoanApp.Models;

namespace LoanApp.Services
{
    public interface ICurrentUserService
    {
        int? EmployeeId { get; }
        string? EmployeeName { get; }
        string Role { get; }
        bool IsAuthenticated { get; }
        bool IsAdmin { get; }

        void SetCurrentEmployee(Employee employee);
        void Clear();
        Task<Employee?> GetCurrentEmployeeAsync(AppDbContext context);
    }
}