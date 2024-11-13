using AutoMapper;
using ChattingApplication.Data.Migrations;
using ChattingApplication.DTOs;
using ChattingApplication.Entities;
using ChattingApplication.Extensions;

namespace ChattingApplication.Helpers
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Photo, PhotoDTO>();
            CreateMap<AppUser, MemberDTO>()
                .ForMember(dest => dest.PhotoUrl,
                opt => opt.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.Age,
                 opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
            CreateMap<MemberUpdateDTO, AppUser>();
        }
    }
}
