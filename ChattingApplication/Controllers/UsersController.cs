using AutoMapper;
using ChattingApplication.Data;
using ChattingApplication.DTOs;
using ChattingApplication.Entities;
using ChattingApplication.Extensions;
using ChattingApplication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChattingApplication.Controllers
{
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository userRepository, IMapper mapper,
                               IPhotoService photoService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _photoService = photoService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task <ActionResult<IEnumerable<MemberDTO>>> GetUsers()
       {
            var users = await _userRepository.GetUsersAsync();
            var mappedUsers = _mapper.Map<IEnumerable<MemberDTO>>(users);
            return Ok(mappedUsers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MemberDTO>> GetUserById(int id)
        {
            var users = await _userRepository.GetUserByIdAsync(id);
            var mappedUsers = _mapper.Map<MemberDTO>(users);
            return Ok(mappedUsers);
        }
        [HttpGet("byuser/{username}")]
        public async Task<ActionResult<MemberDTO>> GetUserByuserName(string username)
        {
            var users = await _userRepository.GetUserByUserNameAsync(username);
            var mappedUsers = _mapper.Map<MemberDTO>(users);
            return Ok(mappedUsers);
        }
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
        {
            var username = User.GetUserName();
            var user = await _userRepository.GetUserByUserNameAsync(username);

            if (user == null) return NotFound();

            _mapper.Map(memberUpdateDTO, user);
            if(await _userRepository.SaveAllAsync()) return NoContent();

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
            var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());
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

            if (await _userRepository.SaveAllAsync())
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
            var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());
            if (user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(i => i.Id == photoId);
            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("this is already the main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;
            if (await _userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("problem setting the main photo");
        }

        [HttpDelete("deletephoto/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());

            var photo = user.Photos.FirstOrDefault(i => i.Id == photoId);
            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("you can't delete main photo");
            if(photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);

            }
            user.Photos.Remove(photo);
            if (await _userRepository.SaveAllAsync()) return Ok();
            return BadRequest("problem deleting photo");
        }
    }
}
