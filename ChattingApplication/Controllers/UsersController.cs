using AutoMapper;
using ChattingApplication.Data;
using ChattingApplication.DTOs;
using ChattingApplication.Entities;
using ChattingApplication.Extensions;
using ChattingApplication.Helpers;
using ChattingApplication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChattingApplication.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUnitOfWork unitOfWork, IMapper mapper,
                               IPhotoService photoService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _photoService = photoService;
        }

        //[AllowAnonymous]
        //[Authorize(Roles ="Admin")]
        [HttpGet]
        public async Task <ActionResult<PagedList<MemberDTO>>> GetUsers([FromQuery] UserParams userParams)
       {
            var gender = await _unitOfWork.UserRepository.GetUserGender(User.GetUserName());
            userParams.CurrentUsername = User.GetUserName();

            if(string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = gender == "male" ? "female" : "male";
            }
            var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);
            //var mappedUsers = _mapper.Map<IEnumerable<MemberDTO>>(users);
            Response.AddPaginationHeader(new PaginationHeader(users.currentPage, users.totalPages, users.pageSize,
                                         users.totalCount));
            return Ok(users);
        }
       // [Authorize(Roles = "Member")]

        [HttpGet("{id}")]
        public async Task<ActionResult<MemberDTO>> GetUserById(int id)
        {
            var users = await _unitOfWork.UserRepository.GetUserByIdAsync(id);
            var mappedUsers = _mapper.Map<MemberDTO>(users);
            return Ok(mappedUsers);
        }
        [HttpGet("byuser/{username}")]
        public async Task<ActionResult<MemberDTO>> GetUserByuserName(string username)
        {
            var users = await _unitOfWork.UserRepository.GetUserByUserNameAsync(username);
            var mappedUsers = _mapper.Map<MemberDTO>(users);
            return Ok(mappedUsers);
        }
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
        {
            var username = User.GetUserName();
            var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(username);

            if (user == null) return NotFound();

            _mapper.Map(memberUpdateDTO, user);
            if(await _unitOfWork.Complete()) return NoContent();

            return BadRequest("failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
        {
            var s = User.GetUserName();
            var headers = HttpContext.Request.Headers;
            foreach (var header in headers)
            {
                Console.WriteLine($"{header.Key}: {header.Value}");
            }
            var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUserName());
            if (user == null) return NotFound();

            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if(user.Photos.Count == 0) photo.IsMain = true;

            user.Photos.Add(photo);

            if (await _unitOfWork.Complete())
            {
                return CreatedAtAction(nameof(GetUserByuserName),
                                       new {username = user.UserName},
                                       _mapper.Map<PhotoDTO>(photo));
            }
            return BadRequest("Problem with Adding Photo");
        }
        [HttpPut("setMainPhoto/{photoId}")]
        public async Task<ActionResult> setMainPhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUserName());
            if (user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(i => i.Id == photoId);
            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("this is already the main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;
            if (await _unitOfWork.Complete()) return NoContent();
            return BadRequest("problem setting the main photo");
        }

        [HttpDelete("deletephoto/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUserName());

            var photo = user.Photos.FirstOrDefault(i => i.Id == photoId);
            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("you can't delete main photo");
            if(photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);

            }
            user.Photos.Remove(photo);
            if (await _unitOfWork.Complete()) return Ok();
            return BadRequest("problem deleting photo");
        }
    }
}
