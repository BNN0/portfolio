using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Luxottica.DataAccess.Migrations
{
    public partial class CreateNewScanlogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScanLogsPickings");

            migrationBuilder.DropTable(
                name: "ScanLogsReceivings");

            migrationBuilder.CreateTable(
                name: "ScanlogsReceivingPickings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToteLPN = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    VirtualZone = table.Column<int>(type: "int", nullable: true),
                    VirtualTote = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: true),
                    Wave = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: true),
                    TotesInWave = table.Column<int>(type: "int", nullable: true),
                    TotalQty = table.Column<int>(type: "int", nullable: true),
                    WCS_Destination_Area = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    Put_Station_Nr = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    Release = table.Column<int>(type: "int", nullable: true),
                    Processed = table.Column<bool>(type: "bit", nullable: true),
                    StatusV10 = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    LapCount = table.Column<int>(type: "int", nullable: true),
                    TrackingId = table.Column<int>(type: "int", nullable: true),
                    Timestamp = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: true),
                    CamId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DivertCode = table.Column<int>(type: "int", nullable: true),
                    Info = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanlogsReceivingPickings", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScanlogsReceivingPickings");

            migrationBuilder.CreateTable(
                name: "ScanLogsPickings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cam_Id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Command = table.Column<int>(type: "int", nullable: true),
                    DestinationArea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Processed = table.Column<int>(type: "int", nullable: true),
                    PutStationNr = table.Column<int>(type: "int", nullable: true),
                    Release = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    TimesTamp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalQTY = table.Column<int>(type: "int", nullable: false),
                    ToteLPN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotesInWave = table.Column<int>(type: "int", nullable: false),
                    Wave = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    CamId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Command = table.Column<int>(type: "int", nullable: true),
                    DivertStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LineCount = table.Column<int>(type: "int", nullable: true),
                    Timestamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToteLPN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToteType = table.Column<int>(type: "int", nullable: true),
                    TrackingId = table.Column<int>(type: "int", nullable: true),
                    VirtualTote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VirtualZone = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanLogsReceivings", x => x.Id);
                });
        }
    }
}
