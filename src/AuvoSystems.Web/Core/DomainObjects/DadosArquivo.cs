using CsvHelper.Configuration.Attributes;

namespace AuvoSystems.Web.Core.DomainObjects
{
    public class DadosArquivo
    {
        [Name("Código")]
        public int Codigo { get; set; }
        [Name("Nome")]
        public string Nome { get; set; }
        [Name("Valor hora")]
        public string ValorHoraRecebido { get; set; }
        [Name("Data")]
        public DateOnly Data { get; set; }
        [Name("Entrada")]
        public TimeOnly Entrada { get; set; }
        [Name("Saída")]
        public TimeOnly Saida { get; set; }
        [Name("Almoço")]
        public string Almoco { get; set; }

        public decimal ValorHora
        {
            get
            {
                var valor = decimal.Parse(ValorHoraRecebido.Replace("R$", null).Replace(" ", null).Trim() ?? "0");
                return valor;
            }
        }

        public TimeOnly HoraInicioAlmoco
        {
            get
            {
                var horaInicial = TimeOnly.Parse(Almoco.Replace(" ", null)[..(Almoco.IndexOf("-") - 1)].Trim());
                return horaInicial;
            }
        }

        public TimeOnly HoraFimAlmoco
        {
            get
            {
                var horaFinal = TimeOnly.Parse(Almoco.Replace(" ", null)[Almoco.IndexOf("-")..].Trim());
                return horaFinal;
            }
        }
    }
}
