using System;
using System.Collections.Generic;
using System.Text;

namespace CoreFaturamento.Domain.Rules;

public class DescontoParceriaStrategy : IDescontoStrategy
{
    public decimal Calcular(decimal valorTotalBruto) => valorTotalBruto * 0.9m; 
}