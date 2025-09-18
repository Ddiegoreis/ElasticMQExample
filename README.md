# ElasticMQExample (ASP.NET Core + SQS/ElasticMQ)

Projeto exemplo em .NET 9 que expõe uma API para produzir mensagens em uma fila SQS (via ElasticMQ) e um consumidor em background (`HostedService`) que lê e remove mensagens da fila.

Componentes principais:
- `Controllers/MessagesController.cs`: Controller com endpoint para envio de mensagens.
- `Services/IMessageProducer.cs` e `Services/SqsMessageProducer.cs`: Serviço responsável por publicar mensagens na fila.
- `Infrastructure/QueueUrlProvider.cs`: Garante criação/obtenção da URL da fila (lazy) e cacheia a URL.
- `HostedServices/SqsConsumerHostedService.cs`: Consumidor em background que faz long-polling, processa e apaga mensagens.
- Swagger/OpenAPI habilitado em `/swagger`.

## Pré-requisitos
- .NET SDK 9
- Docker instalado (para subir o ElasticMQ)

## Subindo o ElasticMQ com Docker

Opção 1: Docker CLI
```
docker run --name elasticmq -p 9324:9324 -p 9325:9325 -d softwaremill/elasticmq
```
- SQS endpoint local: http://localhost:9324
- A console web (opcional) fica em http://localhost:9325

Opção 2: docker-compose.yml
```
version: "3.8"
services:
  elasticmq:
    image: softwaremill/elasticmq
    ports:
      - "9324:9324"
      - "9325:9325"
```
Suba com:
```
docker compose up -d
```

## Configuração da API
Por padrão a API usa `appsettings.json`:
```
{
  "Sqs": {
    "ServiceUrl": "http://localhost:9324",
    "Region": "us-east-1",
    "QueueName": "minha-nova-fila"
  }
}
```
- Credenciais AWS não são necessárias no ElasticMQ; o código usa valores dummy.
- A fila é criada automaticamente na primeira utilização.

## Executando a API
Na pasta do projeto:
```
dotnet restore
dotnet run
```
Observe a URL/porta no console e acesse o Swagger em:
```
http://localhost:<porta>/swagger
```

## Endpoints
- POST `messages` – Envia uma mensagem para a fila.
  - Body (application/json): `{ "message": "Olá ElasticMQ do .NET!" }`
- GET `health` – Health check simples.

Exemplo com curl (ajuste a porta):
```
curl -X POST "http://localhost:5000/messages" \
  -H "Content-Type: application/json" \
  -d "{ \"message\": \"Olá ElasticMQ do .NET!\" }"
```

## Funcionamento do consumidor
O `SqsConsumerHostedService` roda em background e:
- Faz long polling (5s) na fila configurada
- Loga o corpo da mensagem recebida
- Exclui a mensagem após o processamento

Customize o processamento dentro do hosted service conforme sua necessidade.

## Estrutura do projeto (resumo)
- `Controllers/` – controllers da API
- `Services/` – contratos e implementação do produtor
- `Infrastructure/` – utilitários de infraestrutura (URL da fila)
- `HostedServices/` – serviços em background (consumidor)
- `Program.cs` – configuração de DI, Swagger e pipeline HTTP

## Observações
- Projeto usa `Swashbuckle.AspNetCore` para Swagger e `AWSSDK.SQS` para interação com SQS.
- Para ambiente real (AWS SQS), ajuste `ServiceUrl`, `Region` e credenciais. Para ElasticMQ local, mantenha `ServiceUrl` e credenciais dummy.
