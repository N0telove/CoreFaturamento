using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.GuardClauses;

namespace CoreFaturamento.Domain.Models;

public class PedidoItem
{
    public int PedidoItemId { get; private set; }
    public int ProdutoId { get; private set; }
    public string NomeProduto { get; private set; }
    public int Quantidade { get; private set; }
    public decimal PrecoUnitarioVenda { get; private set; }
    public decimal SubTotal => Quantidade * PrecoUnitarioVenda;

    public PedidoItem(int pedidoItemId, int produtoId, string nomeProduto, int quantidade,
                    decimal precoUnitarioVenda)
    {
        Guard.Against.NegativeOrZero(pedidoItemId, nameof(pedidoItemId));
        Guard.Against.NegativeOrZero(produtoId, nameof(produtoId));
        Guard.Against.NegativeOrZero(quantidade, nameof(quantidade));
        Guard.Against.NegativeOrZero(precoUnitarioVenda, nameof(precoUnitarioVenda));
        Guard.Against.NullOrWhiteSpace(nomeProduto, nameof(nomeProduto));

        PedidoItemId = pedidoItemId;
        ProdutoId = produtoId;
        NomeProduto = nomeProduto.Trim();
        Quantidade = quantidade;
        PrecoUnitarioVenda = precoUnitarioVenda;
    }

    public void RemoverQuantidade(int quantidade)
    {
        Guard.Against.NegativeOrZero(quantidade, nameof(quantidade), "Quantidade inválida.");

        Quantidade -= quantidade;
    }

    public void AdicionarQuantidade(int quantidade)
    {
        Guard.Against.NegativeOrZero(quantidade, nameof(quantidade), "Quantidade inválida.");

        Quantidade += quantidade;
    }


}
