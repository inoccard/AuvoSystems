using AuvoSystems.Models;

namespace AuvoSystems.Web.Core.Services.Notificacoes;

public interface INotificador
{
    bool TemNotificacao();
    List<ErrorViewModel> ObterNotificacoes();
    void Handle(ErrorViewModel notificacao);
}
