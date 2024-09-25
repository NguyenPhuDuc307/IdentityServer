using System.ComponentModel.DataAnnotations.Schema;

namespace vnLab.BackendServer.Data.Entities;

public class FileUpLoad
{
    public int Id { get; set; }
    public string? Name { get; set; }

    [NotMapped]
    public IFormFile? Photo { get; set; }
    public string? SavedUrl { get; set; }

    [NotMapped]
    public string? SignedUrl { get; set; }
    public string? SavedFileName { get; set; }
}