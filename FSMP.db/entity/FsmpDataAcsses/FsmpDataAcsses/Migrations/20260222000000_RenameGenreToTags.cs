using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FsmpDataAcsses.Migrations
{
    /// <inheritdoc />
    public partial class RenameGenreToTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename junction tables first (they reference Genres)
            migrationBuilder.RenameTable(name: "TrackGenre", newName: "TrackTag");
            migrationBuilder.RenameTable(name: "AlbumGenre", newName: "AlbumTag");
            migrationBuilder.RenameTable(name: "ArtistGenre", newName: "ArtistTag");

            // Rename FK columns in junction tables (using new table names)
            migrationBuilder.RenameColumn(name: "GenresGenreId", table: "TrackTag", newName: "TagsTagId");
            migrationBuilder.RenameColumn(name: "GenresGenreId", table: "AlbumTag", newName: "TagsTagId");
            migrationBuilder.RenameColumn(name: "GenresGenreId", table: "ArtistTag", newName: "TagsTagId");

            // Rename Genres table to Tags
            migrationBuilder.RenameTable(name: "Genres", newName: "Tags");

            // Rename primary key column
            migrationBuilder.RenameColumn(name: "GenreId", table: "Tags", newName: "TagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "TagId", table: "Tags", newName: "GenreId");
            migrationBuilder.RenameTable(name: "Tags", newName: "Genres");

            migrationBuilder.RenameColumn(name: "TagsTagId", table: "TrackTag", newName: "GenresGenreId");
            migrationBuilder.RenameColumn(name: "TagsTagId", table: "AlbumTag", newName: "GenresGenreId");
            migrationBuilder.RenameColumn(name: "TagsTagId", table: "ArtistTag", newName: "GenresGenreId");

            migrationBuilder.RenameTable(name: "TrackTag", newName: "TrackGenre");
            migrationBuilder.RenameTable(name: "AlbumTag", newName: "AlbumGenre");
            migrationBuilder.RenameTable(name: "ArtistTag", newName: "ArtistGenre");
        }
    }
}
