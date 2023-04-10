using AuvoSystems.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using AuvoSystems.Web.Core.Services;
using AuvoSystems.Web.Core.DomainObjects;
using AuvoSystems.Web.Models;

namespace AuvoSystems.Controllers
{
    public class HomeController : Controller
    {
        private const string ContentType = "application/json";
        private const string FileDownloadName = "Folha de Pagamento.json";
        private readonly ITratamentoArquivosService _service;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ITratamentoArquivosService service, ILogger<HomeController> logger)
        {
            _service = service;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> GerarOrderPagamento()
        {
            var arquivos = _service.BuscarArquivos();

            if (!arquivos.Any()) return BadRequest("Não existem arquivos");

            List<DepartamentoModel> Departamentos = new();

            var options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 1000,
                CancellationToken = CancellationToken.None
            };

            await Parallel.ForEachAsync(arquivos, options, async (arquivo, _) => {
                try
                {
                    var dados = _service.ObterDadosDoArquivo(arquivo);
                    var departamento = await _service.CalcularEAdicionarDados(arquivo.Caminho, dados);
                    Departamentos.Add(departamento);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            });

            var jsonString = JsonConvert.SerializeObject(Departamentos, Formatting.Indented);

            var _byte = Encoding.UTF8.GetBytes(jsonString);

            var ano = Departamentos.First().AnoVigencia;
            var mes = Departamentos.First().MesVigencia;

            _service.EscreverNoAquivoJson(ano, mes, jsonString);

            var result = File(_byte, ContentType, $"{ano}-{mes}-{FileDownloadName}");
            return result;
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}