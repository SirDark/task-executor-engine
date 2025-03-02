using API.Models.Entities;

namespace API.Models.Dto;

public class UpdateTaskDto
{
    public Guid id { get; set; }
    public DateTime? started_at { get; set; }
    public DateTime? finished_at { get; set; }
    public required Status status { get; set; }
    public string? stdout { get; set; }
    public string? stderr { get; set; }
    public int? exit_code { get; set; }
}
