using System.ComponentModel.DataAnnotations;

namespace LoanApp.Models
{
    public class EmployeeSelfEditViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }

        public string? Department { get; set; }
        public string? Phone { get; set; }

        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Password confirmation does not match")]
        public string? ConfirmNewPassword { get; set; }
    }
}