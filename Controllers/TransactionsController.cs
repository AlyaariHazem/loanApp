using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LoanApp.Data;
using LoanApp.Filters;
using LoanApp.Models;
using LoanApp.Services;
using System.Threading.Tasks;
using System.Linq;

namespace LoanApp.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public TransactionsController(AppDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        // GET: Transactions
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            if (!_currentUser.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }

            IQueryable<LoanTransaction> source = _context.Transactions
                .Include(t => t.Borrower)
                .Include(t => t.Lender)
                .OrderByDescending(t => t.CreatedAt);

            if (!_currentUser.IsAdmin && _currentUser.EmployeeId.HasValue)
            {
                var employeeId = _currentUser.EmployeeId.Value;
                source = source.Where(transaction => transaction.LenderId == employeeId || transaction.BorrowerId == employeeId);
            }

            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return View(new PaginatedList<LoanTransaction>(items, count, pageNumber, pageSize));
        }

        // GET: Transactions/Create
        [AdminOnly]
        public IActionResult Create()
        {
            ViewData["BorrowerId"] = new SelectList(_context.Employees.OrderBy(employee => employee.Name), "Id", "Name");
            ViewData["LenderId"] = new SelectList(_context.Employees.OrderBy(employee => employee.Name), "Id", "Name");
            return View();
        }

        // POST: Transactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Create(LoanTransaction transaction)
        {
            if (transaction.LenderId == transaction.BorrowerId)
            {
                ModelState.AddModelError("", "Lender and Borrower cannot be the same person.");
            }

            if (transaction.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Amount must be greater than 0");
            }

            if (ModelState.IsValid)
            {
                transaction.CreatedAt = System.DateTime.UtcNow;
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true });
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["BorrowerId"] = new SelectList(_context.Employees.OrderBy(employee => employee.Name), "Id", "Name", transaction.BorrowerId);
            ViewData["LenderId"] = new SelectList(_context.Employees.OrderBy(employee => employee.Name), "Id", "Name", transaction.LenderId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView(transaction);
            }
            return View(transaction);
        }
    }
}
