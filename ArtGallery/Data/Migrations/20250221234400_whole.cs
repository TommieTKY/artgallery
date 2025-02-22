using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtGallery.Data.Migrations
{
    /// <inheritdoc />
    public partial class whole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Artists",
                columns: table => new
                {
                    ArtistID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArtistName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArtistBiography = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artists", x => x.ArtistID);
                });

            migrationBuilder.CreateTable(
                name: "Exhibitions",
                columns: table => new
                {
                    ExhibitionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExhibitionTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExhibitionDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exhibitions", x => x.ExhibitionID);
                });

            migrationBuilder.CreateTable(
                name: "Artworks",
                columns: table => new
                {
                    ArtworkID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArtworkTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArtworkMedium = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArtworkYearCreated = table.Column<int>(type: "int", nullable: false),
                    ArtistID = table.Column<int>(type: "int", nullable: false),
                    HasPic = table.Column<bool>(type: "bit", nullable: false),
                    PicExtension = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artworks", x => x.ArtworkID);
                    table.ForeignKey(
                        name: "FK_Artworks_Artists_ArtistID",
                        column: x => x.ArtistID,
                        principalTable: "Artists",
                        principalColumn: "ArtistID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtworkExhibition",
                columns: table => new
                {
                    ArtworksArtworkID = table.Column<int>(type: "int", nullable: false),
                    ExhibitionsExhibitionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtworkExhibition", x => new { x.ArtworksArtworkID, x.ExhibitionsExhibitionID });
                    table.ForeignKey(
                        name: "FK_ArtworkExhibition_Artworks_ArtworksArtworkID",
                        column: x => x.ArtworksArtworkID,
                        principalTable: "Artworks",
                        principalColumn: "ArtworkID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtworkExhibition_Exhibitions_ExhibitionsExhibitionID",
                        column: x => x.ExhibitionsExhibitionID,
                        principalTable: "Exhibitions",
                        principalColumn: "ExhibitionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArtworkExhibition_ExhibitionsExhibitionID",
                table: "ArtworkExhibition",
                column: "ExhibitionsExhibitionID");

            migrationBuilder.CreateIndex(
                name: "IX_Artworks_ArtistID",
                table: "Artworks",
                column: "ArtistID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtworkExhibition");

            migrationBuilder.DropTable(
                name: "Artworks");

            migrationBuilder.DropTable(
                name: "Exhibitions");

            migrationBuilder.DropTable(
                name: "Artists");
        }
    }
}
