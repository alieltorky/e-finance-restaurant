using System.ComponentModel.DataAnnotations;

namespace Online_Restaurant.ViewModels
{
    // One row in the Employees table
    public class EmployeeListItemVM
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    // Used for both the "Add Employee" and "Edit Employee" forms
    public class EmployeeFormVM
    {
        // Only set when editing an existing employee
        public string? Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [Display(Name = "Full Name")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        [Required(ErrorMessage = "Please choose a role.")]
        public string Role { get; set; } = string.Empty;

        // Required when creating a new employee.
        // Leave blank when editing to keep the current password.
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}