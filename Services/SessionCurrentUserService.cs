using LoanApp.Data;
using LoanApp.Infrastructure;
using LoanApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LoanApp.Services
{
    public class SessionCurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionCurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession? Session => _httpContextAccessor.HttpContext?.Session;

        public int? EmployeeId => Session?.GetInt32(SessionKeys.EmployeeId);

        public string? EmployeeName => Session?.GetString(SessionKeys.EmployeeName);

        public string Role => Session?.GetString(SessionKeys.EmployeeRole) ?? RoleNames.Employee;

        public bool IsAuthenticated => EmployeeId.HasValue;

        public bool IsAdmin => string.Equals(Role, RoleNames.Admin, StringComparison.OrdinalIgnoreCase);

        public void SetCurrentEmployee(Employee employee)
        {
            if (Session == null)
            {
                return;
            }

            Session.SetInt32(SessionKeys.EmployeeId, employee.Id);
            Session.SetString(SessionKeys.EmployeeName, employee.Name);
            Session.SetString(SessionKeys.EmployeeRole, string.IsNullOrWhiteSpace(employee.Role) ? RoleNames.Employee : employee.Role);
        }

        public void Clear()
        {
            Session?.Clear();
        }

        public async Task<Employee?> GetCurrentEmployeeAsync(AppDbContext context)
        {
            if (!EmployeeId.HasValue)
            {
                return null;
            }

            return await context.Employees.FirstOrDefaultAsync(employee => employee.Id == EmployeeId.Value);
        }
    }
}