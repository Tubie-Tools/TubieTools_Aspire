using System;
using System.Collections.Generic;
using System.Text;

namespace TubieTools_Aspire.Tests.PaymentIntegrations
{
    public class Payment
    {
        public decimal Amount { get; internal set; }
        public string PaymentToken { get; internal set; }
    }
}
