using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tournament_app_server.DTOs;
using tournament_app_server.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace tournament_app_server.Controllers
{
    [Route("/stages")]
    [ApiController]
    public class StageController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public StageController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("all/{tournament_id}/{token?}")]
        public async Task<ActionResult<IEnumerable<Stage>>> GetStagesByTournamentId(long tournament_id, string token = "")
        {
            if (_dbContext.Stages == null)
            {
                return NotFound();
            }

            try
            {
                var tournament = await _dbContext.Tournaments.FindAsync(tournament_id);
                if (tournament == null)
                {
                    return NotFound();
                }

                if (token == "")
                {
                    if (tournament.is_private == true)
                    {
                        throw new Exception("Cannot access or modify these private stages without a valid token.");
                    }
                }
                else
                {
                    var decodedToken = TokenValidation.ValidateToken(token);
                    var payload = decodedToken.Payload;
                    int userId = (int)payload["id"];
                    if (tournament.user_id != userId && tournament.is_private == true)
                    {
                        throw new Exception("Cannot access or modify these stages by your token.");
                    }
                    
                }

                return await _dbContext.Stages
                    .Where(s => s.tournament_id == tournament_id)
                    .OrderBy(s => s.stage_order)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/{token?}")]
        public async Task<ActionResult<Stage>> GetStageById(long id, string token = "")
        {
            if (_dbContext.Stages == null)
            {
                return NotFound();
            }

            try
            {
                var stage = await _dbContext.Stages.FindAsync(id);
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
                        throw new Exception("Cannot access or modify this private stage without a valid token.");
                    }
                }
                else
                {
                    var decodedToken = TokenValidation.ValidateToken(token);
                    var payload = decodedToken.Payload;
                    int userId = (int)payload["id"];
                    
                    if (tournament.user_id != userId && tournament.is_private == true)
                    {
                        throw new Exception("Cannot access or modify this stage by your token.");
                    }
                }

                return stage;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{token}")]
        public async Task<ActionResult<Stage>> CreateStage(string token, [FromBody] StageDTO stageDto)
        {
            if (_dbContext.Stages == null)
            {
                return NotFound();
            }
            try
            {
                var decodedToken = TokenValidation.ValidateToken(token);
                var payload = decodedToken.Payload;
                int userId = (int)payload["id"];
                var tournament = await _dbContext.Tournaments.FindAsync(stageDto.tournament_id);
                if (tournament == null)
                {
                    return NotFound();
                }
                else if (tournament.user_id != userId)
                {
                    throw new Exception("Cannot modify stage to this tournament by your token.");
                }

                Stage stage = new Stage();
                stage.name = stageDto.name;
                stage.format_id = stageDto.format_id;
                if (stageDto.start_date != null)
                {
                    stage.start_date = DateTimeOffset.Parse(stageDto.start_date).ToUniversalTime();
                }
                if (stageDto.end_date != null)
                {
                    stage.end_date = DateTimeOffset.Parse(stageDto.end_date).ToUniversalTime();
                }
                stage.places = stageDto.places;
                stage.tournament_id = stageDto.tournament_id;
                short numberOfRoundsSe = 0;
                short idealNumberOfTeamsPerGroupSe = 0;
                var maxStageOrder = await _dbContext.Stages
                    .Where(s => s.tournament_id == tournament.id)
                    .OrderBy(s => s.stage_order)
                    .Select(s => (short?)s.stage_order)
                    .MaxAsync();
                short currentMaxStageOrder = maxStageOrder ?? 0;
                if (stage.format_id == 1) //Single elimination
                {
                    numberOfRoundsSe = (short)Math.Ceiling(Math.Log2(stageDto.number_of_teams_per_group));
                    idealNumberOfTeamsPerGroupSe = (short)Math.Pow(2, numberOfRoundsSe);
                    if (idealNumberOfTeamsPerGroupSe >= 2 && idealNumberOfTeamsPerGroupSe <= 128)
                    {
                        stage.number_of_teams_per_group = idealNumberOfTeamsPerGroupSe;
                    }
                    else
                    {
                        throw new Exception("Invalid number_of_teams_per_group.");
                    }
                }
                else if (stage.format_id == 2) { //Round robin
                    if (stageDto.number_of_teams_per_group >= 2 && stageDto.number_of_teams_per_group <= 32)
                    {
                        stage.number_of_teams_per_group = stageDto.number_of_teams_per_group;
                    }
                    else
                    {
                        throw new Exception("Invalid number_of_teams_per_group.");
                    }
                }
                if (stageDto.number_of_groups >= 1 && stageDto.number_of_groups <= 32)
                {
                    stage.number_of_groups = stageDto.number_of_groups;
                }
                else
                {
                    throw new Exception("Invalid number_of_groups.");
                }

                if (stageDto.stage_order < 1 || stageDto.stage_order > currentMaxStageOrder + 1)
                {
                    throw new Exception("Invalid stage_order.");
                }
                stage.stage_order = stageDto.stage_order;
                var nextStages = await _dbContext.Stages
                    .Where(s => s.stage_order >= stageDto.stage_order)
                    .ToListAsync();
                foreach (var s in nextStages)
                {
                    s.stage_order++;
                }
                await _dbContext.SaveChangesAsync();
                foreach(short n in stageDto.number_of_legs_per_round) {
                    if (n < 1 || n > 3)
                    {
                        throw new Exception("Invalid number_of_legs_per_round.");
                    }
                }
                stage.number_of_legs_per_round = stageDto.number_of_legs_per_round;
                stage.best_of_per_round = stageDto.best_of_per_round;
                stage.description = stageDto.description;

                if (stage.format_id == 1) //Single elimination
                {
                    stage.include_third_place_match = stageDto.include_third_place_match;
                    if (stageDto.third_place_match_number_of_legs < 1 || stageDto.third_place_match_number_of_legs > 3)
                    {
                        throw new Exception("Invalid third_place_match_number_of_legs.");
                    }
                    stage.third_place_match_number_of_legs = stageDto.third_place_match_number_of_legs;
                    stage.third_place_match_best_of = stageDto.third_place_match_best_of;
                }
                else if (stage.format_id == 2) //Round robin
                {
                    if (stageDto.win_point != null)
                    {
                        stage.win_point = (int)stageDto.win_point;
                    }
                    if (stageDto.draw_point != null)
                    {
                        stage.draw_point = (int)stageDto.draw_point;
                    }
                    if (stageDto.lose_point != null)
                    {
                        stage.lose_point = (int)stageDto.lose_point;
                    }
                }

                _dbContext.Stages.Add(stage);
                await _dbContext.SaveChangesAsync();

                if (stage.format_id == 1) //Single elimination
                {
                    List<int[]> seedingPairs = new List<int[]>();
                    singleEliminationSeeding(seedingPairs, 1, 1, numberOfRoundsSe + 1);
                    //Generate single elimiation matches (except 3rd-place match)
                    for (short i = 1; i <= stage.number_of_groups; i++)
                    {
                        short numberOfPairsPerRound = (short)(idealNumberOfTeamsPerGroupSe / 2);
                        for (short j = 1; j <= numberOfRoundsSe; j++)
                        {
                            for (short k = 1; k <= numberOfPairsPerRound; k++)
                            {
                                string team1Name = null, team2Name = null;
                                if (j == 1)
                                {
                                    team1Name = "G" + i.ToString() + "-T" + seedingPairs[k][0].ToString();
                                    team2Name = "G" + i.ToString() + "-T" + seedingPairs[k][1].ToString();
                                }
                                List<long> initialTeam1Scores = new List<long>(), initialTeam2Scores = new List<long>();
                                for (int a = 0; a < stage.number_of_legs_per_round[j - 1]; a++)
                                {
                                    initialTeam1Scores.Add(0);
                                    initialTeam2Scores.Add(0);
                                }
                                long[] initialTeam1ScoresArray = initialTeam1Scores.ToArray();
                                long[] initialTeam2ScoresArray = initialTeam2Scores.ToArray();
                                List<long> initialTeam1Subscores = new List<long>(), initialTeam2Subscores = new List<long>();
                                if (stage.best_of_per_round[j - 1] > 0)
                                {
                                    for (int a = 0; a < stage.number_of_legs_per_round[j - 1] * stage.best_of_per_round[j - 1]; a++)
                                    {
                                        initialTeam1Subscores.Add(0);
                                        initialTeam2Subscores.Add(0);
                                    }
                                }
                                long[] initialTeam1SubscoresArray = initialTeam1Subscores.ToArray();
                                long[] initialTeam2SubscoresArray = initialTeam2Subscores.ToArray();
                                MatchSe matchSe = new MatchSe
                                {
                                    stage_id = stage.id,
                                    round_number = j,
                                    match_number = k,
                                    team_1 = team1Name,
                                    team_2 = team2Name,
                                    team_1_scores = initialTeam1ScoresArray,
                                    team_2_scores = initialTeam2ScoresArray,
                                    team_1_subscores = initialTeam1SubscoresArray,
                                    team_2_subscores = initialTeam2SubscoresArray,
                                    number_of_legs = stage.number_of_legs_per_round[j - 1],
                                    best_of = stage.best_of_per_round[j - 1],
                                    group_number = i
                                };
                                _dbContext.MatchSes.Add(matchSe);
                            }
                            numberOfPairsPerRound /= 2;
                        }
                        //3rd-place match generation
                        if (stage.include_third_place_match != null)
                        {
                            if ((bool)stage.include_third_place_match)
                            {
                                List<long> initialTeam1Scores = new List<long>(), initialTeam2Scores = new List<long>();
                                for (int a = 0; a < stage.third_place_match_number_of_legs; a++)
                                {
                                    initialTeam1Scores.Add(0);
                                    initialTeam2Scores.Add(0);
                                }
                                long[] initialTeam1ScoresArray = initialTeam1Scores.ToArray();
                                long[] initialTeam2ScoresArray = initialTeam2Scores.ToArray();
                                List<long> initialTeam1Subscores = new List<long>(), initialTeam2Subscores = new List<long>();
                                if (stage.third_place_match_best_of > 0)
                                {
                                    for (int a = 0; a < stage.third_place_match_number_of_legs * stage.third_place_match_best_of; a++)
                                    {
                                        initialTeam1Subscores.Add(0);
                                        initialTeam2Subscores.Add(0);
                                    }
                                }
                                long[] initialTeam1SubscoresArray = initialTeam1Subscores.ToArray();
                                long[] initialTeam2SubscoresArray = initialTeam2Subscores.ToArray();
                                MatchSe thirdPlaceMatchSe = new MatchSe
                                {
                                    stage_id = stage.id,
                                    round_number = (short)(numberOfRoundsSe + 1),
                                    match_number = 1,
                                    team_1_scores = initialTeam1ScoresArray,
                                    team_2_scores = initialTeam2ScoresArray,
                                    team_1_subscores = initialTeam1SubscoresArray,
                                    team_2_subscores = initialTeam2SubscoresArray,
                                    number_of_legs = (short)stage.third_place_match_number_of_legs,
                                    best_of = (short)stage.third_place_match_best_of,
                                    group_number = i
                                };
                                _dbContext.MatchSes.Add(thirdPlaceMatchSe);
                            }
                        }
                    }
                    await _dbContext.SaveChangesAsync();
                }
                else if (stage.format_id == 2) //Round robin
                {
                    List<long> initialTeam1Subscores = new List<long>(), initialTeam2Subscores = new List<long>();
                    if (stage.best_of_per_round[0] > 0)
                    {
                        for (int a = 0; a < stage.best_of_per_round[0]; a++)
                        {
                            initialTeam1Subscores.Add(0);
                            initialTeam2Subscores.Add(0);
                        }
                    }
                    long[] initialTeam1SubscoresArray = initialTeam1Subscores.ToArray();
                    long[] initialTeam2SubscoresArray = initialTeam2Subscores.ToArray();
                    for (short i = 1; i <= stage.number_of_groups; i++)
                    {
                        for (short j = 1; j <= stage.number_of_legs_per_round[0]; j++)
                        {
                            short matchNumber = 1;
                            for (short a = 1; a < stage.number_of_teams_per_group; a++)
                            {
                                for (short b = (short)(a + 1); b <= stage.number_of_teams_per_group; b++)
                                {
                                    MatchRr matchRr = new MatchRr
                                    {
                                        stage_id = stage.id,
                                        group_number = i,
                                        leg_number = j,
                                        match_number = matchNumber,
                                        team_1 = "G" + i.ToString() + "-T" + a.ToString(),
                                        team_2 = "G" + i.ToString() + "-T" + b.ToString(),
                                        team_1_score = 0,
                                        team_2_score = 0,
                                        team_1_subscores = initialTeam1SubscoresArray,
                                        team_2_subscores = initialTeam2SubscoresArray,
                                    };
                                    _dbContext.MatchRrs.Add(matchRr);
                                    matchNumber++;
                                }
                            }
                        }
                    }
                    await _dbContext.SaveChangesAsync();
                }

                return CreatedAtAction(nameof(GetStageById), new { stage.id, token }, stage);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/{token}")]
        public async Task<ActionResult<Stage>> EditStage(long id, string token, [FromBody] StageEditDTO stageEditDto)
        {
            if (_dbContext.Stages == null)
            {
                return NotFound();
            }
            try
            {
                var decodedToken = TokenValidation.ValidateToken(token);
                var payload = decodedToken.Payload;
                int userId = (int)payload["id"];
                var tournament = await _dbContext.Tournaments.FindAsync(stageEditDto.tournament_id);
                if (tournament == null)
                {
                    return NotFound();
                }
                else if (tournament.user_id != userId)
                {
                    throw new Exception("Cannot modify stage to this tournament by your token.");
                }

                var stage = await _dbContext.Stages.FindAsync(id);
                stage.name = stageEditDto.name;
                if (stageEditDto.start_date != null)
                {
                    stage.start_date = DateTimeOffset.Parse(stageEditDto.start_date).ToUniversalTime();
                }
                if (stageEditDto.end_date != null)
                {
                    stage.end_date = DateTimeOffset.Parse(stageEditDto.end_date).ToUniversalTime();
                }
                stage.places = stageEditDto.places;
                stage.description = stageEditDto.description;
                await _dbContext.SaveChangesAsync();
                return new ActionResult<Stage>(stage);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("order/{token}")]
        public async Task<ActionResult<List<StageOrderDTO>>> EditStageOrder(string token, [FromBody] StageOrderDTO[] stageOrderDto)
        {
            if (_dbContext.Stages == null)
            {
                return NotFound();
            }
            try
            {
                var decodedToken = TokenValidation.ValidateToken(token);
                var payload = decodedToken.Payload;
                int userId = (int)payload["id"];
                var tournament = await _dbContext.Tournaments.FindAsync(stageOrderDto[0].tournament_id);
                if (tournament == null)
                {
                    return NotFound();
                }
                else if (tournament.user_id != userId)
                {
                    throw new Exception("Cannot modify stage to this tournament by your token.");
                }

                var distinctOrder = stageOrderDto.DistinctBy(so => so.stage_order).ToList();
                if (distinctOrder.Count != stageOrderDto.Length)
                {
                    throw new Exception("Stage order must be different among the stages.");
                }
                long tournamentId = stageOrderDto[0].tournament_id;
                foreach (var s in stageOrderDto)
                {
                    if (s.tournament_id != tournamentId)
                    {
                        throw new Exception("Stages are not from the same tournament.");
                    }
                }
                var maxStageOrder = await _dbContext.Stages
                    .Where(s => s.tournament_id == tournament.id)
                    .OrderBy(s => s.stage_order)
                    .Select(s => (short?)s.stage_order)
                    .MaxAsync();
                short currentMaxStageOrder = maxStageOrder ?? 0;
                foreach (var s in stageOrderDto)
                {
                    if (s.stage_order < 1 || s.stage_order > currentMaxStageOrder)
                    {
                        throw new Exception("Invalid stage_order.");
                    }
                }
                
                var stages = await _dbContext.Stages
                    .Where(s => s.tournament_id == tournamentId)
                    .ToListAsync();
                foreach (var so in stageOrderDto)
                {
                    int id = stages.FindIndex(s => s.id == so.id);
                    stages[id].stage_order = so.stage_order;
                }
                await _dbContext.SaveChangesAsync();
                return await _dbContext.Stages
                    .Where(s => s.tournament_id == tournamentId)
                    .OrderBy(s => s.stage_order)
                    .Select(s => new StageOrderDTO { id = s.id, name = s.name, stage_order = s.stage_order })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}/{token}")]
        public async Task<IActionResult> DeleteStage(long id, string token)
        {
            if (_dbContext.Stages == null)
            {
                return NotFound();
            }

            try
            {
                var decodedToken = TokenValidation.ValidateToken(token);
                var payload = decodedToken.Payload;
                int userId = (int)payload["id"];
                var stage = await _dbContext.Stages.FindAsync(id);
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
                    throw new Exception("Cannot modify stage to this tournament by your token.");
                }
                else
                {
                    var nextStages = await _dbContext.Stages
                    .Where(s => s.stage_order > stage.stage_order)
                    .ToListAsync();
                    foreach (var s in nextStages)
                    {
                        s.stage_order--;
                    }
                    await _dbContext.SaveChangesAsync();

                    var matchSes = await _dbContext.MatchSes
                        .Where(mse => mse.stage_id == id)
                        .ToListAsync();
                    _dbContext.MatchSes.RemoveRange(matchSes);
                    _dbContext.Stages.Remove(stage);
                    await _dbContext.SaveChangesAsync();
                    return NoContent();
                }
                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public static void singleEliminationSeeding(List<int[]> pairs, int seed, int level, int limit)
        {
            var levelSum = (int)Math.Pow(2, level) + 1;

            if (limit == level + 1)
            {
                pairs.Add([seed, levelSum - seed]);
                return;
            }
            else if (seed % 2 == 1)
            {
                singleEliminationSeeding(pairs, seed, level + 1, limit);
                singleEliminationSeeding(pairs, levelSum - seed, level + 1, limit);
            }
            else
            {
                singleEliminationSeeding(pairs, levelSum - seed, level + 1, limit);
                singleEliminationSeeding(pairs, seed, level + 1, limit);
            }
        }
    }
}