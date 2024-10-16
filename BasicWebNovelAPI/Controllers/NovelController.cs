using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Model.Dto.Novel;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Service.Abstractions;
using BasicWebNovelAPI.Service.Implementations;
using Microsoft.AspNetCore.Authorization;
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
            try 
            {
                var novels = await _novelRepository.GetNovels();
                if (novels == null || !novels.Any())
                {
                    return NotFound("Novel not found.");
                }
                return Ok(novels);
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

        
        [HttpGet("get-novel/{id}")]
        public async Task<IActionResult> GetNovelById(int id)
        {
            try 
            {
                var novel = await _novelRepository.GetNovelById(id);
                if (novel == null)
                {
                    return NotFound("Novel not found.");
                }
                return Ok(novel);
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

        [HttpGet("get-novel-by-name")]
        public async Task<IActionResult> GetNovelByName(string name)
        {
            try 
            {
                var novels = await _novelRepository.GetNovelByName(name);
                if (novels.Count <= 0)
                {
                    return NotFound("Novel not found.");
                }

                return Ok(novels);
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

        [HttpGet("get-all-user-novel/{id}")]
        public async Task<IActionResult> GetAllUserNovel(int id)
        {
            try 
            {

                var novels = await _novelRepository.GetUserAllNovel(id);
                if (novels == null)
                {
                    return NotFound("Novel not found.");
                }

                return Ok(novels);
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

        
        [HttpPost("create-novel/{userId}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateNovel([FromBody] CreateNovelDto createNovelDto, int userId)
        {
            try 
            {
                var createdNovel = await _novelRepository.CreateNovel(createNovelDto, userId);
                return Ok(createdNovel);
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

        

        

        [HttpPut("update-novel/{novelId}/{userId}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateNovel(int novelId, int userId, [FromBody] UpdateNovelDto updateNovelDto)
        {
            try 
            {
                bool isUpdated = await _novelRepository.UpdateNovel(novelId, userId, updateNovelDto);
                if (!isUpdated)
                {
                    return NotFound("Novel not found.");
                }

                return Ok("Novel updated successfully.");
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


        [HttpDelete("delete-novel/{novelId}/{userId}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> DeleteNovel(int novelId, int userId)
        {
            try 
            {
                bool isDeleted = await _novelRepository.DeleteNovel(novelId, userId);
                if (!isDeleted)
                {
                    return NotFound("Novel not found.");
                }

                return Ok(new { Message = "Novel deleted successfully." });
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
