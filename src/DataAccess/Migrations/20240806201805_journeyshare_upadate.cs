using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class journeyshare_upadate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFavorite",
                table: "Journeys");

            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                table: "JourneyShares",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFavorite",
                table: "JourneyShares");

            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                table: "Journeys",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
