﻿using ChattingApplication.DTOs;
using ChattingApplication.Extensions;
using ChattingApplication.Helpers;
using ChattingApplication.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChattingApplication.Controllers
{
    public class LikesController : BaseApiController
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IUserRepository _userRepository;

        public LikesController(ILikeRepository likeRepository, IUserRepository userRepository)
        {
            _likeRepository = likeRepository;
            _userRepository = userRepository;
        }
        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId =int.Parse(User.GetUserId());
            var likedUser = await _userRepository.GetUserByUserNameAsync(username);
            var sourceUser = await _likeRepository.GetUserWithLikes(sourceUserId);

            if (likedUser == null) return NotFound();

            if (sourceUser.UserName == username) return BadRequest("you can't like your self");

            var userLikes = await _likeRepository.GetUserLike(sourceUserId, likedUser.Id);

            if (userLikes != null) return BadRequest("you already liked this user");

            userLikes = new Entities.UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = likedUser.Id,
            };

            sourceUser.LikedUsers.Add(userLikes);

            if(await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to like this user");

        }
        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDTO>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = int.Parse(User.GetUserId());
            var users = await _likeRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(new PaginationHeader(users.currentPage,
                                          users.pageSize, users.totalCount, users.totalPages));
            return Ok(users);
        }
    }
}
