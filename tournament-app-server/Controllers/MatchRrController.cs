using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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

        
        [HttpGet("table_results/{stage_id}/{token?}")]
        public async Task<ActionResult<IEnumerable<MatchRrTableResult>>> GetTableResultsByStageId(long stage_id, string token = "")
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
                        throw new Exception("Cannot access table results without a valid token.");
                    }
                }
                else
                {
                    var decodedToken = TokenValidation.ValidateToken(token);
                    var payload = decodedToken.Payload;
                    int userId = (int)payload["id"];
                    
                    if (tournament.user_id != userId && tournament.is_private == true)
                    {
                        throw new Exception("Cannot access the table results by your token.");
                    }
                }

                List<MatchRrTableResult> tableResults = new List<MatchRrTableResult>();
                for (int i = 1; i <= stage.number_of_groups; i++)
                {
                    var groupMatchRrs = await _dbContext.MatchRrs
                        .Where(mrr => mrr.stage_id == stage_id && mrr.group_number == i)
                        .ToListAsync();
                    var distinctTeamNames = groupMatchRrs
                        .Select(mrr => new List<string> { mrr.team_1, mrr.team_2 })
                        .SelectMany(x => x)
                        .Distinct()
                        .ToList();
                    //Calculate total points, difference and accumulated/earned score for each team
                    for (int j = 0; j < distinctTeamNames.Count; j++)
                    {
                        var teamMatchRrs = groupMatchRrs
                            .Where(mrr => mrr.team_1 == distinctTeamNames[j] || mrr.team_2 == distinctTeamNames[j])
                            .ToList();
                        //Calculate total points
                        long totalPoints = 0;
                        foreach (var mrr in teamMatchRrs)
                        {
                            if (mrr.team_1 == distinctTeamNames[j])
                            {
                                if (mrr.team_1_score > mrr.team_2_score)
                                {
                                    totalPoints += stage.win_point;
                                }
                                else if (mrr.team_1_score < mrr.team_2_score)
                                {
                                    totalPoints += stage.lose_point;
                                }
                                else
                                {
                                    totalPoints += stage.draw_point;
                                }
                            }
                            else if (mrr.team_2 == distinctTeamNames[j])
                            {
                                if (mrr.team_2_score > mrr.team_1_score)
                                {
                                    totalPoints += stage.win_point;
                                }
                                else if (mrr.team_2_score < mrr.team_1_score)
                                {
                                    totalPoints += stage.lose_point;
                                }
                                else
                                {
                                    totalPoints += stage.draw_point;
                                }
                            }
                        }
                        //Calculate difference
                        long? difference = 0;
                        foreach (var mrr in teamMatchRrs)
                        {
                            if (mrr.team_1 == distinctTeamNames[j])
                            {
                                difference += mrr.team_1_score - mrr.team_2_score;
                            }
                            else if (mrr.team_2 == distinctTeamNames[j])
                            {
                                difference += mrr.team_2_score - mrr.team_1_score;
                            }
                        }
                        //Calculate accumulated/earned score
                        long? accumulatedScore = 0;
                        foreach (var mrr in teamMatchRrs)
                        {
                            if (mrr.team_1 == distinctTeamNames[j])
                            {
                                accumulatedScore += mrr.team_1_score;
                            }
                            else if (mrr.team_2 == distinctTeamNames[j])
                            {
                                accumulatedScore += mrr.team_2_score;
                            }
                        }

                        tableResults.Add(new MatchRrTableResult { 
                            name = distinctTeamNames[j],
                            points = totalPoints,
                            difference = (long)difference,
                            accumulated_score = (long)accumulatedScore
                        });
                    }
                }
                tableResults = tableResults
                    .OrderByDescending(tr => tr.points)
                    .ThenByDescending(tr => tr.difference)
                    .ThenByDescending(tr => tr.accumulated_score)
                    .ToList();
                return tableResults;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/team_name/{token}")]
        public async Task<ActionResult<MatchRr>> EditTeamName(long id, string token, [FromBody] MatchRrEditTeamNameDTO matchRrEditTeamNameDto)
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

                if (matchRrEditTeamNameDto.old_team_name == null || matchRrEditTeamNameDto.old_team_name.Trim() == "" ||
                    matchRrEditTeamNameDto.new_team_name == null || matchRrEditTeamNameDto.new_team_name.Trim() == "")
                {
                    throw new Exception("Empty team name.");
                }

                var teamNamesFromDb = await _dbContext.MatchRrs
                    .Where(mrr => mrr.stage_id == stage.id)
                    .Select(mrr => new List<string>{ mrr.team_1, mrr.team_2 })
                    .ToListAsync();
                var teamNameList = teamNamesFromDb.SelectMany(x => x).Distinct().ToList();
                teamNameList.Remove(matchRrEditTeamNameDto.old_team_name);
                if (teamNameList.Contains(matchRrEditTeamNameDto.new_team_name.Trim()))
                {
                    throw new Exception("Team name has already been in this stage");
                }
                var matchRrs = await _dbContext.MatchRrs
                    .Where(mrr => mrr.stage_id == stage.id)
                    .ToListAsync();
                for (int i = 0; i < matchRrs.Count; i++)
                {
                    if (matchRrs[i].team_1 == matchRrEditTeamNameDto.old_team_name)
                    {
                        matchRrs[i].team_1 = matchRrEditTeamNameDto.new_team_name;
                    }
                    if (matchRrs[i].team_2 == matchRrEditTeamNameDto.old_team_name)
                    {
                        matchRrs[i].team_2 = matchRrEditTeamNameDto.new_team_name;
                    }
                }
                await _dbContext.SaveChangesAsync();

                var newMatchRr = await _dbContext.MatchRrs.FindAsync(id);
                return new ActionResult<MatchRr>(newMatchRr);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

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
