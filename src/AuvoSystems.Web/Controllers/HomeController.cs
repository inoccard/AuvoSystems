using AuvoSystems.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using AuvoSystems.Web.Core.Services;
using AuvoSystems.Web.Models;
using AuvoSystems.Web.Core.Services.Notificacoes;
using AuvoSystems.Web.Models.DepartementoAggregate;

namespace AuvoSystems.Web.Controllers;

public class HomeController : MainController
{
    private const string ContentType = "application/json";
    private const string FileDownloadName = "Folha de Pagamento.json";
    private readonly ITratamentoArquivosService _service;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ITratamentoArquivosService service, ILogger<HomeController> logger, INotificador notificador)
        : base(notificador)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GerarOrdemPagamento()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> GerarOrdemPagamento(RequestModel request)
    {
        if (!ModelState.IsValid) return CustomResponse(ModelState);

        if (!RequisicaoValido(request)) return CustomResponse(null, "Error");

        List<DepartamentoModel> Departamentos = new();

        await Parallel.ForEachAsync(request.Arquivos, InitOptions(), async (arquivo, _) =>
        {
            try
            {
                var dados = _service.ObterDadosDoArquivo(arquivo.FileName, arquivo.OpenReadStream());
                var departamento = await _service.CalcularEAdicionarDados(arquivo.FileName.Replace(".csv", null), dados);
                Departamentos.Add(departamento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                NotificarErro(ex.Message);

            }
        });

        if (TemNotificacao()) return CustomResponse(null, "Error");

        var ano = Departamentos.First().AnoVigencia;
        var mes = Departamentos.First().MesVigencia;
        byte[] _byte = null;

        try
        {
            var jsonString = JsonConvert.SerializeObject(new { request.Responsavel, Departamentos }, Formatting.Indented);

            _byte = Encoding.UTF8.GetBytes(jsonString);


            _service.EscreverNoAquivoJson(ano, mes, jsonString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            NotificarErro(ex.Message);
        }

        if (TemNotificacao()) return CustomResponse(null, "Error");

        return File(_byte, ContentType, $"{ano}-{mes}-{FileDownloadName}");
    }

    [HttpGet]
    public IActionResult Sobre()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel(Activity.Current?.Id ?? HttpContext.TraceIdentifier, Activity.Current.StatusDescription ?? ""));
    }

    #region métodos privados
    private static ParallelOptions InitOptions() => new()
    {
        MaxDegreeOfParallelism = 1000,
        CancellationToken = CancellationToken.None
    };


    private bool RequisicaoValido(RequestModel request)
    {
        if (request is null || request.Arquivos is null || !request.Arquivos.Any())
        {
            _logger.LogError("Selecione pelo menos um arquivo");
            NotificarErro("Selecione pelo menos um arquivo");

            return false;
        }

        return true;
    }

    #endregion métodos privados 
}