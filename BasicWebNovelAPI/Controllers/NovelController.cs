using BasicWebNovelAPI.Model.Dto.Novel;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Service.Abstractions;
using BasicWebNovelAPI.Service.Implementations;
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
            if (novels == null || !novels.Any())
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

        
        
    }
}
