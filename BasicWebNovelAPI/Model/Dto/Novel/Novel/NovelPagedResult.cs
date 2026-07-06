using System.Collections.Generic;

namespace BasicWebNovelAPI.Model.Dto.Novel.Novel
{
    public class NovelPagedResult
    {
        public List<GetNovelDto> Novels { get; set; } = new List<GetNovelDto>();
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
} 