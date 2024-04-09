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

        [HttpGet("all/{stage_id}/{token?}")]
        public async Task<ActionResult<IEnumerable<MatchRr>>> GetMatchesByStageId(long stage_id, string token = "")
        {
            if (_dbContext.MatchRrs == null)
            {
                return NotFound();
            }

            try
            {
                var stage = await _dbContext.Stages.FindAsync(stage_id);
                if (stage == null)
                {
                    return NotFound();
                }
                var tournament = await _dbContext.Tournaments.FindAsync(stage.tournament_id);
                if (tournament == null)
                {
                    return NotFound();
                }

                if (token == "")
                {
                    if (tournament.is_private == true)
                    {
                        throw new Exception("Cannot access or modify these matches without a valid token.");
                    }
                }
                else
                {
                    var decodedToken = TokenValidation.ValidateToken(token);
                    var payload = decodedToken.Payload;
                    int userId = (int)payload["id"];

                    if (tournament.user_id != userId && tournament.is_private == true)
                    {
                        throw new Exception("Cannot access or modify this match by your token.");
                    }
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

        [HttpGet("{id}/{token?}")]
        public async Task<ActionResult<MatchRr>> GetMatchById(long id, string token = "")
        {
            if (_dbContext.MatchRrs == null)
            {
                return NotFound();
            }

            try
            {
                var matchRr = await _dbContext.MatchRrs.FindAsync(id);
                if (matchRr == null)
                {
                    return NotFound();
                }
                var stage = await _dbContext.Stages.FindAsync(matchRr.stage_id);
                if (stage == null)
                {
                    return NotFound();
                }
                var tournament = await _dbContext.Tournaments.FindAsync(stage.tournament_id);
                if (tournament == null)
                {
                    return NotFound();
                }

                if (token == "")
                {
                    if (tournament.is_private == true)
                    {
                        throw new Exception("Cannot access or modify these matches without a valid token.");
                    }
                }
                else
                {
                    var decodedToken = TokenValidation.ValidateToken(token);
                    var payload = decodedToken.Payload;
                    int userId = (int)payload["id"];

                    if (tournament.user_id != userId && tournament.is_private == true)
                    {
                        throw new Exception("Cannot access or modify this match by your token.");
                    }
                }

                return matchRr;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Not dev yet
        /*[HttpGet("table_results/{stage_id}/{token?}")]
        public async Task<ActionResult<IEnumerable<MatchRrTableResult>>> GetTableResultsByStageId(long stage_id, string token = "")
        {
            if (_dbContext.MatchRrs == null)
            {
                return NotFound();
            }

            try
            {
                if (token == "")
                {
                    var stage = await _dbContext.Stages.FindAsync(stage_id);
                    if (stage == null)
                    {
                        return NotFound();
                    }
                    var tournament = await _dbContext.Tournaments.FindAsync(stage.tournament_id);
                    if (tournament == null)
                    {
                        return NotFound();
                    }
                    else if (tournament.is_private == true)
                    {
                        throw new Exception("Cannot access table results without a valid token.");
                    }

                    return new List<MatchRrTableResult>();
                }
                else
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

                    return new List<MatchRrTableResult>();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }*/

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
                var matchRr = await _dbContext.MatchRrs.FindAsync(id);
                if (matchRr == null)
                {
                    return NotFound();
                }
                var stage = await _dbContext.Stages.FindAsync(matchRr.stage_id);
                if (stage == null)
                {
                    return NotFound();
                }
                var tournament = await _dbContext.Tournaments.FindAsync(stage.tournament_id);
                if (tournament == null)
                {
                    return NotFound();
                }
                else if (tournament.user_id != userId)
                {
                    throw new Exception("Cannot modify this match by your token.");
                }

                if (matchEditMatchInfoDto.start_datetime != null)
                {
                    matchRr.start_datetime = DateTimeOffset.Parse(matchEditMatchInfoDto.start_datetime).ToUniversalTime();
                }
                matchRr.place = matchEditMatchInfoDto.place;
                matchRr.note = matchEditMatchInfoDto.note;

                await _dbContext.SaveChangesAsync();
                return new ActionResult<MatchRr>(matchRr);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/match_score/{token}")]
        public async Task<ActionResult<MatchRr>> EditMatchScore(long id, string token, [FromBody] MatchRrEditMatchScoreDTO matchRrEditMatchScoreDto)
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
                var matchRr = await _dbContext.MatchRrs.FindAsync(id);
                if (matchRr == null)
                {
                    return NotFound();
                }
                var stage = await _dbContext.Stages.FindAsync(matchRr.stage_id);
                if (stage == null)
                {
                    return NotFound();
                }
                var tournament = await _dbContext.Tournaments.FindAsync(stage.tournament_id);
                if (tournament == null)
                {
                    return NotFound();
                }
                else if (tournament.user_id != userId)
                {
                    throw new Exception("Cannot modify this match by your token.");
                }

                matchRr.winner = matchRrEditMatchScoreDto.winner;
                matchRr.team_1_score = matchRrEditMatchScoreDto.team_1_score;
                matchRr.team_2_score = matchRrEditMatchScoreDto.team_2_score;
                matchRr.team_1_subscores = matchRrEditMatchScoreDto.team_1_subscores;
                matchRr.team_2_subscores = matchRrEditMatchScoreDto.team_2_subscores;

                await _dbContext.SaveChangesAsync();
                return new ActionResult<MatchRr>(matchRr);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
