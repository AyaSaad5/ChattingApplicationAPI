using ChattingApplication.Entities;

namespace ChattingApplication.Srvices.TokenServics
{
    public interface ITokenService
    {
        public Task<string> GenerateToken(AppUser user);
    }
}
