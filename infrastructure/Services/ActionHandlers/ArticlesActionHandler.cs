using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Services;
using infrastructure.Controllers.ResponsePresenters;
using infrastructure.Models;

namespace infrastructure.Services.ActionHandlers
{
    public interface IArticlesActionHandler
    {
        Task<SingleArticleResponsePresenter> CreateArticleAsync(ArticleSavingProcessingModel processingModel);
        Task<SingleArticleResponsePresenter> GetArticleAsync(Guid id);
    }

    public class ArticlesActionHandler : IArticlesActionHandler
    {
        private readonly IArticleDataProvider ArticleDataProvider;
        private readonly IArticleDataWriter ArticleDataWriter;

        private readonly IAuthorDataProvider AuthorDataProvider;

        private readonly ICategoryDataProvider CategoryDataProvider;
        private readonly ICategoryDataWriter CategoryDataWriter;

        public ArticlesActionHandler(IAuthorDataProvider authorDataProvider,
            IArticleDataWriter articleDataWriter,
            IArticleDataProvider articleDataProvider, ICategoryDataWriter categoryDataWriter,
            ICategoryDataProvider categoryDataProvider)
        {
            AuthorDataProvider = authorDataProvider;
            ArticleDataWriter = articleDataWriter;
            ArticleDataProvider = articleDataProvider;
            CategoryDataWriter = categoryDataWriter;
            CategoryDataProvider = categoryDataProvider;
        }

        public async Task<SingleArticleResponsePresenter> CreateArticleAsync(
            ArticleSavingProcessingModel processingModel)
        {
            var author = await AuthorDataProvider.FindByIdAsync(processingModel.AuthorUserId);
            if (author == null)
            {
                return new SingleArticleResponsePresenter(new Error("Author does not exist."));
            }

            IEnumerable<Category> newCategories;
            IEnumerable<Category> existingCategories;
            IEnumerable<Category> modifiedCategories;
            (existingCategories, newCategories, modifiedCategories) =
                await CategoryDataProvider.FilterRequestCategories(
                    processingModel.Categories.Select(x => Category.ProcessRequest(x.Id, x.Name)));

            var added = await CategoryDataWriter.AddCategories(newCategories.ToList());
            var categories = added
                .Concat(await CategoryDataWriter.UpdateCategories(modifiedCategories))
                .Concat(existingCategories).ToArray();

            if (processingModel.Categories.Any() && !categories.Any())
            {
                return new SingleArticleResponsePresenter(
                    new Error("Provided categories does not exist and cannot be created"));
            }

            var article = await ArticleDataWriter.Create(
                new ArticleContent
                {
                    Content = processingModel.Content,
                    Encoding = processingModel.Encoding
                },
                author,
                processingModel.AuthorGiveFilename,
                categories
            );

            return article == null
                ? new SingleArticleResponsePresenter(new Error("Article has not been created."))
                : new SingleArticleResponsePresenter(article);
        }


        public async Task<SingleArticleResponsePresenter> GetArticleAsync(Guid id)
        {
            var article = await ArticleDataProvider.GetById(id);

            return article == null
                ? new SingleArticleResponsePresenter(new Error("Article does not exist."))
                : new SingleArticleResponsePresenter(article);
        }
    }
}