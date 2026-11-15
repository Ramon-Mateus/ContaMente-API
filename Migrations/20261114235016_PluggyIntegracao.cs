using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ContaMente.Migrations
{
    /// <inheritdoc />
    public partial class PluggyIntegracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PluggyItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PluggyItemId = table.Column<string>(type: "text", nullable: false),
                    NomeConector = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    ErroUltimaAtualizacao = table.Column<string>(type: "text", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UltimaSincronizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluggyItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PluggyItens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransacoesPendentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PluggyTransactionId = table.Column<string>(type: "text", nullable: false),
                    PluggyAccountId = table.Column<string>(type: "text", nullable: false),
                    NomeConta = table.Column<string>(type: "text", nullable: true),
                    FinalCartao = table.Column<string>(type: "text", nullable: true),
                    Descricao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Valor = table.Column<double>(type: "double precision", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CategoriaPluggy = table.Column<string>(type: "text", nullable: true),
                    NumeroParcela = table.Column<int>(type: "integer", nullable: true),
                    TotalParcelas = table.Column<int>(type: "integer", nullable: true),
                    Pendente = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ImportadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MovimentacaoId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransacoesPendentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransacoesPendentes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransacoesPendentes_Movimentacoes_MovimentacaoId",
                        column: x => x.MovimentacaoId,
                        principalTable: "Movimentacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PluggyContas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PluggyAccountId = table.Column<string>(type: "text", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Numero = table.Column<string>(type: "text", nullable: true),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    CartaoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluggyContas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PluggyContas_Cartoes_CartaoId",
                        column: x => x.CartaoId,
                        principalTable: "Cartoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PluggyContas_PluggyItens_ItemId",
                        column: x => x.ItemId,
                        principalTable: "PluggyItens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PluggyContas_CartaoId",
                table: "PluggyContas",
                column: "CartaoId");

            migrationBuilder.CreateIndex(
                name: "IX_PluggyContas_ItemId",
                table: "PluggyContas",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PluggyContas_PluggyAccountId",
                table: "PluggyContas",
                column: "PluggyAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PluggyItens_PluggyItemId",
                table: "PluggyItens",
                column: "PluggyItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PluggyItens_UserId",
                table: "PluggyItens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TransacoesPendentes_MovimentacaoId",
                table: "TransacoesPendentes",
                column: "MovimentacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_TransacoesPendentes_UserId_PluggyTransactionId",
                table: "TransacoesPendentes",
                columns: new[] { "UserId", "PluggyTransactionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransacoesPendentes_UserId_Status",
                table: "TransacoesPendentes",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PluggyContas");

            migrationBuilder.DropTable(
                name: "TransacoesPendentes");

            migrationBuilder.DropTable(
                name: "PluggyItens");
        }
    }
}
