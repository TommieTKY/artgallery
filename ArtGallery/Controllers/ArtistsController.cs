﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using ArtGallery.Data;
using ArtGallery.Models;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Data.Migrations;

using Microsoft.AspNetCore.Authorization;

namespace ArtGallery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ArtistsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of all artists along with their artworks.
        /// </summary>
        /// <returns>A list of ArtistDto objects representing all artists and their artworks.</returns>
        /// <example>
        /// curl -X "GET" "https://localhost:7237/api/Artists/List"
        /// -> [{"artistId":1,"artistName":"Kit Ying Tong","artistBiography":"Kit creates nature-inspired watercolor art blending realism and abstraction.\r\n","artworkCount":2},{"artistId":2,"artistName":"Kelly Sun","artistBiography":"A Mexican artist celebrated for her deeply personal and surreal self-portraits that explored identity, pain, and resilience.","artworkCount":2},{"artistId":3,"artistName":"Claude Monet","artistBiography":"A pioneering French Impressionist painter, revolutionized art with his vibrant depictions of light, color, and nature.","artworkCount":1},{"artistId":5,"artistName":"Georgia O’Keeffe","artistBiography":"An American modernist painter known for her bold, abstract depictions of flowers, landscapes, and skyscrapers.","artworkCount":1},{"artistId":8,"artistName":"Yayoi Kusama","artistBiography":"A Japanese contemporary artist known for her immersive installations, polka dot patterns, and Infinity Mirror Rooms.","artworkCount":0}]
        /// </example>
        [HttpGet(template: "List")]
        public async Task<ActionResult<IEnumerable<ArtistToListDto>>> List()
        {
            List<Artist> Artists = await _context.Artists.Include(a => a.Artworks).ToListAsync();

            List<ArtistToListDto> ArtistsDtos = new List<ArtistToListDto>();

            foreach (Artist Artist in Artists)
            {
                ArtistToListDto ArtistDto = new ArtistToListDto();
                ArtistDto.ArtistId = Artist.ArtistID;
                ArtistDto.ArtistName = Artist.ArtistName;
                ArtistDto.ArtistBiography = Artist.ArtistBiography;
                ArtistDto.ArtworkCount = Artist.Artworks?.Count ?? 0;
                ArtistsDtos.Add(ArtistDto);
            }
            return ArtistsDtos;
        }

        /// <summary>
        /// Retrieves the details of a specific artist along with their artworks by the artist's ID.
        /// </summary>
        /// <param name="ArtistID">The ID of the artist to retrieve.</param>
        /// <returns>An ArtistDto object representing the artist and their artworks.</returns>
        /// <example>
        /// curl -X GET "https://localhost:7237/api/Artists/FindArtist/1"
        /// -> {"artistId":1,"artistName":"Kit Ying Tong","artistBiography":"Kit creates nature-inspired watercolor art blending realism and abstraction.\r\n","listArtworks":[{"artworkId":7,"artworkTitle":"Whispers of the Wild"},{"artworkId":12,"artworkTitle":"Golden Horizon"}]}
        /// curl -X GET "https://localhost:7237/api/Artists/FindArtist/0"
        /// -> {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.5","title":"Not Found","status":404,"traceId":"00-9de72146fb4ca16baf1d9a249a874d60-451f0005608d1fb1-00"}
        /// </example>
        [HttpGet(template: "FindArtist/{ArtistID}")]
        public async Task<ActionResult<ArtistPersonDto>> FindArtist(int ArtistID)
        {
            Artist Artist = await _context.Artists.Include(a => a.Artworks).Where(a => a.ArtistID == ArtistID).FirstOrDefaultAsync();

            if (Artist == null)
            {
                return NotFound();
            }

            ArtistPersonDto ArtistDto = new ArtistPersonDto();

            ArtistDto.ArtistId = Artist.ArtistID;
            ArtistDto.ArtistName = Artist.ArtistName;
            ArtistDto.ArtistBiography = Artist.ArtistBiography;
            ArtistDto.ListArtworks = Artist.Artworks?.Select(a => new ArtworkForOtherDto { ArtworkId = a.ArtworkID, ArtworkTitle = a.ArtworkTitle }).ToList();

            return ArtistDto;
        }

        /// <summary>
        /// Updates the details of a specific artist.
        /// </summary>
        /// <param name="ArtistID">The ID of the artist to update.</param>
        /// <param name="artistDto">The updated artist details.</param>
        /// <returns>An IActionResult indicating the result of the update operation.</returns>
        /// <example>
        /// curl -X PUT -H "Content-Type: application/json" -d "{\"artistName\": \"Updated artist.\",  \"artistBiography\": \"Updated biography.\"}" "https://localhost:7237/api/Artists/Update/4"
        /// -> {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.5","title":"Not Found","status":404,"traceId":"00-0053d9ffaac0984a2275ba9aa88304cf-cca629ce9c6f8e07-00"}
        /// curl -X PUT -H "Content-Type: application/json" -d "{\"artistName\": \" \",  \"artistBiography\": \" \"}" "https://localhost:7237/api/Artists/Update/11"
        /// -> {"message":"Invalid artist data"}
        /// curl -X PUT -H "Content-Type: application/json" -d "{\"artistName\": \"Updated artist.\",  \"artistBiography\": \"Updated biography.\"}" "https://localhost:7237/api/Artists/Update/11"
        /// -> (db updated: {"artistId":11,"artistName":"Updated artist.","artistBiography":"Updated biography.","listArtworks":[]})
        /// </example>
        [HttpPut(template: "Update/{ArtistID}")]
        [Authorize]
        public async Task<IActionResult> UpdateArtist(int ArtistID, [FromBody] ArtistPersonDto artistDto)
        {
            if (string.IsNullOrWhiteSpace(artistDto.ArtistName) || string.IsNullOrWhiteSpace(artistDto.ArtistBiography))
            {
                return BadRequest(new { message = "Invalid artist data" });
            }

            // attempt to find associated artist in DB by looking up ArtistId
            var artistGet = await _context.Artists.FindAsync(ArtistID);
            // Posted data must link to valid entity
            if (artistGet == null)
            {
                return NotFound(); //404 Not Found
            }

            // Update the existing artist entity with new values
            artistGet.ArtistName = artistDto.ArtistName;
            artistGet.ArtistBiography = artistDto.ArtistBiography;

            // flags that the object has changed
            _context.Entry(artistGet).State = EntityState.Modified;

            try
            {
                // SQL Equivalent: Update artists set ... where Artist={ArtistID}
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArtistExists(ArtistID))
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
        /// Adds a new artist to the database.
        /// </summary>
        /// <param name="artistDto">The details of the artist to add.</param>
        /// <returns>The created Artist object.</returns>
        /// <example>
        /// curl -X POST -H "Content-Type: application/json" -d "{\"artistName\": \"New Artist\",  \"artistBiography\": \"A new artist.\"}" "https://localhost:7237/api/Artists/Add"
        /// -> {"artistID":7,"artistName":"New Artist","artistBiography":"A new artist.","artworks":null}
        /// curl -X POST -H "Content-Type: application/json" -d "{\"artistName\": \" \",  \"artistBiography\": \" \"}" "https://localhost:7237/api/Artists/Add"
        /// -> {"message":"Invalid artist data"}
        /// </example>
        [HttpPost(template: "Add")]
        [Authorize]
        public async Task<ActionResult<Artist>> AddArtist([FromBody] ArtistPersonDto artistDto)
        {
            if (string.IsNullOrWhiteSpace(artistDto.ArtistName) || string.IsNullOrWhiteSpace(artistDto.ArtistBiography))
            {
                return BadRequest(new { message = "Invalid artist data" });
            }

            // Create a new Artist entity from the ArtistDto
            Artist artist = new Artist
            {
                ArtistName = artistDto.ArtistName,
                ArtistBiography = artistDto.ArtistBiography
            };

            // Add the new artist to the database
            _context.Artists.Add(artist);
            await _context.SaveChangesAsync();

            // Return the created artist
            return Created($"api/Artist/FindArtist/{artist.ArtistID}", artist);
        }

        /// <summary>
        /// Deletes a specific artist from the database.
        /// </summary>
        /// <param name="id">The ID of the artist to delete.</param>
        /// <returns>An IActionResult indicating the result of the delete operation.</returns>
        /// <example>
        /// curl -X DELETE "https://localhost:7237/api/Artists/Delete/6"
        /// -> {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.5","title":"Not Found","status":404,"traceId":"00-a435288b2706e7eae7101e10bd768d6b-a5074cf07403ca79-00"}
        /// curl -X DELETE "https://localhost:7237/api/Artists/Delete/7"
        /// (db updated)
        /// </example>
        [HttpDelete("Delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteArtist(int id)
        {
            // Attempt to find the artist in the database by ID
            var artist = await _context.Artists.FindAsync(id);
            if (artist == null)
            {
                return NotFound();
            }

            // Remove the artist from the database
            _context.Artists.Remove(artist);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }

        ///// <summary>
        ///// Retrieves a list of artworks associated with a specific artist.
        ///// </summary>
        ///// <param name="id">The ID of the artist to retrieve artworks for.</param>
        ///// <returns>A list of ArtworkDto objects representing the artworks associated with the artist.</returns>
        ///// <example>
        ///// curl -X GET "https://localhost:7237/api/Artists/ListArtworkForArtist/1"
        ///// -> [{"artworkId":7,"artworkTitle":"Whispers of the Wild","artworkMedium":"Watercolor","artworkYearCreated":2025,"artistID":1,"listExhibitions":null},{"artworkId":12,"artworkTitle":"Golden Horizon","artworkMedium":"Watercolor","artworkYearCreated":2024,"artistID":1,"listExhibitions":null}]
        ///// curl -X GET "https://localhost:7237/api/Artists/ListArtworkForArtist/0"
        ///// -> {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.5","title":"Not Found","status":404,"traceId":"00-4bfbdc0f7cfee647ae49d63bd9276049-c71cdc402f1796a6-00"}
        ///// </example>
        //[HttpGet(template: "ListArtworkForArtist/{id}")]
        //public async Task<ActionResult<IEnumerable<ArtworkDto>>> ListArtworkForArtist(int id)
        //{
        //    // Retrieve the artist with the specified ID, including the associated artworks
        //    Artist artist = await _context.Artists.Include(a => a.Artworks).Where(a => a.ArtistID == id).FirstOrDefaultAsync();

        //    if (artist == null)
        //    {
        //        return NotFound();
        //    }

        //    // Map the artworks to ArtworkDto objects
        //    List<ArtworkDto> artworkDtos = artist.Artworks.Select(artwork => new ArtworkDto
        //    {
        //        ArtworkId = artwork.ArtworkID,
        //        ArtworkTitle = artwork.ArtworkTitle,
        //        ArtworkMedium = artwork.ArtworkMedium,
        //        ArtworkYearCreated = artwork.ArtworkYearCreated,
        //        ArtistID = artwork.ArtistID
        //    }).ToList();

        //    return artworkDtos;
        //}

        private bool ArtistExists(int id)
        {
            return _context.Artists.Any(e => e.ArtistID == id);
        }
    }
}