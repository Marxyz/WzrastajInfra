using System.ComponentModel.DataAnnotations;

namespace WzrastajAuth.Controllers.Requests
{
    public class LoginRequest
    {
        [Required] public string Login { get; set; }

        [Required] public string Password { get; set; }

    }
}