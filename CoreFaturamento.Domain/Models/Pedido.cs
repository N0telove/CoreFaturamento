using Ardalis.GuardClauses;
using CoreFaturamento.Domain.Enums;
using CoreFaturamento.Domain.Rules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Text;
using System.Text.Json.Serialization;

namespace CoreFaturamento.Domain.Models;

public class Pedido
{
    public int PedidoId { get; private set; }
    public DateTime DataPedido { get; private set; }
    public StatusPedido Status { get; private set; }
    private List<PedidoItem> _itens = new();
    public IReadOnlyCollection<PedidoItem> Itens => _itens.AsReadOnly();
    public decimal ValorBruto => _itens.Sum(i => i.SubTotal);

    [JsonConstructor]
    private Pedido(int pedidoId, DateTime dataPedido, StatusPedido status,
        List<PedidoItem>? itens)
    {
        PedidoId = pedidoId;
        DataPedido = dataPedido;
        Status = status;
        _itens = itens ?? new List<PedidoItem>();
    }

    public Pedido(int pedidoId)
        : this(pedidoId, DateTime.Now, StatusPedido.Pendente, new List<PedidoItem>())
    {
        Guard.Against.NegativeOrZero(pedidoId, nameof(pedidoId));

        PedidoId = pedidoId;
    }

    public void AdicionarItem(Produto produto, int quantidade)
    {
        Guard.Against.Null(produto, nameof(produto));

        if (Status != StatusPedido.Pendente)
            throw new InvalidOperationException("Não é possível adicionar itens a um pedido que não esteja pendente.");

        produto.DebitarEstoque(quantidade);

        PedidoItem? pedido = _itens.FirstOrDefault(x => x.ProdutoId == produto.ProdutoId);

        if (pedido is not null)
        {
            pedido.AdicionarQuantidade(quantidade);
        }
        else
        {
            int pedidoId = (_itens.Count > 0 ? _itens.Max(i => i.PedidoItemId) +1 : 1);
            var novoItem = new PedidoItem(pedidoId, produto.ProdutoId, produto.Nome, quantidade, produto.PrecoUnitario);
            _itens.Add(novoItem);
        }
    }

    public void RemoverItem(Produto produto, int quantidade)
    {
        Guard.Against.Null(produto, nameof(produto));
        Guard.Against.NegativeOrZero(quantidade, nameof(quantidade));

        if (Status != StatusPedido.Pendente)
            throw new InvalidOperationException("Não é possível remover itens a um pedido que não esteja pendente.");

        var itemExistente = _itens.FirstOrDefault
                (x => x.ProdutoId == produto.ProdutoId);

        Guard.Against.Null(itemExistente, nameof(itemExistente), "O produto não foi encontrado no pedido.");



        if (quantidade > itemExistente.Quantidade)
            throw new InvalidOperationException("A quantidade a remover é maior que a quantidade de itens no pedido.");

        if (itemExistente.Quantidade == quantidade)
        {
            _itens.Remove(itemExistente);
        }
        else
        {
            itemExistente.RemoverQuantidade(quantidade);
        }

        produto.ReporEstoque(quantidade);
    }

    public void CancelarPedido()
    {
        if (Status == StatusPedido.Cancelado)
            throw new InvalidOperationException("O pedido já se encontra cancelado.");
        if (Status == StatusPedido.Faturado)
            throw new InvalidOperationException("Não é possível cancelar um produto já faturado.");

        Status = StatusPedido.Cancelado;
    }

    public (decimal valorBruto, decimal valorDesconto, decimal valorLiquido) CalcularFaturamento(IDescontoStrategy estrategia)
    {
        Guard.Against.Null(estrategia);

        if (_itens.Count == 0)
            throw new InvalidOperationException("Não há produtos adicionados ao pedido.");
        if (Status == StatusPedido.Cancelado || Status == StatusPedido.Faturado)
            throw new InvalidOperationException("Pedido já encerrado.");


        decimal valorBruto = ValorBruto;
        decimal valorLiquido = estrategia.Calcular(valorBruto);

        Guard.Against.OutOfRange(valorLiquido, nameof(valorLiquido), 0m, valorBruto, "Desconto Inválido.");
        decimal descontoCalculado = valorBruto - valorLiquido;
        
        Status = StatusPedido.Faturado;
        return (valorBruto, descontoCalculado, valorLiquido);
    }

}
