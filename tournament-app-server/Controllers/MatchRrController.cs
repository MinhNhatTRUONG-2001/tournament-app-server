using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tournament_app_server.DTOs;
using tournament_app_server.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace tournament_app_server.Controllers
{
    [Route("matches/rr")]
    [ApiController]
    public class MatchRrController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public MatchRrController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("all/{stage_id}/{token}")]
        public async Task<ActionResult<IEnumerable<MatchRr>>> GetMatchesByStageId(long stage_id, string token)
        {
            if (_dbContext.MatchRrs == null)
            {
                return NotFound();
            }

            try
            {
                var decodedToken = TokenValidation.ValidateToken(token);
                var payload = decodedToken.Payload;
                int userId = (int)payload["id"];
                var stageUserId = await _dbContext.StageUserIds
                    .Where(su => su.stage_id == stage_id)
                    .FirstAsync();
                if (stageUserId == null)
                {
                    return NotFound();
                }
                else if (stageUserId.user_id != userId)
                {
                    throw new Exception("Cannot access or modify this stage by your token.");
                }

                return await _dbContext.MatchRrs
                    .Where(m => m.stage_id == stage_id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/{token}")]
        public async Task<ActionResult<MatchRr>> GetMatchById(long id, string token)
        {
            if (_dbContext.MatchRrs == null)
            {
                return NotFound();
            }

            try
            {
                var decodedToken = TokenValidation.ValidateToken(token);
                var payload = decodedToken.Payload;
                int userId = (int)payload["id"];
                var matchRrUserId = await _dbContext.MatchRrUserIds
                    .Where(mrru => mrru.match_id == id)
                    .FirstAsync();
                if (matchRrUserId == null)
                {
                    return NotFound();
                }
                else if (matchRrUserId.user_id != userId)
                {
                    throw new Exception("Cannot access or modify this stage by your token.");
                }

                return await _dbContext.MatchRrs.FindAsync(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Not dev yet
        /*[HttpPut("{id}/team_name/{token}")]
        public async Task<ActionResult<MatchRr>> EditTeamName(long id, string token, [FromBody] MatchSeEditTeamNameDTO matchSeEditTeamNameDto)
        {
            if (_dbContext.MatchRrs == null)
            {
                return NotFound();
            }
            try
            {
                var decodedToken = TokenValidation.ValidateToken(token);
                var payload = decodedToken.Payload;
                int userId = (int)payload["id"];
                var matchRrUserId = await _dbContext.MatchRrUserIds
                    .Where(mrru => mrru.match_id != id)
                    .FirstAsync();
                if (matchRrUserId == null)
                {
                    return NotFound();
                }
                else if (matchRrUserId.user_id != userId)
                {
                    throw new Exception("Cannot modify this match by your token.");
                }

                var MatchRr = await _dbContext.MatchRrs.FindAsync(id);
                if (MatchRr == null)
                {
                    return NotFound();
                }

                

                await _dbContext.SaveChangesAsync();
                return new ActionResult<MatchRr>(MatchRr);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }*/

        [HttpPut("{id}/match_info/{token}")]
        public async Task<ActionResult<MatchRr>> EditMatchInfo(long id, string token, [FromBody] MatchEditMatchInfoDTO matchEditMatchInfoDto)
        {
            if (_dbContext.MatchRrs == null)
            {
                return NotFound();
            }
            try
            {
                var decodedToken = TokenValidation.ValidateToken(token);
                var payload = decodedToken.Payload;
                int userId = (int)payload["id"];
                var matchRrUserId = await _dbContext.MatchRrUserIds
                    .Where(mrru => mrru.match_id != id)
                    .FirstAsync();
                if (matchRrUserId == null)
                {
                    return NotFound();
                }
                else if (matchRrUserId.user_id != userId)
                {
                    throw new Exception("Cannot modify this match by your token.");
                }

                var MatchRr = await _dbContext.MatchRrs.FindAsync(id);
                if (MatchRr == null)
                {
                    return NotFound();
                }
                if (matchEditMatchInfoDto.start_datetime != null)
                {
                    MatchRr.start_datetime = DateTimeOffset.Parse(matchEditMatchInfoDto.start_datetime).ToUniversalTime();
                }
                MatchRr.place = matchEditMatchInfoDto.place;
                MatchRr.note = matchEditMatchInfoDto.note;

                await _dbContext.SaveChangesAsync();
                return new ActionResult<MatchRr>(MatchRr);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Not dev yet
        /*[HttpPut("{id}/match_score/{token}")]
        public async Task<ActionResult<MatchRr>> EditMatchScore(long id, string token, [FromBody] MatchSeEditMatchScoreDTO matchSeEditMatchScoreDto)
        {
            if (_dbContext.MatchRrs == null)
            {
                return NotFound();
            }
            try
            {
                var decodedToken = TokenValidation.ValidateToken(token);
                var payload = decodedToken.Payload;
                int userId = (int)payload["id"];
                var matchRrUserId = await _dbContext.MatchRrUserIds
                    .Where(mrru => mrru.match_id != id)
                    .FirstAsync();
                if (matchRrUserId == null)
                {
                    return NotFound();
                }
                else if (matchRrUserId.user_id != userId)
                {
                    throw new Exception("Cannot modify this match by your token.");
                }

                var MatchRr = await _dbContext.MatchRrs.FindAsync(id);
                if (MatchRr == null)
                {
                    return NotFound();
                }
                var stage = await _dbContext.Stages
                    .Where(s => s.id == MatchRr.stage_id)
                    .FirstAsync();
                


                await _dbContext.SaveChangesAsync();
                return new ActionResult<MatchRr>(MatchRr);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }*/
    }
}
