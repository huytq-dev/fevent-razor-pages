using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Infrastructure;
using Domain;
using BCrypt.Net;

namespace UI.Pages;

public class SignUpModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public SignUpModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    [Required(ErrorMessage = "Full Name is required.")]
    public string FullName { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Student ID is required.")]
    [RegularExpression(@"^[A-Z]{2}\d{6}$", ErrorMessage = "Student ID must be in format SE123456.")]
    public string StudentId { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Major is required.")]
    public string Major { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "FPT Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid Email Address.")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@fpt\.edu\.vn$", ErrorMessage = "Must use FPT email (@fpt.edu.vn).")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$", 
        ErrorMessage = "Password must have uppercase, lowercase, number and special character.")]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the Terms of Service and Privacy Policy.")]
    public bool AgreeTerms { get; set; }

    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }


        // Check if email already exists
        var existingUser = _db.Users.FirstOrDefault(u => u.Email == Email);
        if (existingUser != null)
        {
            ModelState.AddModelError(string.Empty, "Email already exists.");
            return Page();
        }

        // Check if student ID already exists
        var existingStudent = _db.Users.FirstOrDefault(u => u.StudentId == StudentId);
        if (existingStudent != null)
        {
            ModelState.AddModelError(string.Empty, "Student ID already exists.");
            return Page();
        }

        // Hash password
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);

        var user = new User
        {
            FullName = FullName,
            Username = Email,
            Email = Email,
            PasswordHash = hashedPassword,
            Major = Major,
            StudentId = StudentId,
            IsVerified = true
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return RedirectToPage("/Login");
    }
}
