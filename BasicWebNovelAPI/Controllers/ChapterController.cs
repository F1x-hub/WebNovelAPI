using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Model.Dto.Novel.Chapter;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost("add-chapter/{novelId}/{userId}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddChapterToNovel(int novelId, int userId, [FromBody] CreateChapterDto chapterDto)
        {
            try 
            {
                var chapter = await _chapterRepository.AddChapterToNovelAsync(novelId, userId, chapterDto);

                return CreatedAtAction(nameof(AddChapterToNovel), new { novelId = novelId, userId = userId, chapterId = chapter.Id }, chapter);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }


        [HttpPut("update-chapter/{novelId}/{userId}/{chapterId}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateChapter(int novelId,int userId, int chapterId, [FromBody] UpdateChapterDto updateChapterDto)
        {
            try 
            {
                var isUpdated = await _chapterRepository.UpdateChapterAsync(novelId, userId, chapterId, updateChapterDto);
                if (!isUpdated)
                {
                    return NotFound("Novel or chapter not found.");
                }
                return Ok("Chapter updated successfully.");
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }


        [HttpDelete("delete-chapter/{novelId}/{userId}/{chapterId}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> DeleteChapter(int novelId,int userId, int chapterId)
        {
            try 
            {
                var isDeleted = await _chapterRepository.DeleteChapterAsync(novelId, userId, chapterId);

                if (!isDeleted)
                {
                    return NotFound("Novel or chapter not found.");
                }

                return Ok("Chapter deleted successfully.");
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        [HttpGet("novel-all-chapters/{novelId}/{userId}")]
        public async Task<IActionResult> GetAllChapters(int novelId, int userId)
        {
            try
            {
                var chapters = await _chapterRepository.GetAllChaptersAsync(novelId, userId);
                return Ok(chapters);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-chapter/{novelId}/{chapterNumber}/{userId}")]
        public async Task<IActionResult> GetChapterByNumber(int novelId, int chapterNumber, int userId)
        {
            try
            {
                var chapter = await _chapterRepository.GetChapterAsync(novelId, chapterNumber, userId);
                return Ok(chapter);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }
    }
}
