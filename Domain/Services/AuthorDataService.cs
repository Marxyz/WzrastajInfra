using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Contexts;
using Data.Entities;
using Domain.Enums;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services
{
    public interface IAuthorDataProvider
    {
        Task<Author> FindByIdAsync(Guid id);
        Task<Author> FindByLoginAsync(string login);
        Task<Author> FindByEmailAsync(string mail);
        Task<IEnumerable<Author>> GetAll();
    }

    public interface IAuthorizationDataHandler
    {
        Task<Token> StartResetPasswordProcessAsync(Guid id);
        Task<bool> ConfirmResetPasswordAsync(Guid id, Token resetPasswordToken, string password);
        Task<Token> GenerateMailConfirmationToken(Guid id);
        Task<bool> ConfirmMailAsync(Guid id, Token mailConfirmationToken);
        Task<Token> RegenerateRefreshToken(Guid id, Token refreshToken, string location);
        Task<Token> GenerateRefreshToken(Guid id, string location);
        Task<bool> CheckPasswordAsync(Guid id, string requestPassword);
    }

    public interface IAuthorDataWriter
    {
        Task<(Author, IEnumerable<Error>)> CreateAsync(string name, string email, string password);
        Task<(Author, IEnumerable<Error>)> UpdateAsync(Guid id, Author user);
    }

    public interface IAuthorDataService : IAuthorizationDataHandler, IAuthorDataProvider, IAuthorDataWriter
    {
    }

    public class AuthorDataService : IAuthorDataService
    {
        private readonly AppDbContext AppDbContext;
        private readonly SignInManager<IdentityUser> SignInManager;
        private readonly ITokenFactory TokenFactory;
        private readonly UserManager<IdentityUser> UserManager;

        public AuthorDataService(UserManager<IdentityUser> userManager, AppDbContext appDbContext,
            SignInManager<IdentityUser> signInManager, ITokenFactory tokenFactory)
        {
            UserManager = userManager;
            AppDbContext = appDbContext;
            SignInManager = signInManager;
            TokenFactory = tokenFactory;
        }


        public async Task<bool> ConfirmResetPasswordAsync(Guid id, Token resetPasswordToken, string password)
        {
            if (resetPasswordToken.Type != TokenType.PasswordReset)
            {
                return false;
            }

            var user = await InternalGetById(id);
            if (user == null)
            {
                return false;
            }

            var appuser = await UserManager.FindByIdAsync(user.IdentityId);
            if (appuser == null)
            {
                throw new ArgumentException("User should always have identity.");
            }

            var identityResult = await UserManager.ResetPasswordAsync(appuser, resetPasswordToken.Value, password);
            return identityResult.Succeeded;
        }

        public async Task<Token> GenerateMailConfirmationToken(Guid id)
        {
            var user = await InternalGetById(id);
            var identity = await UserManager.FindByIdAsync(user.IdentityId);
            var token = await UserManager.GenerateEmailConfirmationTokenAsync(identity);
            return Token.Create(token, TokenType.ConfirmRegistration);
        }

        public async Task<Token> StartResetPasswordProcessAsync(Guid id)
        {
            var user = await InternalGetById(id);
            var appuser = await UserManager.FindByIdAsync(user.IdentityId);
            var token = await UserManager.GeneratePasswordResetTokenAsync(appuser);
            return Token.Create(token, TokenType.PasswordReset);
        }

        public async Task<bool> ConfirmMailAsync(Guid id, Token mailConfirmationToken)
        {
            if (mailConfirmationToken.Type != TokenType.ConfirmRegistration)
            {
                return false;
            }

            var user = await InternalGetById(id);
            if (user == null)
            {
                return false;
            }

            var app = await UserManager.FindByIdAsync(user.IdentityId);
            if (app == null)
            {
                throw new ArgumentException("User should always have identity.");
            }

            var identityResult = await UserManager.ConfirmEmailAsync(app, mailConfirmationToken.Value);
            return identityResult.Succeeded;
        }

        public async Task<Author> FindByIdAsync(Guid id)
        {
            var user = await InternalGetById(id);
            if (user == null)
            {
                return null;
            }

            return FromEntity(user);
        }

        public async Task<Author> FindByLoginAsync(string login)
        {
            var all = await InternalGetAllAsync();
            var user = all.First(x => x.DisplayName == login);
            if (user == null)
            {
                return null;
            }

            return FromEntity(user);
        }


        public async Task<(Author, IEnumerable<Error>)> CreateAsync(string name, string email, string password)
        {
            var appUser = new IdentityUser { Email = email, UserName = name };
            var identityResult = await UserManager.CreateAsync(appUser, password);

            if (!identityResult.Succeeded)
            {
                return (null, identityResult.Errors.Select(x => new Error(x.Description)));
            }

            var user = new UserEntity { IdentityAppUser = appUser };

            await AppDbContext.Users.AddAsync(user);
            await AppDbContext.SaveChangesAsync();


            return (FromEntity(user), Enumerable.Empty<Error>());
        }

        public async Task<(Author, IEnumerable<Error>)> UpdateAsync(Guid id, Author author)
        {
            var user = await InternalGetById(id);
            if (user == null)
            {
                return (null, new[] { new Error("User does not exist") });
            }

            var identityUser = await UserManager.FindByIdAsync(user.IdentityId);

            identityUser.Email = author.AuthorInfo.Mail;
            identityUser.UserName = author.AuthorInfo.Name;

            var res = await UserManager.UpdateAsync(identityUser);
            if (!res.Succeeded)
            {
                return (null, res.Errors.Select(x => new Error(x.Description)));
            }

            await AppDbContext.SaveChangesAsync();

            var updated = await InternalGetById(id);
            return (FromEntity(updated), Enumerable.Empty<Error>());
        }

        public async Task<Token> GenerateRefreshToken(Guid id, string location)
        {
            var user = await InternalGetById(id);
            var token = TokenFactory.GenerateRandomStringToken();
            user.AddRefreshToken(token, location);
            await AppDbContext.SaveChangesAsync();
            return Token.Create(token, TokenType.RefreshAccessToken);
        }

        public async Task<bool> CheckPasswordAsync(Guid id, string requestPassword)
        {
            var user = await InternalGetById(id);
            var identity = await UserManager.FindByIdAsync(user.IdentityId);
            if (identity == null)
            {
                return false;
            }

            var res = await SignInManager.CheckPasswordSignInAsync(identity, requestPassword, false);
            if (!res.Succeeded)
            {
                await UserManager.AccessFailedAsync(identity);
            }

            return res.Succeeded;
        }

        public async Task<Token> RegenerateRefreshToken(Guid id, Token refreshToken, string tokenCreationLocation)
        {
            var user = await InternalGetById(id);
            if (user.HasValidRefreshToken(refreshToken.Value))
            {
                var refreshTokens = AppDbContext.RefreshTokens.Where(x => x.UserId == id);
                AppDbContext.RefreshTokens.RemoveRange(refreshTokens);
                var newToken = TokenFactory.GenerateRandomStringToken();
                user.AddRefreshToken(newToken, tokenCreationLocation);
                await AppDbContext.SaveChangesAsync();

                return Token.Create(newToken, TokenType.RefreshAccessToken);
            }

            throw new ArgumentException(nameof(refreshToken));
        }

        public async Task<Author> FindByEmailAsync(string mail)
        {
            var identity = await UserManager.FindByEmailAsync(mail);

            var user = AppDbContext.Users
                .Include(x => x.RefreshTokens)
                .Include(x => x.IdentityAppUser).First(x => x.IdentityId == identity.Id);

            return FromEntity(user);
        }

        public async Task<IEnumerable<Author>> GetAll()
        {
            return (await InternalGetAllAsync()).Select(x => FromEntity(x));
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


        private async Task<IEnumerable<UserEntity>> InternalGetAllAsync()
        {
            return await AppDbContext.Users
                .Include(x => x.IdentityAppUser)
                .Include(x => x.RefreshTokens)
                .ToListAsync();
        }

        private async Task<UserEntity> InternalGetById(Guid id)
        {
            return (await InternalGetAllAsync()).FirstOrDefault(x => x.Id == id);
        }
    }
}