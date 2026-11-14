using System;
using System.Collections.Generic;
using System.Text;

namespace CoreFaturamento.Domain.Rules;

public interface IDescontoStrategy
{
    decimal Calcular(decimal valorTotalBruto);
}
