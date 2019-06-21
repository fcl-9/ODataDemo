using System;
using System.Collections.Generic;

namespace WebApi.Models
{
    public partial class Shippers
    {
        public Shippers()
        {
            Orders = new HashSet<Order>();
        }

        public int ShipperId { get; set; }
        public string CompanyName { get; set; }
        public string Phone { get; set; }

        public ICollection<Order> Orders { get; set; }
    }
}
