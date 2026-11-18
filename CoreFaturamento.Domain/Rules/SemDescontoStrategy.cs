using System;
using System.Collections.Generic;
using System.Text;

namespace CoreFaturamento.Domain.Rules;

public class SemDescontoStrategy : DescontoBase
{
    public override decimal Percentual => 0m;
}
