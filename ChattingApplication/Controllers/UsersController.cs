using AutoMapper;
using ChattingApplication.Data;
using ChattingApplication.DTOs;
using ChattingApplication.Entities;
using ChattingApplication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChattingApplication.Controllers
{
    [Authorize]
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
    }
}
