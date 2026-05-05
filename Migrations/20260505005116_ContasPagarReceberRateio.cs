using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ContaMente.Migrations
{
    /// <inheritdoc />
    public partial class ContasPagarReceberRateio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContasPagar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GrupoLancamentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ResponsavelId = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    ValorTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    ValorParcela = table.Column<decimal>(type: "numeric", nullable: false),
                    ValorBaixado = table.Column<decimal>(type: "numeric", nullable: false),
                    ValorRestante = table.Column<decimal>(type: "numeric", nullable: false),
                    NumeroParcelas = table.Column<int>(type: "integer", nullable: false),
                    NumeroDaParcela = table.Column<int>(type: "integer", nullable: false),
                    DataEmissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CategoriaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasPagar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContasPagar_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContasPagar_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContasPagar_Responsaveis_ResponsavelId",
                        column: x => x.ResponsavelId,
                        principalTable: "Responsaveis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContasReceber",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GrupoLancamentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ResponsavelId = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    ValorTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    ValorParcela = table.Column<decimal>(type: "numeric", nullable: false),
                    ValorBaixado = table.Column<decimal>(type: "numeric", nullable: false),
                    ValorRestante = table.Column<decimal>(type: "numeric", nullable: false),
                    NumeroParcelas = table.Column<int>(type: "integer", nullable: false),
                    NumeroDaParcela = table.Column<int>(type: "integer", nullable: false),
                    DataEmissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CategoriaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasReceber", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContasReceber_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContasReceber_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContasReceber_Responsaveis_ResponsavelId",
                        column: x => x.ResponsavelId,
                        principalTable: "Responsaveis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimentacoesCategorias",
                columns: table => new
                {
                    MovimentacaoId = table.Column<int>(type: "integer", nullable: false),
                    CategoriaId = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric", nullable: false),
                    Percentual = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesCategorias", x => new { x.MovimentacaoId, x.CategoriaId });
                    table.ForeignKey(
                        name: "FK_MovimentacoesCategorias_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimentacoesCategorias_Movimentacoes_MovimentacaoId",
                        column: x => x.MovimentacaoId,
                        principalTable: "Movimentacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContasPagarCategorias",
                columns: table => new
                {
                    ContaPagarId = table.Column<int>(type: "integer", nullable: false),
                    CategoriaId = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric", nullable: false),
                    Percentual = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasPagarCategorias", x => new { x.ContaPagarId, x.CategoriaId });
                    table.ForeignKey(
                        name: "FK_ContasPagarCategorias_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContasPagarCategorias_ContasPagar_ContaPagarId",
                        column: x => x.ContaPagarId,
                        principalTable: "ContasPagar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContasPagarMovimentacoes",
                columns: table => new
                {
                    ContaPagarId = table.Column<int>(type: "integer", nullable: false),
                    MovimentacaoId = table.Column<int>(type: "integer", nullable: false),
                    ValorBaixado = table.Column<decimal>(type: "numeric", nullable: false),
                    DataBaixa = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasPagarMovimentacoes", x => new { x.ContaPagarId, x.MovimentacaoId });
                    table.ForeignKey(
                        name: "FK_ContasPagarMovimentacoes_ContasPagar_ContaPagarId",
                        column: x => x.ContaPagarId,
                        principalTable: "ContasPagar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContasPagarMovimentacoes_Movimentacoes_MovimentacaoId",
                        column: x => x.MovimentacaoId,
                        principalTable: "Movimentacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContasReceberCategorias",
                columns: table => new
                {
                    ContaReceberId = table.Column<int>(type: "integer", nullable: false),
                    CategoriaId = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric", nullable: false),
                    Percentual = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasReceberCategorias", x => new { x.ContaReceberId, x.CategoriaId });
                    table.ForeignKey(
                        name: "FK_ContasReceberCategorias_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContasReceberCategorias_ContasReceber_ContaReceberId",
                        column: x => x.ContaReceberId,
                        principalTable: "ContasReceber",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContasReceberMovimentacoes",
                columns: table => new
                {
                    ContaReceberId = table.Column<int>(type: "integer", nullable: false),
                    MovimentacaoId = table.Column<int>(type: "integer", nullable: false),
                    ValorBaixado = table.Column<decimal>(type: "numeric", nullable: false),
                    DataBaixa = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasReceberMovimentacoes", x => new { x.ContaReceberId, x.MovimentacaoId });
                    table.ForeignKey(
                        name: "FK_ContasReceberMovimentacoes_ContasReceber_ContaReceberId",
                        column: x => x.ContaReceberId,
                        principalTable: "ContasReceber",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContasReceberMovimentacoes_Movimentacoes_MovimentacaoId",
                        column: x => x.MovimentacaoId,
                        principalTable: "Movimentacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO "MovimentacoesCategorias" ("MovimentacaoId", "CategoriaId", "Valor", "Percentual")
                SELECT m."Id", m."CategoriaId", m."Valor"::numeric, 100
                FROM "Movimentacoes" AS m
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "MovimentacoesCategorias" AS mc
                    WHERE mc."MovimentacaoId" = m."Id"
                      AND mc."CategoriaId" = m."CategoriaId"
                );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ContasPagar_CategoriaId",
                table: "ContasPagar",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasPagar_ResponsavelId",
                table: "ContasPagar",
                column: "ResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasPagar_UserId",
                table: "ContasPagar",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasPagarCategorias_CategoriaId",
                table: "ContasPagarCategorias",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasPagarMovimentacoes_MovimentacaoId",
                table: "ContasPagarMovimentacoes",
                column: "MovimentacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasReceber_CategoriaId",
                table: "ContasReceber",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasReceber_ResponsavelId",
                table: "ContasReceber",
                column: "ResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasReceber_UserId",
                table: "ContasReceber",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasReceberCategorias_CategoriaId",
                table: "ContasReceberCategorias",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasReceberMovimentacoes_MovimentacaoId",
                table: "ContasReceberMovimentacoes",
                column: "MovimentacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesCategorias_CategoriaId",
                table: "MovimentacoesCategorias",
                column: "CategoriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContasPagarCategorias");

            migrationBuilder.DropTable(
                name: "ContasPagarMovimentacoes");

            migrationBuilder.DropTable(
                name: "ContasReceberCategorias");

            migrationBuilder.DropTable(
                name: "ContasReceberMovimentacoes");

            migrationBuilder.DropTable(
                name: "MovimentacoesCategorias");

            migrationBuilder.DropTable(
                name: "ContasPagar");

            migrationBuilder.DropTable(
                name: "ContasReceber");
        }
    }
}
