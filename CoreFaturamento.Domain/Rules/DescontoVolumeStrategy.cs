using System;
using System.Collections.Generic;
using System.Text;

namespace CoreFaturamento.Domain.Rules;

public class DescontoVolumeStrategy : IDescontoStrategy
{
    public decimal Calcular(decimal valorTotalBruto) => valorTotalBruto * 0.95m;

}
