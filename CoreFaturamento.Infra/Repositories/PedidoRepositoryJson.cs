using Ardalis.GuardClauses;
using CoreFaturamento.Domain.Interfaces;
using CoreFaturamento.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreFaturamento.Infra.Repositories;

public class PedidoRepositoryJson : IPedidoRepository
{
    private readonly string _caminhoArquivo;
    private readonly JsonSerializerOptions _opcoesSerializacao;

    public PedidoRepositoryJson(string caminhoArquivo)
    {
        Guard.Against.NullOrEmpty(caminhoArquivo);

        _caminhoArquivo = caminhoArquivo;

        _opcoesSerializacao = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = true };
        _opcoesSerializacao.Converters.Add(new JsonStringEnumConverter());
        
    }

    public Dictionary<int, Pedido> ObterTodos()
    {
        if (!File.Exists(_caminhoArquivo))
            return new Dictionary<int, Pedido>();

        string jsonTexto = File.ReadAllText(_caminhoArquivo);

        if (string.IsNullOrWhiteSpace(jsonTexto))
            return new Dictionary<int, Pedido>();

        var pedidos = JsonSerializer.Deserialize<List<Pedido>>(jsonTexto, _opcoesSerializacao);

        return pedidos?.ToDictionary(p => p.PedidoId) ?? new Dictionary<int, Pedido>();

    }
    public void SalvarTodos(Dictionary<int, Pedido> pedidos)
    {
        string? pasta = Path.GetDirectoryName(_caminhoArquivo);

        if (!string.IsNullOrEmpty(pasta)) Directory.CreateDirectory(pasta);

        string caminhoTemp = $"{_caminhoArquivo}.tmp";
        string jsonTexto = JsonSerializer.Serialize(pedidos.Values, _opcoesSerializacao);

        File.WriteAllText(caminhoTemp, jsonTexto);
        File.Move(caminhoTemp, _caminhoArquivo, overwrite: true);
    }

}
