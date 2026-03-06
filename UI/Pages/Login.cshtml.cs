using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Email { get; set; }
        [BindProperty]
        public string Password { get; set; }
        [BindProperty]
        public bool RememberMe { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            // Any logic for GET
        }

        public IActionResult OnPost()
        {
            // Dummy login logic
            if (Email == "student@fpt.edu.vn" && Password == "password")
            {
                // Redirect to homepage or dashboard
                return RedirectToPage("Index");
            }
            ErrorMessage = "Invalid email or password.";
            return Page();
        }
    }
}
