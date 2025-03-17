namespace Cosmetics.Enum
{
    public enum PaymentStatus
    {
        Pending = 0,      // Default status before payment completion
        Success = 1,      // Payment completed successfully
        Failed = 2,       // Payment failed
        Canceled = 3,     // User canceled the payment
        Processing = 4    // Payment is being processed
    }
}
