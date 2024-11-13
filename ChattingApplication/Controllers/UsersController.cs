using AutoMapper;
using ChattingApplication.Data;
using ChattingApplication.DTOs;
using ChattingApplication.Entities;
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

        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
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
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userRepository.GetUserByUserNameAsync(username);

            if (user == null) return NotFound();

            _mapper.Map(memberUpdateDTO, user);
            if(await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("failed to update user");
        }
    }
}
