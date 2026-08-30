using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Online_Restaurant.Models;
using Online_Restaurant.ViewModels;

// Lets the Admin add/edit/remove Chef, Delivery and Admin accounts from the
// dashboard instead of having them hardcoded (seeded) in Program.cs.
[Authorize(Roles = "Admin")]
public class EmployeesController : Controller
{
    // The only roles this screen is allowed to manage.
    // ("Customer" is intentionally excluded - that role is self-service via Register.)
    private static readonly string[] EmployeeRoles = { "Admin", "Chef", "Delivery" };

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public EmployeesController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: Employees  (list + optional "editId" opens the edit form, same pattern as Suppliers)
    [HttpGet]
    public async Task<IActionResult> Index(string? editId)
    {
        var employees = await GetAllEmployeesAsync();

        EmployeeFormVM? employeeToEdit = null;

        if (!string.IsNullOrEmpty(editId))
        {
            var user = await _userManager.FindByIdAsync(editId);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var currentRole = roles.FirstOrDefault(r => EmployeeRoles.Contains(r)) ?? EmployeeRoles[0];

                employeeToEdit = new EmployeeFormVM
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    Role = currentRole
                };
            }
        }

        ViewBag.EmployeeToEdit = employeeToEdit;
        ViewBag.Roles = EmployeeRoles;
        ViewBag.CurrentUserId = _userManager.GetUserId(User);

        return View(employees);
    }

    // POST: Employees/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeFormVM model)
    {
        // Id isn't posted from the "Add" form, so drop it from validation
        ModelState.Remove(nameof(EmployeeFormVM.Id));

        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "Password is required for a new employee.");
        }

        if (!EmployeeRoles.Contains(model.Role))
        {
            ModelState.AddModelError(nameof(model.Role), "Please choose a valid role.");
        }

        if (!ModelState.IsValid)
        {
            return await RenderIndexWithErrors(formModel: model);
        }

        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            Address = model.Address,
            EmailConfirmed = true,
            LockoutEnabled = true,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, model.Password!);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return await RenderIndexWithErrors(formModel: model);
        }

        if (!await _roleManager.RoleExistsAsync(model.Role))
        {
            await _roleManager.CreateAsync(new IdentityRole(model.Role));
        }

        await _userManager.AddToRoleAsync(user, model.Role);

        TempData["EmployeeMessage"] = $"{model.UserName} was added as {model.Role}.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Employees/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EmployeeFormVM model)
    {
        if (string.IsNullOrEmpty(model.Id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null)
        {
            return NotFound();
        }

        // Password is optional on edit, so it shouldn't block validation
        ModelState.Remove(nameof(EmployeeFormVM.Password));

        if (!EmployeeRoles.Contains(model.Role))
        {
            ModelState.AddModelError(nameof(model.Role), "Please choose a valid role.");
        }

        if (!ModelState.IsValid)
        {
            return await RenderIndexWithErrors(formModel: model);
        }

        user.UserName = model.UserName;
        user.NormalizedUserName = _userManager.NormalizeName(model.UserName);
        user.Email = model.Email;
        user.NormalizedEmail = _userManager.NormalizeEmail(model.Email);
        user.PhoneNumber = model.PhoneNumber;
        user.Address = model.Address;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return await RenderIndexWithErrors(formModel: model);
        }

        // Only reset the password if the admin actually typed a new one
        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, resetToken, model.Password);

            if (!passwordResult.Succeeded)
            {
                foreach (var error in passwordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return await RenderIndexWithErrors(formModel: model);
            }
        }

        // Swap roles only if the selected role actually changed
        var currentEmployeeRoles = (await _userManager.GetRolesAsync(user))
            .Where(r => EmployeeRoles.Contains(r))
            .ToList();

        if (!currentEmployeeRoles.Contains(model.Role))
        {
            if (currentEmployeeRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentEmployeeRoles);
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                await _roleManager.CreateAsync(new IdentityRole(model.Role));
            }

            await _userManager.AddToRoleAsync(user, model.Role);
        }

        TempData["EmployeeMessage"] = $"{model.UserName} was updated.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Employees/ToggleStatus  (deactivate = blocks login without deleting the account)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user != null)
        {
            user.IsActive = !user.IsActive;
            user.LockoutEnabled = true;
            user.LockoutEnd = user.IsActive ? null : DateTimeOffset.MaxValue;

            await _userManager.UpdateAsync(user);
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Employees/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (id == currentUserId)
        {
            TempData["EmployeeError"] = "You cannot delete your own account while logged in.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
        }

        return RedirectToAction(nameof(Index));
    }

    // ---------- helpers ----------

    private async Task<List<EmployeeListItemVM>> GetAllEmployeesAsync()
    {
        var employees = new List<EmployeeListItemVM>();

        foreach (var role in EmployeeRoles)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role);

            foreach (var user in usersInRole)
            {
                employees.Add(new EmployeeListItemVM
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    Role = role,
                    IsActive = user.IsActive
                });
            }
        }

        return employees
            .OrderBy(e => e.Role)
            .ThenBy(e => e.UserName)
            .ToList();
    }

    // Re-renders the Index view (list + form) after a failed Create/Edit so
    // validation errors and the entered data are shown back to the admin.
    private async Task<IActionResult> RenderIndexWithErrors(EmployeeFormVM formModel)
    {
        var employees = await GetAllEmployeesAsync();

        ViewBag.EmployeeToEdit = formModel;
        ViewBag.Roles = EmployeeRoles;
        ViewBag.CurrentUserId = _userManager.GetUserId(User);

        return View("Index", employees);
    }
}