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
    public record PedidoItemDto(
        int PedidoItemId,
        int ProdutoId,
        string ProdutoNome,
        int Quantidade,
        decimal PrecoUnitarioVenda,
        decimal SubTotal
        );

    public record PedidoDetalheDto(
        int PedidoId,
        DateTime DataPedido,
        string Status,
        decimal ValorBruto,
        IReadOnlyCollection<PedidoItemDto> Itens
        );

    public record ProdutoDto(
        int ProdutoId,
        string CodigoSKU,
        string Nome,
        decimal PrecoUnitario,
        int QuantidadeEstoque
        );

    private Dictionary<int, Produto> _catalogo;
    private Dictionary<int, Pedido> _pedidos;
    private readonly IProdutoRepository _produtoRepo;
    private readonly IPedidoRepository _pedidoRepo;
    private readonly IUnitOfWork _unitOfWork;

    public PedidoServico(IProdutoRepository produtoRepo, IPedidoRepository pedidoRepo, IUnitOfWork unitOfWork)
    {
        _produtoRepo = Guard.Against.Null(produtoRepo, nameof(produtoRepo));
        _pedidoRepo = Guard.Against.Null(pedidoRepo, nameof(pedidoRepo));

        _unitOfWork = Guard.Against.Null(unitOfWork, nameof(unitOfWork));
        _catalogo = produtoRepo.ObterTodos();
        _pedidos = pedidoRepo.ObterTodos();
    }

    public PedidoDetalheDto ObterPedido(int pedidoId)
    {
        if (!_pedidos.TryGetValue(pedidoId, out var pedido))
            throw new InvalidOperationException($"Pedido {pedidoId} não encontrado.");

        var itensDto = pedido.Itens
            .Select(item => new PedidoItemDto(
                item.PedidoItemId,
                item.ProdutoId,
                item.NomeProduto,
                item.Quantidade,
                item.PrecoUnitarioVenda,
                item.SubTotal))
            .ToList()
            .AsReadOnly();

        return new PedidoDetalheDto(
            pedido.PedidoId,
            pedido.DataPedido,
            pedido.Status.ToString(),
            pedido.ValorBruto,
            itensDto
        );
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

        PersistirOuReverter();
    }

    public void FaturarPedido(int pedidoId)
    {
        if (!_pedidos.TryGetValue(pedidoId, out var pedido))
            throw new InvalidOperationException($"Pedido {pedidoId} não encontrado.");

        pedido.FaturarPedido();

        PersistirOuReverter();
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

        PersistirOuReverter();

        return pedidoId;
    }

    public void AdicionarItem(int pedidoId, List<(int produtoId, int quantidade)> itens)
    {
        itens = itens.GroupBy(x => x.produtoId)
                    .Select(grupo => (
                        ProdutoId: grupo.Key,
                        Quantidade: grupo.Sum(item => item.quantidade)
                        ))
                    .ToList();

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

        PersistirOuReverter();
    }

    public void RemoverItem(int pedidoId, (int produtoId, int quantidade) item)
    {
        
        if (!_pedidos.TryGetValue(pedidoId, out var pedido))
        {
            throw new InvalidOperationException($"Pedido {pedidoId} não encontrado.");
        }
        if (!_catalogo.TryGetValue(item.produtoId, out var produto))
        {
            throw new InvalidOperationException($"Produto {item.produtoId} não encontrado.");
        }

        pedido.RemoverItem(produto, item.quantidade);
        PersistirOuReverter();
    }
    private void PersistirOuReverter()
    {
        try
        {
            _unitOfWork.SalvarAlteracoes(_catalogo, _pedidos);
        }
        catch
        {
            _catalogo = _produtoRepo.ObterTodos();
            _pedidos = _pedidoRepo.ObterTodos();
            throw;
        }
    }
}
