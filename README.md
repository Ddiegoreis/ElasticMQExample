# ElasticMQExample (ASP.NET Core + SQS/ElasticMQ)

Projeto exemplo em .NET 9 que exp�e uma API para produzir mensagens em uma fila SQS (via ElasticMQ) e um consumidor em background (`HostedService`) que l� e remove mensagens da fila.

Componentes principais:
- `Controllers/MessagesController.cs`: Controller com endpoint para envio de mensagens.
- `Services/IMessageProducer.cs` e `Services/SqsMessageProducer.cs`: Servi�o respons�vel por publicar mensagens na fila.
- `Infrastructure/QueueUrlProvider.cs`: Garante cria��o/obten��o da URL da fila (lazy) e cacheia a URL.
- `HostedServices/SqsConsumerHostedService.cs`: Consumidor em background que faz long-polling, processa e apaga mensagens.
- Swagger/OpenAPI habilitado em `/swagger`.

## Pr�-requisitos
- .NET SDK 9
- Docker instalado (para subir o ElasticMQ)

## Subindo o ElasticMQ com Docker

Op��o 1: Docker CLI
```
docker run --name elasticmq -p 9324:9324 -p 9325:9325 -d softwaremill/elasticmq
```
- SQS endpoint local: http://localhost:9324
- A console web (opcional) fica em http://localhost:9325

Op��o 2: docker-compose.yml
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

## Configura��o da API
Por padr�o a API usa `appsettings.json`:
```
{
  "Sqs": {
    "ServiceUrl": "http://localhost:9324",
    "Region": "us-east-1",
    "QueueName": "minha-nova-fila"
  }
}
```
- Credenciais AWS n�o s�o necess�rias no ElasticMQ; o c�digo usa valores dummy.
- A fila � criada automaticamente na primeira utiliza��o.

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
- POST `messages` � Envia uma mensagem para a fila.
  - Body (application/json): `{ "message": "Ol� ElasticMQ do .NET!" }`
- GET `health` � Health check simples.

Exemplo com curl (ajuste a porta):
```
curl -X POST "http://localhost:5000/messages" \
  -H "Content-Type: application/json" \
  -d "{ \"message\": \"Ol� ElasticMQ do .NET!\" }"
```

## Funcionamento do consumidor
O `SqsConsumerHostedService` roda em background e:
- Faz long polling (5s) na fila configurada
- Loga o corpo da mensagem recebida
- Exclui a mensagem ap�s o processamento

Customize o processamento dentro do hosted service conforme sua necessidade.

## Estrutura do projeto (resumo)
- `Controllers/` � controllers da API
- `Services/` � contratos e implementa��o do produtor
- `Infrastructure/` � utilit�rios de infraestrutura (URL da fila)
- `HostedServices/` � servi�os em background (consumidor)
- `Program.cs` � configura��o de DI, Swagger e pipeline HTTP

## Observa��es
- Projeto usa `Swashbuckle.AspNetCore` para Swagger e `AWSSDK.SQS` para intera��o com SQS.
- Para ambiente real (AWS SQS), ajuste `ServiceUrl`, `Region` e credenciais. Para ElasticMQ local, mantenha `ServiceUrl` e credenciais dummy.
