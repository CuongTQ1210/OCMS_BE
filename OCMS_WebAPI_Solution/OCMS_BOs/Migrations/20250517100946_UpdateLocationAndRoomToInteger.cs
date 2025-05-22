using Microsoft.EntityFrameworkCore.Migrations;

namespace OCMS_BOs.Migrations
{
    public partial class UpdateLocationAndRoomToInteger : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, create temporary columns
            migrationBuilder.AddColumn<int>(
                name: "LocationInt",
                table: "TrainingSchedules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RoomInt",
                table: "TrainingSchedules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Convert existing data
            migrationBuilder.Sql(@"
                UPDATE ""TrainingSchedules""
                SET ""LocationInt"" = CASE 
                    WHEN ""Location"" = 'SectionA' THEN 0
                    WHEN ""Location"" = 'SectionB' THEN 1
                    ELSE 0
                END,
                ""RoomInt"" = CASE 
                    WHEN ""Room"" = 'R001' THEN 0
                    WHEN ""Room"" = 'R002' THEN 1
                    WHEN ""Room"" = 'R003' THEN 2
                    WHEN ""Room"" = 'R004' THEN 3
                    WHEN ""Room"" = 'R005' THEN 4
                    WHEN ""Room"" = 'R006' THEN 5
                    WHEN ""Room"" = 'R007' THEN 6
                    WHEN ""Room"" = 'R008' THEN 7
                    WHEN ""Room"" = 'R009' THEN 8
                    WHEN ""Room"" = 'R101' THEN 9
                    WHEN ""Room"" = 'R102' THEN 10
                    WHEN ""Room"" = 'R103' THEN 11
                    WHEN ""Room"" = 'R104' THEN 12
                    WHEN ""Room"" = 'R105' THEN 13
                    WHEN ""Room"" = 'R106' THEN 14
                    WHEN ""Room"" = 'R107' THEN 15
                    WHEN ""Room"" = 'R108' THEN 16
                    WHEN ""Room"" = 'R109' THEN 17
                    WHEN ""Room"" = 'R201' THEN 18
                    WHEN ""Room"" = 'R202' THEN 19
                    WHEN ""Room"" = 'R203' THEN 20
                    WHEN ""Room"" = 'R204' THEN 21
                    WHEN ""Room"" = 'R205' THEN 22
                    WHEN ""Room"" = 'R206' THEN 23
                    WHEN ""Room"" = 'R207' THEN 24
                    WHEN ""Room"" = 'R208' THEN 25
                    WHEN ""Room"" = 'R209' THEN 26
                    WHEN ""Room"" = 'R301' THEN 27
                    WHEN ""Room"" = 'R302' THEN 28
                    WHEN ""Room"" = 'R303' THEN 29
                    WHEN ""Room"" = 'R304' THEN 30
                    WHEN ""Room"" = 'R305' THEN 31
                    WHEN ""Room"" = 'R306' THEN 32
                    WHEN ""Room"" = 'R307' THEN 33
                    WHEN ""Room"" = 'R308' THEN 34
                    WHEN ""Room"" = 'R309' THEN 35
                    WHEN ""Room"" = 'R401' THEN 36
                    WHEN ""Room"" = 'R402' THEN 37
                    WHEN ""Room"" = 'R403' THEN 38
                    WHEN ""Room"" = 'R404' THEN 39
                    WHEN ""Room"" = 'R405' THEN 40
                    WHEN ""Room"" = 'R406' THEN 41
                    WHEN ""Room"" = 'R407' THEN 42
                    WHEN ""Room"" = 'R408' THEN 43
                    WHEN ""Room"" = 'R409' THEN 44
                    WHEN ""Room"" = 'R501' THEN 45
                    WHEN ""Room"" = 'R502' THEN 46
                    WHEN ""Room"" = 'R503' THEN 47
                    WHEN ""Room"" = 'R504' THEN 48
                    WHEN ""Room"" = 'R505' THEN 49
                    WHEN ""Room"" = 'R506' THEN 50
                    WHEN ""Room"" = 'R507' THEN 51
                    WHEN ""Room"" = 'R508' THEN 52
                    WHEN ""Room"" = 'R509' THEN 53
                    ELSE 0
                END");

            // Drop old columns
            migrationBuilder.DropColumn(
                name: "Location",
                table: "TrainingSchedules");

            migrationBuilder.DropColumn(
                name: "Room",
                table: "TrainingSchedules");

            // Rename new columns
            migrationBuilder.RenameColumn(
                name: "LocationInt",
                table: "TrainingSchedules",
                newName: "Location");

            migrationBuilder.RenameColumn(
                name: "RoomInt",
                table: "TrainingSchedules",
                newName: "Room");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // First, create temporary text columns
            migrationBuilder.AddColumn<string>(
                name: "LocationText",
                table: "TrainingSchedules",
                type: "text",
                nullable: false,
                defaultValue: "SectionA");

            migrationBuilder.AddColumn<string>(
                name: "RoomText",
                table: "TrainingSchedules",
                type: "text",
                nullable: false,
                defaultValue: "R001");

            // Convert back to text
            migrationBuilder.Sql(@"
                UPDATE ""TrainingSchedules""
                SET ""LocationText"" = CASE 
                    WHEN ""Location"" = 0 THEN 'SectionA'
                    WHEN ""Location"" = 1 THEN 'SectionB'
                    ELSE 'SectionA'
                END,
                ""RoomText"" = CASE 
                    WHEN ""Room"" = 0 THEN 'R001'
                    WHEN ""Room"" = 1 THEN 'R002'
                    WHEN ""Room"" = 2 THEN 'R003'
                    WHEN ""Room"" = 3 THEN 'R004'
                    WHEN ""Room"" = 4 THEN 'R005'
                    WHEN ""Room"" = 5 THEN 'R006'
                    WHEN ""Room"" = 6 THEN 'R007'
                    WHEN ""Room"" = 7 THEN 'R008'
                    WHEN ""Room"" = 8 THEN 'R009'
                    WHEN ""Room"" = 9 THEN 'R101'
                    WHEN ""Room"" = 10 THEN 'R102'
                    WHEN ""Room"" = 11 THEN 'R103'
                    WHEN ""Room"" = 12 THEN 'R104'
                    WHEN ""Room"" = 13 THEN 'R105'
                    WHEN ""Room"" = 14 THEN 'R106'
                    WHEN ""Room"" = 15 THEN 'R107'
                    WHEN ""Room"" = 16 THEN 'R108'
                    WHEN ""Room"" = 17 THEN 'R109'
                    WHEN ""Room"" = 18 THEN 'R201'
                    WHEN ""Room"" = 19 THEN 'R202'
                    WHEN ""Room"" = 20 THEN 'R203'
                    WHEN ""Room"" = 21 THEN 'R204'
                    WHEN ""Room"" = 22 THEN 'R205'
                    WHEN ""Room"" = 23 THEN 'R206'
                    WHEN ""Room"" = 24 THEN 'R207'
                    WHEN ""Room"" = 25 THEN 'R208'
                    WHEN ""Room"" = 26 THEN 'R209'
                    WHEN ""Room"" = 27 THEN 'R301'
                    WHEN ""Room"" = 28 THEN 'R302'
                    WHEN ""Room"" = 29 THEN 'R303'
                    WHEN ""Room"" = 30 THEN 'R304'
                    WHEN ""Room"" = 31 THEN 'R305'
                    WHEN ""Room"" = 32 THEN 'R306'
                    WHEN ""Room"" = 33 THEN 'R307'
                    WHEN ""Room"" = 34 THEN 'R308'
                    WHEN ""Room"" = 35 THEN 'R309'
                    WHEN ""Room"" = 36 THEN 'R401'
                    WHEN ""Room"" = 37 THEN 'R402'
                    WHEN ""Room"" = 38 THEN 'R403'
                    WHEN ""Room"" = 39 THEN 'R404'
                    WHEN ""Room"" = 40 THEN 'R405'
                    WHEN ""Room"" = 41 THEN 'R406'
                    WHEN ""Room"" = 42 THEN 'R407'
                    WHEN ""Room"" = 43 THEN 'R408'
                    WHEN ""Room"" = 44 THEN 'R409'
                    WHEN ""Room"" = 45 THEN 'R501'
                    WHEN ""Room"" = 46 THEN 'R502'
                    WHEN ""Room"" = 47 THEN 'R503'
                    WHEN ""Room"" = 48 THEN 'R504'
                    WHEN ""Room"" = 49 THEN 'R505'
                    WHEN ""Room"" = 50 THEN 'R506'
                    WHEN ""Room"" = 51 THEN 'R507'
                    WHEN ""Room"" = 52 THEN 'R508'
                    WHEN ""Room"" = 53 THEN 'R509'
                    ELSE 'R001'
                END");

            // Drop integer columns
            migrationBuilder.DropColumn(
                name: "Location",
                table: "TrainingSchedules");

            migrationBuilder.DropColumn(
                name: "Room",
                table: "TrainingSchedules");

            // Rename text columns back
            migrationBuilder.RenameColumn(
                name: "LocationText",
                table: "TrainingSchedules",
                newName: "Location");

            migrationBuilder.RenameColumn(
                name: "RoomText",
                table: "TrainingSchedules",
                newName: "Room");
        }
    }
} 