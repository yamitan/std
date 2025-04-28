using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace std.Services
{
    public class TimerService : BackgroundService
    {
        private readonly ILogger<TimerService> _logger;
        //private readonly TimeSpan _period = TimeSpan.FromMinutes(10);
        private readonly TimeSpan _period = TimeSpan.FromSeconds(30);

        public TimerService(ILogger<TimerService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new PeriodicTimer(_period);

            // 立即执行一次
            await LogCurrentTimeAsync();

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await LogCurrentTimeAsync();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("定时服务已停止");
            }
        }

        private async Task LogCurrentTimeAsync()
        {
            var currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //_logger.LogInformation("当前时间: {Time}", currentTime);
            await Task.CompletedTask; // 这里可以是其他异步操作
        }
    }
}