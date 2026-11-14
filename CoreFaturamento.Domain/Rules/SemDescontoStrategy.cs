using System;
using System.Collections.Generic;
using System.Text;

namespace CoreFaturamento.Domain.Rules;

public class SemDescontoStrategy : IDescontoStrategy
{
    public decimal Calcular(decimal valorTotalBruto) => valorTotalBruto;
}
