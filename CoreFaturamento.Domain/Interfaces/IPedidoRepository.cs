using CoreFaturamento.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreFaturamento.Domain.Interfaces;

public interface IPedidoRepository
{
    Dictionary<int, Pedido> ObterTodos();
    void SalvarTodos(Dictionary<int, Pedido> pedidos);
}
