using System.ComponentModel.DataAnnotations;

namespace infrastructure.Controllers.Requests
{
    public class ForgetPasswordRequest
    {
        [Required] [EmailAddress] public string Email { get; set; }
    }
}