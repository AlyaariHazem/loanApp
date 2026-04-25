using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanApp.Models
{
    public class LoanTransaction
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Lender")]
        public int LenderId { get; set; }
        
        [ForeignKey("LenderId")]
        public Employee? Lender { get; set; }

        [Required]
        [Display(Name = "Borrower")]
        public int BorrowerId { get; set; }
        
        [ForeignKey("BorrowerId")]
        public Employee? Borrower { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
