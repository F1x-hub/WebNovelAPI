using AutoMapper;
using BasicWebNovelAPI.Model.Dto.Novel.Chapter;
using BasicWebNovelAPI.Model.Dto.Novel.Genre;
using BasicWebNovelAPI.Model.Dto.Novel.Library;
using BasicWebNovelAPI.Model.Dto.Novel.Novel;
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
            CreateMap<User, GetUserDto>()
                .ForMember(dest => dest.HasNewChapters, opt => opt.MapFrom(src => src.HasNewChapters));
            CreateMap<GetUserDto, User>();

            CreateMap<User, RegisterUserDto>();
            CreateMap<RegisterUserDto, User>();

            CreateMap<User, UpdateUserDto>().ReverseMap();

            CreateMap<RegisterUserDto, User>();

            CreateMap<GoogleJsonWebSignature.Payload, User>();

            //Novel
            CreateMap<Novel, GetNovelDto>();
            CreateMap<CreateNovelDto, Novel>();
            CreateMap<UpdateNovelDto, Novel>();

            
            CreateMap<Chapter, GetChapterDto>();
            CreateMap<CreateChapterDto, Chapter>();
            CreateMap<UpdateChapterDto, Chapter>();

            CreateMap<Novel, GetNovelDto>()
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src =>
                src.NovelGenres.Select(ng => ng.Genre.Name).ToList()));


            CreateMap<CreateGenreDto, Genre>();

            
            CreateMap<Genre, GetGenreDto>();

            //userlibrary

            CreateMap<UserLibraryDto, UserLibrary>();
            CreateMap<UserLibrary, UserLibraryDto>();


            CreateMap<UserLibrary, GetUserLibraryDto>()
            .ForMember(dest => dest.NovelTitle, opt => opt.MapFrom(src => src.Novel.Title));


            //Comments

            CreateMap<CreateNovelCommentDto, NovelComments>();
            CreateMap<NovelComments, GetNovelCommentDto>();

            CreateMap<CreateChapterCommentDto, ChapterComments>();
            CreateMap<ChapterComments, GetChapterCommentDto>();
            
            CreateMap<NovelComments, GetNovelCommentDto>()
                .ForMember(dest => dest.LikesCount, 
                    opt => opt.MapFrom(src => src.Likes.Count));
            
            CreateMap<ChapterComments, GetChapterCommentDto>()
                .ForMember(dest => dest.LikesCount, 
                    opt => opt.MapFrom(src => src.Likes.Count));

        }
    }
}
