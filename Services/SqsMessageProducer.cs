using Amazon.SQS;
using Amazon.SQS.Model;

public class SqsMessageProducer : IMessageProducer
{
    private readonly IAmazonSQS _sqs;
    private readonly QueueUrlProvider _urlProvider;

    public SqsMessageProducer(IAmazonSQS sqs, QueueUrlProvider urlProvider)
    {
        _sqs = sqs;
        _urlProvider = urlProvider;
    }

    public async Task<string> SendAsync(string message, CancellationToken ct = default)
    {
        var queueUrl = await _urlProvider.GetQueueUrlAsync(ct);
        var response = await _sqs.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = message
        }, ct);
        return response.MessageId;
    }
}
