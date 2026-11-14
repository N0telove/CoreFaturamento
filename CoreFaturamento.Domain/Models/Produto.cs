using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Ardalis.GuardClauses;

namespace CoreFaturamento.Domain.Models;

public class Produto
{
    public int ProdutoId { get; private set; }
    public string CodigoSKU { get; private set; }
    public string Nome { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public int QuantidadeEstoque { get; private set; }

    [JsonConstructor]
    public Produto(int produtoId, string codigoSKU, string nome, decimal precoUnitario, int quantidadeEstoque)
    {
        Guard.Against.NegativeOrZero(produtoId, nameof(produtoId));
        Guard.Against.NullOrWhiteSpace(codigoSKU, nameof(codigoSKU));
        Guard.Against.NullOrWhiteSpace(nome, nameof(nome));
        Guard.Against.NegativeOrZero(precoUnitario, nameof(precoUnitario));
        Guard.Against.Negative(quantidadeEstoque, nameof(quantidadeEstoque));

        ProdutoId = produtoId;
        CodigoSKU = codigoSKU;
        Nome = nome;
        PrecoUnitario = precoUnitario;
        QuantidadeEstoque = quantidadeEstoque;
    }

    public void DebitarEstoque(int quantidade)
    {
        Guard.Against.OutOfRange(quantidade, nameof(quantidade), 1, QuantidadeEstoque, "Quantidade inválida.");

        QuantidadeEstoque -= quantidade;
    }

    public void ReporEstoque(int quantidade)
    {
        Guard.Against.NegativeOrZero(quantidade, nameof(quantidade));

        QuantidadeEstoque += quantidade;
    }

    public void AtualizarPreco(decimal preco)
    {

        Guard.Against.NegativeOrZero(preco, nameof(preco));

        PrecoUnitario = preco;
    }

}
