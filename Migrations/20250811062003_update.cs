using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamDesk.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
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

            migrationBuilder.AlterColumn<Guid>(
                name: "StaffId",
                table: "Leaves",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

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
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveAttachments_User_UploadedById",
                table: "LeaveAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_Staff_StaffId",
                table: "Leaves");

            migrationBuilder.AlterColumn<Guid>(
                name: "StaffId",
                table: "Leaves",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveAttachments_User_UploadedById",
                table: "LeaveAttachments",
                column: "UploadedById",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_Staff_StaffId",
                table: "Leaves",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
