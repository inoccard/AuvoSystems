using AuvoSystems.Web.Core.Services;
using AuvoSystems.Web.Core.Services.Notificacoes;

namespace AuvoSystems.Web.Config;

public static class DependencyInjectionConfig
{
    public static void RegisterComponents(this IServiceCollection services)
    {
        services.AddScoped<INotificador, Notificador>();
        services.AddScoped<ITratamentoArquivosService, TratamentoArquivosService>();
    }
}
