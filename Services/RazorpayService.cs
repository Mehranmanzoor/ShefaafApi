// using System;
// using System.Net.Http;
// using System.Net.Http.Headers;
// using System.Security.Cryptography;
// using System.Text;
// using System.Text.Json;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Configuration;

// namespace ShefaafAPI.Services
// {
//     public interface IRazorpayService
//     {
//         Task<string> CreateOrderAsync(decimal amount, string currency = "INR");
//         bool VerifyPayment(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature);
//         string GetKeyId();
//     }

//     public class RazorpayService : IRazorpayService
//     {
//         private readonly string _keyId;
//         private readonly string _keySecret;
//         private readonly HttpClient _httpClient;

//         public RazorpayService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
//         {
//             _keyId = configuration["Razorpay:KeyId"] ?? throw new ArgumentNullException("Razorpay KeyId missing in config");
//             _keySecret = configuration["Razorpay:KeySecret"] ?? throw new ArgumentNullException("Razorpay KeySecret missing in config");
//             _httpClient = httpClientFactory.CreateClient();

//             var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_keyId}:{_keySecret}"));
//             _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
//         }

//         public string GetKeyId() => _keyId;

//         public async Task<string> CreateOrderAsync(decimal amount, string currency = "INR")
//         {
//             try
//             {
//                 var orderData = new
//                 {
//                     amount = (int)(amount * 100), // amount in paise
//                     currency,
//                     receipt = Guid.NewGuid().ToString(),
//                     payment_capture = 1
//                 };

//                 var content = new StringContent(JsonSerializer.Serialize(orderData), Encoding.UTF8, "application/json");
//                 var response = await _httpClient.PostAsync("https://api.razorpay.com/v1/orders", content);

//                 var responseJson = await response.Content.ReadAsStringAsync();

//                 if (!response.IsSuccessStatusCode)
//                 {
//                     throw new Exception($"Failed to create Razorpay order: {responseJson}");
//                 }

//                 var orderResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);

//                 return orderResponse.GetProperty("id").GetString() 
//                     ?? throw new Exception("Order ID missing in response");
//             }
//             catch (HttpRequestException httpEx)
//             {
//                 throw new Exception($"HTTP error during Razorpay order creation: {httpEx.Message}");
//             }
//             catch (JsonException jsonEx)
//             {
//                 throw new Exception($"Failed to parse Razorpay response: {jsonEx.Message}");
//             }
//             catch (Exception ex)
//             {
//                 throw new Exception($"Unexpected error in Razorpay order creation: {ex.Message}");
//             }
//         }

//         public bool VerifyPayment(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature)
//         {
//             try
//             {
//                 string payload = razorpayOrderId + "|" + razorpayPaymentId;
//                 using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_keySecret));
//                 var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
//                 var generatedSignature = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

//                 return generatedSignature == razorpaySignature.ToLowerInvariant();
//             }
//             catch
//             {
//                 return false;
//             }
//         }
//     }
// }
