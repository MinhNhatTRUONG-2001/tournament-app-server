using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tournament_app_server.DTOs;
using tournament_app_server.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace tournament_app_server.Controllers
{
    [Route("/matches/se")]
    [ApiController]
    public class MatchSeController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public MatchSeController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("all/{stage_id}/{token}")]
        public async Task<ActionResult<IEnumerable<MatchSe>>> GetMatchesByStageId(int stage_id, string token)
        {
            if (_dbContext.MatchSes == null)
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

                return await _dbContext.MatchSes
                    .Where(m => m.stage_id == stage_id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/{token}")]
        public async Task<ActionResult<MatchSe>> GetMatchById(int id, string token)
        {
            if (_dbContext.MatchSes == null)
            {
                return NotFound();
            }

            try
            {
                var decodedToken = TokenValidation.ValidateToken(token);
                var payload = decodedToken.Payload;
                int userId = (int)payload["id"];
                var matchSeUserId = await _dbContext.MatchSeUserIds
                    .Where(mseu => mseu.match_id == id)
                    .FirstAsync();
                if (matchSeUserId == null)
                {
                    return NotFound();
                }
                else if (matchSeUserId.user_id != userId)
                {
                    throw new Exception("Cannot access or modify this stage by your token.");
                }

                return await _dbContext.MatchSes.FindAsync(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/team_name/{token}")]
        public async Task<ActionResult<MatchSe>> EditTeamName(int id, string token, [FromBody] MatchSeDTO matchSeDto)
        {
            if (_dbContext.MatchSes == null)
            {
                return NotFound();
            }
            try
            {
                var decodedToken = TokenValidation.ValidateToken(token);
                var payload = decodedToken.Payload;
                int userId = (int)payload["id"];
                var matchSeUserId = await _dbContext.MatchSeUserIds
                    .Where(mseu => mseu.match_id != id)
                    .FirstAsync();
                if (matchSeUserId == null)
                {
                    return NotFound();
                }
                else if (matchSeUserId.user_id != userId)
                {
                    throw new Exception("Cannot modify this match by your token.");
                }
                
                var matchSe = await _dbContext.MatchSes.FindAsync(id);
                if (matchSe == null)
                {
                    return NotFound();
                }
                if (matchSe.round_number != 1) //Team names can only be edited in round 1
                {
                    throw new Exception("Invalid round_number");
                }

                //Check distinct team names in a stage
                var teamPairs = await _dbContext.MatchSes
                    .Where(mse => mse.stage_id == matchSe.stage_id && mse.round_number == 1)
                    .Select(mse => new { mse.team_1, mse.team_2 })
                    .ToListAsync();
                List<string> teamNames = new List<string>();
                foreach (var pair in teamPairs)
                {
                    teamNames.Add(pair.team_1);
                    teamNames.Add(pair.team_2);
                }
                if (matchSeDto.team_1 != null && teamNames.Contains(matchSeDto.team_1) 
                    || matchSeDto.team_2 != null && teamNames.Contains(matchSeDto.team_2))
                {
                    throw new Exception("Team name has already been in this stage");
                }
                
                //Change team name
                string old_team_1_name = matchSe.team_1;
                string old_team_2_name = matchSe.team_2;
                if (matchSeDto.team_1 != null && matchSeDto.team_1 != matchSe.team_1)
                {
                    matchSe.team_1 = matchSeDto.team_1;
                    var otherMatchSe1 = await _dbContext.MatchSes
                        .Where(mse => mse.stage_id == matchSe.stage_id && mse.team_1 == old_team_1_name)
                        .ToListAsync();
                    foreach (var m in otherMatchSe1)
                    {
                        m.team_1 = matchSeDto.team_1;
                    }
                    var otherMatchSe2 = await _dbContext.MatchSes
                        .Where(mse => mse.stage_id == matchSe.stage_id && mse.team_2 == old_team_1_name)
                        .ToListAsync();
                    foreach (var m in otherMatchSe2)
                    {
                        m.team_2 = matchSeDto.team_1;
                    }
                }
                if (matchSeDto.team_2 != null && matchSeDto.team_2 != matchSe.team_2)
                {
                    matchSe.team_2 = matchSeDto.team_2;
                    var otherMatchSe1 = await _dbContext.MatchSes
                        .Where(mse => mse.stage_id == matchSe.stage_id && mse.team_1 == old_team_2_name)
                        .ToListAsync();
                    foreach (var m in otherMatchSe1)
                    {
                        m.team_1 = matchSeDto.team_2;
                    }
                    var otherMatchSe2 = await _dbContext.MatchSes
                        .Where(mse => mse.stage_id == matchSe.stage_id && mse.team_2 == old_team_2_name)
                        .ToListAsync();
                    foreach (var m in otherMatchSe2)
                    {
                        m.team_2 = matchSeDto.team_2;
                    }
                }
                await _dbContext.SaveChangesAsync();
                return new ActionResult<MatchSe>(matchSe);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/match_info/{token}")]
        public async Task<ActionResult<MatchSe>> EditMatchInfo(int id, string token, [FromBody] MatchSeDTO matchSeDto)
        {
            if (_dbContext.MatchSes == null)
            {
                return NotFound();
            }
            try
            {
                var decodedToken = TokenValidation.ValidateToken(token);
                var payload = decodedToken.Payload;
                int userId = (int)payload["id"];
                var matchSeUserId = await _dbContext.MatchSeUserIds
                    .Where(mseu => mseu.match_id != id)
                    .FirstAsync();
                if (matchSeUserId == null)
                {
                    return NotFound();
                }
                else if (matchSeUserId.user_id != userId)
                {
                    throw new Exception("Cannot modify this match by your token.");
                }

                var matchSe = await _dbContext.MatchSes.FindAsync(id);
                if (matchSe == null)
                {
                    return NotFound();
                }
                if (matchSeDto.start_datetime != null)
                {
                    matchSe.start_datetime = DateTimeOffset.Parse(matchSeDto.start_datetime).ToUniversalTime();
                }
                matchSe.place = matchSeDto.place;
                matchSe.note = matchSeDto.note;

                return new ActionResult<MatchSe>(matchSe);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/match_score/{token}")]
        public async Task<ActionResult<MatchSe>> EditMatchScore(int id, string token, [FromBody] MatchSeDTO matchSeDto)
        {
            if (_dbContext.MatchSes == null)
            {
                return NotFound();
            }
            try
            {
                var decodedToken = TokenValidation.ValidateToken(token);
                var payload = decodedToken.Payload;
                int userId = (int)payload["id"];
                var matchSeUserId = await _dbContext.MatchSeUserIds
                    .Where(mseu => mseu.match_id != id)
                    .FirstAsync();
                if (matchSeUserId == null)
                {
                    return NotFound();
                }
                else if (matchSeUserId.user_id != userId)
                {
                    throw new Exception("Cannot modify this match by your token.");
                }

                var matchSe = await _dbContext.MatchSes.FindAsync(id);
                if (matchSe == null)
                {
                    return NotFound();
                }
                var stage = await _dbContext.Stages
                    .Where(s => s.id == matchSe.stage_id)
                    .FirstAsync();

                //Verify scores: number of legs, best of
                int matchSeNumberOfLegs, matchSeBestOf;
                if (matchSe.round_number <= stage.number_of_legs_per_round.Length)
                {
                    matchSeNumberOfLegs = stage.number_of_legs_per_round[matchSe.round_number - 1];
                    matchSeBestOf = stage.best_of_per_round[matchSe.round_number - 1];
                }
                else
                {
                    matchSeNumberOfLegs = (int)stage.third_place_match_number_of_legs;
                    matchSeBestOf = (int)stage.third_place_match_best_of;
                }
                if (matchSeDto.team_1_score.Length != matchSeNumberOfLegs || matchSeDto.team_2_score.Length != matchSeNumberOfLegs
                    || matchSeDto.team_1_subscores.Length != matchSeNumberOfLegs || matchSeDto.team_2_subscores.Length != matchSeNumberOfLegs)
                {
                    throw new Exception("Scores provided mismatch number of legs in this round.");
                }
                for (int i = 0; i < matchSeNumberOfLegs; i++)
                {
                    if (matchSeDto.team_1_subscores[i].LongLength != matchSeDto.team_1_score[i]
                        || matchSeDto.team_2_subscores[i].LongLength != matchSeDto.team_2_score[i])
                    {
                        throw new Exception("Subscores provided mismatch scores provided.");
                    }
                    if (matchSeBestOf > 0)
                    {
                        if (matchSeDto.team_1_score[i] + matchSeDto.team_2_score[i] > matchSeBestOf
                            || matchSeDto.team_1_score[i] > Math.Floor((decimal)matchSeBestOf / 2) + 1
                            || matchSeDto.team_2_score[i] > Math.Floor((decimal)matchSeBestOf / 2) + 1)
                        {
                            throw new Exception("Subscores provided mismatch best of in this round.");
                        }
                    }
                }

                string old_winner = matchSe.winner;
                matchSe.team_1 = matchSeDto.team_1;
                matchSe.team_2 = matchSeDto.team_2;
                matchSe.winner = matchSeDto.winner;
                matchSe.team_1_score = matchSeDto.team_1_score;
                matchSe.team_2_score = matchSeDto.team_2_score;
                matchSe.team_1_subscores = matchSeDto.team_1_subscores;
                matchSe.team_2_subscores = matchSeDto.team_2_subscores;
                await _dbContext.SaveChangesAsync();
                //Update match in round+1: change winner name, clear score
                short nextMatchNumber = (short)((matchSe.match_number + 1) / 2);
                var nextMatchSe = await _dbContext.MatchSes
                    .Where(mse => mse.stage_id == matchSe.stage_id && mse.round_number == matchSe.round_number + 1 && mse.match_number == nextMatchNumber)
                    .FirstAsync();
                if (matchSe.match_number % 2 != 0)
                {
                    nextMatchSe.team_1 = matchSe.winner;
                }
                else
                {
                    nextMatchSe.team_2 = matchSe.winner;
                }
                if (matchSe.winner != old_winner)
                {
                    nextMatchSe.winner = null;
                    nextMatchSe.team_1_score = null;
                    nextMatchSe.team_2_score = null;
                    nextMatchSe.team_1_subscores = null;
                    nextMatchSe.team_2_subscores = null;
                }
                //Reset depenedent matches in round+2,+3,...: delete winner name, clear score
                if (matchSe.winner != old_winner)
                {
                    for (int i = matchSe.round_number + 2; i <= stage.number_of_legs_per_round.Length + 1; i++)
                    {
                        var resetMatchSe = await _dbContext.MatchSes
                            .Where(mse => mse.team_1 == old_winner || mse.team_2 == old_winner)
                            .FirstAsync();
                        if (resetMatchSe.team_1 == old_winner)
                        {
                            resetMatchSe.team_1 = null;
                        }
                        else if (resetMatchSe.team_2 == old_winner)
                        {
                            resetMatchSe.team_2 = null;
                        }
                        resetMatchSe.winner = null;
                        resetMatchSe.team_1_score = null;
                        resetMatchSe.team_2_score = null;
                        resetMatchSe.team_1_subscores = null;
                        resetMatchSe.team_2_subscores = null;
                    }
                }
                return new ActionResult<MatchSe>(matchSe);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
