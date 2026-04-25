namespace LoanApp.Models
{
    public class BalancePartyDetailViewModel
    {
        public string PersonName { get; set; } = string.Empty;
        public decimal LentToPerson { get; set; }
        public decimal BorrowedFromPerson { get; set; }
        public decimal NetBalance => LentToPerson - BorrowedFromPerson;
        public DateTime? LastOperationDate { get; set; }
    }
}