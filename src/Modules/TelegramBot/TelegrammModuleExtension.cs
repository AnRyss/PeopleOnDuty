using System.Net;
using System.Threading.RateLimiting;
using PeopleOnDuty.src.Modules.TelegramBot.Handlers;
using PeopleOnDuty.src.Modules.TelegramBot.Handlers.UpdateHandler;
using PeopleOnDuty.src.Modules.TelegramBot.Scenarios;
using src.Modules.TelegramBot;
using Telegram.Bot;

namespace PeopleOnDuty.src.Modules.TelegramBot
{
    public static class TelegrammModuleExtension
    {
        public static IServiceCollection AddTelegrammModule(this IServiceCollection services, string telegramToken)
        {
            var rateLimiter = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 25,
                Window = TimeSpan.FromSeconds(1),
                SegmentsPerWindow = 2,
                QueueLimit = int.MaxValue,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });

            var httpClient = new HttpClient(new RateLimitHandler(rateLimiter)
            {
                InnerHandler = new HttpClientHandler()
            });
            // services.AddScoped<PendingActionQueue>(); 
            services.AddScoped<MainMenuRenderer>();
            services.AddScoped<MainMenuScenario>();
            services.AddScoped<CreateTaskScenario>();
            services.AddScoped<CheckTakenTasks>();
            services.AddScoped<TakeTaskScenario>();


            services.AddScoped<IScenario, MainMenuScenario>();
            services.AddScoped<IScenario, CreateTaskScenario>();
            services.AddScoped<IScenario, CheckTakenTasks>();
            services.AddScoped<IScenario, TakeTaskScenario>();
            services.AddScoped<IScenario, CheckAuthorTasksScenario>();
            services.AddScoped<IScenario, ArchiveTaskScenario>();
            services.AddScoped<IScenario, AdminScenario>();

            services.AddScoped<ScenarioRouter>();
            services.AddScoped<UpdateHandler>();
            services.AddSingleton<UserLockProvider>();
            services.AddSingleton<IUserStateRepository, InMemoryUserStateRepository>();
            services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(telegramToken, httpClient));
            services.AddHostedService<TelegramBotService>();



            return services;
        }
    }
}
public class RateLimitHandler(RateLimiter limiter) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
     HttpRequestMessage request, CancellationToken ct)
    {
        using var lease = await limiter.AcquireAsync(1, ct);

        if (lease.IsAcquired)
            return await base.SendAsync(request, ct);

        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        if (lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
            response.Headers.Add("Retry-After", ((int)retryAfter.TotalSeconds).ToString());
        return response;
    }
}