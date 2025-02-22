namespace ArtGallery.Models
{
    public class ViewArtworkEdit
    {
        public required ArtworkItemDto Artwork { get; set; }
        public IEnumerable<ArtistToListDto> ArtistList { get; set; } = new List<ArtistToListDto>();
        public IFormFile? ArtworkPic { get; set; }
    }
}
