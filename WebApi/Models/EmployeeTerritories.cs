using System;
using System.Collections.Generic;

namespace WebApi.Models
{
    public partial class EmployeeTerritories
    {
        public int EmployeeId { get; set; }
        public int TerritoryId { get; set; }

        public Employee Employee { get; set; }
        public Territories Territory { get; set; }
    }
}
