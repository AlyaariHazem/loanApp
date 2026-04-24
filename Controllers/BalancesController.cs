using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoanApp.Data;
using LoanApp.Models;
using System.Threading.Tasks;
using System.Linq;

namespace LoanApp.Controllers
{
    public class BalancesController : Controller
    {
        private readonly AppDbContext _context;

        public BalancesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Balances
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees.ToListAsync();
            var balances = new System.Collections.Generic.List<BalanceViewModel>();

            foreach (var emp in employees)
            {
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
