using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ContaMente.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTableTiposPagamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movimentacoes_TiposPagamento_TipoPagamentoId",
                table: "Movimentacoes");

            migrationBuilder.DropTable(
                name: "TiposPagamento");

            migrationBuilder.DropIndex(
                name: "IX_Movimentacoes_TipoPagamentoId",
                table: "Movimentacoes");

            migrationBuilder.RenameColumn(
                name: "TipoPagamentoId",
                table: "Movimentacoes",
                newName: "TipoPagamento");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TipoPagamento",
                table: "Movimentacoes",
                newName: "TipoPagamentoId");

            migrationBuilder.CreateTable(
                name: "TiposPagamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposPagamento", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Movimentacoes_TipoPagamentoId",
                table: "Movimentacoes",
                column: "TipoPagamentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Movimentacoes_TiposPagamento_TipoPagamentoId",
                table: "Movimentacoes",
                column: "TipoPagamentoId",
                principalTable: "TiposPagamento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
