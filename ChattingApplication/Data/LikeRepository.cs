using ChattingApplication.DTOs;
using ChattingApplication.Entities;
using ChattingApplication.Extensions;
using ChattingApplication.Helpers;
using ChattingApplication.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChattingApplication.Data
{
    public class LikeRepository : ILikeRepository
    {
        private readonly DataContext _context;

        public LikeRepository(DataContext context)
        {
            _context = context;
        }
        public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, targetUserId);
        }

        public async Task<PagedList<LikeDTO>> GetUserLikes(LikesParams likesParams)
        {
            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
            var likes = _context.Likes.AsQueryable();
            if(likesParams.Predicate == "liked")
            {
                likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
                users = likes.Select(user => user.TargetUser);
            }
            if (likesParams.Predicate == "likedBy")
            {
                likes = likes.Where(like => like.TargetUserId == likesParams.UserId);
                users = likes.Select(user => user.SourceUser);
            }
            var likedUsres = users.Select(user => new LikeDTO
            {
                UserName = user.UserName,
                Age = user.DateOfBirth.CalculateAge(),
                knownAs = user.KnownAs,
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
                City = user.City,
                Id = user.Id,
            });
            return await PagedList<LikeDTO>.CreateAsync(likedUsres, likesParams.PageNumber, likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users.Include(x => x.LikedUsers)
                                 .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}
