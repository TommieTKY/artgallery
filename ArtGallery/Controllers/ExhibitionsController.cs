using ArtGallery.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ArtGallery.Models;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Data.Migrations;
using Microsoft.AspNetCore.Authorization;

namespace ArtGallery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExhibitionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ExhibitionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of all exhibitions along with their associated artworks.
        /// </summary>
        /// <returns>A list of ExhibitionDto objects representing all exhibitions and their artworks.</returns>
        /// <example>
        /// curl -X "GET" "https://localhost:7145/api/Exhibitions/List"
        /// -> [{"exhibitionId":1,"exhibitionTitle":"A Symphony of Nature's Beauty","exhibitionDescription":"A Symphony of Nature's Beauty celebrates nature's beauty and spirit through evocative, harmony-filled artworks.","startDate":"2024-10-09","endDate":"2025-01-19","artworkCount":3},{"exhibitionId":3,"exhibitionTitle":"Defaced! Money, Conflict, Protest","exhibitionDescription":"The exhibition explores how artists and individuals have historically altered currency—through scratching, overprinting, and digital manipulation—as acts of dissent against government authority, spanning events from the French Revolution to the Black Lives Matter movement.","startDate":"2023-11-20","endDate":"2024-12-01","artworkCount":3},{"exhibitionId":4,"exhibitionTitle":"Feels like Home","exhibitionDescription":"The exhibition showcases photography and video works by the creative agency Sunday School, exploring contemporary notions of home through diverse Black identities.","startDate":"2023-05-06","endDate":"2024-06-16","artworkCount":2}]
        /// </example>
        [HttpGet(template: "List")]
        public async Task<ActionResult<IEnumerable<ExhibitionToListDto>>> List()
        {
            List<Exhibition> Exhibitions = await _context.Exhibitions.Include(e => e.Artworks).ToListAsync();

            List<ExhibitionToListDto> ExhibitionsDtos = new List<ExhibitionToListDto>();

            foreach (Exhibition Exhibition in Exhibitions)
            {
                ExhibitionToListDto ExhibitionDto = new ExhibitionToListDto();
                ExhibitionDto.ExhibitionId = Exhibition.ExhibitionID;
                ExhibitionDto.ExhibitionTitle = Exhibition.ExhibitionTitle;
                ExhibitionDto.ExhibitionDescription = Exhibition.ExhibitionDescription;
                ExhibitionDto.StartDate = Exhibition.StartDate;
                ExhibitionDto.EndDate = Exhibition.EndDate;
                ExhibitionDto.ArtworkCount = Exhibition.Artworks?.Count ?? 0;

                if (Exhibition.EndDate < DateOnly.FromDateTime(DateTime.Now))
                {
                    ExhibitionDto.Status = "Past";
                }
                else if (Exhibition.StartDate > DateOnly.FromDateTime(DateTime.Now))
                {
                    ExhibitionDto.Status = "Future";
                }
                else
                {
                    ExhibitionDto.Status = "Ongoing";
                }
                ExhibitionsDtos.Add(ExhibitionDto);
            }
            return ExhibitionsDtos;
        }


        /// <summary>
        /// This method receives an ExhibitionID and outputs the Exhibition associated with that ID.
        /// </summary>
        /// <param name="ExhibitionID">the exhibition ID primary key</param>
        /// <returns>a exhibition object</returns>
        /// <example>
        /// curl -X GET "https://localhost:7145/api/Exhibitions/FindExhibition/1"
        /// -> {"exhibitionId":1,"exhibitionTitle":"A Symphony of Nature's Beauty","exhibitionDescription":"A Symphony of Nature's Beauty celebrates nature's beauty and spirit through evocative, harmony-filled artworks.","startDate":"2024-10-09","endDate":"2025-01-19","listArtworks":[{"artworkId":7,"artworkTitle":"Whispers of the Wild"},{"artworkId":8,"artworkTitle":"Starry Sun"},{"artworkId":12,"artworkTitle":"Golden Horizon"}]}
        /// curl -X GET "https://localhost:7145/api/Exhibitions/FindExhibition/2"
        /// -> {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.5","title":"Not Found","status":404,"traceId":"00-8f6cd700f9fe1fabbd7abb0af8c224f1-081fc72a788077e9-00"}
        /// </example>
        [HttpGet(template: "FindExhibition/{ExhibitionID}")]
        public async Task<ActionResult<ExhibitionItemDto>> FindExhibition(int ExhibitionID)
        {
            Exhibition? Exhibition = await _context.Exhibitions.Include(e => e.Artworks).Where(e => e.ExhibitionID == ExhibitionID).FirstOrDefaultAsync();

            if (Exhibition == null)
            {
                return NotFound();
            }

            ExhibitionItemDto ExhibitionDto = new ExhibitionItemDto();

            ExhibitionDto.ExhibitionId = Exhibition.ExhibitionID;
            ExhibitionDto.ExhibitionTitle = Exhibition.ExhibitionTitle;
            ExhibitionDto.ExhibitionDescription = Exhibition.ExhibitionDescription;
            ExhibitionDto.StartDate = Exhibition.StartDate;
            ExhibitionDto.EndDate = Exhibition.EndDate;
            ExhibitionDto.ListArtworks = Exhibition.Artworks?.Select(e => new ArtworkForOtherDto { ArtworkId = e.ArtworkID, ArtworkTitle = e.ArtworkTitle }).ToList();

            return ExhibitionDto;
        }

        /// <summary>
        /// Updates the details of a specific exhibition.
        /// </summary>
        /// <param name="ExhibitionID">The ID of the exhibition to update.</param>
        /// <param name="exhibitionDto">The updated exhibition details.</param>
        /// <returns>An IActionResult indicating the result of the update operation.</returns>
        /// <example>
        /// curl -X PUT -H "Content-Type: application/json" -d "{\"exhibitionTitle\": \"Update Exhibition Title\", \"exhibitionDescription\": \"Updated description\", \"startDate\": \"2025-03-01\", \"endDate\": \"2025-02-28\"}" "https://localhost:7145/api/Exhibitions/Update/6"
        /// -> {"message":"Invalid exhibition data"}
        /// curl -X PUT -H "Content-Type: application/json" -d "{\"exhibitionTitle\": \" \", \"exhibitionDescription\": \"Updated description\", \"startDate\": \"2025-02-01\", \"endDate\": \"2025-02-28\"}" "https://localhost:7145/api/Exhibitions/Update/6"
        /// -> {"message":"Invalid exhibition data"}
        /// curl -X PUT -H "Content-Type: application/json" -d "{\"exhibitionTitle\": \"Update Exhibition Title\", \"exhibitionDescription\": \"Updated description\", \"startDate\": \"2025-02-01\", \"endDate\": \"2025-02-28\"}" "https://localhost:7145/api/Exhibitions/Update/6"
        /// -> (db updated)
        /// </example>
        [HttpPut("Update/{ExhibitionID}")]
        [Authorize]
        public async Task<IActionResult> UpdateExhibition(int ExhibitionID, [FromBody] ExhibitionItemDto exhibitionDto)
        {
            // attempt to find associated exhibition in DB by looking up ExhibitionId
            var exhibitionGet = await _context.Exhibitions.FindAsync(ExhibitionID);

            if (exhibitionGet == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(exhibitionDto.ExhibitionTitle) || string.IsNullOrWhiteSpace(exhibitionDto.ExhibitionDescription) || exhibitionDto.StartDate > exhibitionDto.EndDate)
            {
                return BadRequest(new { message = "Invalid exhibition data" });
            }

            exhibitionGet.ExhibitionTitle = exhibitionDto.ExhibitionTitle;
            exhibitionGet.ExhibitionDescription = exhibitionDto.ExhibitionDescription;
            exhibitionGet.StartDate = exhibitionDto.StartDate;
            exhibitionGet.EndDate = exhibitionDto.EndDate;

            _context.Entry(exhibitionGet).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExhibitionExists(ExhibitionID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        /// <summary>
        /// Adds a new exhibition to the database.
        /// </summary>
        /// <param name="exhibitionDto">The details of the exhibition to add.</param>
        /// <returns>The created Exhibition object.</returns>
        /// <example>
        /// curl -X POST -H "Content-Type: application/json" -d "{\"exhibitionTitle\": \"New Exhibition\", \"exhibitionDescription\": \"Description of the new exhibition\", \"startDate\": \"2025-04-01\", \"endDate\": \"2025-03-31\"}" "https://localhost:7145/api/Exhibitions/Add"
        /// -> {"message":"Invalid exhibition data"}
        /// curl -X POST -H "Content-Type: application/json" -d "{\"exhibitionTitle\": \" \", \"exhibitionDescription\": \"Description of the new exhibition\", \"startDate\": \"2025-03-01\", \"endDate\": \"2025-03-31\"}" "https://localhost:7145/api/Exhibitions/Add"
        /// -> {"message":"Invalid exhibition data"}
        /// curl -X POST -H "Content-Type: application/json" -d "{\"exhibitionTitle\": \"New Exhibition\", \"exhibitionDescription\": \"Description of the new exhibition\", \"startDate\": \"2025-03-01\", \"endDate\": \"2025-03-31\"}" "https://localhost:7145/api/Exhibitions/Add"
        /// -> {"exhibitionID":6,"exhibitionTitle":"New Exhibition","exhibitionDescription":"Description of the new exhibition","startDate":"2025-03-01","endDate":"2025-03-31","artworks":null}
        /// </example>
        [HttpPost(template: "Add")]
        [Authorize]
        public async Task<ActionResult<Exhibition>> AddExhibition([FromBody] ExhibitionItemDto exhibitionDto)
        {
            if (string.IsNullOrWhiteSpace(exhibitionDto.ExhibitionTitle) || string.IsNullOrWhiteSpace(exhibitionDto.ExhibitionDescription) || exhibitionDto.StartDate > exhibitionDto.EndDate)
            {
                return BadRequest(new { message = "Invalid exhibition data" });
            }

            Exhibition exhibition = new Exhibition
            {
                ExhibitionTitle = exhibitionDto.ExhibitionTitle,
                ExhibitionDescription = exhibitionDto.ExhibitionDescription,
                StartDate = exhibitionDto.StartDate,
                EndDate = exhibitionDto.EndDate
            };

            _context.Exhibitions.Add(exhibition);
            await _context.SaveChangesAsync();

            return CreatedAtAction("FindExhibition", new { ExhibitionID = exhibition.ExhibitionID }, exhibition);
        }

        /// <summary>
        /// Deletes a specific exhibition from the database.
        /// </summary>
        /// <param name="id">The ID of the exhibition to delete.</param>
        /// <returns>An IActionResult indicating the result of the delete operation.</returns>
        /// <example>
        /// curl -X DELETE "https://localhost:7145/api/Exhibitions/Delete/6"
        /// -> (db updated)
        /// curl -X DELETE "https://localhost:7145/api/Exhibitions/Delete/6"
        /// -> {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.5","title":"Not Found","status":404,"traceId":"00-f30dcb0dd93827d8269897b1689b5594-dcbde54f131bb15a-00"}
        /// </example>
        [HttpDelete("Delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteExhibition(int id)
        {
            var exhibition = await _context.Exhibitions.FindAsync(id);
            if (exhibition == null)
            {
                return NotFound();
            }

            _context.Exhibitions.Remove(exhibition);
            await _context.SaveChangesAsync();

            return NoContent(); 
        }

        /// <summary>
        /// Adds an artwork to a specific exhibition.
        /// </summary>
        /// <param name="ExhibitionID">The ID of the exhibition to which the artwork will be added.</param>
        /// <param name="artworkIdDto">The ID of the artwork to add.</param>
        /// <returns>An IActionResult indicating the result of the add operation.</returns>
        /// <example>
        /// curl -X POST -H "Content-Type: application/json" -d "{\"artworkId\": 9}" "https://localhost:7145/api/Exhibitions/AddArtwork/2"
        /// -> {"message":"Exhibition does not exist."}
        /// curl -X POST -H "Content-Type: application/json" -d "{\"artworkId\": 16}" "https://localhost:7145/api/Exhibitions/AddArtwork/7"
        /// ->{"message":"Artwork does not exist."}
        /// curl -X POST -H "Content-Type: application/json" -d "{\"artworkId\": 15}" "https://localhost:7145/api/Exhibitions/AddArtwork/7"
        /// -> (db updated: {"exhibitionId":7,"exhibitionTitle":"New Exhibition","exhibitionDescription":"Description of the new exhibition","startDate":"2025-03-01","endDate":"2025-03-31","listArtworks":[{"artworkId":15,"artworkTitle":"New Artwork"}]})
        /// </example>
        [HttpPost("AddArtwork/{ExhibitionID}")]
        [Authorize]
        public async Task<ActionResult<Exhibition>> AddArtworkToExhibition(int ExhibitionID, [FromBody] ArtworkIdDto artworkIdDto)
        {
            Exhibition? exhibition = await _context.Exhibitions.Include(e => e.Artworks).Where(e => e.ExhibitionID == ExhibitionID).FirstOrDefaultAsync();

            if (exhibition == null)
            {
                return NotFound(new { message = "Exhibition does not exist." });
            }

            Artwork? Artwork = await _context.Artworks.Where(e => e.ArtworkID == artworkIdDto.ArtworkId).FirstOrDefaultAsync();

            if (Artwork == null)
            {
                return NotFound(new { message = "Artwork does not exist." });
            }

            exhibition.Artworks?.Add(Artwork);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExhibitionExists(ExhibitionID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        /// <summary>
        /// Removes an artwork from a specific exhibition.
        /// </summary>
        /// <param name="ExhibitionID">The ID of the exhibition from which the artwork will be removed.</param>
        /// <param name="artworkIdDto">The ID of the artwork to remove.</param>
        /// <returns>An IActionResult indicating the result of the delete operation.</returns>
        /// <example>
        /// curl -X DELETE -H "Content-Type: application/json" -d "{\"artworkId\": 14}" "https://localhost:7145/api/Exhibitions/DeleteArtwork/6"
        /// -> {"message":"Exhibition does not exist."}
        /// curl -X DELETE -H "Content-Type: application/json" -d "{\"artworkId\": 9}" "https://localhost:7145/api/Exhibitions/DeleteArtwork/3"
        /// -> (db updated)
        /// curl -X DELETE -H "Content-Type: application/json" -d "{\"artworkId\": 9}" "https://localhost:7145/api/Exhibitions/DeleteArtwork/3"
        /// -> {"message":"Artwork does not exist."}
        /// </example>
        [HttpDelete("DeleteArtwork/{ExhibitionID}")]
        [Authorize]
        public async Task<IActionResult> DeleteArtworkFromExhibition(int ExhibitionID, [FromBody] ArtworkIdDto artworkIdDto)
        {
            Exhibition? exhibition = await _context.Exhibitions.Include(e => e.Artworks).Where(e => e.ExhibitionID == ExhibitionID).FirstOrDefaultAsync();

            if (exhibition == null)
            {
                return NotFound(new { message = "Exhibition does not exist." });
            }

            Artwork? artwork = exhibition.Artworks?.FirstOrDefault(a => a.ArtworkID == artworkIdDto.ArtworkId);

            if (artwork == null)
            {
                return NotFound(new { message = "Artwork does not exist." });
            }

            exhibition.Artworks?.Remove(artwork);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExhibitionExists(ExhibitionID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        private bool ExhibitionExists(int id)
        {
            return _context.Exhibitions.Any(e => e.ExhibitionID == id);
        }
    }
}