using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers
{
    public class UnboundController : ODataController
    {
        private DatabaseContext dbContext = null;

        public UnboundController(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        [ODataRoute("GetRandomRegion()")]
        // http://localhost:5000/GetRandomRegion()
        public Region GetRandomRegion()
        {
            Random rand = new Random();
            int toSkip = rand.Next(0, dbContext.Region.Count());

            return dbContext.Region.Skip(toSkip).Take(1).First();
        }
    }
}