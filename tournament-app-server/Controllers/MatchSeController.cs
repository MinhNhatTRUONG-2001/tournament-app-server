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
        public async Task<ActionResult<IEnumerable<MatchSe>>> GetMatchesByStageId(long stage_id, string token)
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
        public async Task<ActionResult<MatchSe>> GetMatchById(long id, string token)
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
        public async Task<ActionResult<MatchSe>> EditTeamName(long id, string token, [FromBody] MatchSeEditTeamNameDTO matchSeEditTeamNameDto)
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
                    .Select(mse => new { mse.id, mse.team_1, mse.team_2 })
                    .ToListAsync();
                foreach (var pair in teamPairs)
                {
                    if (matchSeEditTeamNameDto.team_1 != null)
                    {
                        if ((matchSeEditTeamNameDto.team_1 == pair.team_1 && id != pair.id) || (matchSeEditTeamNameDto.team_1 == pair.team_2 && id == pair.id))
                        {
                            throw new Exception("Team name(s) has already been in this stage");
                        }
                    }
                    if (matchSeEditTeamNameDto.team_2 != null)
                    {
                        if ((matchSeEditTeamNameDto.team_2 == pair.team_2 && id != pair.id) || (matchSeEditTeamNameDto.team_2 == pair.team_1 && id == pair.id))
                        {
                            throw new Exception("Team name(s) has already been in this stage");
                        }
                    }
                }

                //Change team name
                string old_team_1_name = matchSe.team_1;
                string old_team_2_name = matchSe.team_2;
                if (matchSeEditTeamNameDto.team_1 != null && matchSeEditTeamNameDto.team_1 != matchSe.team_1)
                {
                    matchSe.team_1 = matchSeEditTeamNameDto.team_1;
                    if (old_team_1_name != null)
                    {
                        var otherMatchSe1 = await _dbContext.MatchSes
                        .Where(mse => mse.stage_id == matchSe.stage_id && mse.team_1 == old_team_1_name)
                        .ToListAsync();
                        foreach (var m in otherMatchSe1)
                        {
                            m.team_1 = matchSeEditTeamNameDto.team_1;
                        }
                        var otherMatchSe2 = await _dbContext.MatchSes
                            .Where(mse => mse.stage_id == matchSe.stage_id && mse.team_2 == old_team_1_name)
                            .ToListAsync();
                        foreach (var m in otherMatchSe2)
                        {
                            m.team_2 = matchSeEditTeamNameDto.team_1;
                        }
                        var otherMatchSeWinner = await _dbContext.MatchSes
                            .Where(mse => mse.stage_id == matchSe.stage_id && mse.winner == old_team_1_name)
                            .ToListAsync();
                        foreach (var m in otherMatchSeWinner)
                        {
                            m.winner = matchSeEditTeamNameDto.team_1;
                        }
                    }
                }
                if (matchSeEditTeamNameDto.team_2 != null && matchSeEditTeamNameDto.team_2 != matchSe.team_2)
                {
                    matchSe.team_2 = matchSeEditTeamNameDto.team_2;
                    if (old_team_2_name != null)
                    {
                        var otherMatchSe1 = await _dbContext.MatchSes
                        .Where(mse => mse.stage_id == matchSe.stage_id && mse.team_1 == old_team_2_name)
                        .ToListAsync();
                        foreach (var m in otherMatchSe1)
                        {
                            m.team_1 = matchSeEditTeamNameDto.team_2;
                        }
                        var otherMatchSe2 = await _dbContext.MatchSes
                            .Where(mse => mse.stage_id == matchSe.stage_id && mse.team_2 == old_team_2_name)
                            .ToListAsync();
                        foreach (var m in otherMatchSe2)
                        {
                            m.team_2 = matchSeEditTeamNameDto.team_2;
                        }
                        var otherMatchSeWinner = await _dbContext.MatchSes
                            .Where(mse => mse.stage_id == matchSe.stage_id && mse.winner == old_team_2_name)
                            .ToListAsync();
                        foreach (var m in otherMatchSeWinner)
                        {
                            m.winner = matchSeEditTeamNameDto.team_2;
                        }
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
        public async Task<ActionResult<MatchSe>> EditMatchInfo(long id, string token, [FromBody] MatchSeEditMatchInfoDTO matchSeEditMatchInfoDto)
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
                if (matchSeEditMatchInfoDto.start_datetime != null)
                {
                    matchSe.start_datetime = DateTimeOffset.Parse(matchSeEditMatchInfoDto.start_datetime).ToUniversalTime();
                }
                matchSe.place = matchSeEditMatchInfoDto.place;
                matchSe.note = matchSeEditMatchInfoDto.note;

                await _dbContext.SaveChangesAsync();
                return new ActionResult<MatchSe>(matchSe);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/match_score/{token}")]
        public async Task<ActionResult<MatchSe>> EditMatchScore(long id, string token, [FromBody] MatchSeEditMatchScoreDTO matchSeEditMatchScoreDto)
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
                //Verify team names
                if (matchSe.team_1 == null || matchSe.team_2 == null)
                {
                    throw new Exception("Team names must be defined before updating scores.");
                }
                //Verify winner name
                if (matchSeEditMatchScoreDto.winner != matchSe.team_1 && matchSeEditMatchScoreDto.winner != matchSe.team_2)
                {
                    throw new Exception("Invalid winner.");
                }
                //Verify scores: number of legs, best of
                short matchSeNumberOfLegs = matchSe.number_of_legs;
                short matchSeBestOf = matchSe.best_of;
                if (matchSeEditMatchScoreDto.team_1_scores.Length != matchSeNumberOfLegs || matchSeEditMatchScoreDto.team_2_scores.Length != matchSeNumberOfLegs)
                {
                    throw new Exception("Scores provided mismatch number of legs in this round.");
                }
                if (matchSeEditMatchScoreDto.team_1_subscores != null && matchSeEditMatchScoreDto.team_2_subscores != null)
                {
                    if ((short)matchSeEditMatchScoreDto.team_1_subscores.Length < matchSeNumberOfLegs * matchSeBestOf || (short)matchSeEditMatchScoreDto.team_2_subscores.Length != matchSeNumberOfLegs * matchSeBestOf)
                    {
                        throw new Exception("Subscores provided mismatch number of legs in this round.");
                    }
                    if ((short)matchSeEditMatchScoreDto.team_1_subscores.Count() / matchSeBestOf != matchSeNumberOfLegs || (short)matchSeEditMatchScoreDto.team_2_subscores.Count() / matchSeBestOf != matchSeNumberOfLegs)
                    {
                        throw new Exception("Subscores provided mismatch scores provided.");
                    }
                    for (int i = 0; i < matchSeNumberOfLegs; i++)
                    {
                        if (matchSeBestOf > 0)
                        {
                            if (matchSeEditMatchScoreDto.team_1_scores[i] + matchSeEditMatchScoreDto.team_2_scores[i] > matchSeBestOf
                                || matchSeEditMatchScoreDto.team_1_scores[i] > Math.Floor((decimal)matchSeBestOf / 2) + 1
                                || matchSeEditMatchScoreDto.team_2_scores[i] > Math.Floor((decimal)matchSeBestOf / 2) + 1)
                            {
                                throw new Exception("Scores provided mismatch best of in this round.");
                            }
                        }
                    }
                }
                string oldWinner = matchSe.winner;
                string loser;
                matchSe.winner = matchSeEditMatchScoreDto.winner;
                if (matchSe.winner == matchSe.team_1)
                {
                    loser = matchSe.team_2;
                }
                else
                {
                    loser = matchSe.team_1;
                }
                matchSe.team_1_scores = matchSeEditMatchScoreDto.team_1_scores;
                matchSe.team_2_scores = matchSeEditMatchScoreDto.team_2_scores;
                matchSe.team_1_subscores = matchSeEditMatchScoreDto.team_1_subscores;
                matchSe.team_2_subscores = matchSeEditMatchScoreDto.team_2_subscores;
                await _dbContext.SaveChangesAsync();
                //Update match in round+1: change winner name, clear score
                string nextOldWinner = oldWinner;
                if (matchSe.round_number < (short)stage.number_of_legs_per_round.Length)
                {
                    short nextMatchNumber = (short)((matchSe.match_number + 1) / 2);
                    var nextMatchSe = await _dbContext.MatchSes
                        .Where(mse => mse.stage_id == matchSe.stage_id && mse.group_number == matchSe.group_number && mse.round_number == matchSe.round_number + 1 && mse.match_number == nextMatchNumber)
                        .FirstAsync();
                    if (matchSe.match_number % 2 != 0)
                    {
                        nextMatchSe.team_1 = matchSe.winner;
                    }
                    else
                    {
                        nextMatchSe.team_2 = matchSe.winner;
                    }
                    if (matchSe.winner != oldWinner)
                    {
                        nextOldWinner = nextMatchSe.winner;
                        nextMatchSe.winner = null;
                        for (int i = 0; i < nextMatchSe.team_1_scores.Length; i++)
                        {
                            nextMatchSe.team_1_scores[i] = 0;
                        }
                        for (int i = 0; i < nextMatchSe.team_2_scores.Length; i++)
                        {
                            nextMatchSe.team_2_scores[i] = 0;
                        }
                        for (int i = 0; i < nextMatchSe.team_1_subscores.Length; i++)
                        {
                            nextMatchSe.team_1_subscores[i] = 0;
                        }
                        for (int i = 0; i < nextMatchSe.team_2_subscores.Length; i++)
                        {
                            nextMatchSe.team_2_subscores[i] = 0;
                        }
                    }
                    if (stage.include_third_place_match)
                    {
                        if (nextMatchSe.round_number == (short)stage.number_of_legs_per_round.Length)
                        {
                            var thirdPlaceMatchSe = await _dbContext.MatchSes
                                .Where(mse => mse.stage_id == matchSe.stage_id && mse.group_number == matchSe.group_number && mse.round_number == nextMatchSe.round_number + 1)
                                .FirstAsync();
                            if (loser == matchSe.team_2)
                            {
                                if (matchSe.match_number % 2 != 0)
                                {
                                    thirdPlaceMatchSe.team_1 = matchSe.team_2;
                                }
                                else
                                {
                                    thirdPlaceMatchSe.team_2 = matchSe.team_2;
                                }
                            }
                            else
                            {
                                if (matchSe.match_number % 2 != 0)
                                {
                                    thirdPlaceMatchSe.team_1 = matchSe.team_1;
                                }
                                else
                                {
                                    thirdPlaceMatchSe.team_2 = matchSe.team_1;
                                }
                            }
                            if (matchSe.winner != oldWinner)
                            {
                                thirdPlaceMatchSe.winner = null;
                                for (int i = 0; i < thirdPlaceMatchSe.team_1_scores.Length; i++)
                                {
                                    thirdPlaceMatchSe.team_1_scores[i] = 0;
                                }
                                for (int i = 0; i < thirdPlaceMatchSe.team_2_scores.Length; i++)
                                {
                                    thirdPlaceMatchSe.team_2_scores[i] = 0;
                                }
                                for (int i = 0; i < thirdPlaceMatchSe.team_1_subscores.Length; i++)
                                {
                                    thirdPlaceMatchSe.team_1_subscores[i] = 0;
                                }
                                for (int i = 0; i < thirdPlaceMatchSe.team_2_subscores.Length; i++)
                                {
                                    thirdPlaceMatchSe.team_2_subscores[i] = 0;
                                }
                            }
                        }
                    }
                }
                await _dbContext.SaveChangesAsync();
                //Reset depenedent matches in round+2,+3,...: delete winner name, clear score
                if (matchSe.winner != oldWinner)
                {
                    for (int i = matchSe.round_number + 2; i <= stage.number_of_legs_per_round.Length; i++)
                    {
                        var resetMatchSe = await _dbContext.MatchSes
                            .Where(mse => mse.stage_id == matchSe.stage_id && mse.group_number == matchSe.group_number && mse.round_number == i && (mse.team_1 == nextOldWinner || mse.team_2 == nextOldWinner))
                            .FirstAsync();
                        if (resetMatchSe.team_1 == nextOldWinner)
                        {
                            resetMatchSe.team_1 = null;
                        }
                        else if (resetMatchSe.team_2 == nextOldWinner)
                        {
                            resetMatchSe.team_2 = null;
                        }
                        nextOldWinner = resetMatchSe.winner;
                        resetMatchSe.winner = null;
                        for (int j = 0; j < resetMatchSe.team_1_scores.Length; j++)
                        {
                            resetMatchSe.team_1_scores[j] = 0;
                        }
                        for (int j = 0; j < resetMatchSe.team_2_scores.Length; j++)
                        {
                            resetMatchSe.team_2_scores[j] = 0;
                        }
                        for (int j = 0; j < resetMatchSe.team_1_subscores.Length; j++)
                        {
                            resetMatchSe.team_1_subscores[j] = 0;
                        }
                        for (int j = 0; j < resetMatchSe.team_2_subscores.Length; j++)
                        {
                            resetMatchSe.team_2_subscores[j] = 0;
                        }
                    }
                    await _dbContext.SaveChangesAsync();
                    if (stage.include_third_place_match)
                    {
                        var resetThirdPlaceMatchSe = await _dbContext.MatchSes
                            .Where(mse => mse.stage_id == matchSe.stage_id && mse.group_number == matchSe.group_number && mse.round_number == (short)(stage.number_of_legs_per_round.Length + 1))
                            .FirstAsync();
                        if (resetThirdPlaceMatchSe.id != matchSe.id && matchSe.round_number + 2 < resetThirdPlaceMatchSe.round_number)
                        {
                            var semiFinalMatchSes = await _dbContext.MatchSes
                                .Where(mse => mse.stage_id == matchSe.stage_id && mse.group_number == matchSe.group_number && mse.round_number == resetThirdPlaceMatchSe.round_number - 2)
                                .ToListAsync();
                            for (int i = 1; i <= semiFinalMatchSes.Count; i++)
                            { 
                                if (semiFinalMatchSes[i - 1].match_number == 1 && semiFinalMatchSes[i - 1].winner == null)
                                {
                                    resetThirdPlaceMatchSe.team_1 = null;
                                }
                                else if (semiFinalMatchSes[i - 1].match_number == 2 && semiFinalMatchSes[i - 1].winner == null)
                                {
                                    resetThirdPlaceMatchSe.team_2 = null;
                                }
                            }
                            resetThirdPlaceMatchSe.winner = null;
                            for (int i = 0; i < resetThirdPlaceMatchSe.team_1_scores.Length; i++)
                            {
                                resetThirdPlaceMatchSe.team_1_scores[i] = 0;
                            }
                            for (int i = 0; i < resetThirdPlaceMatchSe.team_2_scores.Length; i++)
                            {
                                resetThirdPlaceMatchSe.team_2_scores[i] = 0;
                            }
                            for (int i = 0; i < resetThirdPlaceMatchSe.team_1_subscores.Length; i++)
                            {
                                resetThirdPlaceMatchSe.team_1_subscores[i] = 0;
                            }
                            for (int i = 0; i < resetThirdPlaceMatchSe.team_2_subscores.Length; i++)
                            {
                                resetThirdPlaceMatchSe.team_2_subscores[i] = 0;
                            }
                        }
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
    }
}
