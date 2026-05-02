# SERVICE_NAME

Breve descrição do serviço.

## Definição do ambiente

- SDK: .NET 8.0
- Banco de dados: MSSQL 2025
- Serviço de E-mail: MailPit
- Chave pública para JWT: AWS Secrets Manager

## Serviços consumidos

- [Identity](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-identity): Informações de usuários.

Para executar o projeto rodando as dependências pela AWS, suba os serviços e altere as [configurações](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/Mechanics-13soat/blob/main/docs/configuration.md) com a URL do Load Balancer.
O comando abaixo retorna essa URL:

```powershell
aws elbv2 describe-load-balancers --names fiap-mechanics-dev --query "LoadBalancers[*].DNSName" --output text
```

## Messageria

As filas devem ser criadas pela camada `messaging` do [repositório de infraestrutura](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-infra).

<!-- Remover seção se não for usada -->

### Consumers

| Fila                              | Descrição                              |
|-----------------------------------|----------------------------------------|
| `mechanics-{env}-entidade-evento` | O que este serviço consome desta fila. |

### Publishers

| Fila                              | Descrição                              |
|-----------------------------------|----------------------------------------|
| `mechanics-{env}-entidade-evento` | O que este serviço publica nesta fila. |

## Execução do projeto

Primeiro crie uma cópia do arquivo de configurações do Docker:

```powershell
cp .env.example .env
```

Em cada nova fase do projeto, é recomendável apagar os volumes do Docker para evitar conflitos com a estrutura do banco
de dados criado em fases anteriores. Para fazer isso, execute o seguinte comando na raiz do projeto:

```powershell
docker compose down -v
```

### Execução local

Ao executar o projeto em ambientes de desenvolvimento, o token de autenticação **NÃO** é validado, portanto, pode-se usar um token expirado ou mesmo gerar um com uma chave genérica, facilitando o desenvolvimento.

Primeiro inicie o banco de dados, o serviço de e-mail e o emulador da AWS:

```powershell
docker compose up mssql mailpit localstack -d
```

Aguarde até o serviço `mssql` estar iniciando. O processo leva cerca de 40 segundos.
Com os recursos em execução, execute o projeto com o comando abaixo:

```powershell
dotnet run --project ./src/Mechanics.Api/Mechanics.Api.csproj
```

Caso precise gerar um novo token, use o script `new-token.ps1`:

```powershell
.\scripts\new-token.ps1
```

### Docker Compose

Edite o arquivo `.env` com as URLs dos serviços a serem consumidos.
Para apontar para serviços, o mais fácil é executar o projeto em ambiente DEV na AWS e buscar a URL do Load Balancer.

Após configurar as variáveis do projeto, inicie via Docker Compose:

```powershell
docker compose up -d --build
```

O projeto estará disponível nas seguintes URLs:

- Swagger do projeto: <http://localhost:5000/SERVICE_NAME/swagger>
- Cliente de e-mail: <http://localhost:8025>

### API Gateway

Ao acessar o projeto via API Gateway, é necessário obter um token de acesso.
Utilize o script `invoke-getToken.ps1` para obter um. É necessário que o serviço [Mechanics.Auth](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-auth) já esteja em execução.

```powershell
.\scripts\invoke-getToken.ps1
```

Obtenha a URL do API Gateway com o seguinte comando:

```powershell
aws apigatewayv2 get-apis --query "Items[?Name=='fiap-mechanics-dev-api'].ApiEndpoint" --output text
```

## Pipeline de CI/CD

Ao criar uma PR para as branches abaixo, os testes automatizados serão executados.
Ao completar o PR, os testes são novamente executados e é feito o deploy no ambiente.

| Branch    | Ambiente    |
|-----------|-------------|
| `main`    | Production  |
| `release` | Staging     |
| `develop` | Development |
