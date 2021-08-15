using Microsoft.AspNetCore.Mvc;
using WzrastajAuth.Auth;
using WzrastajAuth.Controllers.ResponsePresenters.Base;

namespace WzrastajAuth.Controllers.ResponsePresenters
{
    public class ExchangeRefreshTokenResponsePresenter : PresentableResponse
    {
        private readonly AccessToken AccessToken;
        private readonly string RefreshToken;


        public ExchangeRefreshTokenResponsePresenter(AccessToken accessToken, string refreshToken, bool success) : base()
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