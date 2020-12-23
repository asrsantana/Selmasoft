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
    public class DirectCardPaymentController : Controller
    {
        [HttpPost]
        public IActionResult MakePayment([FromBody] CardPayment payment)
        {        
            try
            {
                var client = new RpcClient();
                client.CreateConnection();
                client.MakePayment(payment);

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
