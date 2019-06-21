using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers
{
    public class NorthwindController : ODataController
    {
        public static Company Northwind;
        static NorthwindController()
        {
            InitData();
        }
        private static void InitData()
        {
            Northwind = new Company()
            {
                Name = "Northwind",
                Revenue = 1000
            };
        }

        [EnableQuery]
        public Company Get()
        {
            return Northwind;
        }
    }
}