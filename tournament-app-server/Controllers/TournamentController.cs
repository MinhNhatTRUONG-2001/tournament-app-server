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

        // GET: /tournaments
        [HttpGet("all/{token}")]
        public async Task<ActionResult<IEnumerable<Tournament>>> GetTournamentsByUserId(string token)
        {
            if (_dbContext.Tournaments == null)
            {
                return NotFound();
            }

            try
            {
                var decodedToken = TokenValidation.ValidateToken(token);
                var payload = decodedToken.Payload;
                int userId = (int)payload["id"];

                return await _dbContext.Tournaments.Where(t => t.user_id == userId).ToListAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET /tournaments/5
        [HttpGet("{id}/{token}")]
        public async Task<ActionResult<Tournament>> GetTournamentById(int id, string token)
        {
            if (_dbContext.Tournaments == null)
            {
                return NotFound();
            }
            try
            {
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
                return tournament;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST /tournaments
        [HttpPost("{token}")]
        public async Task<ActionResult<Tournament>> CreateTournament(string token, [FromBody] TournamentDTO tournamentDto)
        {
            if (_dbContext.Tournaments == null)
            {
                return NotFound();
            }
            try
            {
                var decodedToken = TokenValidation.ValidateToken(token);
                var payload = decodedToken.Payload;
                int userId = (int)payload["id"];

                Tournament tournament = new Tournament();
                tournament.name = tournamentDto.name;
                tournament.start_date = DateTimeOffset.Parse(tournamentDto.start_date).ToUniversalTime();
                tournament.end_date = DateTimeOffset.Parse(tournamentDto.end_date).ToUniversalTime();
                tournament.places = tournamentDto.places;
                tournament.user_id = userId;
                _dbContext.Tournaments.Add(tournament);
                await _dbContext.SaveChangesAsync();
                return CreatedAtAction(nameof(GetTournamentById), new { tournament.id, token }, tournament);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT /tournaments/5
        [HttpPut("{id}/{token}")]
        public async Task<ActionResult<Tournament>> EditTournament(int id, string token, [FromBody] TournamentDTO tournamentDto)
        {
            if (_dbContext.Tournaments == null)
            {
                return NotFound();
            }
            try
            {
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
                tournament.start_date = DateTimeOffset.Parse(tournamentDto.start_date).ToUniversalTime();
                tournament.end_date = DateTimeOffset.Parse(tournamentDto.end_date).ToUniversalTime();
                tournament.places = tournamentDto.places;
                tournament.user_id = userId;
                await _dbContext.SaveChangesAsync();
                return new ActionResult<Tournament>(tournament);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE /tournaments/5
        [HttpDelete("{id}/{token}")]
        public async Task<IActionResult> DeleteTournament(int id, string token)
        {
            if (_dbContext.Tournaments == null)
            {
                return NotFound();
            }

            try
            {
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
