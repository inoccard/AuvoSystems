namespace AuvoSystems.Models;

public class ErrorViewModel
{
    public string? RequestId { get; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public string Mensagem { get; }

    public ErrorViewModel(string requestId, string mensagem)
    {
        RequestId = requestId;
        Mensagem = mensagem;
    }
}

public class NotificacaoModel
{
    public List<ErrorViewModel> Notificacoes { get; }

    public NotificacaoModel(List<ErrorViewModel> notificacoes) => Notificacoes = notificacoes;

}