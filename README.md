# ContaMente

![](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![](https://img.shields.io/badge/Microsoft_SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![](https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white)
![](https://img.shields.io/badge/-Swagger-%23Clojure?style=for-the-badge&logo=swagger&logoColor=white)

API do Projeto de gerenciamento de finanças pessoais. ([Front-end](https://github.com/Ramon-Mateus/ContaMente))

# Diagrama de Classes

```mermaid
  classDiagram
    direction LR
    Movimentacao "*"--"1" Categoria
    Movimentacao "*"--"1" TipoPagamento
    Movimentacao "*"--"1" Parcela
    Movimentacao "*"--"1" Recorrencia

    class Movimentacao
    Movimentacao : int Id
    Movimentacao : double Valor
    Movimentacao : DateTime Data
    Movimentacao : string? Descricao
    Movimentacao : bool Fixa
    Movimentacao : int? NumeroParcela
    Movimentacao : int CategoriaId
    Movimentacao :  Categoria? Categoria
    Movimentacao : int TipoPagamentoId
    Movimentacao :  TipoPagamento? TipoPagamento
    Movimentacao : int? RecorrenciaId
    Movimentacao :  Recorrencia? Recorrencia
    Movimentacao : int? ParcelaId
    Movimentacao :  Parcela? Parcela

    class Categoria
    Categoria : int Id
    Categoria : string Nome
    Categoria : string UserId
    Categoria : bool Entrada
    Categoria : IdentityUser User
    Categoria : List<Movimentacao> Movimentacoes

    class TipoPagamento
    TipoPagamento : int Id
    TipoPagamento : string Nome
    TipoPagamento : List<Movimentacao> Movimentacoes

    class Parcela
    Parcela : int Id
    Parcela : double ValorTotal
    Parcela : int NumeroParcelas
    Parcela : double ValorParcela
    Parcela : DateTime DataInicio
    Parcela : DateTime? DataFim
    Parcela : List<Movimentacao> Movimentacoes

    class Recorrencia
    Recorrencia : int Id
    Recorrencia : DateTime DataInicio
    Recorrencia : DateTime DataFim
    Recorrencia : List<Movimentacao> Movimentacoes
```

# Instalar
- .Net 8+: [Aqui](https://dotnet.microsoft.com/pt-br/download)
- Execute esse comando para baixar a ferramenta do Entity Framework para gerenciar migrations e updates no banco:
```shell
dotnet tool install --global dotnet-ef
```

## Docker

- Subir o container do Postgres:
```shell
docker run --name postgres -e POSTGRES_USER=admin -e POSTGRES_PASSWORD=SenhaForte123# -p 5432:5432 -d postgres
```

- Se já criou o container anteriormente, para subir novamente basta rodar esse comando:
```shell
docker start postgres
```

- Para verificar se o container subiu e rodou corretamente execute o comando abaixo no terminal e veja se o status está UP:
```shell
docker ps
```

_Adiante, para rodar o projeto basta estar na raiz e rodar os comandos abaixo em sequência. Eles vão, respectivamente, criar a migration e atualizar o banco e ,por fim, rodar o projeto._

## .Net

- Criar a migration
```shell
dotnet ef migrations add CreateTables
```

- Atualizar o banco com as migrations criadas
```shell
dotnet ef database update
```

- Rodar o projeto
```shell
dotnet run
```
