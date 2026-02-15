using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASPLeadMapInsightsAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTreeCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Trees",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Trees",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Trees");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Trees");
        }
    }
}
