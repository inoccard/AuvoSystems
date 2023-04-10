namespace AuvoSystems.Web.Core.DomainObjects;

public class Arquivo
{
    public string Nome { get; private set; }
    public string Caminho { get; private set; }

    public Arquivo(string nome, string caminho)
    {
        Nome = nome;
        Caminho = caminho;
    }
}
