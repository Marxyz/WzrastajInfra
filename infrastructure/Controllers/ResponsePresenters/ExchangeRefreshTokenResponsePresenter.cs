using infrastructure.Auth;
using infrastructure.Controllers.ResponsePresenters.Base;
using Microsoft.AspNetCore.Mvc;

namespace infrastructure.Controllers.ResponsePresenters
{
    public class ExchangeRefreshTokenResponsePresenter : PresentableResponse
    {
        private readonly AccessToken AccessToken;
        private readonly string RefreshToken;


        public ExchangeRefreshTokenResponsePresenter(AccessToken accessToken, string refreshToken, bool success)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            Success = success;
        }


        public override IActionResult ToPresentable()
        {
            return Success ? SuccesContentResult() : FailureResult();
        }

        private IActionResult SuccesContentResult()
        {
            return new JsonResult(new
            {
                AccessToken,
                RefreshToken
            });
        }
    }
}