using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Luxottica.DataAccess.Migrations
{
    public partial class initscanlogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScanLogsPickings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToteLPN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Wave = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    TotesInWave = table.Column<int>(type: "int", nullable: false),
                    TotalQTY = table.Column<int>(type: "int", nullable: false),
                    DestinationArea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimesTamp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PutStationNr = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    Release = table.Column<int>(type: "int", nullable: true),
                    Processed = table.Column<int>(type: "int", nullable: true),
                    Cam_Id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Command = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanLogsPickings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScanLogsReceivings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToteLPN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VirtualTote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VirtualZone = table.Column<int>(type: "int", nullable: true),
                    LineCount = table.Column<int>(type: "int", nullable: true),
                    TrackingId = table.Column<int>(type: "int", nullable: true),
                    CamId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Command = table.Column<int>(type: "int", nullable: true),
                    DivertStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToteType = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanLogsReceivings", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScanLogsPickings");

            migrationBuilder.DropTable(
                name: "ScanLogsReceivings");
        }
    }
}
