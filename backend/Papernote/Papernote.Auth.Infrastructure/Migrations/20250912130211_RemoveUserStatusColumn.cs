using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Papernote.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserStatusColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_status",
                schema: "papernote",
                table: "users");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "papernote",
                table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "status",
                schema: "papernote",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_users_status",
                schema: "papernote",
                table: "users",
                column: "status");
        }
    }
}
