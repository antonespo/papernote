using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Papernote.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRevokedByIpField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "revoked_by_ip",
                schema: "papernote",
                table: "refresh_tokens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "revoked_by_ip",
                schema: "papernote",
                table: "refresh_tokens",
                type: "character varying(45)",
                maxLength: 45,
                nullable: true);
        }
    }
}
