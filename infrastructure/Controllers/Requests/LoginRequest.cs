using System.ComponentModel.DataAnnotations;

namespace infrastructure.Controllers.Requests
{
    public class LoginRequest
    {
        [Required] public string Login { get; set; }

        [Required] public string Password { get; set; }
    }
}