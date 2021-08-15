using System.ComponentModel.DataAnnotations;

namespace WzrastajAuth.Controllers.Requests
{
    public class ForgetPasswordRequest
    {
        [Required] [EmailAddress] public string Email { get; set; }
    }
}