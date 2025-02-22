using ArtGallery.Data.Migrations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtGallery.Models
{
    public class Artwork
    {
        [Key]
        public int ArtworkID { get; set; }
        public string ArtworkTitle { get; set; }
        public string ArtworkMedium { get; set; }
        public int ArtworkYearCreated { get; set; }

        [ForeignKey("Artists")]
        public int ArtistID { get; set; }
        public virtual Artist? Artist { get; set; }

        public bool HasPic { get; set; } = false;

        // images stored in /wwwroot/image/artwork/{ArtworkId}.{PicExtension}
        public string? PicExtension { get; set; }

        //An artwork can be featured in many exhibitions.
        public ICollection<Exhibition>? Exhibitions { get; set; }
    }

    public class ArtworkToListDto
    {
        public int ArtworkId { get; set; }
        public string ArtworkTitle { get; set; }
        public string ArtworkMedium { get; set; }
        public int ArtworkYearCreated { get; set; }
        public int ArtistID { get; set; }
        public int ExhibitionCount { get; set; }
    }

    public class ArtworkItemDto
    {
        public int ArtworkId { get; set; }
        public string ArtworkTitle { get; set; }
        public string ArtworkMedium { get; set; }
        public int ArtworkYearCreated { get; set; }

        public bool HasArtworkPic { get; set; }=false;

        // images stored in /wwwroot/image/artwork/{ArtworkId}.{PicExtension}
        public string? ArtworkImagePath { get; set; }
        public int ArtistID { get; set; }
        public List<ExhibitionForOtherDto>? ListExhibitions { get; set; }
    }


    public class ArtworkForOtherDto
    {
        public int ArtworkId { get; set; }
        public string ArtworkTitle { get; set; }
    }

    public class ArtworkIdDto
    {
        public int ArtworkId { get; set; }
    }

}