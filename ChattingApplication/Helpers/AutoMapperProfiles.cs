﻿using AutoMapper;
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

            CreateMap<RegisterDTO, AppUser>().ReverseMap();

            CreateMap<Message, MessageDTO>()
                     .ForMember(d => d.SenderPhotoUrl, o => o.MapFrom(s => s.Sender.Photos
                     .FirstOrDefault(x => x.IsMain).Url))
                     .ForMember(d => d.RecipientPhotoUrl, o => o.MapFrom(s => s.Recipient.Photos
                     .FirstOrDefault(x => x.IsMain).Url));

            CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
            CreateMap<DateTime?, DateTime?>().ConvertUsing(d => d.HasValue ? 
                                 DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) :
                                 null);

        }
    }
}
