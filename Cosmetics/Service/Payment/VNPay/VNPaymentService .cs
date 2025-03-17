using Cosmetics.DTO.Payment;
using Cosmetics.Enum;
using Cosmetics.Models;
using Cosmetics.Repositories.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Cosmetics.Utilities; // Namespace của VnPayLibrary

namespace Cosmetics.Service.Payment
{
    public class VNPayService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _vnpayTmnCode;
        private readonly string _vnpayHashSecret;
        private readonly string _vnpayUrl;
        private readonly string _vnpayReturnUrl;

        public VNPayService(IUnitOfWork unitOfWork, string vnpayTmnCode, string vnpayHashSecret, string vnpayUrl, string vnpayReturnUrl)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _vnpayTmnCode = vnpayTmnCode ?? throw new ArgumentNullException(nameof(vnpayTmnCode));
            _vnpayHashSecret = vnpayHashSecret ?? throw new ArgumentNullException(nameof(vnpayHashSecret));
            _vnpayUrl = vnpayUrl ?? throw new ArgumentNullException(nameof(vnpayUrl));
            _vnpayReturnUrl = vnpayReturnUrl ?? throw new ArgumentNullException(nameof(vnpayReturnUrl));
        }

        public async Task<string> CreatePaymentUrlAsync(PaymentRequestDTO request)
        {
            Debug.WriteLine("Entering CreatePaymentUrlAsync");
            Debug.WriteLine($"Current UTC Time: {DateTime.UtcNow.ToString("yyyyMMddHHmmss")}");
            Debug.WriteLine($"System Local Time: {DateTime.Now.ToString("yyyyMMddHHmmss")}");

            var paymentTransaction = new PaymentTransaction
            {
                PaymentTransactionId = Guid.NewGuid(),
                OrderId = request.OrderId,
                PaymentMethod = request.PaymentMethod,
                Amount = request.Amount,
                Status = PaymentStatus.Pending,
                TransactionDate = DateTime.UtcNow,
                TransactionId = DateTime.Now.Ticks.ToString()
            };

            try
            {
                Debug.WriteLine($"Adding PaymentTransaction: PaymentTransactionId={paymentTransaction.PaymentTransactionId}, OrderId={paymentTransaction.OrderId}, TransactionId={paymentTransaction.TransactionId}");
                await _unitOfWork.PaymentTransactions.AddAsync(paymentTransaction);
                await _unitOfWork.CompleteAsync();
                Debug.WriteLine("PaymentTransaction saved successfully");
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine($"DbUpdateException: {ex.Message}");
                Debug.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                throw new Exception("Failed to save payment transaction", ex);
            }

            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // Hardcode vì không dùng IConfiguration
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var pay = new VnPayLibrary();

            pay.AddRequestData("vnp_Version", "2.1.0"); // Hardcode giá trị mặc định
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", _vnpayTmnCode);
            pay.AddRequestData("vnp_Amount", ((int)(request.Amount * 100)).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_IpAddr", GetIpAddress()); // Thay vì dùng HttpContext
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang {paymentTransaction.TransactionId}");
            pay.AddRequestData("vnp_OrderType", "billpayment");
            pay.AddRequestData("vnp_ReturnUrl", _vnpayReturnUrl);
            pay.AddRequestData("vnp_TxnRef", paymentTransaction.TransactionId);
            pay.AddRequestData("vnp_ExpireDate", timeNow.AddMinutes(30).ToString("yyyyMMddHHmmss"));

            var paymentUrl = pay.CreateRequestUrl(_vnpayUrl, _vnpayHashSecret);
            Debug.WriteLine($"Payment URL: {paymentUrl}");

            return paymentUrl;
        }

        private static string GetIpAddress()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        return ip.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetIpAddress Error: {ex.Message}");
            }
            return "127.0.0.1";
        }

        public async Task<bool> HandlePaymentResponseAsync(PaymentResponseDTO response)
        {
            var transaction = await _unitOfWork.PaymentTransactions.GetByTransactionIdAsync(response.TransactionId);
            if (transaction == null)
                return false;

            transaction.Status = response.ResultCode switch
            {
                0 => PaymentStatus.Pending,
                1 => PaymentStatus.Success,
                2 => PaymentStatus.Failed,
                3 => PaymentStatus.Canceled,
                4 => PaymentStatus.Processing,
                _ => PaymentStatus.Failed
            };
            transaction.ResultCode = response.ResultCode;
            transaction.ResponseTime = response.ResponseTime.ToString("yyyy-MM-dd HH:mm:ss");
            transaction.Amount = response.Amount;

            await _unitOfWork.PaymentTransactions.UpdateAsync(transaction);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<PaymentTransactionDTO> GetPaymentByTransactionIdAsync(string transactionId)
        {
            var transaction = await _unitOfWork.PaymentTransactions.GetByTransactionIdAsync(transactionId);
            if (transaction == null)
                return null;

            return new PaymentTransactionDTO
            {
                PaymentTransactionId = transaction.PaymentTransactionId,
                OrderId = transaction.OrderId,
                PaymentMethod = transaction.PaymentMethod,
                TransactionId = transaction.TransactionId,
                RequestId = transaction.RequestId,
                Amount = transaction.Amount,
                Status = transaction.Status,
                TransactionDate = transaction.TransactionDate
            };
        }
    }
}