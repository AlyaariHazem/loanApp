using System;
using System.ComponentModel.DataAnnotations;

namespace LoanApp.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee name is required")]
        public string Name { get; set; }

        public string? EmployeeCode { get; set; }
        public string? Department { get; set; }
        public string? Phone { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
