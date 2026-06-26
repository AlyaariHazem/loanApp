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

            Employee? currentEmployee = await _currentUser.GetCurrentEmployeeAsync(_context);
            if (currentEmployee == null)
            {
                _currentUser.Clear();
                return RedirectToAction("Login", "Home");
            }

            var balances = new List<BalanceViewModel>();

            if (_currentUser.IsAdmin)
            {
                // Efficiently fetch all balances for Admin view
                var balancesQuery = _context.Employees
                    .Select(e => new
                    {
                        e.Id,
                        e.Name,
                        TotalLentRaw = _context.Transactions.Where(t => t.LenderId == e.Id).Sum(t => (decimal?)t.Amount) ?? 0,
                        TotalBorrowedRaw = _context.Transactions.Where(t => t.BorrowerId == e.Id).Sum(t => (decimal?)t.Amount) ?? 0,
                        LastOpDate = _context.Transactions
                            .Where(t => t.LenderId == e.Id || t.BorrowerId == e.Id)
                            .Max(t => (DateTime?)t.CreatedAt)
                    });

                var rawData = await balancesQuery.ToListAsync();
                
                balances = rawData.Select(d => new BalanceViewModel
                {
                    EmployeeId = d.Id,
                    EmployeeName = d.Name,
                    TotalLent = d.TotalLentRaw > d.TotalBorrowedRaw ? d.TotalLentRaw - d.TotalBorrowedRaw : 0,
                    TotalBorrowed = d.TotalBorrowedRaw > d.TotalLentRaw ? d.TotalBorrowedRaw - d.TotalLentRaw : 0,
                    Balance = d.TotalLentRaw - d.TotalBorrowedRaw,
                    LastOperationDate = d.LastOpDate
                }).ToList();
            }
            else
            {
                // Simple fetch for individual user
                var totalLent = await _context.Transactions
                    .Where(t => t.LenderId == currentEmployee.Id)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0;

                var totalBorrowed = await _context.Transactions
                    .Where(t => t.BorrowerId == currentEmployee.Id)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0;

                var lastOperationDate = await _context.Transactions
                    .Where(t => t.LenderId == currentEmployee.Id || t.BorrowerId == currentEmployee.Id)
                    .MaxAsync(t => (DateTime?)t.CreatedAt);

                balances.Add(new BalanceViewModel
                {
                    EmployeeId = currentEmployee.Id,
                    EmployeeName = currentEmployee.Name,
                    TotalLent = totalLent > totalBorrowed ? totalLent - totalBorrowed : 0,
                    TotalBorrowed = totalBorrowed > totalLent ? totalBorrowed - totalLent : 0,
                    Balance = totalLent - totalBorrowed,
                    LastOperationDate = lastOperationDate
                });

                // Detailed balances per person for the current user
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

                var detailedList = detailsMap.Values.ToList();
                ApplyAutomaticSettlement(detailedList);

                var orderedDetails = detailedList
                    .OrderByDescending(detail => detail.LastOperationDate ?? DateTime.MinValue)
                    .ThenBy(detail => detail.PersonName)
                    .ToList();

                var detailsCount = orderedDetails.Count;
                var pagedDetailsItems = orderedDetails
                    .Skip((detailsPageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                ViewBag.BalanceDetails = new PaginatedList<BalancePartyDetailViewModel>(
                    pagedDetailsItems,
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

        private void ApplyAutomaticSettlement(System.Collections.Generic.List<BalancePartyDetailViewModel> details)
        {
            // 1. Direct Offset (same person)
            foreach (var d in details)
            {
                if (d.LentToPerson > 0 && d.BorrowedFromPerson > 0)
                {
                    decimal min = System.Math.Min(d.LentToPerson, d.BorrowedFromPerson);
                    d.LentToPerson -= min;
                    d.BorrowedFromPerson -= min;
                }
            }

            // 2. Indirect Offset (cross-person)
            // Separate into those who owe me (Positive net) and those I owe (Negative net)
            // We use 'BorrowedFromPerson' as my liability (Negative) and 'LentToPerson' as my asset (Positive)
            var liabilities = details.Where(d => d.BorrowedFromPerson > 0).OrderByDescending(d => d.BorrowedFromPerson).ToList();
            var assets = details.Where(d => d.LentToPerson > 0).OrderByDescending(d => d.LentToPerson).ToList();

            foreach (var l in liabilities)
            {
                foreach (var a in assets)
                {
                    if (l.BorrowedFromPerson == 0) break;
                    if (a.LentToPerson == 0) continue;

                    decimal settle = System.Math.Min(l.BorrowedFromPerson, a.LentToPerson);
                    l.BorrowedFromPerson -= settle;
                    a.LentToPerson -= settle;
                }
            }
        }
    }
}
