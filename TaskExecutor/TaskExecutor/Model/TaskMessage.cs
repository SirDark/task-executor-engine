namespace TaskExecutor.Model;

public class TaskMessage
{
    public Guid TaskId {  get; set; }
    public string Command { get; set; }
}
