# ContaMente

![](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![](https://img.shields.io/badge/Microsoft_SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![](https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white)
![](https://img.shields.io/badge/-Swagger-%23Clojure?style=for-the-badge&logo=swagger&logoColor=white)

API do Projeto de gerenciamento de finan�as pessoais. ([Front-end](https://github.com/Ramon-Mateus/ContaMente))

# Instalar
- .Net 8+: [Aqui](https://dotnet.microsoft.com/pt-br/download)
- Execute esse comando para baixar a ferramenta do Entity Framework para gerenciar migrations e updates no banco:
```shell
dotnet tool install --global dotnet-ef
```

_Ap�s instalar os itens listados acima, vamos baixar a imagem docker do SQL Server e subir o container com a imagem baixada._

## Docker
- Baixar a imagem do MSSQL:
```shell
docker pull mcr.microsoft.com/mssql/server
```

- Subir o container do MSSQL:
```shell
docker run --name sqlserver -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=SenhaForte123#" -p 1433:1433 -d mcr.microsoft.com/mssql/server
```

- Para verificar se o container subiu corretamente execute o comando abaixo no terminal e veja se o status est� UP:
```shell
docker ps
```

_Adiante, para rodar o projeto basta estar na raiz e rodar os comandos abaixo em sequ�ncia. Eles v�o, respectivamente, criar a migration e atualizar o banco e ,por fim, rodar o projeto._

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
