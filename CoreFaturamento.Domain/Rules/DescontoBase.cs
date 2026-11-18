using System;
using System.Collections.Generic;
using System.Text;

namespace CoreFaturamento.Domain.Rules;

public abstract class DescontoBase : IDescontoStrategy
{
    public abstract decimal Percentual { get; }

    public string DescricaoDesconto => Percentual.ToString("P0");

    public decimal Calcular(decimal valorOriginal)
    {
        return valorOriginal - (valorOriginal * Percentual);
    }

}
