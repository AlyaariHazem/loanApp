using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoanApp.Data;
using LoanApp.Models;
using LoanApp.Services;

namespace LoanApp.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPasswordService _passwordService;

    public HomeController(AppDbContext context, ICurrentUserService currentUser, IPasswordService passwordService)
    {
        _context = context;
        _currentUser = currentUser;
        _passwordService = passwordService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Login()
    {
        return View(await BuildLoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(EmployeeLoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildLoginViewModel(model));
        }

        var employee = await _context.Employees.FirstOrDefaultAsync(item => item.Id == model.SelectedEmployeeId);

        if (employee == null)
        {
            model.ErrorMessage = "Please select a valid employee.";
            return View(await BuildLoginViewModel(model));
        }

        var passwordValid = _passwordService.Verify(model.Password, employee.PasswordHash, employee.PasswordSalt);
        if (!passwordValid)
        {
            model.ErrorMessage = "Invalid password.";
            return View(await BuildLoginViewModel(model));
        }

        _currentUser.SetCurrentEmployee(employee);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    private async Task<EmployeeLoginViewModel> BuildLoginViewModel(EmployeeLoginViewModel? model = null)
    {
        var employees = await _context.Employees
            .OrderBy(employee => employee.Name)
            .ToListAsync();

        return new EmployeeLoginViewModel
        {
            Employees = employees,
            SelectedEmployeeId = model?.SelectedEmployeeId,
            Password = string.Empty,
            ErrorMessage = model?.ErrorMessage,
            CurrentEmployeeName = _currentUser.EmployeeName,
            CurrentEmployeeRole = _currentUser.IsAuthenticated ? _currentUser.Role : null
        };
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
