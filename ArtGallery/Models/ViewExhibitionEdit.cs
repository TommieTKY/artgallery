namespace ArtGallery.Models
{
    public class ViewExhibitionEdit
    {
        public required ExhibitionItemDto Exhibition { get; set; }
        public IEnumerable<ArtworkToListDto> ArtworkList { get; set; }
    }
}
