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
        private readonly ArtworksController _api;
        private readonly ArtistsController _artistsApi;

        public ArtworkPageController(ArtworksController api, ArtistsController artistsApi)
        {
            _api = api;
            _artistsApi = artistsApi;
        }

        // GET: ArtworkPage/List -> A webpage that shows all artworks in the db
        [HttpGet]
        public IActionResult List()
        {
            List<ArtworkToListDto> artworks = _api.List().Result.Value.ToList();

            // Direct to the /Views/ArtworkPage/List.cshtml
            return View(artworks);
        }

        // GET: ArtworkPage/Details/{id} -> A webpage that displays an artwork by the Artwork’s ID
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var selectedArtwork = (await _api.FindArtwork(id)).Value;

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


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(ViewArtworkEdit model)
        {
            if (!ModelState.IsValid)
            {
                // Populate the ArtistList again in case of validation errors
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

            var result = await _api.AddArtwork(newArtwork);
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
                var imageResult = await _api.UpdateArtworkImage(artwork.ArtworkID, model.ArtworkPic);
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


        [HttpGet]
        [Authorize]
        public IActionResult ConfirmDelete(int id)
        {
            var selectedArtwork = _api.FindArtwork(id).Result.Value;
            return View(selectedArtwork);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _api.DeleteArtwork(id);
            return RedirectToAction("List");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var selectedArtwork = await _api.FindArtwork(id);
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Update(ViewArtworkEdit model)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", model); // Return the Edit view with the model to display validation errors
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
                var result = await _api.UpdateArtworkImage(model.Artwork.ArtworkId, model.ArtworkPic);
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

            var updateResult = await _api.UpdateArtwork(model.Artwork.ArtworkId, updateArtwork);

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
