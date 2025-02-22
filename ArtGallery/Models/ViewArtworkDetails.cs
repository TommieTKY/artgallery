namespace ArtGallery.Models
{
    public class ViewArtworkDetails
    {
        public required ArtworkItemDto Artwork { get; set; }
        public ArtistPersonDto Artist { get; set; }
    }
}
