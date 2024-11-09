using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Model.Dto.Novel.Chapter;
using BasicWebNovelAPI.Model.Dto.Novel.Novel;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Service.Abstractions;
using BasicWebNovelAPI.Service.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BasicWebNovelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _commentRepository;

        public CommentController(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        [HttpPost("send-novel-comment/{userId}/{novelId}")]
        //[Authorize(Roles = "User")]
        public async Task<IActionResult> SendNovelComment([FromBody] CreateNovelCommentDto createNovelCommentDto, int userId, int novelId)
        {
            try
            {
                var novelComment = await _commentRepository.SendNovelComment(createNovelCommentDto, userId, novelId);
                return Ok(novelComment);
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

        [HttpPost("send-chapter-comment/{userId}/{chapterId}")]
        //[Authorize(Roles = "User")]
        public async Task<IActionResult> SendChapterComment([FromBody] CreateChapterCommentDto createChapterCommentDto, int userId, int chapterId)
        {
            try
            {
                var chapterComment = await _commentRepository.SendChapterComment(createChapterCommentDto, userId, chapterId);
                return Ok(chapterComment);
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

        [HttpGet("get-novel-comment/{novelId}")]
        public async Task<IActionResult> GetNovelComments(int novelId)
        {
            try
            {
                var novelComment = await _commentRepository.GetAllCommentNovel(novelId);
                return Ok(novelComment);
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

        [HttpGet("get-chapter-comment/{chapterId}")]
        public async Task<IActionResult> GetChapterComments(int chapterId)
        {
            try
            {
                var chapterComment = await _commentRepository.GetAllCommentChapter(chapterId);
                return Ok(chapterComment);
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
