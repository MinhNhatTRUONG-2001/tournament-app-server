using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tournament_app_server.DTOs;
using tournament_app_server.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace tournament_app_server.Controllers
{
    [Route("[controller]/tournaments")]
    [ApiController]
    public class TournamentController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public TournamentController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: <TournamentController>/tournaments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tournament>>> GetTournaments()
        {
            if (_dbContext.Tournaments == null)
            {
                return NotFound();
            }
            return await _dbContext.Tournaments.ToListAsync();
        }

        // GET <TournamentController>/tournaments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tournament>> GetTournamentById(int id)
        {
            if (_dbContext.Tournaments == null)
            {
                return NotFound();
            }
            var tournament = await _dbContext.Tournaments.FindAsync(id);

            if (tournament == null)
            {
                return NotFound();
            }

            return tournament;
        }

        // POST <TournamentController>/tournaments
        [HttpPost]
        public async Task<ActionResult<Tournament>> CreateTournament([FromBody] TournamentDTO tournamentDto)
        {
            if (_dbContext.Tournaments == null)
            {
                return NotFound();
            }
            Tournament tournament = new Tournament();
            tournament.name = tournamentDto.name;
            tournament.start_date = DateTimeOffset.Parse(tournamentDto.start_date).ToUniversalTime();
            tournament.end_date = DateTimeOffset.Parse(tournamentDto.end_date).ToUniversalTime();
            tournament.places = tournamentDto.places;
            tournament.user_id = tournamentDto.user_id;
            _dbContext.Tournaments.Add(tournament);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTournamentById), new { id = tournament.id }, tournament);
        }

        // PUT <TournamentController>/tournaments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> EditTournament(int id, [FromBody] TournamentDTO tournamentDto)
        {
            if (_dbContext.Tournaments == null)
            {
                return NotFound();
            }

            var tournament = await _dbContext.Tournaments.FindAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }
            tournament.name = tournamentDto.name;
            tournament.start_date = DateTimeOffset.Parse(tournamentDto.start_date).ToUniversalTime();
            tournament.end_date = DateTimeOffset.Parse(tournamentDto.end_date).ToUniversalTime();
            tournament.places = tournamentDto.places;
            tournament.user_id = tournamentDto.user_id;
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        // DELETE <TournamentController>/tournaments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTournament(int id)
        {
            if (_dbContext.Tournaments == null)
            {
                return NotFound();
            }

            var tournament = await _dbContext.Tournaments.FindAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }
            _dbContext.Tournaments.Remove(tournament);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
