using Common;
using Microsoft.AspNetCore.Mvc;
using PaymentsAPI.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PaymentsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueueCardPaymentController : Controller
    {
        [HttpPost]
        public IActionResult MakePayment([FromBody] CardPayment payment)
        {
            try
            {
                RabbitMQClient client = new RabbitMQClient();
                client.SendPayment(payment);
                client.Close();
            }
            catch (Exception)
            {
                return BadRequest();
            }
            return Ok(payment);
        }
    }
}
