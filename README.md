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

# 8. Na página inicial da aplicação, clice no botão Criar Conta para criar uma conta

# 9. Precione [Ctrl] + [C] no terminal onde foi executado o frontend para finalizá-lo

# 10. Precione [Ctrl] + [C] no terminal onde foi executado o backend para finalizá-lo

# 11. Abra o terminal na pasta raiz do projeto para finalizar o container e apagar os dados do banco
docker compose down -v

# 12. Lista as imagens para identificar o ID da imágem do SQL Server
docker images

# 13. Copie o ID da imagem do SQL Server e utilize-o no comando abaixo para removê-la
docker rmi [IMAGE ID]
```

## Rubricas

### 1. Desenvolvimento de sistemas web e a utilização de arquiteturas em camadas

O aluno desenvolveu a camada de apresentação da aplicação? 👉 [Frontend](https://github.com/alexcar/PlayByte/tree/main/frontend)  
O aluno desenvolveu a camada de serviços da aplicação? 👉 [Application](https://github.com/alexcar/PlayByte/tree/main/src/PlayByte.Application)  
O aluno desenvolveu a camada de negócios da aplicação? 👉 [Domain](https://github.com/alexcar/PlayByte/tree/main/src/PlayByte.Domain)  
O aluno desenvolveu a camada de acesso a dados da aplicação? 👉 [Infrastructure](https://github.com/alexcar/PlayByte/tree/main/src/PlayByte.Infrastructure)  

### 2.  Projetar aplicativos Web com Angular e Web API 

O aluno desenvolveu o módulo de cadastro e login? 👉 [Cadastro](https://github.com/alexcar/PlayByte/blob/main/src/PlayByte.Api/Controllers/UsersController.cs) [Login](https://github.com/alexcar/PlayByte/blob/main/src/PlayByte.Api/Controllers/AuthController.cs)  
O aluno desenvolveu o módulo de transação? 👉 [Transação](https://github.com/alexcar/PlayByte/blob/main/src/PlayByte.Api/Controllers/PaymentsController.cs)  
O aluno desenvolveu o módulo de busca de música? 👉 [Busca](https://github.com/alexcar/PlayByte/blob/main/src/PlayByte.Api/Controllers/SearchController.cs)  
O aluno desenvolveu o módulo de favoritar musica? 👉 [Favoritar](https://github.com/alexcar/PlayByte/blob/main/src/PlayByte.Api/Controllers/FavoritesController.cs)  

### 3. Implementar o acesso a dados utilizando o Entity Framework

 O aluno criou o modelo de acesso utilizando EF? 👉 [Modelo de acesso](https://github.com/alexcar/PlayByte/blob/main/src/PlayByte.Infrastructure/Persistence/ApplicationDbContext.cs)  
 O aluno utilizou migrações para criar o banco de dados? 👉 [Migrações](https://github.com/alexcar/PlayByte/tree/main/src/PlayByte.Infrastructure/Persistence/Migrations)  
 O aluno utilizou utilizou o padrão repository para operações de acesso a dados? 👉 [Repository](https://github.com/alexcar/PlayByte/tree/main/src/PlayByte.Infrastructure/Persistence/Repositories)  
 O aluno utilizou corretamente a injeção de dependência para o acesso ao dados? 👉 [Injeção de dependência](https://github.com/alexcar/PlayByte/blob/main/src/PlayByte.Infrastructure/DependencyInjection.cs)  

### 4. Disponibilizar aplicativos Web no Microsoft Azure

O aluno demonstrou compreender o Microsoft Azure?  
O aluno demonstrou compreender os serviços de armazenamento de dados do Microsoft Azure?  
O aluno demonstrou compreender o serviço de SQL do Microsoft Azure?  
O aluno demonstrou compreender o serviço de aplicativos web do Microsoft Azure?  

