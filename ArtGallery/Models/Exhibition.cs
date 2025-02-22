using System.ComponentModel.DataAnnotations;

namespace ArtGallery.Models
{
    public class Exhibition
    {
        [Key]
        public int ExhibitionID { get; set; }
        public string ExhibitionTitle { get; set; }
        public string ExhibitionDescription { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        // An exhibition can include many artworks.
        public ICollection<Artwork>? Artworks { get; set; }

    }

    public class ExhibitionToListDto
    {
        public int ExhibitionId { get; set; }
        public string ExhibitionTitle { get; set; }
        public string ExhibitionDescription { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int ArtworkCount { get; set; }
    }

    public class ExhibitionItemDto
    {
        public int ExhibitionId { get; set; }
        public string ExhibitionTitle { get; set; }
        public string ExhibitionDescription { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public List<ArtworkForOtherDto>? ListArtworks { get; set; }
    }

    public class ExhibitionForOtherDto
    {
        public int ExhibitionId { get; set; }
        public string ExhibitionTitle { get; set; }
    }

    public class ExhibitionArtwork
    {
        public ICollection<Artwork>? Artworks { get; set; }
        public ICollection<Exhibition>? Exhibitions { get; set; }

    }


}
