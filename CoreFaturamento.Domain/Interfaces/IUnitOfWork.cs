using CoreFaturamento.Domain.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CoreFaturamento.Domain.Interfaces;

public interface IUnitOfWork
{
    void SalvarAlteracoes(Dictionary<int, Produto> catalogo, Dictionary<int, Pedido> pedidos);
}
