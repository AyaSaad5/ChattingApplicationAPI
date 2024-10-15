using ChattingApplication.Entities;

namespace ChattingApplication.Srvices.TokenServics
{
    public interface ITokenService
    {
        public string GenerateToken(AppUser user);
    }
}
