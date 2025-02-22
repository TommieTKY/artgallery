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

            // Direct to the /Views/ArtistPage/List.cshtml
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

        [HttpGet]
        [Authorize]
        public IActionResult ConfirmDelete(int id)
        {
            var selectedArtist = _api.FindArtist(id).Result.Value;
            return View(selectedArtist);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _api.DeleteArtist(id);
            return RedirectToAction("List");
        }

        [HttpGet]
        [Authorize]
        public IActionResult Edit(int id)
        {
            var selectedArtist = _api.FindArtist(id).Result.Value;
            return View(selectedArtist);
        }


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