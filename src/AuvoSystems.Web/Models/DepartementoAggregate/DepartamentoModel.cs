using AuvoSystems.Web.Core.DomainObjects;
using AuvoSystems.Web.Models.FuncionarioAggregate;

namespace AuvoSystems.Web.Models.DepartementoAggregate;

public class DepartamentoModel : IAggregateRoot
{
    public string Nome { get; set; }
    public int MesVigencia { get; private set; }
    public int AnoVigencia { get; private set; }
    public decimal TotalPagar { get; private set; }
    public decimal TotalDescontos { get; private set; }
    public decimal TotalExtras { get; private set; }

    public ICollection<FuncionarioModel> Funcionarios { get; private set; }

    public DepartamentoModel(string nome, int mesVigencia, int anoVigencia)
    {
        Nome = nome;
        MesVigencia = mesVigencia;
        AnoVigencia = anoVigencia;
        Funcionarios = new List<FuncionarioModel>();
    }

    public void AdicionarTotais(decimal totalPagar, decimal totalDescontos, decimal totalExtras)
    {
        TotalPagar = totalPagar;
        TotalDescontos = totalDescontos;
        TotalExtras = totalExtras;
    }

    public void AdicionarFuncionario(FuncionarioModel funcionario) => Funcionarios.Add(funcionario);
}
