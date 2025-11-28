// using Microsoft.AspNetCore.Mvc;
// using ShefaafAPI.Services;

// namespace ShefaafAPI.Controllers.v1
// {
//     [Route("v1/[controller]")]
//     [ApiController]
//     public class PaymentController : ControllerBase
//     {
//         private readonly IRazorpayService _razorpayService;

//         public PaymentController(IRazorpayService razorpayService)
//         {
//             _razorpayService = razorpayService;
//         }

//         [HttpPost("CreateOrder")]
//         public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
//         {
//             try
//             {
//                 var razorpayOrderId = await _razorpayService.CreateOrderAsync(request.Amount);

//                 return Ok(new
//                 {
//                     success = true,
//                     razorpayOrderId,
//                     amount = request.Amount,
//                     currency = "INR",
//                     keyId = _razorpayService.GetKeyId()
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new
//                 {
//                     success = false,
//                     message = ex.Message
//                 });
//             }
//         }

//         [HttpPost("VerifyPayment")]
//         public IActionResult VerifyPayment([FromBody] VerifyPaymentRequest request)
//         {
//             try
//             {
//                 bool isValid = _razorpayService.VerifyPayment(
//                     request.RazorpayOrderId,
//                     request.RazorpayPaymentId,
//                     request.RazorpaySignature
//                 );

//                 if (!isValid)
//                 {
//                     return BadRequest(new { success = false, message = "Payment verification failed" });
//                 }

//                 return Ok(new
//                 {
//                     success = true,
//                     message = "Payment verified successfully",
//                     paymentId = request.RazorpayPaymentId
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new { success = false, message = ex.Message });
//             }
//         }
//     }

//     public class CreateOrderRequest
//     {
//         public decimal Amount { get; set; }
//     }

//     public class VerifyPaymentRequest
//     {
//         public string RazorpayOrderId { get; set; } = string.Empty;
//         public string RazorpayPaymentId { get; set; } = string.Empty;
//         public string RazorpaySignature { get; set; } = string.Empty;
//     }
// }
