using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamDesk.Migrations
{
    /// <inheritdoc />
    public partial class updateleaves : Migration
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

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_Staff_StaffId",
                table: "Leaves",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_User_ApprovedById",
                table: "Leaves",
                column: "ApprovedById",
                principalTable: "User",
                principalColumn: "Id");
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
    }
}
