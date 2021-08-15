using System;
using System.Threading.Tasks;
using Domain.Services;
using infrastructure.Controllers.Requests;
using infrastructure.Controllers.ResponsePresenters;
using infrastructure.Services.External;

namespace infrastructure.Services.ActionHandlers
{
    public interface IUserActionHandler
    {
        Task<SingleUserResponsePresenter> GetById(Guid id);
        Task<CreateUserResponsePresenter> CreateAsync(RegistrationRequest request);
    }

    public sealed class UserActionHandler : IUserActionHandler
    {
        private readonly IAuthorDataProvider AuthorDataProvider;
        private readonly IAuthorDataWriter AuthorDataWriter;
        private readonly IAuthorizationDataHandler AuthorizationDataHandler;
        private readonly IMailService MailService;

        public UserActionHandler(IAuthorDataProvider authorDataProvider, IMailService mailService,
            IAuthorDataWriter authorDataWriter, IAuthorizationDataHandler authorizationDataHandler)
        {
            AuthorDataProvider = authorDataProvider;
            MailService = mailService;
            AuthorDataWriter = authorDataWriter;
            AuthorizationDataHandler = authorizationDataHandler;
        }

        public async Task<SingleUserResponsePresenter> GetById(Guid id)
        {
            var user = await AuthorDataProvider.FindByIdAsync(id);

            return new SingleUserResponsePresenter(user, user != default);
        }

        public async Task<CreateUserResponsePresenter> CreateAsync(RegistrationRequest request)
        {
            var createResult = await AuthorDataWriter.CreateAsync(request.Login, request.Email, request.Password);
            if (createResult.Item1 == null)
            {
                return new CreateUserResponsePresenter(createResult.Item2);
            }

            var author = createResult.Item1;
            var token = await AuthorizationDataHandler.GenerateMailConfirmationToken(author.Id);
            await MailService.SendRegistrationAsync(token.Value, author.AuthorInfo.Mail, author.AuthorInfo.Name,
                author.Id.ToString());
            return new CreateUserResponsePresenter(author);
        }
    }
}