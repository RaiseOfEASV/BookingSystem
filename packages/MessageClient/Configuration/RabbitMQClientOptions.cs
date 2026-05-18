namespace MessageClient.Configuration;

public class RabbitMqClientOptions : IMessageClientOptions
{
    public required string ConnectionString { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? HostName { get; set; }
    public int? Port { get; set; }
}