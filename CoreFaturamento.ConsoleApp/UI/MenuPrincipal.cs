using Ardalis.GuardClauses;
using CoreFaturamento.Domain.Models;
using CoreFaturamento.Domain.Services;
using CoreFaturamento.ConsoleApp.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using static CoreFaturamento.Domain.Services.PedidoServico;

namespace CoreFaturamento.ConsoleApp.UI;

public class MenuPrincipal
{
    private List<(int ProdutoId, int Quantidade)> _lista = new();
    private int _pedidoId;
    private readonly PedidoServico _pedidoServico;
    private IReadOnlyCollection<ProdutoDto> Produtos => _pedidoServico.ListarProdutos();
    private PedidoDetalheDto ProdutosPedido => _pedidoServico.ObterPedido(_pedidoId);

    public MenuPrincipal(PedidoServico pedidoServico)
    {
        _pedidoServico = Guard.Against.Null(pedidoServico, nameof(pedidoServico));

    }

    public void MenuInicial()
    {
        while (true)
        {
            Console.WriteLine("---- Central de Faturamento World Skills ----");
            Console.WriteLine("1 - Criar Pedido");
            Console.WriteLine("0 - Sair");
            string? opcao = Console.ReadLine();

            try
            {
                if (opcao == "1")
                {
                    ListarProdutos();
                    AdicionarProdutos();
                    _pedidoId = _pedidoServico.CriarPedido(_lista);
                    _lista.Clear();
                    break;
                }
                else if (opcao == "0")
                {
                    return;
                }
                else
                    Console.WriteLine("Opção inválida.");
            }
            catch (InvalidOperationException ex)
            {
                ResponderExcecao(ex);
            }    
        }
        Menu();
    }

    private void Menu()
    {
        while (true)
        {
            Console.WriteLine("---- Central de Faturamento World Skills ----");
            Console.WriteLine("1 - Adicionar Item");
            Console.WriteLine("2 - Retirar Item");
            Console.WriteLine("3 - Visualizar Pedido");
            Console.WriteLine("4 - Faturar Pedido");
            Console.WriteLine("5 - Cancelar Pedido");
            Console.WriteLine("0 - Sair");

            if (!int.TryParse(Console.ReadLine(), out int opcao))
            {
                Console.WriteLine("Opção inválida.");
                continue;
            }

            try
            {
                switch (opcao)
                {
                    case 0:
                        Console.WriteLine("Até logo.");
                        return;
                    case 1:
                        ListarProdutos();
                        AdicionarProdutos();
                        _pedidoServico.AdicionarItem(_pedidoId, _lista);
                        _lista.Clear();
                        break;
                    case 2:
                        ListarPedido();
                        _pedidoServico.RemoverItem(_pedidoId, RemoverProduto());
                        break;
                    case 3:
                        ListarPedido();
                        break;
                    case 4:
                        _pedidoServico.FaturarPedido(_pedidoId);
                        break;
                    case 5:
                        _pedidoServico.CancelarPedido(_pedidoId);
                        break;
                    default:
                        Console.WriteLine("Opção inválida.");
                        break;
                }
            }
            catch (InvalidOperationException ex)
            {
                ResponderExcecao(ex);
            }
        }
    }

    private void ListarProdutos()
    {
        Console.WriteLine("Produtos disponíveis:");
        foreach (var produto in Produtos)
        {
            Console.WriteLine($"Produto Id: {produto.ProdutoId}.");
            Console.WriteLine($"Produto: {produto.Nome}.");
            Console.WriteLine($"Quantidade: {produto.QuantidadeEstoque}.");
            Console.WriteLine($"Preço unitário: {produto.PrecoUnitario}.");
        }
        
    }

    private void AdicionarProdutos()
    {
        while (true)
        {

            int produtoId = ConsoleInput.LerInteiro("Informe o ID do produto: ");
            int produtoQuantidade = ConsoleInput.LerInteiro("Informe a quantidade do produto: ");

            _lista.Add((produtoId, produtoQuantidade));

            string? opcao;
            while (true)
            {
                Console.WriteLine("1 - Continuar");
                Console.WriteLine("0 - Sair");
                opcao = Console.ReadLine();
                if (opcao != "0" && opcao != "1")
                {
                    Console.WriteLine("Opção inválida.");
                    continue;
                }
                break;
            }
            if (opcao == "0")
                break;
        }
    }

    private void ListarPedido()
    {
        Console.WriteLine($"Pedido #{ProdutosPedido.PedidoId} | Status {ProdutosPedido.Status} | Total: R$ {ProdutosPedido.ValorBruto:C}");
        foreach(var item in ProdutosPedido.Itens)
        {
            Console.WriteLine($" - [{item.ProdutoId}] {item.NomeProduto} | Qtd: {item.Quantidade} | {item.SubTotal:C}");
        }
    }

    private (int, int) RemoverProduto()
    {
        int produtoId = ConsoleInput.LerInteiro("Informe o ID do produto: ");
        int produtoQuantidade = ConsoleInput.LerInteiro("Informe a quantidade que deseja remover do produto: ");
        return (produtoId, produtoQuantidade);
    }

    private void ResponderExcecao(InvalidOperationException ex)
    {
        _lista.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\nOperação não realizada: {ex.Message}");
        Console.ResetColor();
    }
}
