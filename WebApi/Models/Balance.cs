using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class Balance
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public int SendingPartyId { get; set; }
        public int ReceivingPartyId { get; set; }
        public DateTime SettlementDate { get; set; }
        public double CashBuy { get; set; }
        public double CashSell { get; set; }
        public double UnitBuy { get; set; }
        public double UnitSell { get; set; }
        public int MessageBuy { get; set; }
        public int MessageSell { get; set; }
        public string Isin { get; set; }
        public string Ccy { get; set; }
        public string ReceivingPartyName { get; set; }
        public string SendingPartyName { get; set; }

    
        public int NetCash { get; set; }
        public int TotalMessage
        { 
            get;
            set; }
    }
}
