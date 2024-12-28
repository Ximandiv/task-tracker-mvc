using Task_Tracker_WebApp.Auth;
using Task_Tracker_WebApp.Database.Entities;
using Task_Tracker_WebApp.Repositories.Interfaces;

namespace Task_Tracker_WebApp.Use_Cases.Auth
{
    public class TokenHandler
        (IUnitOfWork unitOfWork,
        ILogger<TokenHandler> logger,
        TokenGenerator tokenGen)
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<TokenHandler> _logger = logger;
        private readonly TokenGenerator _tokenGenerator = tokenGen;

        public string GenerateJWT(User user) => _tokenGenerator.GenerateJWT(user);

        public async Task<string> GetRememberToken(HttpRequest request)
        {
            string? rememberMeToken;
            if (!request.Cookies.TryGetValue("RememberMe", out rememberMeToken)
                && rememberMeToken == null)
                return string.Empty;

            try
            {
                var tokenModel = await _unitOfWork.RememberTokens.GetByToken(rememberMeToken);

                if (tokenModel is null) return string.Empty;

                if (tokenModel.Expiration <= DateTime.UtcNow)
                {
                    await _unitOfWork.BeginTransaction();
                    _unitOfWork.RememberTokens.Delete(tokenModel);
                    await _unitOfWork.SaveChanges();
                    await _unitOfWork.CommitTransaction();

                    return string.Empty;
                }

                return GenerateJWT(tokenModel.User!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in JWT renewal on Remember Me Token");
                throw;
            }
        }

        public async Task DeleteRememberToken(string token)
        {
            var tokenModel = await _unitOfWork.RememberTokens.GetByToken(token);

            if (tokenModel is null) return;

            await _unitOfWork.BeginTransaction();
            _unitOfWork.RememberTokens.Delete(tokenModel);
            await _unitOfWork.SaveChanges();
            await _unitOfWork.CommitTransaction();
        }

        public async Task<string> ActivateRememberToken(int id)
        {
            var rememberMeToken = _tokenGenerator.GenerateRememberMe(id);

            await _unitOfWork.BeginTransaction();
            await _unitOfWork.RememberTokens.Add(rememberMeToken);
            await _unitOfWork.SaveChanges();
            await _unitOfWork.CommitTransaction();

            return rememberMeToken.Token;
        }
    }
}
