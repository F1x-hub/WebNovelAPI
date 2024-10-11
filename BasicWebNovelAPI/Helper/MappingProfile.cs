using AutoMapper;
using BasicWebNovelAPI.Model.Dto.Novel;
using BasicWebNovelAPI.Model.Dto.User;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Model.UserManagement;
using Google.Apis.Auth;

namespace BasicWebNovelAPI.Helper
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            //User
            CreateMap<GetLoginDto, User>();
            CreateMap<User, GetLoginDto>();
            CreateMap<User, GetUserDto>();
            CreateMap<GetUserDto, User>();

            CreateMap<User, RegisterUserDto>();
            CreateMap<RegisterUserDto, User>();

            CreateMap<RegisterUserDto, User>();

            CreateMap<GoogleJsonWebSignature.Payload, User>();

            //Novel
            CreateMap<Novel, GetNovelDto>();
            CreateMap<CreateNovelDto, Novel>();
            CreateMap<UpdateNovelDto, Novel>();

            
            CreateMap<Chapter, GetChapterDto>();
            CreateMap<CreateChapterDto, Chapter>();
            CreateMap<UpdateChapterDto, Chapter>();


            

            CreateMap<CreateGenreDto, Genre>();

            
            CreateMap<Genre, GetGenreDto>();
        }
    }
}
