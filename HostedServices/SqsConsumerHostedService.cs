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
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 20
                };

                var response = await _sqs.ReceiveMessageAsync(receiveRequest, stoppingToken);

                if (response.Messages != null && response.Messages.Any())
                {
                    foreach (var message in response.Messages)
                    {
                        _logger.LogInformation("Received message: {MessageId} - {Body}", message.MessageId, message.Body);



                        await _sqs.DeleteMessageAsync(queueUrl, message.ReceiptHandle, stoppingToken);
                        _logger.LogInformation("Deleted message: {MessageId}", message.MessageId);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consumir mensagens da fila");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }

        _logger.LogInformation("SQS consumer stopping for {QueueUrl}", queueUrl);
    }
}
