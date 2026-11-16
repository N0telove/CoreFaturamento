using Ardalis.GuardClauses;
using CoreFaturamento.Domain.Interfaces;
using CoreFaturamento.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CoreFaturamento.Infra.Repositories;

public class ProdutoRepositoryJson : IProdutoRepository
{
    private readonly string _caminhoArquivo;
    private readonly JsonSerializerOptions _opcoesSerializacao;

    public ProdutoRepositoryJson(string caminhoArquivo)
    {
        Guard.Against.NullOrWhiteSpace(caminhoArquivo);

        _caminhoArquivo = caminhoArquivo;

        _opcoesSerializacao = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = true };

    }

    public Dictionary<int, Produto> ObterTodos()
    {
        if (!File.Exists(_caminhoArquivo))
            return new Dictionary<int, Produto>();

        string jsonTexto = File.ReadAllText(_caminhoArquivo);

        if (string.IsNullOrWhiteSpace(jsonTexto))
            return new Dictionary<int, Produto>();

        var produtos = JsonSerializer.Deserialize<List<Produto>>(jsonTexto, _opcoesSerializacao);

        return produtos?.ToDictionary(p => p.ProdutoId) ?? new Dictionary<int, Produto>();
    }

    public void SalvarTodos(Dictionary<int, Produto> produtos)
    {
        string? pasta = Path.GetDirectoryName(_caminhoArquivo);

        if (!string.IsNullOrEmpty(pasta)) Directory.CreateDirectory(pasta);

        string caminhoTemp = $"{_caminhoArquivo}.tmp";

        string jsonTexto = JsonSerializer.Serialize(produtos.Values, _opcoesSerializacao);

        File.WriteAllText(caminhoTemp, jsonTexto);
        File.Move(caminhoTemp, _caminhoArquivo, overwrite: true);
    }

}
