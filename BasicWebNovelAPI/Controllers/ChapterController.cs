using BasicWebNovelAPI.Model.Dto.Novel;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BasicWebNovelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChapterController : ControllerBase
    {
        private readonly IChapterRepository _chapterRepository;

        public ChapterController(IChapterRepository chapterRepository)
        {
            _chapterRepository = chapterRepository;
        }

        [HttpPost("add-chapter/{novelId}")]
        public async Task<IActionResult> AddChapterToNovel(int novelId, [FromBody] CreateChapterDto chapterDto)
        {
            var chapter = await _chapterRepository.AddChapterToNovelAsync(novelId, chapterDto);

            return CreatedAtAction(nameof(AddChapterToNovel), new { novelId = novelId, chapterId = chapter.Id }, chapter);
        }


        [HttpPut("update-chapter/{novelId}/{chapterId}")]
        public async Task<IActionResult> UpdateChapter(int novelId, int chapterId, [FromBody] Chapter chapter)
        {
            var isUpdated = await _chapterRepository.UpdateChapterAsync(novelId, chapterId, chapter);
            if (!isUpdated)
            {
                return NotFound("Novel or chapter not found.");
            }
            return Ok("Chapter updated successfully.");
        }


        [HttpDelete("delete-chapter/{novelId}/{chapterId}")]
        public async Task<IActionResult> DeleteChapter(int novelId, int chapterId)
        {
            var isDeleted = await _chapterRepository.DeleteChapterAsync(novelId, chapterId);
            if (!isDeleted)
            {
                return NotFound("Novel or chapter not found.");
            }
            return Ok("Chapter deleted successfully.");
        }
    }
}
