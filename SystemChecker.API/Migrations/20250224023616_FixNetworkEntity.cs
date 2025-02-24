using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemChecker.API.Migrations
{
    /// <inheritdoc />
    public partial class FixNetworkEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MonitoredAddresses_SystemCheckHistory_SystemCheckHistoryId",
                table: "MonitoredAddresses");

            migrationBuilder.AlterColumn<int>(
                name: "SystemCheckHistoryId",
                table: "MonitoredAddresses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "NetworkStatusId",
                table: "MonitoredAddresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MonitoredAddresses_NetworkStatusId",
                table: "MonitoredAddresses",
                column: "NetworkStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_MonitoredAddresses_NetworkStatuses_NetworkStatusId",
                table: "MonitoredAddresses",
                column: "NetworkStatusId",
                principalTable: "NetworkStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MonitoredAddresses_SystemCheckHistory_SystemCheckHistoryId",
                table: "MonitoredAddresses",
                column: "SystemCheckHistoryId",
                principalTable: "SystemCheckHistory",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MonitoredAddresses_NetworkStatuses_NetworkStatusId",
                table: "MonitoredAddresses");

            migrationBuilder.DropForeignKey(
                name: "FK_MonitoredAddresses_SystemCheckHistory_SystemCheckHistoryId",
                table: "MonitoredAddresses");

            migrationBuilder.DropIndex(
                name: "IX_MonitoredAddresses_NetworkStatusId",
                table: "MonitoredAddresses");

            migrationBuilder.DropColumn(
                name: "NetworkStatusId",
                table: "MonitoredAddresses");

            migrationBuilder.AlterColumn<int>(
                name: "SystemCheckHistoryId",
                table: "MonitoredAddresses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MonitoredAddresses_SystemCheckHistory_SystemCheckHistoryId",
                table: "MonitoredAddresses",
                column: "SystemCheckHistoryId",
                principalTable: "SystemCheckHistory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
