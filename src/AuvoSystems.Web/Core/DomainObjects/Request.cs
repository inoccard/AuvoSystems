namespace AuvoSystems.Web.Core.DomainObjects;

public class Request
{
    public string? Responsavel { get; set; }
    public List<IFormFile>? Arquivos { get; set; }
}
