using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamDesk.Migrations
{
    /// <inheritdoc />
    public partial class updatenewnew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveAttachments_User_UploadedById",
                table: "LeaveAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_Staff_StaffId",
                table: "Leaves");

            migrationBuilder.RenameColumn(
                name: "StaffId",
                table: "Leaves",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Leaves_StaffId_Status_IsActive",
                table: "Leaves",
                newName: "IX_Leaves_UserId_Status_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Leaves_StaffId",
                table: "Leaves",
                newName: "IX_Leaves_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "OriginalFileName",
                table: "LeaveAttachments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "LeaveAttachments",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "FileUrl",
                table: "LeaveAttachments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FileType",
                table: "LeaveAttachments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "LeaveAttachments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "LeaveAttachments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "LeaveAttachments",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveAttachments_IsActive",
                table: "LeaveAttachments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveAttachments_LeaveId_IsActive",
                table: "LeaveAttachments",
                columns: new[] { "LeaveId", "IsActive" });

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveAttachments_User_UploadedById",
                table: "LeaveAttachments",
                column: "UploadedById",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_User_UserId",
                table: "Leaves",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveAttachments_User_UploadedById",
                table: "LeaveAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_User_UserId",
                table: "Leaves");

            migrationBuilder.DropIndex(
                name: "IX_LeaveAttachments_IsActive",
                table: "LeaveAttachments");

            migrationBuilder.DropIndex(
                name: "IX_LeaveAttachments_LeaveId_IsActive",
                table: "LeaveAttachments");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Leaves",
                newName: "StaffId");

            migrationBuilder.RenameIndex(
                name: "IX_Leaves_UserId_Status_IsActive",
                table: "Leaves",
                newName: "IX_Leaves_StaffId_Status_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Leaves_UserId",
                table: "Leaves",
                newName: "IX_Leaves_StaffId");

            migrationBuilder.AlterColumn<string>(
                name: "OriginalFileName",
                table: "LeaveAttachments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "LeaveAttachments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "FileUrl",
                table: "LeaveAttachments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "FileType",
                table: "LeaveAttachments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "LeaveAttachments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "LeaveAttachments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "LeaveAttachments",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveAttachments_User_UploadedById",
                table: "LeaveAttachments",
                column: "UploadedById",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_Staff_StaffId",
                table: "Leaves",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
