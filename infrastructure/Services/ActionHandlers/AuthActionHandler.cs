using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Models;
using Domain.Services;
using infrastructure.Auth;
using infrastructure.Controllers.ResponsePresenters;
using infrastructure.Models;
using infrastructure.Services.External;

namespace infrastructure.Services.ActionHandlers
{
    public interface IAuthActionHandler
    {
        Task<ExchangeRefreshTokenResponsePresenter> ProcessRefreshTokenRequestAsync(
            RefreshTokenProcessingModel processingModel);

        Task<MailConfirmationResponsePresenter> ProcessMailConfirmationRequest(
            MailConfirmationProcessingModel processingModel);

        Task<LoginResponsePresenter> AuthenticateUserAsync(LoginProcessingModel processingModel);
        Task ProcessForgetPasswordRequestAsync(string mail);

        Task<PasswordResetResponsePresenter> ConfirmForgetPasswordRequestAsync(string mail, string token,
            string password);
    }

    public class AuthActionHandler : IAuthActionHandler
    {
        private readonly IAuthorDataProvider AuthorDataProvider;
        private readonly IAuthorizationDataHandler AuthorizationDataHandler;
        private readonly IJwtFactory JwtFactory;
        private readonly IJwtTokenValidator JwtTokenValidator;
        private readonly IMailService MailService;


        public AuthActionHandler(IMailService mailService, IJwtTokenValidator jwtTokenValidator, IJwtFactory jwtFactory,
            IAuthorDataProvider authorDataProvider, IAuthorizationDataHandler authorizationDataHandler)
        {
            MailService = mailService;
            JwtTokenValidator = jwtTokenValidator;
            JwtFactory = jwtFactory;
            AuthorDataProvider = authorDataProvider;
            AuthorizationDataHandler = authorizationDataHandler;
        }

        public async Task<ExchangeRefreshTokenResponsePresenter> ProcessRefreshTokenRequestAsync(
            RefreshTokenProcessingModel processingModel)
        {
            var cp = JwtTokenValidator.GetPrincipalFromToken(processingModel.AccessToken, processingModel.SigningKey);
            if (cp == null)
            {
                return new ExchangeRefreshTokenResponsePresenter(null, null, false);
            }

            var guid = cp.Claims.First(c => c.Type == "UserId");
            var author = await AuthorDataProvider.FindByIdAsync(Guid.Parse(guid.Value));

            var refreshToken = await AuthorizationDataHandler.RegenerateRefreshToken(
                author.Id,
                Token.Create(processingModel.RefreshToken, TokenType.RefreshAccessToken),
                processingModel.Location);

            var jwtToken = await JwtFactory.GenerateEncodedToken(author.Id.ToString(), author.AuthorInfo.Name);

            return new ExchangeRefreshTokenResponsePresenter(jwtToken, refreshToken.Value, true);
        }

        public async Task<MailConfirmationResponsePresenter> ProcessMailConfirmationRequest(
            MailConfirmationProcessingModel processingModel)
        {
            var author = await AuthorDataProvider.FindByIdAsync(processingModel.UserId);
            if (author == null)
            {
                return new MailConfirmationResponsePresenter(false);
            }

            var confirmedSuccessfully = await AuthorizationDataHandler.ConfirmMailAsync(
                author.Id,
                Token.Create(processingModel.Token, TokenType.ConfirmRegistration));

            return confirmedSuccessfully
                ? new MailConfirmationResponsePresenter(true)
                : new MailConfirmationResponsePresenter(false);
        }

        public async Task<LoginResponsePresenter> AuthenticateUserAsync(LoginProcessingModel processingModel)
        {
            if (!string.IsNullOrEmpty(processingModel.Login) && !string.IsNullOrEmpty(processingModel.Password))
            {
                var user = await AuthorDataProvider.FindByLoginAsync(processingModel.Login);
                if (user != null &&
                    await AuthorizationDataHandler.CheckPasswordAsync(user.Id, processingModel.Password))
                {
                    var refreshToken =
                        await AuthorizationDataHandler.GenerateRefreshToken(user.Id, processingModel.RemoteIpAddress);
                    var accessToken = await JwtFactory.GenerateEncodedToken(user.Id.ToString(), user.AuthorInfo.Name);

                    return new LoginResponsePresenter(accessToken, refreshToken.Value, true);
                }
            }

            return new LoginResponsePresenter(null, null, false);
        }

        public async Task ProcessForgetPasswordRequestAsync(string mail)
        {
            var user = await AuthorDataProvider.FindByEmailAsync(mail);
            if (user == null)
            {
                return;
            }

            var resetToken = await AuthorizationDataHandler.StartResetPasswordProcessAsync(user.Id);
            await MailService.SendRegistrationAsync(resetToken.Value, user.AuthorInfo.Mail, user.AuthorInfo.Name,
                user.Id.ToString());
        }

        public async Task<PasswordResetResponsePresenter> ConfirmForgetPasswordRequestAsync(string mail, string token,
            string password)
        {
            var user = await AuthorDataProvider.FindByEmailAsync(mail);
            if (user == null)
            {
                return new PasswordResetResponsePresenter(false);
            }

            var confirmResetPasswordAsync = await AuthorizationDataHandler.ConfirmResetPasswordAsync(
                user.Id,
                Token.Create(token, TokenType.PasswordReset),
                password);

            return new PasswordResetResponsePresenter(confirmResetPasswordAsync);
        }
    }
}