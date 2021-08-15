using System.Linq;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using WzrastajAuth.Controllers.ResponsePresenters.Base;

namespace WzrastajAuth.Controllers.ResponsePresenters
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