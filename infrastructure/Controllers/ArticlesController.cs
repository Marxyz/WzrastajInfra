using System;
using System.Threading.Tasks;
using infrastructure.Controllers.Requests;
using infrastructure.Models;
using infrastructure.Services.ActionHandlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace infrastructure.Controllers
{
    public class ArticlesController : ControllerBase
    {
        private readonly IArticlesActionHandler ArticlesActionHandler;

        public ArticlesController(IArticlesActionHandler articlesActionHandler)
        {
            ArticlesActionHandler = articlesActionHandler;
        }

        [HttpPost("/article")]
        [Authorize(Policy = "Author")]
        [ValidateModel]
        public async Task<IActionResult> CreateArticle([FromBody] CreateArticleRequest createArticleRequest)
        {
            var res = await ArticlesActionHandler.CreateArticleAsync(new ArticleSavingProcessingModel
            {
                AuthorGiveFilename = createArticleRequest.Name,
                AuthorUserId = Guid.Parse(createArticleRequest.AuthorId),
                Categories = createArticleRequest.Categories,
                Content = createArticleRequest.Content,
                Links = createArticleRequest.Links
            });

            return res.ToPresentable();
        }

        [HttpGet("/article/{id}")]
        [Authorize(Policy = "Author")]
        [ValidateModel]
        public async Task<IActionResult> GetArticle(Guid id)
        {
            var art = await ArticlesActionHandler.GetArticleAsync(id);
            return art.ToPresentable();
        }
    }
}