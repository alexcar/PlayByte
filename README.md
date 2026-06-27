# PlayByte

API de streaming de música (projeto acadêmico) em Angular no frontend e no backend com **.NET 10 / C#**,
estruturada em **Clean Architecture + DDD**, com **CQRS** (Dapper para leitura, EF Core para escrita
com *soft delete*), **SQL Server**, **Result pattern**, filtro global de
exceções e **Serilog** com persistência de erros no banco e *fallback* para arquivo.

## Arquitetura

```
PlayByte Angular        -> Angular 19 + PrimeNG 19
   |
PlayByte.Api            -> ASP.NET Core Web API (Controllers + OpenAPI/Scalar)
   |  depende de
PlayByte.Application     -> Casos de uso (CQRS via MediatR), portas, behaviors, validação
   |  depende de
PlayByte.Domain          -> Agregados, value objects, eventos, Result pattern (SEM dependências externas, nem MediatR)
PlayByte.Infrastructure  -> EF Core + Dapper + SQL Server, repositorios, interceptors, BCrypt, logger
```

Regra de dependência: tudo aponta para dentro. O `Domain` não conhece EF, Dapper, ASP.NET
nem MediatR. A `Infrastructure` e a `Api` dependem da `Application`; a inversão acontece pelas
portas (`IUserRepository`, `IApplicationDbContext`, `ISqlConnectionFactory`, `IPasswordHasher`,
`IExceptionLogger`, `IDomainEventDispatcher`).

## Pré-requisitos

- Node.js 18.19+ ou 20.11+ (recomendado: 20 LTS)
- npm 10+
- .NET SDK 10.0.100+
- Docker (para subir o SQL Server) ou uma instância de SQL Server local
- `dotnet-ef` (`dotnet tool install --global dotnet-ef`)

## Como rodar

```bash
# 1. Abra o terminal em uma pasta e clone o repositório
git clone https://github.com/alexcar/PlayByte.git

# 2. Mude para a pasta raiz do projeto para subir o banco.
cd PlayByte
docker compose up -d

# 3. Restaura os pacotes e atualiza o banco de dados
dotnet restore
dotnet ef database update -p src/PlayByte.Infrastructure -s src/PlayByte.Api

# 4. Executa a API
dotnet run --project src/PlayByte.Api --launch-profile http

# 5. Abra o terminal na pasta raiz do projeto e popule o catálogo (banda/álbuns/faixas)
./scripts/seed-catalog.ps1

# 6. Execute o frontend Angular
cd frontend
npm install
npm start

# 7. Execute o navegador e acesse
http://localhost:4200

# 8. Na página inicial da aplicação, clice no botão Criar Conta para criar uma conta.

# 9. Precione [Ctrl] + [C] no terminal para finalizar o frontend.

# 10. Precione [Ctrl] + [C] no terminal para finalizar o backend.

# 11. Abra o terminal na pasta raiz do projeto, finalize o container e apaga os dados do banco de dados
docker compose down -v

# 12. Lista as imagens
docker images

# 13. Remove a imagem SQL Server
docker rmi [IMAGE ID]
```

