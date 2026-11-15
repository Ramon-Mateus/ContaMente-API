using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContaMente.Migrations
{
    /// <inheritdoc />
    public partial class PluggyCompraParcelada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChaveCompra",
                table: "TransacoesPendentes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParcelaId",
                table: "TransacoesPendentes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ValorTotal",
                table: "TransacoesPendentes",
                type: "double precision",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransacoesPendentes_ParcelaId",
                table: "TransacoesPendentes",
                column: "ParcelaId");

            migrationBuilder.CreateIndex(
                name: "IX_TransacoesPendentes_UserId_ChaveCompra",
                table: "TransacoesPendentes",
                columns: new[] { "UserId", "ChaveCompra" });

            migrationBuilder.AddForeignKey(
                name: "FK_TransacoesPendentes_Parcelas_ParcelaId",
                table: "TransacoesPendentes",
                column: "ParcelaId",
                principalTable: "Parcelas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransacoesPendentes_Parcelas_ParcelaId",
                table: "TransacoesPendentes");

            migrationBuilder.DropIndex(
                name: "IX_TransacoesPendentes_ParcelaId",
                table: "TransacoesPendentes");

            migrationBuilder.DropIndex(
                name: "IX_TransacoesPendentes_UserId_ChaveCompra",
                table: "TransacoesPendentes");

            migrationBuilder.DropColumn(
                name: "ChaveCompra",
                table: "TransacoesPendentes");

            migrationBuilder.DropColumn(
                name: "ParcelaId",
                table: "TransacoesPendentes");

            migrationBuilder.DropColumn(
                name: "ValorTotal",
                table: "TransacoesPendentes");
        }
    }
}
