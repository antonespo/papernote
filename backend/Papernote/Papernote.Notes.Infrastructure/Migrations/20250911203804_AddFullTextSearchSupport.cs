using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace Papernote.Notes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearchSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "search_vector",
                schema: "papernote",
                table: "notes",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "\r\n                setweight(to_tsvector('italian', coalesce(title,   '')), 'A') ||\r\n                setweight(to_tsvector('italian', coalesce(content, '')), 'B')\r\n            ",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "ix_notes_search_vector",
                schema: "papernote",
                table: "notes",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "gin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_notes_search_vector",
                schema: "papernote",
                table: "notes");

            migrationBuilder.DropColumn(
                name: "search_vector",
                schema: "papernote",
                table: "notes");
        }
    }
}
