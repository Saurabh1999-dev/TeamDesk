using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamDesk.Migrations
{
    /// <inheritdoc />
    public partial class updatenew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_Staff_StaffId",
                table: "Leaves");

            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_User_ApprovedById",
                table: "Leaves");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Leaves",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "StaffId",
                table: "Leaves",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Leaves",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Leaves",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_EndDate",
                table: "Leaves",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_IsActive",
                table: "Leaves",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_LeaveType",
                table: "Leaves",
                column: "LeaveType");

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_LeaveType_StartDate_Status",
                table: "Leaves",
                columns: new[] { "LeaveType", "StartDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_StaffId_Status_IsActive",
                table: "Leaves",
                columns: new[] { "StaffId", "Status", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_StartDate",
                table: "Leaves",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_Status",
                table: "Leaves",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_Status_CreatedAt",
                table: "Leaves",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_Staff_StaffId",
                table: "Leaves",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_User_ApprovedById",
                table: "Leaves",
                column: "ApprovedById",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_Staff_StaffId",
                table: "Leaves");

            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_User_ApprovedById",
                table: "Leaves");

            migrationBuilder.DropIndex(
                name: "IX_Leaves_EndDate",
                table: "Leaves");

            migrationBuilder.DropIndex(
                name: "IX_Leaves_IsActive",
                table: "Leaves");

            migrationBuilder.DropIndex(
                name: "IX_Leaves_LeaveType",
                table: "Leaves");

            migrationBuilder.DropIndex(
                name: "IX_Leaves_LeaveType_StartDate_Status",
                table: "Leaves");

            migrationBuilder.DropIndex(
                name: "IX_Leaves_StaffId_Status_IsActive",
                table: "Leaves");

            migrationBuilder.DropIndex(
                name: "IX_Leaves_StartDate",
                table: "Leaves");

            migrationBuilder.DropIndex(
                name: "IX_Leaves_Status",
                table: "Leaves");

            migrationBuilder.DropIndex(
                name: "IX_Leaves_Status_CreatedAt",
                table: "Leaves");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Leaves",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<Guid>(
                name: "StaffId",
                table: "Leaves",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Leaves",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Leaves",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_Staff_StaffId",
                table: "Leaves",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_User_ApprovedById",
                table: "Leaves",
                column: "ApprovedById",
                principalTable: "User",
                principalColumn: "Id");
        }
    }
}
