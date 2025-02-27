﻿using AuvoSystems.Web.Core.DomainObjects;
using AuvoSystems.Web.Models.DepartementoAggregate;

namespace AuvoSystems.Web.Core.Services;

public interface ITratamentoArquivosService
{
    /// <summary>
    /// Busca todos os arquivos do diretório
    /// </summary>
    /// <returns></returns>
    [Obsolete]
    public IList<Arquivo> BuscarArquivos(IEnumerable<string> nomeArquivos);

    /// <summary>
    /// Busca todos os dados de cada arquivo .csv
    /// </summary>
    /// <param name="arquivo"></param>
    /// <returns></returns>
    public DadosArquivo[] ObterDadosDoArquivo(string nomeArquivo, Stream arquivo);

    /// <summary>
    ///  Este método agrupa os funcionario para calcular suas horas, dias e salário a receber
    /// </summary>
    /// <param name="caminho"></param>
    /// <param name="dados"></param>
    /// <returns></returns>
    public Task<DepartamentoModel> CalcularEAdicionarDados(string caminho, DadosArquivo[] dados);

    /// <summary>
    /// Realizar escrita no arquivo para download
    /// </summary>
    /// <param name="anoVigencia"></param>
    /// <param name="mesVigencia"></param>
    /// <param name="jsonString"></param>
    public void EscreverNoAquivoJson(int anoVigencia, int mesVigencia, string jsonString);
}
