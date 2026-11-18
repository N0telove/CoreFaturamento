using System;
using System.Collections.Generic;
using System.Text;

namespace CoreFaturamento.Domain.Rules;

public class DescontoVolumeStrategy : DescontoBase
{
    public override decimal Percentual => 0.95m;

}
