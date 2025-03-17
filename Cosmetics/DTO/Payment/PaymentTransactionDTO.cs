using Cosmetics.Enum;
using System.Text.Json.Serialization;

namespace Cosmetics.DTO.Payment
{
    public class PaymentTransactionDTO
    {
        public Guid PaymentTransactionId { get; set; }
        public Guid OrderId { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        public string RequestId { get; set; }
        public decimal Amount { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))] // Converts enum to string in API responses
        public PaymentStatus Status { get; set; }

        public DateTime TransactionDate { get; set; }
    }
}
