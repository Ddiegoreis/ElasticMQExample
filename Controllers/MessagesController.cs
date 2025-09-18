using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageProducer _producer;

    public MessagesController(IMessageProducer producer)
    {
        _producer = producer;
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] SendMessageRequestDto dto, CancellationToken ct)
    {
        var messageId = await _producer.SendAsync(dto.Message, ct);
        return Ok(new { MessageId = messageId });
    }
}
