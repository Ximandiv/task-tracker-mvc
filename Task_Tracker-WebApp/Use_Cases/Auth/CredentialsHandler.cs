using System.Security.Claims;
using Task_Tracker_WebApp.Cache.Enums;
using Task_Tracker_WebApp.Database.Entities;
using Task_Tracker_WebApp.Models;
using Task_Tracker_WebApp.Repositories.Interfaces;
using Task_Tracker_WebApp.Use_Cases.Auth.Models;
using Task_Tracker_WebApp.Use_Cases.Cache;

namespace Task_Tracker_WebApp.Use_Cases.Auth
{
    public class CredentialsHandler
        (
        TokenHandler tokenHandler,
        IUnitOfWork unitOfWork,
        ILogger<CredentialsHandler> logger,
        ICacheHandler cacheHandler)
    {
        private readonly TokenHandler _tokenHandler = tokenHandler;
        private readonly ILogger<CredentialsHandler> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICacheHandler _cacheHandler =  cacheHandler;

        public AuthorizedUser GetAuthUser(ClaimsPrincipal User)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue("username");

            return new AuthorizedUser(userIdString, userName);
        }

        public async Task Register(SignInModel userModel)
        {
            userModel.Password = BCrypt.Net.BCrypt.HashPassword(userModel.Password);
            try
            {
                await _unitOfWork.BeginTransaction();

                var userEntity = userModel.MapToUser();

                await _unitOfWork.Users.Add(userEntity);
                await _unitOfWork.SaveChanges();
                await _unitOfWork.CommitTransaction();

                await _cacheHandler.Set(CachePrefix.User, userModel.Email!, userEntity);
            }
            catch(Exception ex)
            {
                _logger.LogError
                    (ex, 
                    $"An error occurred during User Registration with email {userModel.Email}");
                throw;
            }
        }

        public async Task<AuthTokens?> Login
            (LogInModel userModel)
        {
            try
            {
                User? foundUser = await findUserByCreds(userModel.Email!, userModel.Password!);
                if (foundUser is null)
                    return null;

                string rememberToken = string.Empty;
                if (userModel.RememberMe)
                    rememberToken = await _tokenHandler.ActivateRememberToken(foundUser.Id);

                string jwt = _tokenHandler.GenerateJWT(foundUser);

                return new AuthTokens(jwt, rememberToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred during User Login with email {userModel.Email}");
                throw;
            }
        }

        private async Task<User?> findUserByCreds
            (string email,
            string password)
        {
            User? foundUser = await _cacheHandler.Get<User>(CachePrefix.User, email)
                            ?? await _unitOfWork.Users.GetByEmail(email);

            if(foundUser is null)
                return null;

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, foundUser.Password);

            if (!isPasswordValid)
                return null;

            return foundUser;
        }
    }
}
