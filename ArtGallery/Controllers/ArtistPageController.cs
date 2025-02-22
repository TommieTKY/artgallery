using Microsoft.AspNetCore.Mvc;
using ArtGallery.Models;
using Azure;
using Microsoft.AspNetCore.Authorization;

namespace ArtGallery.Controllers
{
    public class ArtistPageController : Controller
    {
        private readonly ArtistsController _api;
        public ArtistPageController(ArtistsController api)
        {
            _api = api;
        }

        // GET: ArtistPage/List -> A webpage that shows all artists in the db
        [HttpGet]
        public IActionResult List()
        {
            List<ArtistToListDto> artists = _api.List().Result.Value.ToList();
            return View(artists);
        }

        // GET: ArtistPage/Details/{id} -> A webpage that displays an artist by the Artist’s ID
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var selectedArtist = (await _api.FindArtist(id)).Value;
            return View(selectedArtist);
        }

        // GET: ArtistPage/New -> A webpage that prompts the user to enter new artist information
        [HttpGet]
        [Authorize]
        public IActionResult New()
        {
            return View();
        }

        // POST: ArtistPage/Create -> Handles the creation of a new artist
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(string artistName, string artistBiography)
        {
            var newArtist = new ArtistPersonDto
            {
                ArtistName = artistName,
                ArtistBiography = artistBiography
            };

            var result = await _api.AddArtist(newArtist);

            if (result.Result == null)
            {
                return View("Error");
            }

            var artist = ((CreatedResult)result.Result).Value;

            if (artist == null)
            {
                return View("Error");
            }

            return RedirectToAction("Details", new { id = ((Artist)artist).ArtistID });
        }

        // GET: ArtistPage/ConfirmDelete/{id} -> A webpage that prompts the user to confirm the deletion of an artist
        [HttpGet]
        [Authorize]
        public IActionResult ConfirmDelete(int id)
        {
            var selectedArtist = _api.FindArtist(id).Result.Value;
            return View(selectedArtist);
        }

        // POST: ArtistPage/Delete/{id} -> Handles the deletion of an artist
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _api.DeleteArtist(id);
            return RedirectToAction("List");
        }

        // GET: ArtistPage/Edit/{id} -> A webpage that prompts the user to edit an artist's information
        [HttpGet]
        [Authorize]
        public IActionResult Edit(int id)
        {
            var selectedArtist = _api.FindArtist(id).Result.Value;
            return View(selectedArtist);
        }

        // POST: ArtistPage/Update/{id} -> Handles the update of an artist's information
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Update(int id, string artistName, string artistBiography)
        {
            var updateArtist = new ArtistPersonDto
            {
                ArtistId = id,
                ArtistName = artistName,
                ArtistBiography = artistBiography
            };

            var result = await _api.UpdateArtist(id, updateArtist);

            if (result is NoContentResult)
            {
                return RedirectToAction("Details", new { id = id });
            }
            return View("Error");
        }
    }
}
