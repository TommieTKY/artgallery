using Microsoft.AspNetCore.Mvc;
using ArtGallery.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ArtGallery.Controllers
{
    public class ArtworkPageController : Controller
    {
        private readonly ArtworksController _artworkApi;
        private readonly ArtistsController _artistsApi;

        public ArtworkPageController(ArtworksController artworkApi, ArtistsController artistsApi)
        {
            _artworkApi = artworkApi;
            _artistsApi = artistsApi;
        }

        // GET: ArtworkPage/List -> A webpage that shows all artworks in the db
        [HttpGet]
        public IActionResult List()
        {
            List<ArtworkToListDto> artworks = _artworkApi.List().Result.Value.ToList();
            return View(artworks);
        }

        // GET: ArtworkPage/Details/{id} -> A webpage that displays an artwork by the Artwork’s ID
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var selectedArtwork = (await _artworkApi.FindArtwork(id)).Value;

            if (selectedArtwork == null)
            {
                return NotFound();
            }

            var artist = (await _artistsApi.FindArtist(selectedArtwork.ArtistID)).Value;

            var viewModel = new ViewArtworkDetails
            {
                Artwork = selectedArtwork,
                Artist = artist
            };

            return View(viewModel);
        }

        // GET: ArtworkPage/New -> A webpage that prompts the user to enter new artwork information
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> New()
        {
            var artists = await _artistsApi.List();
            var model = new ViewArtworkEdit
            {
                Artwork = new ArtworkItemDto(),
                ArtistList = artists.Value.ToList()
            };
            ViewData["Title"] = "New Artwork";
            return View(model);
        }

        // POST: ArtworkPage/Create -> Handles the creation of a new artwork
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(ViewArtworkEdit model)
        {
            if (!ModelState.IsValid)
            {
                model.ArtistList = (await _artistsApi.List()).Value.ToList();
                return View("New", model);
            }

            var newArtwork = new ArtworkItemDto
            {
                ArtworkTitle = model.Artwork.ArtworkTitle,
                ArtworkMedium = model.Artwork.ArtworkMedium,
                ArtworkYearCreated = model.Artwork.ArtworkYearCreated,
                ArtistID = model.Artwork.ArtistID
            };

            var result = await _artworkApi.AddArtwork(newArtwork);
            if (result.Result == null)
            {
                return View("Error");
            }

            var createdResult = result.Result as CreatedAtActionResult;
            if (createdResult?.Value == null)
            {
                return View("Error");
            }

            var artwork = createdResult.Value as Artwork;
            if (artwork == null)
            {
                return View("Error");
            }

            if (model.ArtworkPic != null && model.ArtworkPic.Length > 0)
            {
                var imageResult = await _artworkApi.UpdateArtworkImage(artwork.ArtworkID, model.ArtworkPic);
                if (imageResult is NoContentResult)
                {
                    newArtwork.HasArtworkPic = true;
                    newArtwork.ArtworkImagePath = $"/image/artwork/{artwork.ArtworkID}{Path.GetExtension(model.ArtworkPic.FileName)}";
                }
                else
                {
                    var errorViewModel1 = new ErrorViewModel
                    {
                        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                    };
                    return View("Error", errorViewModel1);
                }
            }

            return RedirectToAction("Details", new { id = artwork.ArtworkID });
        }

        // GET: ArtworkPage/ConfirmDelete/{id} -> A webpage that prompts the user to confirm the deletion of an artwork
        [HttpGet]
        [Authorize]
        public IActionResult ConfirmDelete(int id)
        {
            var selectedArtwork = _artworkApi.FindArtwork(id).Result.Value;
            return View(selectedArtwork);
        }

        // POST: ArtworkPage/Delete/{id} -> Handles the deletion of an artwork
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _artworkApi.DeleteArtwork(id);
            return RedirectToAction("List");
        }

        // GET: ArtworkPage/Edit/{id} -> A webpage that prompts the user to edit an artwork's information
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var selectedArtwork = await _artworkApi.FindArtwork(id);
            if (selectedArtwork.Value is ArtworkItemDto artworkItemDto)
            {
                var artists = await _artistsApi.List();
                var artworkDetails = new ViewArtworkEdit
                {
                    Artwork = artworkItemDto,
                    ArtistList = artists.Value.ToList()
                };
                return View(artworkDetails);
            }
            return View("Error");
        }

        // POST: ArtworkPage/Update -> Handles the update of an artwork's information
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Update(ViewArtworkEdit model)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", model);
            }

            var updateArtwork = new ArtworkItemDto
            {
                ArtworkId = model.Artwork.ArtworkId,
                ArtworkTitle = model.Artwork.ArtworkTitle,
                ArtworkMedium = model.Artwork.ArtworkMedium,
                ArtworkYearCreated = model.Artwork.ArtworkYearCreated,
                ArtistID = model.Artwork.ArtistID
            };

            if (model.ArtworkPic != null && model.ArtworkPic.Length > 0)
            {
                var result = await _artworkApi.UpdateArtworkImage(model.Artwork.ArtworkId, model.ArtworkPic);
                if (result is NoContentResult)
                {
                    updateArtwork.HasArtworkPic = true;
                    updateArtwork.ArtworkImagePath = $"/image/artwork/{model.Artwork.ArtworkId}{Path.GetExtension(model.ArtworkPic.FileName)}";
                }
                else
                {
                    var errorViewModel1 = new ErrorViewModel
                    {
                        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                    };
                    return View("Error", errorViewModel1);
                }
            }

            var updateResult = await _artworkApi.UpdateArtwork(model.Artwork.ArtworkId, updateArtwork);

            if (updateResult is NoContentResult)
            {
                return RedirectToAction("Details", new { id = model.Artwork.ArtworkId });
            }

            var errorViewModel2 = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View("Error", errorViewModel2);
        }
    }
}
