using AuvoSystems.Models;
using AuvoSystems.Web.Core.Services.Notificacoes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics;

namespace AuvoSystems.Web.Controllers;

public class MainController : Controller
{
    private readonly INotificador _notificador;

    protected MainController(INotificador notificador)
    {
        _notificador = notificador;

    }

    protected bool TemNotificacao() => _notificador.TemNotificacao();

    protected ActionResult CustomResponse(object result = null, string viewName = null)
    {
        if (!TemNotificacao())
            return View(result);

        return View(viewName, new NotificacaoModel(_notificador.ObterNotificacoes()));
    }

    protected ActionResult CustomResponse(ModelStateDictionary modelState)
    {
        if (!modelState.IsValid) NotificarErroModelInvalida(modelState);
        return CustomResponse(null, "Error");
    }

    protected void NotificarErroModelInvalida(ModelStateDictionary modelState)
    {
        var erros = modelState.Values.SelectMany(e => e.Errors);
        foreach (var erro in erros)
        {
            var errorMsg = erro.Exception == null ? erro.ErrorMessage : erro.Exception.Message;
            NotificarErro(errorMsg);
        }
    }

    protected void NotificarErro(string mensagem) => _notificador.Handle(new ErrorViewModel(Activity.Current?.Id ?? HttpContext.TraceIdentifier, mensagem));
}
