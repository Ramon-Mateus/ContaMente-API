using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ContaMente.Migrations
{
    /// <inheritdoc />
    public partial class CartaoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CartaoId",
                table: "Movimentacoes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cartoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Apelido = table.Column<string>(type: "text", nullable: false),
                    DiaFechamento = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cartoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cartoes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Movimentacoes_CartaoId",
                table: "Movimentacoes",
                column: "CartaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cartoes_UserId",
                table: "Cartoes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Movimentacoes_Cartoes_CartaoId",
                table: "Movimentacoes",
                column: "CartaoId",
                principalTable: "Cartoes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movimentacoes_Cartoes_CartaoId",
                table: "Movimentacoes");

            migrationBuilder.DropTable(
                name: "Cartoes");

            migrationBuilder.DropIndex(
                name: "IX_Movimentacoes_CartaoId",
                table: "Movimentacoes");

            migrationBuilder.DropColumn(
                name: "CartaoId",
                table: "Movimentacoes");
        }
    }
}
