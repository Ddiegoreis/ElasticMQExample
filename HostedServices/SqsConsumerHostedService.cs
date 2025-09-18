using Amazon.SQS;
using Amazon.SQS.Model;

public class SqsConsumerHostedService : BackgroundService
{
    private readonly IAmazonSQS _sqs;
    private readonly QueueUrlProvider _urlProvider;
    private readonly ILogger<SqsConsumerHostedService> _logger;

    public SqsConsumerHostedService(IAmazonSQS sqs, QueueUrlProvider urlProvider, ILogger<SqsConsumerHostedService> logger)
    {
        _sqs = sqs;
        _urlProvider = urlProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueUrl = await _urlProvider.GetQueueUrlAsync(stoppingToken);
        _logger.LogInformation("SQS consumer started for {QueueUrl}", queueUrl);

        while (true)
        {
            try
            {
                var receiveRequest = new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MaxNumberOfMessages = 5,
                    WaitTimeSeconds = 5
                };

                var response = await _sqs.ReceiveMessageAsync(receiveRequest, stoppingToken);

                foreach (var message in response.Messages)
                {
                    _logger.LogInformation("Mensagem recebida: {Body}", message.Body);

                    // processing logic here

                    await _sqs.DeleteMessageAsync(queueUrl, message.ReceiptHandle, stoppingToken);
                    _logger.LogInformation("Mensagem apagada da fila. ID: {Id}", message.MessageId);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // shutdown requested
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consumir mensagens da fila");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }
}
