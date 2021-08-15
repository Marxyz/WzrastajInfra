using infrastructure.Controllers.ResponsePresenters.Base;
using Microsoft.AspNetCore.Mvc;

namespace infrastructure.Controllers.ResponsePresenters
{
    public class MailConfirmationResponsePresenter : PresentableResponse
    {
        public MailConfirmationResponsePresenter(bool success)
        {
            Success = success;
        }

        public override IActionResult ToPresentable()
        {
            return Success ? new OkResult() : FailureResult();
        }
    }
}