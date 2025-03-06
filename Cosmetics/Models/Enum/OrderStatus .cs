namespace Cosmetics.Models.Enum
{
    public enum OrderStatus
        {
            Pending = 1,       // Chờ xác nhận
            Confirmed = 2,     // Xác nhận
            Delivering = 3,    // Đang giao
            Received = 4,      // Đã nhận
            Canceled = 5       // Hủy
        }
    }


