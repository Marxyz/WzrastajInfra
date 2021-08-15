using System;
using System.Threading.Tasks;
using infrastructure.Controllers.Requests;
using infrastructure.Models;
using infrastructure.Services.ActionHandlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace infrastructure.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthActionHandler AuthActionHandler;
        private readonly AuthConfig Options;

        public AuthController(IOptions<AuthConfig> options,
            IAuthActionHandler authActionHandler)
        {
            Options = options.Value;
            AuthActionHandler = authActionHandler;
        }


        [HttpPost("/login")]
        [ValidateModel]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var res = await AuthActionHandler.AuthenticateUserAsync(new LoginProcessingModel
            {
                Login = request.Login,
                Password = request.Password,
                RemoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString()
            });

            return res.ToPresentable();
        }

        [HttpPost("/refreshtoken")]
        [ValidateModel]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var res = await AuthActionHandler.ProcessRefreshTokenRequestAsync(
                new RefreshTokenProcessingModel(
                    request.AccessToken,
                    request.RefreshToken,
                    Options.SecretKey,
                    Request.HttpContext.Connection.RemoteIpAddress.ToString()
                )
            );

            return res.ToPresentable();
        }

        [AllowAnonymous]
        [HttpGet("/verify")]
        [ValidateModel]
        public async Task<IActionResult> ConfirmPassword(string userId, string token)
        {
            var request = new MailConfirmationProcessingModel { Token = token, UserId = Guid.Parse(userId) };
            var res = await AuthActionHandler.ProcessMailConfirmationRequest(request);

            return res.ToPresentable();
        }


        [HttpPost("/forget")]
        [ValidateModel]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
        {
            await AuthActionHandler.ProcessForgetPasswordRequestAsync(request.Email);
            return NoContent();
        }


        [HttpPost("/reset")]
        [ValidateModel]
        public async Task<IActionResult> ForgetPassword([FromBody] ResetPasswordRequest request)
        {
            var res = await AuthActionHandler.ConfirmForgetPasswordRequestAsync(request.Email, request.Token,
                request.Password);
            return res.ToPresentable();
        }
    }
}