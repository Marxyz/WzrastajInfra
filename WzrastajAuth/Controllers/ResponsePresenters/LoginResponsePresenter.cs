using Microsoft.AspNetCore.Mvc;
using WzrastajAuth.Auth;
using WzrastajAuth.Controllers.ResponsePresenters.Base;

namespace WzrastajAuth.Controllers.ResponsePresenters
{
    public class LoginResponsePresenter : PresentableResponse
    {
        private readonly AccessToken GenerateEncodedToken;
        private readonly string RefreshToken;

        public LoginResponsePresenter(AccessToken generateEncodedToken, string refreshToken, bool success)
        {
            GenerateEncodedToken = generateEncodedToken;
            RefreshToken = refreshToken;
            Success = success;
        }

        public override IActionResult ToPresentable()
        {
            return Success ? SuccessResult() : FailureResult();
        }

        private IActionResult SuccessResult()
        {
            return new JsonResult(new
            {
                AccessToken = GenerateEncodedToken,
                RefreshToken
            });
        }
    }
}