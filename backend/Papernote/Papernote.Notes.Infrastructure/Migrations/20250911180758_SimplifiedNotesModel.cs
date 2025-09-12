using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Papernote.Notes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SimplifiedNotesModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_note_tags_tags_tag_id",
                schema: "papernote",
                table: "note_tags");

            migrationBuilder.DropTable(
                name: "note_shares",
                schema: "papernote");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "papernote");

            migrationBuilder.DropIndex(
                name: "ix_notes_user_id",
                schema: "papernote",
                table: "notes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_note_tags",
                schema: "papernote",
                table: "note_tags");

            migrationBuilder.DropIndex(
                name: "ix_note_tags_tag_id",
                schema: "papernote",
                table: "note_tags");

            migrationBuilder.DropColumn(
                name: "user_id",
                schema: "papernote",
                table: "notes");

            migrationBuilder.DropColumn(
                name: "tag_id",
                schema: "papernote",
                table: "note_tags");

            migrationBuilder.AddColumn<string>(
                name: "tag_name",
                schema: "papernote",
                table: "note_tags",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_note_tags",
                schema: "papernote",
                table: "note_tags",
                columns: new[] { "note_id", "tag_name" });

            migrationBuilder.CreateIndex(
                name: "ix_note_tags_tag_name",
                schema: "papernote",
                table: "note_tags",
                column: "tag_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_note_tags",
                schema: "papernote",
                table: "note_tags");

            migrationBuilder.DropIndex(
                name: "ix_note_tags_tag_name",
                schema: "papernote",
                table: "note_tags");

            migrationBuilder.DropColumn(
                name: "tag_name",
                schema: "papernote",
                table: "note_tags");

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                schema: "papernote",
                table: "notes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "tag_id",
                schema: "papernote",
                table: "note_tags",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_note_tags",
                schema: "papernote",
                table: "note_tags",
                columns: new[] { "note_id", "tag_id" });

            migrationBuilder.CreateTable(
                name: "note_shares",
                schema: "papernote",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    note_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    shared_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    shared_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shared_with_user_id = table.Column<Guid>(type: "uuid", nullable: false)
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
                name: "tags",
                schema: "papernote",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_notes_user_id",
                schema: "papernote",
                table: "notes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_note_tags_tag_id",
                schema: "papernote",
                table: "note_tags",
                column: "tag_id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_note_tags_tags_tag_id",
                schema: "papernote",
                table: "note_tags",
                column: "tag_id",
                principalSchema: "papernote",
                principalTable: "tags",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
