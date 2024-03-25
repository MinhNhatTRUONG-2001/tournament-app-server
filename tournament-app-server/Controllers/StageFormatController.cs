using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tournament_app_server.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace tournament_app_server.Controllers
{
    [Route("stage_format")]
    [ApiController]
    public class StageFormatController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public StageFormatController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StageFormat>>> GetStageFormats()
        {
            if (_dbContext.Tournaments == null)
            {
                return NotFound();
            }

            try
            {
                return await _dbContext.StageFormats.ToListAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StageFormat>> GetStageFormatById(long id)
        {
            if (_dbContext.Tournaments == null)
            {
                return NotFound();
            }

            try
            {
                return await _dbContext.StageFormats.FindAsync(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
