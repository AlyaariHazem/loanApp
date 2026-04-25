using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoanApp.Data;
using LoanApp.Filters;
using LoanApp.Infrastructure;
using LoanApp.Models;
using LoanApp.Services;
using System.Threading.Tasks;

namespace LoanApp.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IPasswordService _passwordService;

        public EmployeesController(AppDbContext context, ICurrentUserService currentUser, IPasswordService passwordService)
        {
            _context = context;
            _currentUser = currentUser;
            _passwordService = passwordService;
        }

        // GET: Employees
        [AdminOnly]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 5)
        {
            var source = _context.Employees.OrderByDescending(e => e.CreatedAt);
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            
            return View(new PaginatedList<Employee>(items, count, pageNumber, pageSize));
        }

        // GET: Employees/Create
        [AdminOnly]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.Password) || employee.Password.Length < 6)
            {
                ModelState.AddModelError("Password", "Password must be at least 6 characters.");
            }

            if (ModelState.IsValid)
            {
                // Generate Employee Code automatically
                var count = await _context.Employees.CountAsync();
                employee.EmployeeCode = $"EMP-{(count + 1):D3}";
                employee.Role = string.IsNullOrWhiteSpace(employee.Role) ? RoleNames.Employee : employee.Role;
                var (hash, salt) = _passwordService.HashPassword(employee.Password!);
                employee.PasswordHash = hash;
                employee.PasswordSalt = salt;

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

        // GET: Employees/Edit/5
        [AdminOnly]
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(item => item.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            var model = new EmployeeEditViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Role = employee.Role,
                EmployeeCode = employee.EmployeeCode,
                Department = employee.Department,
                Phone = employee.Phone
            };

            return View(model);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Edit(int id, EmployeeEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(item => item.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            employee.Name = model.Name;
            employee.Role = string.IsNullOrWhiteSpace(model.Role) ? RoleNames.Employee : model.Role;
            employee.Department = model.Department;
            employee.Phone = model.Phone;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                var (hash, salt) = _passwordService.HashPassword(model.Password);
                employee.PasswordHash = hash;
                employee.PasswordSalt = salt;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Employees/MyProfile
        public async Task<IActionResult> MyProfile()
        {
            if (!_currentUser.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }

            var employee = await _currentUser.GetCurrentEmployeeAsync(_context);
            if (employee == null)
            {
                _currentUser.Clear();
                return RedirectToAction("Login", "Home");
            }

            return View(employee);
        }
    }
}
