using AuvoSystems.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using AuvoSystems.Web.Core.Services;
using AuvoSystems.Web.Core.DomainObjects;
using AuvoSystems.Web.Models;

namespace AuvoSystems.Controllers;

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

    public async Task<IActionResult> GerarOrderPagamento(Request request)
    {
        if (request is null || request.Arquivos is null || !request.Arquivos.Any())
            throw new Exception("Selecione pelo menos um arquivo");

        List<DepartamentoModel> Departamentos = new();

        await Parallel.ForEachAsync(request.Arquivos, InitOptions(), async (arquivo, _) =>
        {
            try
            {
                var dados = _service.ObterDadosDoArquivo(arquivo.OpenReadStream());
                var departamento = await _service.CalcularEAdicionarDados(arquivo.FileName.Replace(".csv", null), dados);
                Departamentos.Add(departamento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        });

        var jsonString = JsonConvert.SerializeObject(new { request.Responsavel, Departamentos }, Formatting.Indented);

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

    private static ParallelOptions InitOptions() => new()
    {
        MaxDegreeOfParallelism = 1000,
        CancellationToken = CancellationToken.None
    };

}