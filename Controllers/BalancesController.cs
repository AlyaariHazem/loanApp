using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoanApp.Data;
using LoanApp.Models;
using LoanApp.Services;
using System.Threading.Tasks;
using System.Linq;

namespace LoanApp.Controllers
{
    public class BalancesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public BalancesController(AppDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        // GET: Balances
        public async Task<IActionResult> Index()
        {
            if (!_currentUser.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }

            var employees = new System.Collections.Generic.List<Employee?>();
            if (_currentUser.IsAdmin)
            {
                employees.AddRange(await _context.Employees.OrderBy(employee => employee.Name).ToListAsync());
            }
            else
            {
                employees.Add(await _currentUser.GetCurrentEmployeeAsync(_context));
            }

            var balances = new System.Collections.Generic.List<BalanceViewModel>();

            foreach (var emp in employees)
            {
                if (emp == null)
                {
                    _currentUser.Clear();
                    return RedirectToAction("Login", "Home");
                }

                var totalLent = await _context.Transactions
                    .Where(t => t.LenderId == emp.Id)
                    .SumAsync(t => t.Amount);

                var totalBorrowed = await _context.Transactions
                    .Where(t => t.BorrowerId == emp.Id)
                    .SumAsync(t => t.Amount);

                balances.Add(new BalanceViewModel
                {
                    EmployeeId = emp.Id,
                    EmployeeName = emp.Name,
                    TotalLent = totalLent,
                    TotalBorrowed = totalBorrowed,
                    Balance = totalLent - totalBorrowed
                });
            }

            return View(balances);
        }
    }
}
