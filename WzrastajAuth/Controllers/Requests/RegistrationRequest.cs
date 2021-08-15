using System.ComponentModel.DataAnnotations;

namespace WzrastajAuth.Controllers.Requests
{
    public class RegistrationRequest
    {
        [Compare(nameof(Password))] public string ConfirmPassword { get; set; }

        [Required] [MinLength(8)] public string Password { get; set; }
        [Required] [StringLength(128)] public string Login { get; set; }
        [Required] [EmailAddress] public string Email { get; set; }
    }
}