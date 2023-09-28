using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace EnotPayment
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _enotPaymentService;
        public PaymentController(PaymentService paymentService) { 
            _enotPaymentService = paymentService;
        }

        //used for webhook handling
        [HttpPost("enot-verify")]

        public async Task<IActionResult> EnotPaymentReceived([FromBody] string enotModel)
        {
            try
            {
                var signature = StringValues.Empty;
                Request.Headers.TryGetValue("x-api-sha256-signature", out signature);
                Console.WriteLine("signature: " + signature.ToString());

                string message = _enotPaymentService.ProcessPayment(enotModel, signature);
                return Ok(message);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error: " + ex.Message);
            }
        }

        //get link for payment

        [HttpGet("getEnotLink")]
        public IActionResult GetEnotLink([FromQuery] string email, [FromQuery] string amount, [FromQuery] string server)
        {
            try
            {
                string link = _enotPaymentService.GetEnotPaymentLink(amount, email);
                return Ok(new { link = link });
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error: " + ex.Message);
            }
        }
    }
}
