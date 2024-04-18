using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tournament_app_server.DTOs;
using tournament_app_server.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace tournament_app_server.Controllers
{
    [Route("/tournaments")]
    [ApiController]
    public class TournamentController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public TournamentController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("public")]
        public async Task<ActionResult<IEnumerable<Tournament>>> GetPublicTournaments()
        {
            if (_dbContext.Tournaments == null)
            {
                return NotFound();
            }

            try
            {
                return await _dbContext.Tournaments
                    .Where(t => t.is_private == false)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Tournament>>> GetTournamentsByUserId([FromHeader(Name = "Authorization")] string token = "")
        {
            if (_dbContext.Tournaments == null)
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

                return await _dbContext.Tournaments
                    .Where(t => t.user_id == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Tournament>> GetTournamentById(long id, [FromHeader(Name = "Authorization")] string token = "")
        {
            if (_dbContext.Tournaments == null)
            {
                return NotFound();
            }
            try
            {
                if (token.Contains("Bearer "))
                {
                    token = token.Split("Bearer ")[1];
                }
                var tournament = await _dbContext.Tournaments.FindAsync(id);
                if (tournament == null)
                {
                    return NotFound();
                }

                if (token.Trim() == "")
                {
                    if (tournament.is_private == true)
                    {
                        throw new Exception("Cannot access or modify this private tournament without a valid token.");
                    }
                }
                else
                {
                    var decodedToken = TokenValidation.ValidateToken(token);
                    var payload = decodedToken.Payload;
                    int userId = (int)payload["id"];

                    if (tournament.user_id != userId && tournament.is_private == true)
                    {
                        throw new Exception("Cannot access or modify this tournament by your token.");
                    }   
                }

                return tournament;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<Tournament>> CreateTournament([FromBody] TournamentDTO tournamentDto, [FromHeader(Name = "Authorization")] string token = "")
        {
            if (_dbContext.Tournaments == null)
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

                Tournament tournament = new Tournament();
                tournament.name = tournamentDto.name;
                if (tournamentDto.start_date != null)
                {
                    tournament.start_date = DateTimeOffset.Parse(tournamentDto.start_date).ToUniversalTime();
                }
                if (tournamentDto.end_date != null)
                {
                    tournament.end_date = DateTimeOffset.Parse(tournamentDto.end_date).ToUniversalTime();
                }
                tournament.places = tournamentDto.places;
                tournament.user_id = userId;
                tournament.description = tournamentDto.description;
                tournament.is_private = tournamentDto.is_private;
                _dbContext.Tournaments.Add(tournament);
                await _dbContext.SaveChangesAsync();
                return CreatedAtAction(nameof(GetTournamentById), new { tournament.id, token }, tournament);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Tournament>> EditTournament(long id, [FromBody] TournamentDTO tournamentDto, [FromHeader(Name = "Authorization")] string token = "")
        {
            if (_dbContext.Tournaments == null)
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

                var tournament = await _dbContext.Tournaments.FindAsync(id);
                if (tournament == null)
                {
                    return NotFound();
                }
                else if (tournament.user_id != userId)
                {
                    throw new Exception("Cannot access or modify this tournament by your token.");
                }
                tournament.name = tournamentDto.name;
                if (tournamentDto.start_date != null)
                {
                    tournament.start_date = DateTimeOffset.Parse(tournamentDto.start_date).ToUniversalTime();
                }
                if (tournamentDto.end_date != null)
                {
                    tournament.end_date = DateTimeOffset.Parse(tournamentDto.end_date).ToUniversalTime();
                }
                tournament.places = tournamentDto.places;
                tournament.user_id = userId;
                tournament.description = tournamentDto.description;
                tournament.is_private = tournamentDto.is_private;
                await _dbContext.SaveChangesAsync();
                return new ActionResult<Tournament>(tournament);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTournament(long id, [FromHeader(Name = "Authorization")] string token = "")
        {
            if (_dbContext.Tournaments == null)
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

                var tournament = await _dbContext.Tournaments.FindAsync(id);
                if (tournament == null)
                {
                    return NotFound();
                }
                else if (tournament.user_id != userId)
                {
                    throw new Exception("Cannot access or modify this tournament by your token.");
                }
                var stages = await _dbContext.Stages
                    .Where(s => s.tournament_id == id)
                    .ToListAsync();
                foreach (var s in stages)
                {
                    var matchSes = await _dbContext.MatchSes
                        .Where(mse => mse.stage_id == s.id)
                        .ToListAsync();
                    _dbContext.MatchSes.RemoveRange(matchSes);
                }
                _dbContext.Stages.RemoveRange(stages);
                _dbContext.Tournaments.Remove(tournament);
                await _dbContext.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
