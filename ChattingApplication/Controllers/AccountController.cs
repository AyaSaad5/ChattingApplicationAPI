using AutoMapper;
using ChattingApplication.Data;
using ChattingApplication.DTOs;
using ChattingApplication.Entities;
using ChattingApplication.Srvices.TokenServics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ChattingApplication.Controllers
{
 
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService,
                                 IMapper mapper)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDto)
        {
            if (await UserExists(registerDto.userName)) return BadRequest("UserName is taken");

            var user = _mapper.Map<AppUser>(registerDto);


            user.UserName = registerDto.userName.ToLower();
           

            var result = await _userManager.CreateAsync(user, registerDto.password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return new UserDTO
            {
                userName = user.UserName,
                token = await _tokenService.GenerateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                knownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDto)
        {
            var user = await _userManager.Users.Include(p => p.Photos).SingleOrDefaultAsync(u => u.UserName == loginDto.userName);
            if (user == null) return Unauthorized("Invalid userName");

            var result = _userManager.CheckPasswordAsync(user, loginDto.password);

            if (!result.Result) return Unauthorized("invalid password");
            var userrole = await _userManager.GetRolesAsync(user);
            if(!userrole.Any())
            {
                var roleResult = await _userManager.AddToRoleAsync(user, "Member");

                if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);
            }
            

            return new UserDTO
            {
                userName = user.UserName,
                token =await _tokenService.GenerateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                knownAs = user.KnownAs,
                Gender = user.Gender
            };
        }
        private async Task<bool> UserExists(string userName)
        {
            return await _userManager.Users.AnyAsync(u => u.UserName == userName.ToLower());
        }
    }
}
