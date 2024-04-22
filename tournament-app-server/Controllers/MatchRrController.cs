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

        [HttpGet("all/{stage_id}")]
        public async Task<ActionResult<IEnumerable<MatchRr>>> GetMatchesByStageId(long stage_id, [FromHeader(Name = "Authorization")] string token = "")
        {
            if (_dbContext.MatchRrs == null)
            {
                return NotFound();
            }

            try
            {
                if (token.Contains("Bearer "))
                {
                    token = token.Split("Bearer ")[1];
                }
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

                if (token.Trim() == "")
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

        [HttpGet("{id}")]
        public async Task<ActionResult<MatchRr>> GetMatchById(long id, [FromHeader(Name = "Authorization")] string token = "")
        {
            if (_dbContext.MatchRrs == null)
            {
                return NotFound();
            }

            try
            {
                if (token.Contains("Bearer "))
                {
                    token = token.Split("Bearer ")[1];
                }
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

                if (token.Trim() == "")
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

        
        [HttpGet("table_results/{stage_id}/{group_number}")]
        public async Task<ActionResult<IEnumerable<MatchRrTableResult>>> GetTableResultsByStageIdAndGroupNumber(long stage_id, int group_number, [FromHeader(Name = "Authorization")] string token = "")
        {
            if (_dbContext.MatchRrs == null)
            {
                return NotFound();
            }

            try
            {
                if (token.Contains("Bearer "))
                {
                    token = token.Split("Bearer ")[1];
                }
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

                if (token.Trim() == "")
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

                if (group_number < 1 || group_number > stage.number_of_groups)
                {
                    throw new Exception("Invalid group number.");
                }
                List<MatchRrTableResult> tableResults = new List<MatchRrTableResult>();
                var groupMatchRrs = await _dbContext.MatchRrs
                    .Where(mrr => mrr.stage_id == stage_id && mrr.group_number == group_number)
                    .ToListAsync();
                var distinctTeamNames = groupMatchRrs
                    .Select(mrr => new List<string> { mrr.team_1, mrr.team_2 })
                    .SelectMany(x => x)
                    .Distinct()
                    .ToList();
                //Calculate total points, difference, accumulated/earned score and other criteria value for each team
                for (int j = 0; j < distinctTeamNames.Count; j++)
                {
                    var teamMatchRrs = groupMatchRrs
                        .Where(mrr => mrr.team_1 == distinctTeamNames[j] || mrr.team_2 == distinctTeamNames[j])
                        .ToList();
                    //Calculate total points
                    decimal totalPoints = 0;
                    foreach (var mrr in teamMatchRrs)
                    {
                        if (mrr.winner == null)
                        {
                            continue;
                        }
                        else
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
                    }
                    //Calculate difference
                    decimal difference = 0;
                    foreach (var mrr in teamMatchRrs)
                    {
                        if (mrr.team_1 == distinctTeamNames[j])
                        {
                            difference += (decimal)mrr.team_1_score - (decimal)mrr.team_2_score;
                        }
                        else if (mrr.team_2 == distinctTeamNames[j])
                        {
                            difference += (decimal)mrr.team_2_score - (decimal)mrr.team_1_score;
                        }
                    }
                    //Calculate accumulated/earned score
                    decimal accumulatedScore = 0;
                    foreach (var mrr in teamMatchRrs)
                    {
                        if (mrr.team_1 == distinctTeamNames[j])
                        {
                            accumulatedScore += (decimal)mrr.team_1_score;
                        }
                        else if (mrr.team_2 == distinctTeamNames[j])
                        {
                            accumulatedScore += (decimal)mrr.team_2_score;
                        }
                    }
                    //Calculate other criteria values
                    List<decimal> otherCriteriaValues = new List<decimal>();
                    if (stage.other_criteria_names != null)
                    {
                        for (int k = 0; k < stage.other_criteria_names.Length; k++)
                        {
                            decimal criteriaValue = 0;
                            foreach (var mrr in teamMatchRrs)
                            {
                                if (mrr.team_1 == distinctTeamNames[j])
                                {
                                    criteriaValue += mrr.team_1_other_criteria_values[k];
                                }
                                else if (mrr.team_2 == distinctTeamNames[j])
                                {
                                    criteriaValue += mrr.team_2_other_criteria_values[k];
                                }
                            }
                            otherCriteriaValues.Add(criteriaValue);
                        }
                    }

                    tableResults.Add(new MatchRrTableResult { 
                        name = distinctTeamNames[j],
                        points = totalPoints,
                        difference = difference,
                        accumulated_score = accumulatedScore,
                        other_criteria_values = otherCriteriaValues.ToArray()
                    });
                }

                //Sort table results
                tableResults = tableResults
                    .OrderByDescending(tr => tr.points)
                    .ThenByDescending(tr => tr.difference)
                    .ThenByDescending(tr => tr.accumulated_score)
                    .ToList();
                if (stage.other_criteria_sort_direction != null)
                {
                    for (int i = 0; i < stage.other_criteria_sort_direction.Length; i++)
                    {
                        if (stage.other_criteria_sort_direction[i] == "ASC")
                        {
                            tableResults = tableResults
                                .OrderBy(tr => tr.other_criteria_values[i])
                                .ToList();
                        }
                        else if (stage.other_criteria_sort_direction[i] == "DESC")
                        {
                            tableResults = tableResults
                                .OrderByDescending(tr => tr.other_criteria_values[i])
                                .ToList();
                        }
                    }
                }
                    
                return tableResults;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/team_name")]
        public async Task<ActionResult<MatchRr>> EditTeamName(long id, [FromBody] MatchRrEditTeamNameDTO matchRrEditTeamNameDto, [FromHeader(Name = "Authorization")] string token = "")
        {
            if (_dbContext.MatchRrs == null)
            {
                return NotFound();
            }
            try
            {
                if (token.Contains("Bearer "))
                {
                    token = token.Split("Bearer ")[1];
                }
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

        [HttpPut("{id}/match_info")]
        public async Task<ActionResult<MatchRr>> EditMatchInfo(long id, [FromBody] MatchEditMatchInfoDTO matchEditMatchInfoDto, [FromHeader(Name = "Authorization")] string token = "")
        {
            if (_dbContext.MatchRrs == null)
            {
                return NotFound();
            }
            try
            {
                if (token.Contains("Bearer "))
                {
                    token = token.Split("Bearer ")[1];
                }
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

        [HttpPut("{id}/match_score")]
        public async Task<ActionResult<MatchRr>> EditMatchScore(long id, [FromBody] MatchRrEditMatchScoreDTO matchRrEditMatchScoreDto, [FromHeader(Name = "Authorization")] string token = "")
        {
            if (_dbContext.MatchRrs == null)
            {
                return NotFound();
            }
            try
            {
                if (token.Contains("Bearer "))
                {
                    token = token.Split("Bearer ")[1];
                }
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
                matchRr.team_1_other_criteria_values = matchRrEditMatchScoreDto.team_1_other_criteria_values;
                matchRr.team_2_other_criteria_values = matchRrEditMatchScoreDto.team_2_other_criteria_values;

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
