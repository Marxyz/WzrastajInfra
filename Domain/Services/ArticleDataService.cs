using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Data.BlobStorage;
using Data.Contexts;
using Data.Entities;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services
{
    public class ArticleDataService : IArticleDataService
    {
        private readonly AppDbContext AppDbContext;
        private readonly IContentStorageService ContentStorageService;
        private readonly IStoragePathResolver StoragePathResolver;

        public ArticleDataService(
            AppDbContext appDbContext,
            IContentStorageService contentStorageService,
            IStoragePathResolver storagePathResolver)
        {
            AppDbContext = appDbContext;
            ContentStorageService = contentStorageService;
            StoragePathResolver = storagePathResolver;
        }

        public async Task<Article> GetById(Guid id)
        {
            var currentConstEncoding = Encoding.UTF8;
            var meta = await InternalGetById(id);
            var content =
                await ContentStorageService.DownloadContentAsync(
                    await StoragePathResolver.GetArticlePath(FromEntity(meta.Author), meta.Title),
                    currentConstEncoding,
                    CancellationToken.None
                );

            var articleContent = new ArticleContent
            {
                Content = content,
                Encoding = currentConstEncoding
            };
            return new Article
            {
                AuthorId = meta.Author.Id,
                Categories = meta.Categories.Select(x => x.Id),
                Content = articleContent,
                CreationDate = meta.Created,
                Id = meta.Id,
                Modification = meta.Modified,
                Title = meta.Title
            };
        }

        public async Task<Article> Create(ArticleContent articleContent, Author author, string title,
            IEnumerable<Category> categories)
        {
            var authorEnt = AppDbContext.Users.FirstOrDefault(x => x.Id == author.Id);
            var categoryEntities = AppDbContext.Categories.Where(x => categories.Select(z => z.Id).Contains(x.Id));

            var ent = new ArticleMetadataEntity
            {
                Categories = categoryEntities.ToList(),
                Author = authorEnt,
                Title = title
            };

            await ContentStorageService.SaveContentAsync(
                articleContent.Content,
                Encoding.UTF8,
                StoragePathResolver.CreateArticlePath(author, title),
                CancellationToken.None);


            await AppDbContext.Articles.AddAsync(ent);
            await AppDbContext.SaveChangesAsync();

            return await FromEntity(ent);
        }

        public async Task<Article> Update(Article article)
        {
            var author = AppDbContext.Users.FirstOrDefault(x => x.Id == article.Id);

            var categories = AppDbContext.Categories
                .Include(x => x.Articles)
                .Where(x => article.Categories.Contains(x.Id))
                .Select(z => FromEntity(z));

            var previous = InternalGetById(article.Id);
            var newA = await Create(article.Content, FromEntity(author), article.Title, categories);

            AppDbContext.Remove(previous);
            return newA;
        }

        private Category FromEntity(CategoryEntity entity)
        {
            return new Category { Articles = entity.Articles.Select(x => x.Id), Id = entity.Id, Name = entity.Name };
        }

        private async Task<Article> FromEntity(ArticleMetadataEntity entity)
        {
            var content = await GetContentByEntity(entity);
            return new Article
            {
                AuthorId = entity.Author.Id, Categories = entity.Categories.Select(x => x.Id),
                Content = content,
                CreationDate = entity.Created,
                Id = entity.Id,
                Modification = entity.Modified,
                Title = entity.Title
            };
        }

        private async Task<ArticleContent> GetContentByEntity(ArticleMetadataEntity entity)
        {
            var author = FromEntity(entity.Author);
            var path = await StoragePathResolver.GetArticlePath(author, entity.Title);
            var content = await ContentStorageService.DownloadContentAsync(path, Encoding.UTF8, CancellationToken.None);

            return new ArticleContent
            {
                Content = content,
                Encoding = Encoding.UTF8
            };
        }

        private async Task<IEnumerable<Guid>> GetCategoryConnectedArticles(CategoryEntity x)
        {
            return (await InternalGetAllAsync())
                .Where(m => m.Categories
                    .Select(cat => cat.Id)
                    .Contains(x.Id))
                .Select(m => m.Id);
        }


        private async Task<IEnumerable<ArticleMetadataEntity>> InternalGetAllAsync()
        {
            return await AppDbContext.Articles
                .Include(x => x.Author).ThenInclude(z => z.IdentityAppUser)
                .Include(x => x.Author).ThenInclude(z => z.RefreshTokens)
                .Include(x => x.Categories).ThenInclude(x => x.Articles)
                .ToListAsync();
        }

        private async Task<ArticleMetadataEntity> InternalGetById(Guid articleId)
        {
            return (await InternalGetAllAsync()).FirstOrDefault(x => x.Id == articleId);
        }

        private Author FromEntity(UserEntity entity)
        {
            var articles = GetUserArticles(entity.Id);
            return new Author
            {
                AuthorInfo = new AuthorInfo
                {
                    Mail = entity.Email,
                    Name = entity.DisplayName,
                    ModifiedDate = entity.Modified,
                    RegistrationDate = entity.Created
                },

                Id = entity.Id,
                Articles = articles
            };
        }

        private IEnumerable<Guid> GetUserArticles(Guid id)
        {
            var articles = AppDbContext.Articles
                .Include(a => a.Author)
                .Where(a => a.Author.Id == id)
                .Select(a => a.Id);

            return articles;
        }
    }

    public interface IStoragePathResolver
    {
        Task<string> GetArticlePath(Author author, string title);
        string CreateArticlePath(Author author, string title);
    }

    internal class StoragePathResolver : IStoragePathResolver
    {
        private const string BlobNameSeparator = "_";
        private const string DirectorySeparator = "/";
        private readonly IContentStorageService ContentStorageService;

        public StoragePathResolver(IContentStorageService contentStorageService)
        {
            ContentStorageService = contentStorageService;
        }

        public async Task<string> GetArticlePath(Author author, string title)
        {
            var blobNames = ContentStorageService.ListItemsAtPath(author.Id.ToString());
            var lastBlob = blobNames
                .Where(x => x.Contains(title))
                .Select(x => ResolveBlobName(x));

            var currentArticleVersionName = lastBlob.OrderBy(suff => suff.UnixDatetimeSuffix).First();
            var blobname = MakeTitle(currentArticleVersionName.Title, currentArticleVersionName.UnixDatetimeSuffix);

            var containerName = author.Id.ToString();
            return $"{containerName}/{blobname}";
        }

        public string CreateArticlePath(Author author, string title)
        {
            var containerName = author.Id.ToString();
            var blobname = MakeTitle(title, DateTime.Now);
            return $"{containerName}/{blobname}";
        }

        private DateTime ParseSuffix(string s)
        {
            return DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(s)).UtcDateTime;
        }

        private (DateTime UnixDatetimeSuffix, string Title) ResolveBlobName(string blobName)
        {
            var blob = blobName.Split(DirectorySeparator).Last();
            var split = blob.Split(BlobNameSeparator);
            if (split.Length >= 2)
            {
                var title = split[^2];
                var suff = blobName.Split(BlobNameSeparator)[^1];
                return (ParseSuffix(suff), title);
            }

            throw new Exception("Blobname does not contain separator value");
        }


        private string CreateSuffix(DateTime date)
        {
            return ((DateTimeOffset)date).ToUnixTimeSeconds().ToString();
        }

        private string MakeTitle(string title, DateTime date)
        {
            return $"{title}{BlobNameSeparator}{CreateSuffix(date)}";
        }
    }

    public interface IArticleDataService : IArticleDataProvider, IArticleDataWriter
    {
    }

    public interface IArticleDataProvider
    {
        public Task<Article> GetById(Guid id);
    }

    public interface IArticleDataWriter
    {
        public Task<Article> Create(ArticleContent articleContent, Author author, string title,
            IEnumerable<Category> categories);

        public Task<Article> Update(Article article);
    }
}