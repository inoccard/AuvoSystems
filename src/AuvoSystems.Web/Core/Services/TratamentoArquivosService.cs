using AuvoSystems.Web.Core.DomainObjects;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using System.Text;
using AuvoSystems.Web.Models;
using AuvoSystems.Web.Controllers;

namespace AuvoSystems.Web.Core.Services
{
    public class TratamentoArquivosService : ITratamentoArquivosService
    {
        private readonly ILogger<HomeController> _logger;

        public TratamentoArquivosService(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Obsolete]
        public IList<Arquivo> BuscarArquivos(IEnumerable<string> nomeArquivos)
        {
            try
            {
                DirectoryInfo dirInfo = new(@"C:\Arquivo a Importar\");

                _logger.LogInformation("Buscando todos os arquivos...");
                var arquivos = dirInfo.GetFiles().Where(arquivo => nomeArquivos.Select(n => n).Contains(arquivo.Name)).AsParallel();

                if(!arquivos.Any())
                    throw new Exception($"o diretório padrão não contém arquivos para importar, coloque os arquivos em: {dirInfo.FullName}");

                // todos os arquivos neste diretório devem ser do tipo .csv
                if (arquivos.Any(a => a.Exists && a.Extension != ".csv"))
                    throw new Exception($"O diretório {dirInfo.FullName} deve conter somente arquivos '.csv'");

                List<Arquivo> arquivosResult = new();
                arquivosResult.AddRange(arquivos.Select(x => new Arquivo(x.Name, x.FullName)));

                return arquivosResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public DadosArquivo[] ObterDadosDoArquivo(string nomeArquivo, Stream arquivo)
        {
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                HasHeaderRecord = true,
                DetectDelimiter = true,
                DetectColumnCountChanges = true,
                AllowComments = true,
            };

            _logger.LogInformation($"Buscando todos os dados do arquivo {nomeArquivo}...");
            using var reader = new StreamReader(arquivo, Encoding.Latin1);
            using var csv = new CsvReader(reader, config);
            var dados = csv.GetRecords<DadosArquivo>().ToArray();
            reader.Close();

            return dados;
        }

        public async Task<DepartamentoModel> CalcularEAdicionarDados(string nomeArquivo, DadosArquivo[] dados)
        {
            var diasDeTrabalho = 30;
            Dictionary<string, decimal> totais = new();
            var data = dados.FirstOrDefault().Data;

            // agrupa por código do funcionário
            var funcionariosAgrupados = dados.GroupBy(d => d.Codigo).OrderBy(e => e.Key);

            ValidarData(data, dados);

            DepartamentoModel departamento = new(nomeArquivo, data.Month, data.Year);
            _logger.LogInformation($"Calculando dados do departamento {nomeArquivo}...");

            // Processamento paralelo
            await Parallel.ForEachAsync(funcionariosAgrupados, (funcionario, _) =>
            {
                var diasTrabalhados = funcionario.Select(f => f.Data).ToArray().Length;
                var codigo = funcionario.Key;
                var nomeFuncionario = funcionario.First().Nome;
                var valorHora = funcionario.First().ValorHora;

                var diasExtra = CalcularDiasExtras(diasTrabalhados, diasDeTrabalho);
                var diasFalta = CalcularDiasFalta(diasTrabalhados, diasDeTrabalho);

                var (horasExtra, horasFalta, horasTrabalhadasMes) = CalcularHoras(funcionario);

                var (salarioExtra, salarioDesconto, salarioFinal) = CalcularSalarios(horasTrabalhadasMes, horasExtra, horasFalta, valorHora, diasExtra, diasFalta);

                AdicionarTotais(ref totais, salarioExtra, salarioDesconto, salarioFinal);

                departamento.AdicionarFuncionario(
                    new FuncionarioModel(nomeFuncionario, codigo, salarioFinal, TimeOnly.FromTimeSpan(horasExtra), TimeOnly.FromTimeSpan(horasFalta), diasFalta, diasExtra, diasTrabalhados)
                    );

                return new ValueTask();
            });

            departamento.AdicionarTotais(totais["totalPagar"], totais["totalDescontos"], totais["totalExtras"]);

            return departamento;
        }

        public void EscreverNoAquivoJson(int anoVigencia, int mesVigencia, string jsonString)
        {
            _logger.LogInformation($"Escrevendo no arquivo json...");
            var file = new FileInfo(Path.GetFullPath($@"{Path.GetTempPath()}Ordem-de-Pagamento-{anoVigencia}-{mesVigencia}.json"));

            DeleteFile(file);

            File.WriteAllText(file.FullName, jsonString);

            DeleteFile(file);
        }

        #region Metodos Privados
        /// <summary>
        /// Caso o arquivo já eista, excluir
        /// </summary>
        /// <param name="file"></param>
        private static void DeleteFile(FileInfo file)
        {
            if (file.Exists)
                file.Delete();
        }

        /// <summary>
        /// Incrementa os valores totais dos funcionarios
        /// </summary>
        /// <param name="pagar"></param>
        /// <param name="salarioExtra"></param>
        /// <param name="salarioDesconto"></param>
        /// <param name="salarioFinal"></param>
        private static void AdicionarTotais(ref Dictionary<string, decimal> pagar, decimal salarioExtra, decimal salarioDesconto, decimal salarioFinal)
        {
            if (pagar.Count == 0)
            {
                pagar.Add("totalPagar", salarioFinal);
                pagar.Add("totalExtras", salarioExtra);
                pagar.Add("totalDescontos", salarioDesconto);
            }
            else
            {
                pagar["totalPagar"] += salarioFinal;
                pagar["totalExtras"] += salarioExtra;
                pagar["totalDescontos"] += salarioDesconto;
            }
        }

        /// <summary>
        /// Realiza cálculo dos dias extras
        /// </summary>
        /// <param name="diasTrabalhados"></param>
        /// <param name="diasDeTrabalho"></param>
        /// <returns></returns>
        private static int CalcularDiasExtras(int diasTrabalhados, int diasDeTrabalho) => diasTrabalhados > diasDeTrabalho ? diasTrabalhados - diasDeTrabalho : 0;
        private static int CalcularDiasFalta(int diasTrabalhados, int diasDeTrabalho) => diasTrabalhados < diasDeTrabalho ? diasDeTrabalho - diasTrabalhados : 0;

        /// <summary>
        /// Realiza o cálculo das horas trabalhadas e não trabalhadas
        /// </summary>
        /// <param name="funcionario"></param>
        /// <returns></returns>
        private static (TimeSpan horasExtra, TimeSpan horasFalta, TimeSpan horasTrabalhadasMes) CalcularHoras(IGrouping<int, DadosArquivo> funcionario)
        {
            TimeOnly cargaHoraria = new(9, 0, 0);
            var horasExtra = TimeSpan.Zero;
            var horasFalta = TimeSpan.Zero;
            var horasTrabalhadasMes = TimeSpan.Zero;

            foreach (var dado in funcionario)
            {
                var horasTrabalhadasDia = (dado.HoraInicioAlmoco - dado.Entrada + (dado.Saida - dado.HoraFimAlmoco));
                horasExtra += horasTrabalhadasDia > cargaHoraria.ToTimeSpan() ? horasTrabalhadasDia - cargaHoraria.ToTimeSpan() : new();
                horasFalta += horasTrabalhadasDia < cargaHoraria.ToTimeSpan() ? cargaHoraria.ToTimeSpan() - horasTrabalhadasDia : new();
                horasTrabalhadasMes += horasTrabalhadasDia;
            }

            return (horasExtra, horasFalta, horasTrabalhadasMes);
        }

        /// <summary>
        /// Realiza cálculos do salário final a receber
        /// </summary>
        /// <param name="horasTrabalhadasMes"></param>
        /// <param name="horasExtra"></param>
        /// <param name="horasFalta"></param>
        /// <param name="valorHora"></param>
        /// <param name="diasExtra"></param>
        /// <param name="diasFalta"></param>
        /// <returns></returns>
        private static (decimal salarioExtra, decimal salarioDesconto, decimal salarioFinal) CalcularSalarios(
            TimeSpan horasTrabalhadasMes, TimeSpan horasExtra, TimeSpan horasFalta, decimal valorHora, int diasExtra, int diasFalta)
        {
            TimeOnly cargaHoraria = new(9, 0, 0);

            var _cargaHoraria = cargaHoraria.ToTimeSpan();

            var salarioBruto = (decimal)horasTrabalhadasMes.TotalHours * valorHora;
            var salarioExtra = ((diasExtra * (decimal)_cargaHoraria.TotalHours) + (decimal)horasExtra.TotalHours) * valorHora;
            var salarioDesconto = ((diasFalta * (decimal)_cargaHoraria.TotalHours) + (decimal)horasFalta.TotalHours) * valorHora;
            var salarioFinal = salarioBruto + salarioExtra - salarioDesconto;

            return (salarioExtra, salarioDesconto, salarioBruto);
        }

        /// <summary>
        /// em cada arquivo deve conter dados somente de 1 mês
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dados"></param>
        /// <exception cref="Exception"></exception>
        private static void ValidarData(DateOnly data, DadosArquivo[] dados)
        {
            if (dados.Any(x => x.Data.Month != data.Month))
                throw new Exception("Todoas os dados devem ser somentes de um mês, neste arquivo contém dados de mais de um!");
        }
        #endregion Métodos Privados
    }
}
