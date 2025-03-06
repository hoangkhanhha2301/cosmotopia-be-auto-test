using Cosmetics.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cosmetics.Service.OTP
{
    public class ExpiredOtpCleanerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ExpiredOtpCleanerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanExpiredOtpsAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi xóa OTP hết hạn: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Chạy mỗi 1 phút
            }
        }

        private async Task CleanExpiredOtpsAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<ComedicShopDBContext>();
                if (context == null)
                {
                    Console.WriteLine("Không thể lấy `ComedicShopDBContext` từ DI container.");
                    return;
                }

                var expiredUsers = await context.Users
                    .Where(u => u.OtpExpiration <= DateTime.UtcNow && u.Verify != 4)
                    .ToListAsync();

                if (expiredUsers.Any())
                {
                    Console.WriteLine($"🔍 Đang xóa {expiredUsers.Count} người dùng có OTP hết hạn...");
                    context.Users.RemoveRange(expiredUsers);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✅ Đã xóa {expiredUsers.Count} người dùng.");
                }
            }
        }
    }
}
