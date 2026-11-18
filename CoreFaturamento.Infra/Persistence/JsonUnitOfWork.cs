using Ardalis.GuardClauses;
using CoreFaturamento.Domain.Interfaces;
using CoreFaturamento.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreFaturamento.Infra.Persistence;

public class JsonUnitOfWork : IUnitOfWork
{
    private readonly IProdutoRepository _produtoRepo;
    private readonly IPedidoRepository _pedidoRepo;
    private readonly string _caminhoProdutos;

    public JsonUnitOfWork(
        IProdutoRepository produtoRepo,
        IPedidoRepository pedidoRepo,
        string caminhoProdutos
        )
    {
        _produtoRepo = Guard.Against.Null(produtoRepo);
        _pedidoRepo = Guard.Against.Null(pedidoRepo);
        _caminhoProdutos = Guard.Against.NullOrWhiteSpace(caminhoProdutos);
    }


    public void SalvarAlteracoes(Dictionary<int, Produto> catalogo, Dictionary<int, Pedido> pedidos)
    {
        string? backupProdutos = File.Exists(_caminhoProdutos)
            ? File.ReadAllText(_caminhoProdutos)
            : null;

        try
        {
            _produtoRepo.SalvarTodos(catalogo);
            _pedidoRepo.SalvarTodos(pedidos);
        }
        catch
        {
            if (backupProdutos is not null) 
            {
                string caminhoTemp = $"{_caminhoProdutos}.tmp";
                File.WriteAllText(caminhoTemp, backupProdutos);
                File.Move(caminhoTemp, _caminhoProdutos, overwrite: true);

            }
            else if (File.Exists(_caminhoProdutos))
            {
                File.Delete(_caminhoProdutos);
            }

            throw;
        }
    }
}
