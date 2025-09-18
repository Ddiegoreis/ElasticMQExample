using Amazon.SQS;
using Amazon.SQS.Model;

public class QueueUrlProvider
{
    private readonly IAmazonSQS _sqs;
    private readonly string _queueName;
    private string? _queueUrl;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public QueueUrlProvider(IAmazonSQS sqs, string queueName)
    {
        _sqs = sqs;
        _queueName = queueName;
    }

    public async Task<string> GetQueueUrlAsync(CancellationToken ct = default)
    {
        if (_queueUrl is not null) return _queueUrl;
        await _lock.WaitAsync(ct);
        try
        {
            if (_queueUrl is not null) return _queueUrl;
            var createResponse = await _sqs.CreateQueueAsync(new CreateQueueRequest { QueueName = _queueName }, ct);
            _queueUrl = createResponse.QueueUrl;
            return _queueUrl;
        }
        finally
        {
            _lock.Release();
        }
    }
}
