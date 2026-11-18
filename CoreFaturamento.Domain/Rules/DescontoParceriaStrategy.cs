using System;
using System.Collections.Generic;
using System.Text;

namespace CoreFaturamento.Domain.Rules;

public class DescontoParceriaStrategy : DescontoBase
{
    public override decimal Percentual => 0.9m; 
}