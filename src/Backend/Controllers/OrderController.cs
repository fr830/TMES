using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Backend;
using Entities;

namespace Launcher.Controllers
{
    [Route("api/[controller]")]
    public class OrderController : Controller
    {
        public OrderService Service;

        public OrderController()
        {
            this.Service = new OrderService();
        }

        private Int32 Id;
        // GET api/order/5
        [HttpGet("{id}")]
        public Order Get(int Id)
        {
            try
            {
                return Service.GetOrderById(Id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        [HttpGet("{id}/{prod}/build")]
        //:: Provide full data set for gantt-controll to build diagramm 'by division'
        public IList<GanttData> GetDetails(int Id, int prod)
        {
            var Result = new List<GanttData>();
            var Order = this.Get(Id);
            Result = Service.GetDetails(Order,prod) as List<GanttData>;

            return Result;
        }

        [HttpGet("{id}/{parent}/Exploder")]
        public IList<Exploder> GetExploder(int id, int parent)
        {
            var result = Service.GetExploderOrder(id, parent);
            return result;
        }

        [HttpGet("{idRecord}/WorkDetail")]
        public IList<WorkDetail> GetWorkDetails(int idRecord)
        {

            var result = Service.GetWorkDetails(idRecord);

            return result;
        }
        [HttpGet("StandartWorks")]
        public IList<StandartWorks> GetStandartWorks()
        {
            var result = Service.GetStandartWorks();
            return result;

        }

        [HttpGet("{naim}/{duration}/AddStandartWorks")]
        public void AddStandartWorks(string naim, string duration)
        {
            Service.AddStandartWorks(naim, duration);
        }

        [HttpGet("{id}/DeleteStandartWorks")]
        public void DeleteStandartWorks(int id)
        {
            Service.DeleteStandartWorks(id);
        }
    }
}