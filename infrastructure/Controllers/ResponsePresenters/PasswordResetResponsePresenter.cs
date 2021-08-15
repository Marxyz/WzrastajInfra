using infrastructure.Controllers.ResponsePresenters.Base;
using Microsoft.AspNetCore.Mvc;

namespace infrastructure.Controllers.ResponsePresenters
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