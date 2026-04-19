using Amazon.SQS;
using Amazon.SQS.Model;
using Mechanics.Infra.Messaging.Options;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Mechanics.Infra.Messaging.Publishers;

public class EventPublisher(IAmazonSQS sqsClient, IOptions<MessagingOptions> options) : IEventPublisher
{
    private readonly Dictionary<string, string> _queueNames = options.Value.QueueNames;

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        var key = typeof(T).Name.Replace("Event", string.Empty);

        if (!_queueNames.TryGetValue(key, out var queueName))
            throw new InvalidOperationException(
                $"Queue not configured for event '{key}'.");

        var queueUrlResponse = await sqsClient.GetQueueUrlAsync(queueName, cancellationToken);

        await sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = queueUrlResponse.QueueUrl,
            MessageBody = JsonSerializer.Serialize(message),
        }, cancellationToken);
    }
}
