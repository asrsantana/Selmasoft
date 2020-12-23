using Common;
using Microsoft.AspNetCore.Mvc;
using PaymentsAPI.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueuePurchaseOrderController : Controller
    {
        [HttpPost]
        public IActionResult MakePayment([FromBody] PurchaseOrder order)
        {
            try
            {
                RabbitMQClient client = new RabbitMQClient();
                client.SendPurchaseOrder(order);
                client.Close();
            }
            catch (Exception)
            {
                return BadRequest();
            }
            return Ok(order);
        }
    }
}
