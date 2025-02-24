using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemChecker.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFolderAndNetworkMonitoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NetworkStatus_SystemCheckHistory_SystemCheckHistoryId",
                table: "NetworkStatus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NetworkStatus",
                table: "NetworkStatus");

            migrationBuilder.RenameTable(
                name: "NetworkStatus",
                newName: "NetworkStatuses");

            migrationBuilder.RenameIndex(
                name: "IX_NetworkStatus_SystemCheckHistoryId",
                table: "NetworkStatuses",
                newName: "IX_NetworkStatuses_SystemCheckHistoryId");

            migrationBuilder.AddColumn<string>(
                name: "ActiveInterfaces",
                table: "NetworkStatuses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NetworkStatuses",
                table: "NetworkStatuses",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "FolderChanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastChangeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SystemCheckHistoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FolderChanges_SystemCheckHistory_SystemCheckHistoryId",
                        column: x => x.SystemCheckHistoryId,
                        principalTable: "SystemCheckHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FolderStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Exists = table.Column<bool>(type: "bit", nullable: false),
                    IsEmpty = table.Column<bool>(type: "bit", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HasZeroByteFiles = table.Column<bool>(type: "bit", nullable: false),
                    IsValid = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZeroByteFiles = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SystemCheckHistoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FolderStatuses_SystemCheckHistory_SystemCheckHistoryId",
                        column: x => x.SystemCheckHistoryId,
                        principalTable: "SystemCheckHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonitoredAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsReachable = table.Column<bool>(type: "bit", nullable: false),
                    ResponseTime = table.Column<int>(type: "int", nullable: false),
                    SystemCheckHistoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoredAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonitoredAddresses_SystemCheckHistory_SystemCheckHistoryId",
                        column: x => x.SystemCheckHistoryId,
                        principalTable: "SystemCheckHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FolderChanges_SystemCheckHistoryId",
                table: "FolderChanges",
                column: "SystemCheckHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FolderStatuses_SystemCheckHistoryId",
                table: "FolderStatuses",
                column: "SystemCheckHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoredAddresses_SystemCheckHistoryId",
                table: "MonitoredAddresses",
                column: "SystemCheckHistoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_NetworkStatuses_SystemCheckHistory_SystemCheckHistoryId",
                table: "NetworkStatuses",
                column: "SystemCheckHistoryId",
                principalTable: "SystemCheckHistory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NetworkStatuses_SystemCheckHistory_SystemCheckHistoryId",
                table: "NetworkStatuses");

            migrationBuilder.DropTable(
                name: "FolderChanges");

            migrationBuilder.DropTable(
                name: "FolderStatuses");

            migrationBuilder.DropTable(
                name: "MonitoredAddresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NetworkStatuses",
                table: "NetworkStatuses");

            migrationBuilder.DropColumn(
                name: "ActiveInterfaces",
                table: "NetworkStatuses");

            migrationBuilder.RenameTable(
                name: "NetworkStatuses",
                newName: "NetworkStatus");

            migrationBuilder.RenameIndex(
                name: "IX_NetworkStatuses_SystemCheckHistoryId",
                table: "NetworkStatus",
                newName: "IX_NetworkStatus_SystemCheckHistoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NetworkStatus",
                table: "NetworkStatus",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NetworkStatus_SystemCheckHistory_SystemCheckHistoryId",
                table: "NetworkStatus",
                column: "SystemCheckHistoryId",
                principalTable: "SystemCheckHistory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
