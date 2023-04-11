namespace AuvoSystems.Web.Models;

public class RequestModel
{
    public string? Responsavel { get; set; }
    public List<IFormFile>? Arquivos { get; set; }
}
