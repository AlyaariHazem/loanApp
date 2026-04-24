using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LoanApp.Data;
using LoanApp.Models;
using System.Threading.Tasks;
using System.Linq;

namespace LoanApp.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly AppDbContext _context;

        public TransactionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Transactions
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            var source = _context.Transactions
                .Include(t => t.Borrower)
                .Include(t => t.Lender)
                .OrderByDescending(t => t.CreatedAt);

            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return View(new PaginatedList<LoanTransaction>(items, count, pageNumber, pageSize));
        }

        // GET: Transactions/Create
        public IActionResult Create()
        {
            ViewData["BorrowerId"] = new SelectList(_context.Employees, "Id", "Name");
            ViewData["LenderId"] = new SelectList(_context.Employees, "Id", "Name");
            return View();
        }

        // POST: Transactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
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

            ViewData["BorrowerId"] = new SelectList(_context.Employees, "Id", "Name", transaction.BorrowerId);
            ViewData["LenderId"] = new SelectList(_context.Employees, "Id", "Name", transaction.LenderId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView(transaction);
            }
            return View(transaction);
        }
    }
}
