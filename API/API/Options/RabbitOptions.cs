namespace API.Options;

public class RabbitOptions
{
    public string Host {  get; set; }   
    public string Username { get; set; }
    public string Password { get; set; }
    public string TaskQueueName { get; set; }
    public string UpdateQueueName { get; set; }
}
