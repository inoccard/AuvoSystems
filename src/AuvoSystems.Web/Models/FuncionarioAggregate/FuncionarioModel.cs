using AuvoSystems.Web.Core.DomainObjects;

namespace AuvoSystems.Web.Models.FuncionarioAggregate;

/// <summary>
/// Classe Raiz agregada
/// </summary>
public class FuncionarioModel : Pessoa, IAggregateRoot
{
    public decimal TotalReceber { get; set; }
    public TimeOnly HorasExtras { get; private set; }
    public TimeOnly HorasDebito { get; private set; }
    public int DiasFalta { get; private set; }
    public int DiasExtras { get; private set; }
    public int DiasTrabalhados { get; private set; }

    public FuncionarioModel(
        string nome,
        int codigo,
        decimal totalReceber,
        TimeOnly horasExtras,
        TimeOnly horasFalta,
        int diasFalta,
        int diasExtras,
        int diasTrabalhados
        )
        : base(nome)
    {
        Codigo = codigo;
        TotalReceber = totalReceber;
        DiasFalta = diasFalta;
        HorasDebito = horasFalta;
        HorasExtras = horasExtras;
        DiasExtras = diasExtras;
        DiasTrabalhados = diasTrabalhados;
    }
}
