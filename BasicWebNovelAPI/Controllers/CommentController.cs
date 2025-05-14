using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Hubs;
using BasicWebNovelAPI.Model.Dto.Novel.Chapter;
using BasicWebNovelAPI.Model.Dto.Novel.Novel;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Service.Abstractions;
using BasicWebNovelAPI.Service.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BasicWebNovelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IHubContext<CommentHub> _commentHub;

        public CommentController(ICommentRepository commentRepository, IHubContext<CommentHub> commentHub)
        {
            _commentRepository = commentRepository;
            _commentHub = commentHub;
        }

        [HttpPost("send-novel-comment/{userId}/{novelId}")]
        [Authorize(Roles = "User")]
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
        [Authorize(Roles = "User")]
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

        [HttpGet("get-novel-comment-like/{commentId}")]
        public async Task<IActionResult> GetNovelCommentLike(int commentId)
        {
            try
            {
                var novelCommetnLike = await _commentRepository.GetNovelCommentLikesCount(commentId);
                return Ok(novelCommetnLike);

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

        [HttpGet("get-chapter-comment-like/{commentId}")]
        public async Task<IActionResult> GetChapterCommentLike(int commentId)
        {
            try
            {
                var chapterCommentLike = await _commentRepository.GetChapterCommentLikesCount(commentId);
                return Ok(chapterCommentLike);

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

        [HttpPost("set-novel-comment-like/{commentId}/{userId}")]
        public async Task<IActionResult> SetNovelCommentLike(int commentId, int userId)
        {
            try
            {
                var novelCommentLike = await _commentRepository.ToggleNovelCommentLike(commentId, userId);
                return Ok(novelCommentLike);

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

        [HttpGet("has-user-liked-novel-comment/{commentId}/{userId}")]
        public async Task<IActionResult> GetUserLikedNovelComment(int commentId, int userId)
        {
            try
            {
                var userLikedNovelComment = await _commentRepository.HasUserLikedNovelComment(commentId, userId);
                return Ok(userLikedNovelComment);

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

        [HttpPost("set-chapter-comment-like/{commentId}/{userId}")]
        public async Task<IActionResult> SetChapterCommentLike(int commentId, int userId)
        {
            try
            {
                var chapterCommentLike = await _commentRepository.ToggleChapterCommentLike(commentId, userId);
                return Ok(chapterCommentLike);

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
        
        [HttpGet("has-user-liked-chapter-comment/{commentId}/{userId}")]
        public async Task<IActionResult> GetUserLikedChapterComment(int commentId, int userId)
        {
            try
            {
                var userLikedChapterComment = await _commentRepository.HasUserLikedChapterComment(commentId, userId);
                return Ok(userLikedChapterComment);

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

        [HttpDelete("delete-novel-comments/{commentId}/{novelId}/{userId}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> DeleteNovelComments(int commentId, int novelId, int userId)
        {
            try
            {
                bool isDeleted = await _commentRepository.DeleteNovelComments(commentId, novelId, userId);
                if (!isDeleted)
                {
                    return NotFound("Novel comments not found.");
                }

                return Ok(new { Message = "Novel comments deleted successfully." });
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

        [HttpDelete("delete-chapter-comments/{commentId}/{chapterId}/{userId}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> DeleteChapterComments(int commentId, int chapterId, int userId)
        {
            try
            {
                bool isDeleted = await _commentRepository.DeleteChapterComments(commentId, chapterId, userId);
                if (!isDeleted)
                {
                    return NotFound("Chapter comments not found.");
                }

                return Ok(new { Message = "Chapter comments deleted successfully." });
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
