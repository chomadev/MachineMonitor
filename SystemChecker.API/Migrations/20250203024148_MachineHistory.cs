using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemChecker.API.Migrations
{
    /// <inheritdoc />
    public partial class MachineHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemCheckHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemCheckHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemCheckHistory_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CpuStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsagePercentage = table.Column<double>(type: "float", nullable: false),
                    SystemCheckHistoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CpuStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CpuStatus_SystemCheckHistory_SystemCheckHistoryId",
                        column: x => x.SystemCheckHistoryId,
                        principalTable: "SystemCheckHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiskStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalSpace = table.Column<long>(type: "bigint", nullable: false),
                    FreeSpace = table.Column<long>(type: "bigint", nullable: false),
                    UsagePercentage = table.Column<double>(type: "float", nullable: false),
                    SystemCheckHistoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiskStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiskStatus_SystemCheckHistory_SystemCheckHistoryId",
                        column: x => x.SystemCheckHistoryId,
                        principalTable: "SystemCheckHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemoryStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TotalPhysicalMemory = table.Column<long>(type: "bigint", nullable: false),
                    AvailablePhysicalMemory = table.Column<long>(type: "bigint", nullable: false),
                    UsagePercentage = table.Column<double>(type: "float", nullable: false),
                    SystemCheckHistoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemoryStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemoryStatus_SystemCheckHistory_SystemCheckHistoryId",
                        column: x => x.SystemCheckHistoryId,
                        principalTable: "SystemCheckHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NetworkStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsConnected = table.Column<bool>(type: "bit", nullable: false),
                    HasInternetAccess = table.Column<bool>(type: "bit", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SystemCheckHistoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NetworkStatus_SystemCheckHistory_SystemCheckHistoryId",
                        column: x => x.SystemCheckHistoryId,
                        principalTable: "SystemCheckHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRunning = table.Column<bool>(type: "bit", nullable: false),
                    SystemCheckHistoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceStatus_SystemCheckHistory_SystemCheckHistoryId",
                        column: x => x.SystemCheckHistoryId,
                        principalTable: "SystemCheckHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TcpPortStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Port = table.Column<int>(type: "int", nullable: false),
                    IsOpen = table.Column<bool>(type: "bit", nullable: false),
                    Service = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SystemCheckHistoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TcpPortStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TcpPortStatus_SystemCheckHistory_SystemCheckHistoryId",
                        column: x => x.SystemCheckHistoryId,
                        principalTable: "SystemCheckHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CpuStatus_SystemCheckHistoryId",
                table: "CpuStatus",
                column: "SystemCheckHistoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiskStatus_SystemCheckHistoryId",
                table: "DiskStatus",
                column: "SystemCheckHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryStatus_SystemCheckHistoryId",
                table: "MemoryStatus",
                column: "SystemCheckHistoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NetworkStatus_SystemCheckHistoryId",
                table: "NetworkStatus",
                column: "SystemCheckHistoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceStatus_SystemCheckHistoryId",
                table: "ServiceStatus",
                column: "SystemCheckHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemCheckHistory_MachineId",
                table: "SystemCheckHistory",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_TcpPortStatus_SystemCheckHistoryId",
                table: "TcpPortStatus",
                column: "SystemCheckHistoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CpuStatus");

            migrationBuilder.DropTable(
                name: "DiskStatus");

            migrationBuilder.DropTable(
                name: "MemoryStatus");

            migrationBuilder.DropTable(
                name: "NetworkStatus");

            migrationBuilder.DropTable(
                name: "ServiceStatus");

            migrationBuilder.DropTable(
                name: "TcpPortStatus");

            migrationBuilder.DropTable(
                name: "SystemCheckHistory");
        }
    }
}
