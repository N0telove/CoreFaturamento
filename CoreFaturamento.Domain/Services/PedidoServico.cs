using Ardalis.GuardClauses;
using CoreFaturamento.Domain.Interfaces;
using CoreFaturamento.Domain.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace CoreFaturamento.Domain.Services;

public class PedidoServico
{
    public record ProdutoDto(
        int ProdutoId,
        string CodigoSKU,
        string Nome,
        decimal PrecoUnitario,
        int QuantidadeEstoque
        );
    private readonly Dictionary<int, Produto> _catalogo;
    private readonly Dictionary<int, Pedido> _pedidos;
    private readonly IUnitOfWork _unitOfWork;

    public PedidoServico(IProdutoRepository produtoRepo, IPedidoRepository pedidoRepo, IUnitOfWork unitOfWork)
    {
        Guard.Against.Null(produtoRepo, nameof(produtoRepo));
        Guard.Against.Null(pedidoRepo, nameof(pedidoRepo));

        _unitOfWork = Guard.Against.Null(unitOfWork, nameof(unitOfWork));
        _catalogo = produtoRepo.ObterTodos();
        _pedidos = pedidoRepo.ObterTodos();
    }

    public IReadOnlyCollection<ProdutoDto> ListarProdutos() => _catalogo.Values.Select(p => new ProdutoDto(
                                                               ProdutoId: p.ProdutoId,
                                                               CodigoSKU: p.CodigoSKU,
                                                               Nome: p.Nome,
                                                               PrecoUnitario: p.PrecoUnitario,
                                                               QuantidadeEstoque: p.QuantidadeEstoque))
                                                                .ToList().AsReadOnly();
    public void CancelarPedido(int pedidoId)
    {
        if (!_pedidos.TryGetValue(pedidoId, out var pedido))
            throw new InvalidOperationException($"Pedido {pedidoId} não encontrado.");

        pedido.CancelarPedido();

        foreach (var item in pedido.Itens)
        {
            if (!_catalogo.TryGetValue(item.ProdutoId, out var produto))
                throw new InvalidOperationException($"Produto {item.ProdutoId} não existe no catálogo.");

            produto.ReporEstoque(item.Quantidade);
        }

        _unitOfWork.SalvarAlteracoes(_catalogo, _pedidos);
    }
     
    public int CriarPedido(List<(int ProdutoId, int Quantidade)> itensDesejados)
    {

        int pedidoId = (_pedidos.Count > 0 ? _pedidos.Keys.Max() : 0) + 1;

        if (_pedidos.ContainsKey(pedidoId))
            throw new InvalidOperationException($"Já existe um pedido cadastrado com o ID {pedidoId}.");

        itensDesejados = itensDesejados.GroupBy(x => x.ProdutoId)
            .Select(grupo => (
                    ProdutoId: grupo.Key,
                    Quantidade: grupo.Sum(item => item.Quantidade)
                    ))
                .ToList();


        foreach (var (produtoId, quantidade) in itensDesejados)
        {
            if (!_catalogo.TryGetValue(produtoId, out var produto))
                throw new InvalidOperationException($"Produto {produtoId} não encontrado no catálogo.");

            if (produto.QuantidadeEstoque < quantidade)
                throw new InvalidOperationException($"Estoque insuficiente para o produto '{produto.Nome}'. Disponível: {produto.QuantidadeEstoque}, Solicitado {quantidade}.");
        }

        var novoPedido = new Pedido(pedidoId);

        foreach (var (produtoId, quantidade) in itensDesejados)
        {
            var produto = _catalogo[produtoId];
            novoPedido.AdicionarItem(produto, quantidade);
        }

        _pedidos.Add(novoPedido.PedidoId, novoPedido);

        _unitOfWork.SalvarAlteracoes(_catalogo, _pedidos);

        return pedidoId;
    }

    public void AdicionarItem(int pedidoId, List<(int produtoId, int quantidade)> itens)
    {
        foreach ((int produtoId, int quantidade) in itens)
        {
            if (!_pedidos.TryGetValue(pedidoId, out var pedido))
            {
                throw new InvalidOperationException($"Pedido {pedidoId} não encontrado.");
            }
            if (!_catalogo.TryGetValue(produtoId, out var produto))
            {
                throw new InvalidOperationException($"Produto {produtoId} não encontrado.");
            }

            pedido.AdicionarItem(produto, quantidade);
        }

        _unitOfWork.SalvarAlteracoes(_catalogo, _pedidos);
    }
}
