using Domain.Models;
using infrastructure.Controllers.ResponsePresenters.Base;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace infrastructure.Controllers.ResponsePresenters
{
    public class SingleArticleResponsePresenter : PresentableResponse
    {
        private readonly Article Data;

        public SingleArticleResponsePresenter(Article data)
        {
            Data = data;
        }

        public SingleArticleResponsePresenter(params Error[] errors) : base(errors)
        {
        }


        public override IActionResult ToPresentable()
        {
            if (Success)
            {
                return new OkObjectResult(JsonConvert.SerializeObject(new
                {
                    Data.AuthorId,
                    Content = new
                    {
                        Data.Content.Content,
                        Encoding = Data.Content.Encoding.EncodingName
                    },
                    Data.Categories,
                    Data.Title,
                    Data.CreationDate,
                    Data.Id,
                    LastModified = Data.Modification
                }, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Serialize }));
            }

            return FailureResult();
        }
    }
}