namespace Mechanics.Infra.Messaging.Options;

public class MessagingOptions
{
    public required Dictionary<string, string> QueueNames { get; init; }
}
