using System;
using System.Collections.Generic;
using System.Text;

namespace CoreFaturamento.Domain.Rules;

public interface IDescontoStrategy
{
    decimal Percentual { get; }
    string DescricaoDesconto { get; }
    decimal Calcular(decimal valorOriginal);
}
