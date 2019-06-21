using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.UriParser;
using WebApi.Models;

namespace WebApi.Controllers
{
    public class EmployeesController : ODataController
    {
        private DatabaseContext dbContext = null;

        public EmployeesController(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        #region Basic CRUD of Employees

        [EnableQuery]
        public IQueryable<Employee> Get()
        {
            return dbContext.Employees;
        }

        [EnableQuery]
        public SingleResult<Employee> Get([FromODataUri] int key)
        {
            IQueryable<Employee> result = dbContext.Employees.Where(p => p.EmployeeId == key);
            return SingleResult.Create(result);
        }

        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var employee = await dbContext.Employees.FindAsync(key);
            if (employee == null)
            {
                return NotFound();
            }
            dbContext.Employees.Remove(employee);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }

        public async Task<IActionResult> Patch([FromODataUri] int key, [FromBody] Delta<Employee> employee)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await dbContext.Employees.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            employee.Patch(entity);
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Updated(entity);
        }

        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] Employee update)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (key != update.EmployeeId)
            {
                return BadRequest();
            }
            dbContext.Entry(update).State = EntityState.Modified;
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Updated(update);
        }


        private bool EmployeeExists(int key)
        {
            return dbContext.Employees.Any(e => e.EmployeeId == key);
        }

        public async Task<IActionResult> Post([FromBody] Employee employee)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            dbContext.Employees.Add(employee);
            await dbContext.SaveChangesAsync();
            return Created(employee);
        }
        #endregion

        #region Action
        [HttpPost]
        // POST http://localhost:5000/Employees(1)/Default.UpdateNotes
        // Body: {"Notes": "This is updated Note"}
        public async Task<IActionResult> UpdateNotes([FromODataUri] int key, ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var employee = await dbContext.Employees.FindAsync(key);
            employee.Notes = parameters["Notes"].ToString();

            await dbContext.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        #region Containment
        [EnableQuery]
        // Employees(1)/Orders
        public IEnumerable<Order> GetOrdersFromEmployee([FromODataUri] int key)
        {
            return dbContext.Employees.Where(e => e.EmployeeId == key).SelectMany(e => e.Orders);
        }

        [EnableQuery]
        [ODataRoute("Employees({employeeId})/Orders({orderId})")]
        // Employees(1)/Orders(10258)
        public Order GetSingleOrder(int employeeId, int orderId)
        {
            var orders = dbContext.Employees.Include(e => e.Orders).Single(a => a.EmployeeId == employeeId).Orders;
            return orders.Single(o => o.OrderId == orderId);
        }
        #endregion

        #region Employees-InverseReportsToNavigation Relationships

        // Access InverseReportsToNavigation from Employee
        [EnableQuery]
        // http://localhost:5000/Employees(1)/InverseReportsToNavigation
        public IEnumerable<Employee> GetInverseReportsToNavigationFromEmployee([FromODataUri] int key)
        {
            return dbContext.Employees.Where(e => e.EmployeeId == key).SelectMany(e => e.InverseReportsToNavigation);
        }


        // Create Employee - InverseReportsToNavigation Relationship
        [HttpPost]
        // POST http://localhost:5000/Employees(1)/InverseReportsToNavigation/$ref
        // Body : {"@odata.id":"http://localhost:5000/InverseReportsToNavigation(2)"}
        public async Task<IActionResult> CreateRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            var employee = await dbContext.Employees.SingleOrDefaultAsync(p => p.EmployeeId == key);
            if (employee == null)
            {
                return NotFound();
            }
            switch (navigationProperty)
            {
                case "InverseReportsToNavigation":
                    int relatedKey = ExtractRelatedKey(link);
                    var reportsToEmployee = await dbContext.Employees.SingleOrDefaultAsync(o => o.EmployeeId == relatedKey);
                    if (reportsToEmployee == null)
                    {
                        return NotFound();
                    }

                    employee.InverseReportsToNavigation.Add(reportsToEmployee);
                    break;

                default:
                    return StatusCode((int)HttpStatusCode.NotImplemented);
            }
            await dbContext.SaveChangesAsync();
            return NoContent();
        }


        // Delete Employee - InverseReportsToNavigation Relationship
        [HttpDelete]
        // DELETE http://localhost:5000/Employees(2)/InverseReportsToNavigation/$ref?$id=http://localhost:5000/Employees(1)
        public async Task<IActionResult> DeleteRef([FromODataUri] int key, [FromODataUri] string relatedKey, string navigationProperty)
        {
            var employee = dbContext.Employees.SingleOrDefault(p => p.EmployeeId == key);
            if (employee == null)
            {
                return NotFound();
            }

            switch (navigationProperty)
            {
                case "InverseReportsToNavigation":
                    int relatedEmployeeId = Convert.ToInt32(relatedKey);
                    var reportsToEmployee = await dbContext.Employees.SingleOrDefaultAsync(o => o.EmployeeId == relatedEmployeeId);
                    if (reportsToEmployee == null)
                    {
                        return NotFound();
                    }

                    employee.InverseReportsToNavigation.Remove(reportsToEmployee);
                    break;

                default:
                    return StatusCode((int)HttpStatusCode.NotImplemented);
            }
            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        private int ExtractRelatedKey(Uri link)
        {
            var pathHandler = Request.GetPathHandler();
            var serviceRoot = Request.GetUrlHelper().CreateODataLink(
                                    Request.ODataFeature().RouteName,
                                    pathHandler,
                                    new List<ODataPathSegment>());

            var relatedEntityLinkParsed = pathHandler.Parse(serviceRoot, link.LocalPath, Request.GetRequestContainer());
            var relatedKey = Convert.ToInt32(relatedEntityLinkParsed.Segments.OfType<KeySegment>().FirstOrDefault().Keys.FirstOrDefault().Value);
            return relatedKey;
        }

        #endregion

        #region Function

        [HttpGet]
        // Bound Function
        // http://localhost:5000/Employees/Default.SellToHomeTown()
        public IEnumerable<Employee> SellToHomeTown()
        {
            return dbContext.Employees.Where(e => e.Orders.Any(o => o.ShipCity == e.City));
        }

        #endregion
    }
}