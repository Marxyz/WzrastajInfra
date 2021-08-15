using System.Collections.Generic;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using WzrastajAuth.Controllers.ResponsePresenters.Base;

namespace WzrastajAuth.Controllers.ResponsePresenters
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