using Microsoft.AspNetCore.Mvc;
using WzrastajAuth.Controllers.ResponsePresenters.Base;

namespace WzrastajAuth.Controllers.ResponsePresenters
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