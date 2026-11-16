using Ardalis.GuardClauses;
using CoreFaturamento.ConsoleApp.UI;
using CoreFaturamento.Domain.Services;
using CoreFaturamento.Infra.Persistence;
using CoreFaturamento.Infra.Repositories;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
    .Build();

string pastaRelativa = Guard.Against.NullOrEmpty(config["ConfiguracoesPersistencia:PastaDados"], nameof(pastaRelativa));
string pastaDados = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, pastaRelativa));

Directory.CreateDirectory(pastaDados);

string caminhoProdutos = Path.Combine(pastaDados, config["ConfiguracoesPersistencia:ArquivoProdutos"]!);
string caminhoPedidos = Path.Combine(pastaDados, config["ConfiguracoesPersistencia:ArquivoPedidos"]!);

var produtoRepo = new ProdutoRepositoryJson(caminhoProdutos);
var pedidoRepo = new PedidoRepositoryJson(caminhoPedidos);
var unitOfWork = new JsonUnitOfWork(produtoRepo, pedidoRepo, caminhoProdutos);

var pedidoServico = new PedidoServico(produtoRepo, pedidoRepo, unitOfWork);
var interfaceUsuario = new MenuPrincipal(pedidoServico);

try
{
    interfaceUsuario.MenuInicial();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\nOcorreu um erro inesperado: {ex.Message}.");
    Console.ResetColor();
}