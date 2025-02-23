# Art Gallery

---

ArtGallery is a web application built with Blazor and ASP.NET Core MVC that provides a comprehensive platform for managing and exploring art galleries. The application allows guests to view lists and detailed information about artworks, artists, and exhibitions. Registered users have additional capabilities to create, edit, and delete artworks, artists, and exhibitions, making it a versatile tool for both art enthusiasts and gallery administrators.

Presentation Video:
[presentation](https://youtu.be/D-xBUHYTuz4)

---

## Features

• Artists Management: View, find, add, update, and delete artists.

• Artworks Management: View, find, add, update, and delete artworks.

• Exhibitions Management: View, find, add, update, and delete exhibitions.

• Artwork-Artist Association: Update the artist from artworks.

• Artwork-Exhibition Association: Add and remove artworks from exhibitions.

• View Artists: View a list of artists with details and associated artists.

• View Artworks: View a list of artworks with details and associated artworks.

• View Exhibitions: View a list of exhibitions with details and associated exhibitions..

• Extra features:

- Authentication: Secure authentication for creating, editing, and deleting content.
- Artwork Image Management: Upload and display images for artworks.
- Exhibition Status: View a list of exhibitions with their status (past, ongoing, future)

## Technologies Used

- .NET 8
- Blazor
- ASP.NET Core MVC
- Entity Framework Core
- Bootstrap

## Project Structure

• Controllers: Contains API controllers for managing artists, artworks, and exhibitions.

• Models: Contains data models representing artists, artworks, and exhibitions.

• Data: Contains the database context and migrations.

• Views: Contains the Razor views for the web pages. These views are used to render the HTML for the web pages.

## API Endpoints

### Artists

• GET /api/Artists/List: Retrieves a list of all artists.

• GET /api/Artists/FindArtist/{ArtistID}: Retrieves details of a specific artist.

• POST /api/Artists/Add: Adds a new artist.

• PUT /api/Artists/Update/{ArtistID}: Updates an existing artist.

• DELETE /api/Artists/Delete/{id}: Deletes an artist.

### Artworks

• GET /api/Artworks/List: Retrieves a list of all artworks.

• GET /api/Artworks/FindArtwork/{ArtworkID}: Retrieves details of a specific artwork.

• POST /api/Artworks/Add: Adds a new artwork.

• PUT /api/Artworks/Update/{ArtworkID}: Updates an existing artwork.

• DELETE /api/Artworks/Delete/{id}: Deletes an artwork.

### Exhibitions

• GET /api/Exhibitions/List: Retrieves a list of all exhibitions.

• GET /api/Exhibitions/FindExhibition/{ExhibitionID}: Retrieves details of a specific exhibition.

• POST /api/Exhibitions/Add: Adds a new exhibition.

• PUT /api/Exhibitions/Update/{ExhibitionID}: Updates an existing exhibition.

• DELETE /api/Exhibitions/Delete/{id}: Deletes an exhibition.

• POST /api/Exhibitions/AddArtwork/{ExhibitionID}: Adds an artwork to an exhibition.

• DELETE /api/Exhibitions/DeleteArtwork/{ExhibitionID}: Removes an artwork from an exhibition.
