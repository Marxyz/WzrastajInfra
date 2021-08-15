using System.Collections.Generic;
using System.Linq;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace infrastructure.Controllers.ResponsePresenters.Base
{
    public abstract class PresentableResponse
    {
        protected IEnumerable<Error> Errors;
        protected bool Success;

        protected PresentableResponse(IEnumerable<Error> errors)
        {
            Errors = errors;
            Success = false;
        }


        protected PresentableResponse()
        {
            Errors = Enumerable.Empty<Error>();
            Success = true;
        }

        public abstract IActionResult ToPresentable();

        protected IActionResult FailureResult()
        {
            if (Errors.Any())
            {
                return new BadRequestObjectResult(Errors);
            }

            return new BadRequestResult();
        }
    }
}