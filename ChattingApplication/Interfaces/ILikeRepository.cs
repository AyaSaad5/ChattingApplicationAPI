using ChattingApplication.DTOs;
using ChattingApplication.Entities;
using ChattingApplication.Helpers;

namespace ChattingApplication.Interfaces
{
    public interface ILikeRepository
    {
        Task<UserLike> GetUserLike(int sourceUserId, int targetUserId);
        Task<AppUser> GetUserWithLikes(int userId);

        Task<PagedList<LikeDTO>> GetUserLikes(LikesParams likesParams);
    }
}
