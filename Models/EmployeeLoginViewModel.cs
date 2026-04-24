using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LoanApp.Models
{
    public class EmployeeLoginViewModel
    {
        public List<Employee> Employees { get; set; } = new List<Employee>();
        public int? SelectedEmployeeId { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
        public string? CurrentEmployeeName { get; set; }
        public string? CurrentEmployeeRole { get; set; }
    }
}