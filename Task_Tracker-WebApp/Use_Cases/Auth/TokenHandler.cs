using Task_Tracker_WebApp.Auth;
using Task_Tracker_WebApp.Cache.Enums;
using Task_Tracker_WebApp.Database.Entities;
using Task_Tracker_WebApp.Repositories.Interfaces;
using Task_Tracker_WebApp.Use_Cases.Cache;

namespace Task_Tracker_WebApp.Use_Cases.Auth
{
    public class TokenHandler
        (TokenGenerator tokenGen,
        IUnitOfWork unitOfWork,
        ILogger<TokenHandler> logger,
        ICacheHandler cacheHandler)
    {
        private readonly TokenGenerator _tokenGenerator = tokenGen;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<TokenHandler> _logger = logger;
        private readonly ICacheHandler _cacheHandler = cacheHandler;

        public string GenerateJWT(User user) => _tokenGenerator.GenerateJWT(user);

        public async Task<string> GetJWTOnRememberToken(HttpRequest request)
        {
            string? rememberMeToken = getRememberToken(request);

            if (rememberMeToken is null) return string.Empty;

            try
            {
                var tokenModel = await _cacheHandler
                    .Get<RememberMeToken>(CachePrefix.RememberToken, rememberMeToken);
                if (tokenModel is null)
                    tokenModel = await _unitOfWork.RememberTokens.GetByToken(rememberMeToken);

                if (await isRememberExpiredOrNull(tokenModel)) return string.Empty;

                return GenerateJWT(tokenModel!.User!);
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
            
            await _cacheHandler.Remove(CachePrefix.RememberToken, token);
        }

        public async Task<string> ActivateRememberToken(int id)
        {
            var rememberMeToken = _tokenGenerator.GenerateRememberMe(id);

            await _unitOfWork.BeginTransaction();
            await _unitOfWork.RememberTokens.Add(rememberMeToken);
            await _unitOfWork.SaveChanges();
            await _unitOfWork.CommitTransaction();

            rememberMeToken = await _unitOfWork.RememberTokens.GetByToken(rememberMeToken.Token);

            await _cacheHandler.Set(CachePrefix.RememberToken, rememberMeToken!.Token, rememberMeToken);

            return rememberMeToken.Token;
        }

        private async Task<bool> isRememberExpiredOrNull(RememberMeToken? tokenModel)
        {
            if (tokenModel is null) return true;

            if (tokenModel.Expiration <= DateTime.UtcNow)
            {
                await _unitOfWork.BeginTransaction();
                _unitOfWork.RememberTokens.Delete(tokenModel);
                await _unitOfWork.SaveChanges();
                await _unitOfWork.CommitTransaction();

                await _cacheHandler.Remove(CachePrefix.RememberToken, tokenModel.Token);

                return true;
            }

            return false;
        }

        private string getRememberToken(HttpRequest request)
        {
            string? rememberMeToken;
            if (!request.Cookies.TryGetValue("RememberMe", out rememberMeToken)
                && rememberMeToken == null)
                return string.Empty;

            return rememberMeToken;
        }
    }
}
