using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Papernote.Notes.Infrastructure.Migrations
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
                name: "notes",
                schema: "papernote",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "character varying(50000)", maxLength: 50000, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "papernote",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "note_shares",
                schema: "papernote",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    note_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shared_with_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shared_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shared_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_note_shares", x => x.id);
                    table.ForeignKey(
                        name: "FK_note_shares_notes_note_id",
                        column: x => x.note_id,
                        principalSchema: "papernote",
                        principalTable: "notes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "note_tags",
                schema: "papernote",
                columns: table => new
                {
                    note_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    added_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_note_tags", x => new { x.note_id, x.tag_id });
                    table.ForeignKey(
                        name: "FK_note_tags_notes_note_id",
                        column: x => x.note_id,
                        principalSchema: "papernote",
                        principalTable: "notes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_note_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalSchema: "papernote",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_note_shares_is_revoked",
                schema: "papernote",
                table: "note_shares",
                column: "is_revoked");

            migrationBuilder.CreateIndex(
                name: "ix_note_shares_note_id",
                schema: "papernote",
                table: "note_shares",
                column: "note_id");

            migrationBuilder.CreateIndex(
                name: "ix_note_shares_note_user_active",
                schema: "papernote",
                table: "note_shares",
                columns: new[] { "note_id", "shared_with_user_id" },
                unique: true,
                filter: "is_revoked = false");

            migrationBuilder.CreateIndex(
                name: "ix_note_shares_shared_at",
                schema: "papernote",
                table: "note_shares",
                column: "shared_at");

            migrationBuilder.CreateIndex(
                name: "ix_note_shares_shared_by_user_id",
                schema: "papernote",
                table: "note_shares",
                column: "shared_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_note_shares_shared_with_user_id",
                schema: "papernote",
                table: "note_shares",
                column: "shared_with_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_note_tags_added_at",
                schema: "papernote",
                table: "note_tags",
                column: "added_at");

            migrationBuilder.CreateIndex(
                name: "ix_note_tags_note_id",
                schema: "papernote",
                table: "note_tags",
                column: "note_id");

            migrationBuilder.CreateIndex(
                name: "ix_note_tags_tag_id",
                schema: "papernote",
                table: "note_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_notes_created_at",
                schema: "papernote",
                table: "notes",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_notes_is_deleted",
                schema: "papernote",
                table: "notes",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_notes_updated_at",
                schema: "papernote",
                table: "notes",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "ix_notes_user_id",
                schema: "papernote",
                table: "notes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tags_created_at",
                schema: "papernote",
                table: "tags",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_tags_name_unique",
                schema: "papernote",
                table: "tags",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "note_shares",
                schema: "papernote");

            migrationBuilder.DropTable(
                name: "note_tags",
                schema: "papernote");

            migrationBuilder.DropTable(
                name: "notes",
                schema: "papernote");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "papernote");
        }
    }
}
