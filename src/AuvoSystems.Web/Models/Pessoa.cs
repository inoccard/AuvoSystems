using AuvoSystems.Web.Core.SeedWork;

namespace AuvoSystems.Web.Models;

/// <summary>
/// Objeto de Valor
/// </summary>
public class Pessoa : Entidade
{
    public Pessoa(string nome)
    {
        Nome = nome;
    }

    public string Nome { get; private set; }
}
