using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoanApp.Data;
using LoanApp.Models;
using System.Threading.Tasks;

namespace LoanApp.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly AppDbContext _context;

        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Employees
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            var source = _context.Employees.OrderByDescending(e => e.CreatedAt);
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            
            return View(new PaginatedList<Employee>(items, count, pageNumber, pageSize));
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                // Generate Employee Code automatically
                var count = await _context.Employees.CountAsync();
                employee.EmployeeCode = $"EMP-{(count + 1):D3}";

                employee.CreatedAt = System.DateTime.UtcNow;
                _context.Add(employee);
                await _context.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true });
                }
                return RedirectToAction(nameof(Index));
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView(employee);
            }
            return View(employee);
        }
    }
}
