using System.Linq;
using Domain.Models;
using infrastructure.Controllers.ResponsePresenters.Base;
using Microsoft.AspNetCore.Mvc;

namespace infrastructure.Controllers.ResponsePresenters
{
    public class SingleUserResponsePresenter : PresentableResponse
    {
        private readonly Author Data;


        public SingleUserResponsePresenter(Author data, bool success)
        {
            Data = data;
            Success = success;
        }


        private IActionResult SuccessResult => new OkObjectResult(Data);

        public override IActionResult ToPresentable()
        {
            if (Success)
            {
                return SuccessResult;
            }

            Errors.ToList().Add(new Error("User cannot be fetched"));
            return FailureResult();
        }
    }
}