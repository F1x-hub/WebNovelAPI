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
    /// <summary>
    /// Controller for managing comments on novels and chapters
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IHubContext<CommentHub> _commentHub;

        public CommentController(ICommentRepository commentRepository, IHubContext<CommentHub> commentHub)
        {
            _commentRepository = commentRepository;
            _commentHub = commentHub;
        }

        /// <summary>
        /// Adds a comment to a novel
        /// </summary>
        /// <param name="createNovelCommentDto">Object containing the comment text and details</param>
        /// <param name="userId">The unique identifier of the user creating the comment</param>
        /// <param name="novelId">The unique identifier of the novel to comment on</param>
        /// <returns>The newly created comment with its assigned ID</returns>
        /// <response code="200">Returns the created comment</response>
        /// <response code="400">If the comment data is invalid</response>
        /// <response code="404">If the novel or user is not found</response>
        /// <remarks>
        /// This endpoint is restricted to authenticated users.
        /// New comments are broadcast to connected clients via SignalR for real-time updates.
        /// 
        /// Sample request:
        ///
        ///     POST /api/Comment/send-novel-comment/1/2
        ///     {
        ///         "content": "This is a great novel! I love the characters."
        ///     }
        /// </remarks>
        [HttpPost("send-novel-comment/{userId}/{novelId}")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Adds a comment to a chapter
        /// </summary>
        /// <param name="createChapterCommentDto">Object containing the comment text and details</param>
        /// <param name="userId">The unique identifier of the user creating the comment</param>
        /// <param name="chapterId">The unique identifier of the chapter to comment on</param>
        /// <returns>The newly created comment with its assigned ID</returns>
        /// <response code="200">Returns the created comment</response>
        /// <response code="400">If the comment data is invalid</response>
        /// <response code="404">If the chapter or user is not found</response>
        /// <remarks>
        /// This endpoint is restricted to authenticated users.
        /// New comments are broadcast to connected clients via SignalR for real-time updates.
        /// 
        /// Sample request:
        ///
        ///     POST /api/Comment/send-chapter-comment/1/3
        ///     {
        ///         "content": "This chapter had an amazing plot twist!"
        ///     }
        /// </remarks>
        [HttpPost("send-chapter-comment/{userId}/{chapterId}")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Retrieves all comments for a novel
        /// </summary>
        /// <param name="novelId">The unique identifier of the novel</param>
        /// <returns>List of comments for the specified novel</returns>
        /// <response code="200">Returns the list of comments</response>
        /// <response code="400">If there was an error retrieving the comments</response>
        /// <response code="404">If the novel is not found</response>
        /// <remarks>
        /// Comments are returned in chronological order with the newest comments first.
        /// Each comment includes the user information of the commenter.
        /// </remarks>
        [HttpGet("get-novel-comment/{novelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Retrieves all comments for a chapter
        /// </summary>
        /// <param name="chapterId">The unique identifier of the chapter</param>
        /// <returns>List of comments for the specified chapter</returns>
        /// <response code="200">Returns the list of comments</response>
        /// <response code="400">If there was an error retrieving the comments</response>
        /// <response code="404">If the chapter is not found</response>
        /// <remarks>
        /// Comments are returned in chronological order with the newest comments first.
        /// Each comment includes the user information of the commenter.
        /// </remarks>
        [HttpGet("get-chapter-comment/{chapterId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Gets the number of likes for a novel comment
        /// </summary>
        /// <param name="commentId">The unique identifier of the comment</param>
        /// <returns>Number of likes for the specified comment</returns>
        /// <response code="200">Returns the like count</response>
        /// <response code="400">If there was an error retrieving the like count</response>
        /// <response code="404">If the comment is not found</response>
        [HttpGet("get-novel-comment-like/{commentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Gets the number of likes for a chapter comment
        /// </summary>
        /// <param name="commentId">The unique identifier of the comment</param>
        /// <returns>Number of likes for the specified comment</returns>
        /// <response code="200">Returns the like count</response>
        /// <response code="400">If there was an error retrieving the like count</response>
        /// <response code="404">If the comment is not found</response>
        [HttpGet("get-chapter-comment-like/{commentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Toggles a like on a novel comment
        /// </summary>
        /// <param name="commentId">The unique identifier of the comment</param>
        /// <param name="userId">The unique identifier of the user toggling the like</param>
        /// <returns>Updated like status</returns>
        /// <response code="200">Returns the updated like status</response>
        /// <response code="400">If there was an error toggling the like</response>
        /// <response code="404">If the comment or user is not found</response>
        /// <remarks>
        /// This endpoint acts as a toggle:
        /// - If the user has not liked the comment, it adds a like
        /// - If the user has already liked the comment, it removes the like
        /// </remarks>
        [HttpPost("set-novel-comment-like/{commentId}/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Checks if a user has liked a novel comment
        /// </summary>
        /// <param name="commentId">The unique identifier of the comment</param>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>Boolean indicating whether the user has liked the comment</returns>
        /// <response code="200">Returns true if the user has liked the comment, false otherwise</response>
        /// <response code="400">If there was an error checking the like status</response>
        /// <response code="404">If the comment or user is not found</response>
        [HttpGet("has-user-liked-novel-comment/{commentId}/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Toggles a like on a chapter comment
        /// </summary>
        /// <param name="commentId">The unique identifier of the comment</param>
        /// <param name="userId">The unique identifier of the user toggling the like</param>
        /// <returns>Updated like status</returns>
        /// <response code="200">Returns the updated like status</response>
        /// <response code="400">If there was an error toggling the like</response>
        /// <response code="404">If the comment or user is not found</response>
        /// <remarks>
        /// This endpoint acts as a toggle:
        /// - If the user has not liked the comment, it adds a like
        /// - If the user has already liked the comment, it removes the like
        /// </remarks>
        [HttpPost("set-chapter-comment-like/{commentId}/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        
        /// <summary>
        /// Checks if a user has liked a chapter comment
        /// </summary>
        /// <param name="commentId">The unique identifier of the comment</param>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>Boolean indicating whether the user has liked the comment</returns>
        /// <response code="200">Returns true if the user has liked the comment, false otherwise</response>
        /// <response code="400">If there was an error checking the like status</response>
        /// <response code="404">If the comment or user is not found</response>
        [HttpGet("has-user-liked-chapter-comment/{commentId}/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Deletes a comment from a novel
        /// </summary>
        /// <param name="commentId">The unique identifier of the comment to delete</param>
        /// <param name="novelId">The unique identifier of the novel</param>
        /// <param name="userId">The unique identifier of the user requesting deletion</param>
        /// <returns>Confirmation of successful deletion</returns>
        /// <response code="200">Comment deleted successfully</response>
        /// <response code="404">If the comment is not found</response>
        /// <response code="400">If there was an error deleting the comment</response>
        /// <remarks>
        /// This endpoint is restricted to authenticated users with the "User" or "Admin" role.
        /// Users can only delete their own comments, unless they are administrators or the novel author.
        /// </remarks>
        [HttpDelete("delete-novel-comments/{commentId}/{novelId}/{userId}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Deletes a comment from a chapter
        /// </summary>
        /// <param name="commentId">The unique identifier of the comment to delete</param>
        /// <param name="chapterId">The unique identifier of the chapter</param>
        /// <param name="userId">The unique identifier of the user requesting deletion</param>
        /// <returns>Confirmation of successful deletion</returns>
        /// <response code="200">Comment deleted successfully</response>
        /// <response code="404">If the comment is not found</response>
        /// <response code="400">If there was an error deleting the comment</response>
        /// <remarks>
        /// This endpoint is restricted to authenticated users with the "User" or "Admin" role.
        /// Users can only delete their own comments, unless they are administrators or the novel author.
        /// </remarks>
        [HttpDelete("delete-chapter-comments/{commentId}/{chapterId}/{userId}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
