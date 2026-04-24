using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Luxottica.DataAccess.Migrations
{
    public partial class AddLastCam : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastCam",
                table: "ToteInformations",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastCam",
                table: "ToteInformations");
        }
    }
}
