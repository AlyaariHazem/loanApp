namespace LoanApp.Models
{
    public class BalanceViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public decimal TotalLent { get; set; }
        public decimal TotalBorrowed { get; set; }
        public decimal Balance { get; set; }
    }
}
