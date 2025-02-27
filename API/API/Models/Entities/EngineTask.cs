using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.Entities;

[Table("tasks")]
public class EngineTask
{
    [Key]
    [Column("id")]
    public Guid id { get; set; }
    [Column("command")]
    public required string command { get; set; }
    [Column("started_at")]
    public DateTime? started_at { get; set; }
    [Column("finished_at")]
    public DateTime? finished_at { get; set; }
    [Column("status")]
    public required Status status { get; set; }
    [Column("stdout")]
    public string? stdout { get; set; }
    [Column("stderr")]
    public string? stderr { get; set; }
    [Column("exit_code")]
    public int? exitcode { get; set; }
}
public enum Status
{
    queued,
    in_progress,
    finished,
}
