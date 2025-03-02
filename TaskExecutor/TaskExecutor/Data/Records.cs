namespace TaskExecutor.Data;
public record TaskMessage(Guid TaskId, string Command);
public record UpdateMessage(Guid Id, DateTime? started_at, DateTime? finished_at, Status? status, string? stdout, string? stderr, int? exit_code);
public enum Status
{
    queued,
    in_progress,
    finished,
}