using CoreFaturamento.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreFaturamento.Domain.Interfaces;

public interface IProdutoRepository
{
    Dictionary<int, Produto> ObterTodos();
    void SalvarTodos(Dictionary<int, Produto> produtos);
}
