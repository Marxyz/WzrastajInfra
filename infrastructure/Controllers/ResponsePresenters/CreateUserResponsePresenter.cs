using System.Collections.Generic;
using Domain.Models;
using infrastructure.Controllers.ResponsePresenters.Base;
using Microsoft.AspNetCore.Mvc;

namespace infrastructure.Controllers.ResponsePresenters
{
    public class CreateUserResponsePresenter : PresentableResponse
    {
        private readonly Author Author;

        public CreateUserResponsePresenter(Author user)
        {
            Author = user;
        }

        public CreateUserResponsePresenter(IEnumerable<Error> errors) : base(errors)
        {
        }


        public override IActionResult ToPresentable()
        {
            return Success ? SuccessResult() : FailureResult();
        }


        private IActionResult SuccessResult()
        {
            return new OkObjectResult(Author);
        }
    }
}