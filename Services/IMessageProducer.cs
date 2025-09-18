public interface IMessageProducer
{
    Task<string> SendAsync(string message, CancellationToken ct = default);
}
