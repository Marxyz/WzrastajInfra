using Microsoft.AspNetCore.Mvc;
using WzrastajAuth.Controllers.ResponsePresenters.Base;

namespace WzrastajAuth.Controllers.ResponsePresenters
{
    public class PasswordResetResponsePresenter : PresentableResponse
    {
        private readonly bool Success;

        public PasswordResetResponsePresenter(bool success)
        {
            Success = success;
        }

        public override IActionResult ToPresentable()
        {
            return Success ? new OkResult() : FailureResult();
        }
    }
}