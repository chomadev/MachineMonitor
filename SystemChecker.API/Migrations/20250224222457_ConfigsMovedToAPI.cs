using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemChecker.API.Migrations
{
    /// <inheritdoc />
    public partial class ConfigsMovedToAPI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MachineConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    CheckSchedule = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServicesToMonitor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddressesToMonitor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TcpPorts = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineConfigurations_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FolderMonitorConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineConfigurationId = table.Column<int>(type: "int", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShouldBeEmpty = table.Column<bool>(type: "bit", nullable: false),
                    MonitorLastModified = table.Column<bool>(type: "bit", nullable: false),
                    CheckZeroByteFiles = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderMonitorConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FolderMonitorConfigs_MachineConfigurations_MachineConfigurationId",
                        column: x => x.MachineConfigurationId,
                        principalTable: "MachineConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FolderMonitorConfigs_MachineConfigurationId",
                table: "FolderMonitorConfigs",
                column: "MachineConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineConfigurations_MachineId",
                table: "MachineConfigurations",
                column: "MachineId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FolderMonitorConfigs");

            migrationBuilder.DropTable(
                name: "MachineConfigurations");
        }
    }
}
