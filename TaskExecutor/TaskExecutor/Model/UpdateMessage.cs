namespace TaskExecutor.Model;
public class UpdateMessage { 
    public Guid Id{  get; set; }
    public DateTime? started_at{  get; set; } 
    public DateTime? finished_at{  get; set; } 
    public Status? status{  get; set; }
    public string? stdout{  get; set; } 
    public string? stderr{  get; set; }
    public int? exit_code { get; set; }
};
public enum Status
{
    queued,
    in_progress,
    finished,
}