using ChattingApplication.DTOs;
using ChattingApplication.Entities;
using ChattingApplication.Helpers;

namespace ChattingApplication.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);
        Task<bool> SaveAllAsync();
        Task<IEnumerable<AppUser>> GetUsersAsync();
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByUserNameAsync(string username);
        Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams);
    }
}
