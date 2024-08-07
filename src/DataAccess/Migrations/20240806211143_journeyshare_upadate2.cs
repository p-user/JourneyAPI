using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class journeyshare_upadate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SocialMediaLink",
                table: "JourneyShares");

            migrationBuilder.AddColumn<string>(
                name: "SocialMediaLink",
                table: "Journeys",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SocialMediaLink",
                table: "Journeys");

            migrationBuilder.AddColumn<string>(
                name: "SocialMediaLink",
                table: "JourneyShares",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
