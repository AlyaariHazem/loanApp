using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoanApp.Data;
using LoanApp.Models;
using LoanApp.Services;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace LoanApp.Controllers
{
    public class BalancesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private const int PageSize = 5;

        public BalancesController(AppDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        // GET: Balances
        public async Task<IActionResult> Index(int pageNumber = 1, int detailsPageNumber = 1)
        {
            if (!_currentUser.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }

            Employee? currentEmployee = null;
            var employees = new System.Collections.Generic.List<Employee?>();
            if (_currentUser.IsAdmin)
            {
                employees.AddRange(await _context.Employees.OrderBy(employee => employee.Name).ToListAsync());
            }
            else
            {
                currentEmployee = await _currentUser.GetCurrentEmployeeAsync(_context);
                employees.Add(currentEmployee);
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

                var lastOperationDate = await _context.Transactions
                    .Where(t => t.LenderId == emp.Id || t.BorrowerId == emp.Id)
                    .MaxAsync(t => (DateTime?)t.CreatedAt);

                balances.Add(new BalanceViewModel
                {
                    EmployeeId = emp.Id,
                    EmployeeName = emp.Name,
                    TotalLent = totalLent,
                    TotalBorrowed = totalBorrowed,
                    Balance = totalLent - totalBorrowed,
                    LastOperationDate = lastOperationDate
                });
            }

            if (!_currentUser.IsAdmin && currentEmployee != null)
            {
                var lent = await _context.Transactions
                    .Where(transaction => transaction.LenderId == currentEmployee.Id)
                    .Include(transaction => transaction.Borrower)
                    .GroupBy(transaction => transaction.Borrower != null ? transaction.Borrower.Name : "-")
                    .Select(group => new
                    {
                        PersonName = group.Key,
                        Amount = group.Sum(transaction => transaction.Amount)
                    })
                    .ToListAsync();

                var borrowed = await _context.Transactions
                    .Where(transaction => transaction.BorrowerId == currentEmployee.Id)
                    .Include(transaction => transaction.Lender)
                    .GroupBy(transaction => transaction.Lender != null ? transaction.Lender.Name : "-")
                    .Select(group => new
                    {
                        PersonName = group.Key,
                        Amount = group.Sum(transaction => transaction.Amount)
                    })
                    .ToListAsync();

                var detailsMap = new Dictionary<string, BalancePartyDetailViewModel>();

                foreach (var item in lent)
                {
                    if (!detailsMap.TryGetValue(item.PersonName, out var detail))
                    {
                        detail = new BalancePartyDetailViewModel { PersonName = item.PersonName };
                        detailsMap[item.PersonName] = detail;
                    }

                    detail.LentToPerson = item.Amount;
                }

                foreach (var item in borrowed)
                {
                    if (!detailsMap.TryGetValue(item.PersonName, out var detail))
                    {
                        detail = new BalancePartyDetailViewModel { PersonName = item.PersonName };
                        detailsMap[item.PersonName] = detail;
                    }

                    detail.BorrowedFromPerson = item.Amount;
                }

                var detailDates = await _context.Transactions
                    .Where(transaction => transaction.LenderId == currentEmployee.Id || transaction.BorrowerId == currentEmployee.Id)
                    .GroupBy(transaction => transaction.LenderId == currentEmployee.Id
                        ? transaction.Borrower != null ? transaction.Borrower.Name : "-"
                        : transaction.Lender != null ? transaction.Lender.Name : "-")
                    .Select(group => new
                    {
                        PersonName = group.Key,
                        LastOperationDate = group.Max(transaction => transaction.CreatedAt)
                    })
                    .ToListAsync();

                foreach (var item in detailDates)
                {
                    if (detailsMap.TryGetValue(item.PersonName, out var detail))
                    {
                        detail.LastOperationDate = item.LastOperationDate;
                    }
                }

                var orderedDetails = detailsMap.Values
                    .OrderByDescending(detail => detail.LastOperationDate ?? DateTime.MinValue)
                    .ThenBy(detail => detail.PersonName)
                    .ToList();

                var detailsCount = orderedDetails.Count;
                var pagedDetails = orderedDetails
                    .Skip((detailsPageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                ViewBag.BalanceDetails = new PaginatedList<BalancePartyDetailViewModel>(
                    pagedDetails,
                    detailsCount,
                    detailsPageNumber,
                    PageSize);
            }

            var orderedBalances = balances
                .OrderByDescending(balance => balance.LastOperationDate ?? DateTime.MinValue)
                .ThenByDescending(balance => System.Math.Abs(balance.Balance))
                .ThenBy(balance => balance.EmployeeName)
                .ToList();

            var balancesCount = orderedBalances.Count;
            var pagedBalances = orderedBalances
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return View(new PaginatedList<BalanceViewModel>(pagedBalances, balancesCount, pageNumber, PageSize));
        }
    }
}
