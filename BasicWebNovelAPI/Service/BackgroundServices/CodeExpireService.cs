
using BasicWebNovelAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Writers;

namespace BasicWebNovelAPI.Service.BackgroundServices
{
    public class CodeExpireService :BackgroundService 
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public CodeExpireService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<BasicWebNovelContext>();

                    var expiredCodesUsers = await dbContext.Users.Where(u => u.CodeExpirationTime < DateTime.Now && u.TemporaryCode != null).ToListAsync();

                    foreach (var user in expiredCodesUsers)
                    {
                        user.CodeExpirationTime = null;
                        user.TemporaryCode = null;

                    }

                    await dbContext.SaveChangesAsync();
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
