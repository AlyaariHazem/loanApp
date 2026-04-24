using System.ComponentModel.DataAnnotations;

namespace LoanApp.Models
{
    public class EmployeeEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee name is required")]
        public string Name { get; set; } = string.Empty;

        public string Role { get; set; } = "Employee";

        public string? EmployeeCode { get; set; }
        public string? Department { get; set; }
        public string? Phone { get; set; }

        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }
    }
}