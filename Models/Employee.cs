using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using LoanApp.Infrastructure;

namespace LoanApp.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee name is required")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Role")]
        public string Role { get; set; } = RoleNames.Employee;

        [ValidateNever]
        public string PasswordHash { get; set; } = string.Empty;

        [ValidateNever]
        public string PasswordSalt { get; set; } = string.Empty;

        [NotMapped]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }

        public string? EmployeeCode { get; set; }
        public string? Department { get; set; }
        public string? Phone { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
