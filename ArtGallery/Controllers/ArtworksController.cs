using ArtGallery.Data;
using ArtGallery.Data.Migrations;
using ArtGallery.Models;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtGallery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtworksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ArtworksController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of all artworks along with their associated exhibitions.
        /// </summary>
        /// <returns>A list of ArtworkDto objects representing all artworks and their exhibitions.</returns>
        /// <example>
        /// -> [{"artworkId":7,"artworkTitle":"Whispers of the Wild","artworkMedium":"Watercolor","artworkYearCreated":2025,"artistID":1,"exhibitionCount":2},{"artworkId":8,"artworkTitle":"Starry Sun","artworkMedium":"Oil on canvas","artworkYearCreated":2023,"artistID":2,"exhibitionCount":1},{"artworkId":9,"artworkTitle":"The Night Show","artworkMedium":"Oil on canvas","artworkYearCreated":2024,"artistID":3,"exhibitionCount":2},{"artworkId":12,"artworkTitle":"Golden Horizon","artworkMedium":"Watercolor","artworkYearCreated":2024,"artistID":1,"exhibitionCount":3},{"artworkId":13,"artworkTitle":"Unbroken Spirit","artworkMedium":"Oil on canvas","artworkYearCreated":2019,"artistID":2,"exhibitionCount":0},{"artworkId":15,"artworkTitle":"New Artwork","artworkMedium":"Acrylic on Canvas","artworkYearCreated":2025,"artistID":5,"exhibitionCount":0}]
        /// </example>
        [HttpGet(template: "List")]
        public async Task<ActionResult<IEnumerable<ArtworkToListDto>>> List()
        {
            List<Artwork> Artworks = await _context.Artworks.Include(w => w.Exhibitions).ToListAsync();

            List<ArtworkToListDto> ArtworksDtos = new List<ArtworkToListDto>();

            foreach (Artwork Artwork in Artworks)
            {
                ArtworkToListDto ArtworkDto = new ArtworkToListDto();
                ArtworkDto.ArtworkId = Artwork.ArtworkID;
                ArtworkDto.ArtworkTitle = Artwork.ArtworkTitle;
                ArtworkDto.ArtworkMedium = Artwork.ArtworkMedium;
                ArtworkDto.ArtworkYearCreated = Artwork.ArtworkYearCreated;

                ArtworkDto.ArtistID = Artwork.ArtistID;
                ArtworkDto.ExhibitionCount = Artwork.Exhibitions?.Count ?? 0;

                ArtworksDtos.Add(ArtworkDto);
            }
            return ArtworksDtos;
        }

        /// <summary>
        /// Retrieves the details of a specific artwork along with its associated exhibitions by the artwork's ID.
        /// </summary>
        /// <param name="ArtworkID">The ID of the artwork to retrieve.</param>
        /// <returns>An ArtworkDto object representing the artwork and its exhibitions.</returns>
        /// <example>
        /// curl -X GET "https://localhost:7237/api/Artworks/FindArtwork/1"
        /// -> {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.5","title":"Not Found","status":404,"traceId":"00-5f0313a8428ff59a614f95e61069534a-6bce3e3b8336c57b-00"}
        /// curl -X GET "https://localhost:7237/api/Artworks/FindArtwork/7"
        /// -> {"artworkId":7,"artworkTitle":"Whispers of the Wild","artworkMedium":"Watercolor","artworkYearCreated":2025,"artistID":1,"listExhibitions":[{"exhibitionId":1,"exhibitionTitle":"A Symphony of Nature's Beauty"},{"exhibitionId":3,"exhibitionTitle":"Defaced! Money, Conflict, Protest"}]}
        /// </example>
        [HttpGet(template: "FindArtwork/{ArtworkID}")]
        public async Task<ActionResult<ArtworkItemDto>> FindArtwork(int ArtworkID)
        {
            Artwork? Artwork = await _context.Artworks.Include(w => w.Exhibitions).Where(w => w.ArtworkID == ArtworkID).FirstOrDefaultAsync();

            if (Artwork == null)
            {
                return NotFound();
            }

            ArtworkItemDto ArtworkDto = new ArtworkItemDto
            {
                ArtworkId = Artwork.ArtworkID,
                ArtworkTitle = Artwork.ArtworkTitle,
                ArtworkMedium = Artwork.ArtworkMedium,
                ArtworkYearCreated = Artwork.ArtworkYearCreated,

                HasArtworkPic = Artwork.HasPic,

                ArtistID = Artwork.ArtistID,
                ListExhibitions = Artwork.Exhibitions?.Select(w => new ExhibitionForOtherDto { ExhibitionId = w.ExhibitionID, ExhibitionTitle = w.ExhibitionTitle }).ToList()
            };
            if (Artwork.HasPic)
            {
                ArtworkDto.ArtworkImagePath = $"/image/artwork/{Artwork.ArtworkID}{Artwork.PicExtension}";
            }
            return ArtworkDto;
        }

        /// <summary>
        /// Updates the details of a specific artwork.
        /// </summary>
        /// <param name="ArtworkID">The ID of the artwork to update.</param>
        /// <param name="artworkDto">The updated artwork details.</param>
        /// <returns>An IActionResult indicating the result of the update operation.</returns>
        /// <example>
        /// curl -X PUT -H "Content-Type: application/json" -d "{\"artworkTitle\": \"Updated Artwork Title\", \"artworkMedium\": \"Oil on Canvas\", \"artworkYearCreated\": 2026, \"artistID\": 4}" "https://localhost:7237/api/Artworks/Update/16"
        /// -> {"message":"Invalid artwork data"}
        /// curl -X PUT -H "Content-Type: application/json" -d "{\"artworkTitle\": \" \", \"artworkMedium\": \"Oil on Canvas\", \"artworkYearCreated\": 2021, \"artistID\": 4}" "https://localhost:7237/api/Artworks/Update/16"
        /// -> {"message":"Invalid artwork data"}
        /// curl -X PUT -H "Content-Type: application/json" -d "{\"artworkTitle\": \"Updated Artwork Title\", \"artworkMedium\": \"Oil on Canvas\", \"artworkYearCreated\": 2021, \"artistID\": 4}" "https://localhost:7237/api/Artworks/Update/16"
        /// -> {"message":"Artist with ID 4 does not exist"}
        /// curl -X PUT -H "Content-Type: application/json" -d "{\"artworkTitle\": \"Updated Artwork Title\", \"artworkMedium\": \"Oil on Canvas\", \"artworkYearCreated\": 2021, \"artistID\": 11}" "https://localhost:7237/api/Artworks/Update/16"
        /// -> {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.5","title":"Not Found","status":404,"traceId":"00-c05cd8d1b3a58322c04703685e236050-a0ad45dde66b3b21-00"}
        /// curl -X PUT -H "Content-Type: application/json" -d "{\"artworkTitle\": \"Updated Artwork Title\", \"artworkMedium\": \"Oil on Canvas\", \"artworkYearCreated\": 2021, \"artistID\": 11}" "https://localhost:7237/api/Artworks/Update/15"
        /// -> (db updated: {"artworkId":15,"artworkTitle":"Updated Artwork Title","artworkMedium":"Oil on Canvas","artworkYearCreated":2021,"artistID":11,"listExhibitions":[{"exhibitionId":7,"exhibitionTitle":"New Exhibition"}]})
        /// </example>
        [HttpPut(template: "Update/{ArtworkID}")]
        [Authorize]
        public async Task<IActionResult> UpdateArtwork(int ArtworkID, [FromBody] ArtworkItemDto artworkDto)
        {
            if (string.IsNullOrWhiteSpace(artworkDto.ArtworkTitle) || string.IsNullOrWhiteSpace(artworkDto.ArtworkMedium) || artworkDto.ArtworkYearCreated < 0 || artworkDto.ArtworkYearCreated > DateTime.Now.Year)
            {
                return BadRequest(new { message = "Invalid artwork data" });
            }

            var artistExists = await _context.Artists.AnyAsync(a => a.ArtistID == artworkDto.ArtistID);
            if (!artistExists)
            {
                return BadRequest(new { message = $"Artist with ID {artworkDto.ArtistID} does not exist" });
            }

            // attempt to find associated artwork in DB by looking up ArtworkId
            var artworkGet = await _context.Artworks.FindAsync(ArtworkID);

            if (artworkGet == null)
            {
                return NotFound();
            }

            // Update the existing artwork entity with new values
            artworkGet.ArtworkTitle = artworkDto.ArtworkTitle;
            artworkGet.ArtworkMedium = artworkDto.ArtworkMedium;
            artworkGet.ArtworkYearCreated = artworkDto.ArtworkYearCreated;
            artworkGet.ArtistID = artworkDto.ArtistID;

            // flags that the object has changed
            _context.Entry(artworkGet).State = EntityState.Modified;

            try
            {
                // SQL Equivalent: Update artworks set ... where ArtworkID={ArtworkID}
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArtworkExists(ArtworkID))
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
        /// Adds a new artwork to the database.
        /// </summary>
        /// <param name="artworkDto">The details of the artwork to add.</param>
        /// <returns>The created Artwork object.</returns>
        /// <example>
        /// curl -X POST -H "Content-Type: application/json" -d "{\"artworkTitle\": \"New Artwork\", \"artworkMedium\": \"Acrylic on Canvas\", \"artworkYearCreated\": 2025, \"artistID\": 5}" "https://localhost:7237/api/Artworks/Add"
        /// -> {"artworkID":15,"artworkTitle":"New Artwork","artworkMedium":"Acrylic on Canvas","artworkYearCreated":2025,"artistID":5,"exhibitions":null}
        /// C:\Users\tongk>curl -X POST -H "Content-Type: application/json" -d "{\"artworkTitle\": \"New Artwork\", \"artworkMedium\": \"Acrylic on Canvas\", \"artworkYearCreated\": 2025, \"artistID\": 4}" "https://localhost:7237/api/Artworks/Add"
        /// -> {"message":"Artist with ID 4 does not exist"}
        /// curl -X POST -H "Content-Type: application/json" -d "{\"artworkTitle\": \"New Artwork\", \"artworkMedium\": \"Acrylic on Canvas\", \"artworkYearCreated\": 2026, \"artistID\": 5}" "https://localhost:7237/api/Artworks/Add"
        /// -> {"message":"Invalid artwork data"}
        /// curl -X POST -H "Content-Type: application/json" -d "{\"artworkTitle\": \" \", \"artworkMedium\": \"Acrylic on Canvas\", \"artworkYearCreated\": 2025, \"artistID\": 5}" "https://localhost:7237/api/Artworks/Add"
        /// -> {"message":"Invalid artwork data"}
        /// </example>
        [HttpPost(template: "Add")]
        [Authorize]
        public async Task<ActionResult<Artwork>> AddArtwork([FromBody] ArtworkItemDto artworkDto)
        {
            if (string.IsNullOrWhiteSpace(artworkDto.ArtworkTitle) || string.IsNullOrWhiteSpace(artworkDto.ArtworkMedium) || artworkDto.ArtworkYearCreated < 0 || artworkDto.ArtworkYearCreated > DateTime.Now.Year)
            {
                return BadRequest(new { message = "Invalid artwork data" });
            }

            var artistExists = await _context.Artists.AnyAsync(a => a.ArtistID == artworkDto.ArtistID);
            if (!artistExists)
            {
                return BadRequest(new { message = $"Artist with ID {artworkDto.ArtistID} does not exist" });
            }

            // Create a new Artwork entity from the ArtworkDto
            Artwork artwork = new Artwork
            {
                ArtworkTitle = artworkDto.ArtworkTitle,
                ArtworkMedium = artworkDto.ArtworkMedium,
                ArtworkYearCreated = artworkDto.ArtworkYearCreated,
                ArtistID = artworkDto.ArtistID
            };

            // Add the new artwork to the database
            _context.Artworks.Add(artwork);
            await _context.SaveChangesAsync();

            // Return the created artwork
            return CreatedAtAction("FindArtwork", new { ArtworkID = artwork.ArtworkID }, artwork);
        }

        /// <summary>
        /// Deletes a specific artwork from the database.
        /// </summary>
        /// <param name="id">The ID of the artwork to delete.</param>
        /// <returns>An IActionResult indicating the result of the delete operation.</returns>
        /// <example>
        /// curl -X DELETE "https://localhost:7237/api/Artworks/Delete/16"
        /// -> (db updated)
        /// curl -X DELETE "https://localhost:7237/api/Artworks/Delete/16"
        /// -> {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.5","title":"Not Found","status":404,"traceId":"00-ff1d5f16f07554eea66dc625c8550442-a44ec122e7a83285-00"}
        /// </example>
        [HttpDelete("Delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteArtwork(int id)
        {
            // Attempt to find the artwork in the database by ID
            var artwork = await _context.Artworks.FindAsync(id);
            if (artwork == null)
            {
                return NotFound();
            }

            // Remove the artwork from the database
            _context.Artworks.Remove(artwork);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }



        [HttpPost("UpdateArtworkImage/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateArtworkImage(int id, IFormFile ArtworkPic)
        {
            var artwork = await _context.Artworks.FindAsync(id);
            if (artwork == null)
            {
                return NotFound();
            }
            if (ArtworkPic == null)
            {
                return BadRequest(new { message = "Artwork picture is required." });
            }
            if (ArtworkPic.Length == 0)
            {
                return BadRequest(new { message = "Artwork picture cannot be empty." });
            }
            if (ArtworkPic.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new { message = "Artwork picture size cannot exceed 10MB." });
            }

            // remove old picture if exists
            if (artwork.HasPic)
            {
                string OldFileName = $"{artwork.ArtworkID}{artwork.PicExtension}";
                string OldFilePath = Path.Combine("wwwroot/image/artwork/", OldFileName);
                if (System.IO.File.Exists(OldFilePath))
                {
                    System.IO.File.Delete(OldFilePath);
                }
            }

            // establish valid file types (can be changed to other file extensions if desired!)
            List<string> Extensions = new List<string> { ".jpeg", ".jpg", ".png", ".gif" };
            string ArtworkPicExtension = Path.GetExtension(ArtworkPic.FileName).ToLowerInvariant();
            if (!Extensions.Contains(ArtworkPicExtension))
            {
                return BadRequest(new { message = "Invalid file type. Only .jpeg, .jpg, .png, and .gif are allowed." });
            }
            string FileName = $"{id}{ArtworkPicExtension}";
            string FilePath = Path.Combine("wwwroot/image/artwork/", FileName);
            using (var targetStream = System.IO.File.Create(FilePath))
            {
                ArtworkPic.CopyTo(targetStream);
            }

            // check if file was uploaded
            if (System.IO.File.Exists(FilePath))
            {
                artwork.PicExtension = ArtworkPicExtension;
                artwork.HasPic = true;

                _context.Entry(artwork).State = EntityState.Modified;

                try
                {
                    // SQL Equivalent: Update Artwork set HasPic=True, PicExtension={ext} where ArtworkId={id}
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArtworkExists(id))
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

            // If the file was not uploaded successfully, return a BadRequest
            return BadRequest(new { message = "Failed to upload artwork picture." });
        }


        private bool ArtworkExists(int id)
        {
            return _context.Artworks.Any(e => e.ArtworkID == id);
        }
    }
}