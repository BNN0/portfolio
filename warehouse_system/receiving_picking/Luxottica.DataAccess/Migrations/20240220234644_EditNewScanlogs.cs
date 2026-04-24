using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Luxottica.DataAccess.Migrations
{
    public partial class EditNewScanlogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WCS_Destination_Area",
                table: "ScanlogsReceivingPickings",
                newName: "DestinationArea");

            migrationBuilder.RenameColumn(
                name: "Put_Station_Nr",
                table: "ScanlogsReceivingPickings",
                newName: "PutStation");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PutStation",
                table: "ScanlogsReceivingPickings",
                newName: "Put_Station_Nr");

            migrationBuilder.RenameColumn(
                name: "DestinationArea",
                table: "ScanlogsReceivingPickings",
                newName: "WCS_Destination_Area");
        }
    }
}
