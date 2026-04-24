using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxotticaSorting.DataAccess.Migrations
{
    public partial class AddTableMultiBoxesWave : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "multiBox_Wave",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContainerId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ContainerType = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    DivertLane = table.Column<int>(type: "int", nullable: false),
                    ConfirmationNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Qty = table.Column<int>(type: "int", nullable: false),
                    QtyCount = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_multiBox_Wave", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecirculationLimits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountLimit = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecirculationLimits", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "multiBox_Wave");

            migrationBuilder.DropTable(
                name: "RecirculationLimits");

          
        }
    }
}
