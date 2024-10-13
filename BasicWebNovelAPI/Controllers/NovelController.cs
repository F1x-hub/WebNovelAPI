using BasicWebNovelAPI.Model.Dto.Novel;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BasicWebNovelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NovelController : ControllerBase
    {
        private readonly INovelRepository _novelRepository;

        public NovelController(INovelRepository novelRepository)
        {
            _novelRepository = novelRepository;
        }

        
        [HttpGet("get-all-novels")]
        public async Task<IActionResult> GetNovels()
        {
            var novels = await _novelRepository.GetNovels();
            if (novels == null)
            {
                return NotFound("Novel not found.");
            }
            return Ok(novels);
        }

        
        [HttpGet("get-novel/{id}")]
        public async Task<IActionResult> GetNovelById(int id)
        {
            var novel = await _novelRepository.GetNovelById(id);
            if (novel == null)
            {
                return NotFound("Novel not found.");
            }
            return Ok(novel);
        }

        [HttpGet("get-novel-by-name")]
        public async Task<IActionResult> GetNovelByName(string name)
        {
            var novels = await _novelRepository.GetNovelByName(name);
            if (novels.Count <= 0)
            {
                return NotFound("Novel not found.");
            }

            return Ok(novels);
        }

        [HttpGet("get-all-user-novel/{id}")]
        public async Task<IActionResult> GetAllUserNovel(int id)
        {
            var novels = await _novelRepository.GetUserAllNovel(id);
            if (novels == null)
            {
                return NotFound("Novel not found.");
            }

            return Ok(novels);
        }

        
        [HttpPost("create-novel")]
        public async Task<IActionResult> CreateNovel([FromBody] CreateNovelDto createNovelDto)
        {
            var createdNovel = await _novelRepository.CreateNovel(createNovelDto);
            return Ok(createdNovel);
        }

        [HttpPost("create-genre")]
        public async Task<IActionResult> CreateGenre([FromBody] CreateGenreDto createGenreDto)
        {
            try
            {
                var createdGenre = await _novelRepository.CreateGenre(createGenreDto);
                return Ok(createdGenre);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update-novel/{id}")]
        public async Task<IActionResult> UpdateNovel(int id, [FromBody] UpdateNovelDto updateNovelDto)
        {
            bool isUpdated = await _novelRepository.UpdateNovel(id, updateNovelDto);
            if (!isUpdated)
            {
                return NotFound("Novel not found.");
            }

            return Ok("Novel updated successfully.");
        }


        [HttpDelete("delete-novel/{id}")]
        public async Task<IActionResult> DeleteNovel(int id)
        {
            bool isDeleted = await _novelRepository.DeleteNovel(id);
            if (!isDeleted)
            {
                return NotFound("Novel not found.");
            }

            return Ok(new { Message = "Novel deleted successfully." });
        }

        
        [HttpPost("add-chapter/{novelId}")]
        public async Task<IActionResult> AddChapterToNovel(int novelId, [FromBody] CreateChapterDto chapterDto)
        {
            var chapter = await _novelRepository.AddChapterToNovelAsync(novelId, chapterDto );

            return CreatedAtAction(nameof(AddChapterToNovel), new { novelId = novelId, chapterId = chapter.Id }, chapter);
        }

        
        [HttpPut("update-chapter/{novelId}/{chapterId}")]
        public async Task<IActionResult> UpdateChapter(int novelId, int chapterId, [FromBody] Chapter chapter)
        {
            var isUpdated = await _novelRepository.UpdateChapter(novelId, chapterId, chapter);
            if (!isUpdated)
            {
                return NotFound("Novel or chapter not found.");
            }
            return Ok("Chapter updated successfully.");
        }

        
        [HttpDelete("delete-chapter/{novelId}/{chapterId}")]
        public async Task<IActionResult> DeleteChapter(int novelId, int chapterId)
        {
            var isDeleted = await _novelRepository.DeleteChapter(novelId, chapterId);
            if (!isDeleted)
            {
                return NotFound("Novel or chapter not found.");
            }
            return Ok("Chapter deleted successfully.");
        }
    }
}
