using AuvoSystems.Web.Core.Services;

namespace AuvoSystems.Web.Config
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterComponents(this IServiceCollection services)
        {
            services.AddSingleton<ITratamentoArquivosService, TratamentoArquivosService>();
        }
}
}
