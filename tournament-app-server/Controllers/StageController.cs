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

        [HttpGet("all/{tournament_id}/{token}")]
        public async Task<ActionResult<IEnumerable<Stage>>> GetStagesByTournamentId(long tournament_id, string token)
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
                var tournament = await _dbContext.Tournaments.FindAsync(tournament_id);
                if (tournament == null)
                {
                    return NotFound();
                }
                else if (tournament.user_id != userId)
                {
                    throw new Exception("Cannot access or modify these stages by your token.");
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

        [HttpGet("{id}/{token}")]
        public async Task<ActionResult<Stage>> GetStageById(long id, string token)
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
                var stageUserId = await _dbContext.StageUserIds
                    .Where(su => su.stage_id == id)
                    .FirstAsync();
                if (stageUserId == null)
                {
                    return NotFound();
                }
                else if (stageUserId.user_id != userId)
                {
                    throw new Exception("Cannot access or modify this stage by your token.");
                }

                return await _dbContext.Stages.FindAsync(id);
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
                short number_of_rounds = (short)Math.Ceiling(Math.Log2(stageDto.number_of_teams_per_group));
                short ideal_number_of_teams_per_group = (short)Math.Pow(2, number_of_rounds);
                var maxStageOrder = await _dbContext.Stages
                    .Where(s => s.tournament_id == tournament.id)
                    .OrderBy(s => s.stage_order)
                    .Select(s => (short?)s.stage_order)
                    .MaxAsync();
                short currentMaxStageOrder = maxStageOrder ?? 0;
                if (ideal_number_of_teams_per_group >= 2 && ideal_number_of_teams_per_group <= 128)
                {
                    stage.number_of_teams_per_group = ideal_number_of_teams_per_group;
                }
                else
                {
                    throw new Exception("Invalid number_of_teams_per_group.");
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
                stage.number_of_legs_per_round = stageDto.number_of_legs_per_round.ToArray();
                stage.best_of_per_round = stageDto.best_of_per_round.ToArray();
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

                _dbContext.Stages.Add(stage);
                await _dbContext.SaveChangesAsync();

                if (stage.format_id == 1) //Single elimination
                {
                    //Generate single elimiation matches (except 3rd-place match)
                    for (short i = 1; i <= stage.number_of_groups; i++)
                    {
                        short number_of_pairs_per_round = (short)(ideal_number_of_teams_per_group / 2);
                        for (short j = 1; j <= number_of_rounds; j++)
                        {
                            for (short k = 1; k <= number_of_pairs_per_round; k++)
                            {
                                MatchSe matchSe = new MatchSe
                                {
                                    stage_id = stage.id,
                                    round_number = j,
                                    match_number = k,
                                    number_of_legs = stage.number_of_legs_per_round[j - 1],
                                    best_of = stage.best_of_per_round[j - 1],
                                    group_number = i
                                };
                                _dbContext.MatchSes.Add(matchSe);
                            }
                            number_of_pairs_per_round /= 2;
                        }
                        //3rd-place match generation
                        if (stage.include_third_place_match != null)
                        {
                            if ((bool)stage.include_third_place_match)
                            {
                                MatchSe thirdPlaceMatchSe = new MatchSe
                                {
                                    stage_id = stage.id,
                                    round_number = (short)(number_of_rounds + 1),
                                    match_number = 1,
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
                var stageUserId = await _dbContext.StageUserIds
                    .Where(su => su.stage_id == id)
                    .FirstAsync();
                if (stageUserId == null)
                {
                    return NotFound();
                }
                else if (stageUserId.user_id != userId)
                {
                    throw new Exception("Cannot modify stage to this tournament by your token.");
                }
                var stage = await _dbContext.Stages.FindAsync(id);
                if (stage == null)
                {
                    return NotFound();
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
    }
}