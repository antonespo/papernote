using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Papernote.Notes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserOwnershipAndSharing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "owner_user_id",
                schema: "papernote",
                table: "notes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "note_shares",
                schema: "papernote",
                columns: table => new
                {
                    note_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reader_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shared_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_note_shares", x => new { x.note_id, x.reader_user_id });
                    table.ForeignKey(
                        name: "FK_note_shares_notes_note_id",
                        column: x => x.note_id,
                        principalSchema: "papernote",
                        principalTable: "notes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_notes_owner_user_id",
                schema: "papernote",
                table: "notes",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_note_shares_note_id",
                schema: "papernote",
                table: "note_shares",
                column: "note_id");

            migrationBuilder.CreateIndex(
                name: "ix_note_shares_note_reader",
                schema: "papernote",
                table: "note_shares",
                columns: new[] { "note_id", "reader_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_note_shares_reader_user_id",
                schema: "papernote",
                table: "note_shares",
                column: "reader_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_note_shares_shared_at",
                schema: "papernote",
                table: "note_shares",
                column: "shared_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "note_shares",
                schema: "papernote");

            migrationBuilder.DropIndex(
                name: "ix_notes_owner_user_id",
                schema: "papernote",
                table: "notes");

            migrationBuilder.DropColumn(
                name: "owner_user_id",
                schema: "papernote",
                table: "notes");
        }
    }
}
