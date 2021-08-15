using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Contexts;
using Data.Entities;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services
{
    public interface ICategoryDataProvider
    {
        public IEnumerable<Category> GetCategoriesByArticle(Guid articleId);

        public Task<(IEnumerable<Category> existingCategories,
                IEnumerable<Category> newCategories,
                IEnumerable<Category> changedCategories)>
            FilterRequestCategories(IEnumerable<Category> requestCategories);
    }

    public interface ICategoryDataWriter
    {
        public Task<IEnumerable<Category>> UpdateCategories(IEnumerable<Category> categories);
        public Task<IEnumerable<Category>> AddCategories(IEnumerable<Category> categories);
    }

    public interface ICategoryDataService : ICategoryDataWriter, ICategoryDataProvider
    {
    }

    public class CategoryDataService : ICategoryDataService
    {
        private readonly AppDbContext AppDbContext;

        public CategoryDataService(AppDbContext appDbContext)
        {
            AppDbContext = appDbContext;
        }

        public async Task<IEnumerable<Category>> UpdateCategories(IEnumerable<Category> categories)
        {
            var enumerable = categories.ToList();
            foreach (var category in enumerable)
            {
                var ent = await InternalGetById(category.Id);
                ent.Name = ent.Name;
                AppDbContext.Categories.Update(ent);
            }

            await AppDbContext.SaveChangesAsync();


            return (await InternalGetAllAsync()).Where(x =>
                    enumerable
                        .Select(z => z.Id)
                        .Contains(x.Id))
                .Select(FromEntity);
        }

        public async Task<IEnumerable<Category>> AddCategories(IEnumerable<Category> categories)
        {
            var added = new List<CategoryEntity>();
            foreach (var category in categories)
            {
                var newEntity = await AppDbContext.Categories.AddAsync(new CategoryEntity { Name = category.Name });
                added.Add(newEntity.Entity);
            }

            await AppDbContext.SaveChangesAsync();
            return added.Select(FromEntity);
        }


        public IEnumerable<Category> GetCategoriesByArticle(Guid articleId)
        {
            var all = AppDbContext.Articles
                .Include(x => x.Categories).ThenInclude(x => x.Articles)
                .FirstOrDefault()
                ?.Categories.Select(FromEntity);
            return all;
        }

        public async
            Task<(IEnumerable<Category> existingCategories, IEnumerable<Category> newCategories, IEnumerable<Category>
                changedCategories)> FilterRequestCategories(IEnumerable<Category> requestCategories)
        {
            var all = (await InternalGetAllAsync()).ToArray();
            var categories = requestCategories as Category[] ?? requestCategories.ToArray();

            var existingCategories =
                categories.Where(category =>
                    {
                        return all
                            .Select(categoryEntity => NormalizeName(categoryEntity.Name))
                            .Contains(NormalizeName(category.Name));
                    }
                ).Select(category => InternalGetByNormalizedName(category.Name).GetAwaiter().GetResult());


            var modifiedCategories = categories
                .Where(t =>
                {
                    var (id, name) = (t.Id, t.Name);

                    return all.Select(categoryEntity => categoryEntity.Id).Contains(id)
                           &&
                           !all.Select(categoryEntity => NormalizeName(categoryEntity.Name))
                               .Contains(NormalizeName(name));
                });

            var newCategories = categories.Where(cat => !existingCategories.Select(x => x.Name).Contains(cat.Name));

            return (existingCategories.Select(FromEntity).ToList(), newCategories.ToList(),
                modifiedCategories.ToList());
        }

        private static Category FromEntity(CategoryEntity entity)
        {
            return new Category { Articles = entity.Articles.Select(x => x.Id), Id = entity.Id, Name = entity.Name };
        }

        private static string NormalizeName(string name)
        {
            return name.ToLowerInvariant();
        }

        private async Task<IEnumerable<ArticleMetadataEntity>> InternalGetArticlesWithCategory(Guid categoryId)
        {
            var allArts = await AppDbContext.Articles
                .Include(x => x.Author).ThenInclude(z => z.IdentityAppUser)
                .Include(x => x.Author).ThenInclude(z => z.RefreshTokens)
                .Include(x => x.Categories).ThenInclude(x => x.Articles)
                .ToListAsync();

            return allArts.Where(m => m.Categories
                .Select(cat => cat.Id)
                .Contains(categoryId));
        }

        private async Task<CategoryEntity> InternalGetById(Guid id)
        {
            return (await InternalGetAllAsync()).FirstOrDefault(x => x.Id == id);
        }

        private async Task<CategoryEntity> InternalGetByNormalizedName(string name)
        {
            return (await InternalGetAllAsync()).FirstOrDefault(x => NormalizeName(x.Name) == name);
        }

        private async Task<IEnumerable<CategoryEntity>> InternalGetAllAsync()
        {
            return await AppDbContext.Categories
                .Include(x => x.Articles).ThenInclude(x => x.Author)
                .ToListAsync();
        }
    }
}