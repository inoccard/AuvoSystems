using AuvoSystems.Models;

namespace AuvoSystems.Web.Core.Services.Notificacoes;

public class Notificador : INotificador
{
    private readonly List<ErrorViewModel> _notificacoes;

    public Notificador()
    {
        _notificacoes = new List<ErrorViewModel>();
    }

    public void Handle(ErrorViewModel notificacao)
    {
        _notificacoes.Add(notificacao);
    }

    public List<ErrorViewModel> ObterNotificacoes()
    {
        return _notificacoes;
    }

    public bool TemNotificacao()
    {
        return _notificacoes.Any();
    }
}
