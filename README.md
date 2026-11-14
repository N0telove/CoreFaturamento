# CoreFaturamento

Sistema de gerenciamento de pedidos, controle de estoque e cálculo de faturamento desenvolvido em **C# (.NET 10)** como laboratório de revisão arquitetural pós-seletiva WorldSkills (#09 Soluções de Software).

## Arquitetura e Decisões Técnicas

A solução foi estruturada seguindo os princípios de **Clean Architecture** e **SOLID**, isolando completamente as regras de negócio de detalhes de infraestrutura e interface:

- **`CoreFaturamente.Domain`**: Núcleo da aplicação sem dependências externas de projeto.
- **Rich Domain Model**: Entidades (`Pedido`, `Pedidoitem`, `Produto`) com encapsulamento estrito (`private set`, `IReadOnlyColletion<T>`) e validação defensiva via `Ardalis.GuardClauses`.
- **Strategy Pattern**: Regras de desconto desacopladas (`iDescontoStrategy`) permitindo extensão sem alteração do núcleo de cálculo.
- **Application Service & DTOs**: `PedidoServico` orquestra casos de uso, normaliza entradas (`GroupBy`), gerencia transações e projeta dados de leitura em `record` DTOs imutáveis.
- **`CoreFaturamento.Infra`**: Implementação dos contratos de persistência (`IProdutoRepository`, `IPedidoRepository`, `IUnitOfWork`).
- **Persistência Atômica**: Escrita segura em disco utilizando padrão *Write-Temp-and-Replace* (`.tmp` + `File.Move`) e ação compensátória (*rollback*) no `JsonUnitOfWork` para evitar corrupção ou dessincronização entre catálogo e pedidos.
- **`CoreFaturamento.ConsoleApp`**: Camada de apresentação CLI e *Composition Root* (`Program.cs`), configurada via `appsettings.json`.

## Estrutura da Solução

```text
CoreFaturamento/
├── CoreFaturamento.Domain/			# Entidades, Enums, Interfaces, Rules (Strategy) e Services
├── CoreFaturamento.Infra/			# Repositórios Json e JsonUnitOfWork
├── CoreFaturamento.ConsoleApp/		# Interface CLI, Helpers de entrada e Composition Root
└── Dados /							# Arquivos de persistência local (JSON)
```

## Como Executar
Pré-requisitos:
- .NET 10 SDK

Passos
```text
# Clonar o repositório
git clone [https://github.com/N0telove/CoreFaturamento.git](https://github.com/N0telove/CoreFaturamento.git)
cd CoreFaturamento

# Restaurar dependências e compilar
dotnet build

# Executar a aplicação console
dotnet run --project CoreFaturamento.ConsoleApp
```

## Roadmap de Evolução
- [] Concluir fluxos de remoção de itens, visualização, futuramento e cancelamento no `MenuPrincipal`
- [] Adicionar suíte de testes unitários (`xUnit`) cobrindo invariantes de domínio e estratégias de desconto
- [] Implementar segunda camada de persistência com Entity FrameWork Core e SQL Server