using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Papernote.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "papernote");

            migrationBuilder.CreateTable(
                name: "users",
                schema: "papernote",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "papernote",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_by_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    replaced_by_token_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "papernote",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_created_at",
                schema: "papernote",
                table: "refresh_tokens",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_expires_at",
                schema: "papernote",
                table: "refresh_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token_hash_unique",
                schema: "papernote",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id",
                schema: "papernote",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_revoked",
                schema: "papernote",
                table: "refresh_tokens",
                columns: new[] { "user_id", "is_revoked" });

            migrationBuilder.CreateIndex(
                name: "ix_users_created_at",
                schema: "papernote",
                table: "users",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_users_last_login_at",
                schema: "papernote",
                table: "users",
                column: "last_login_at");

            migrationBuilder.CreateIndex(
                name: "ix_users_status",
                schema: "papernote",
                table: "users",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_users_username_unique",
                schema: "papernote",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "papernote");

            migrationBuilder.DropTable(
                name: "users",
                schema: "papernote");
        }
    }
}
