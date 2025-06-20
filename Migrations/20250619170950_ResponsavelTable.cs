using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ContaMente.Migrations
{
    /// <inheritdoc />
    public partial class ResponsavelTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResponsavelId",
                table: "Movimentacoes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Responsaveis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responsaveis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Responsaveis_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Movimentacoes_ResponsavelId",
                table: "Movimentacoes",
                column: "ResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_Responsaveis_UserId",
                table: "Responsaveis",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Movimentacoes_Responsaveis_ResponsavelId",
                table: "Movimentacoes",
                column: "ResponsavelId",
                principalTable: "Responsaveis",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movimentacoes_Responsaveis_ResponsavelId",
                table: "Movimentacoes");

            migrationBuilder.DropTable(
                name: "Responsaveis");

            migrationBuilder.DropIndex(
                name: "IX_Movimentacoes_ResponsavelId",
                table: "Movimentacoes");

            migrationBuilder.DropColumn(
                name: "ResponsavelId",
                table: "Movimentacoes");
        }
    }
}
