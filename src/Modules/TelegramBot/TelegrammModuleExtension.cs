using PeopleOnDuty.src.Modules.TelegramBot.Handlers;
using PeopleOnDuty.src.Modules.TelegramBot.Handlers.UpdateHandler;
using PeopleOnDuty.src.Modules.TelegramBot.Scenarios;
using Telegram.Bot;

namespace PeopleOnDuty.src.Modules.TelegramBot
{
    public static class TelegrammModuleExtension
    {
        public static IServiceCollection AddTelegrammModule(this IServiceCollection services, string telegramToken)
        {
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
             services.AddScoped<IScenario, CheckAuthorTasks>();


            services.AddScoped<ScenarioRouter>();
            services.AddScoped<UpdateHandler>();
            services.AddSingleton<IUserStateRepository, InMemoryUserStateRepository>();
            services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(telegramToken));
            services.AddHostedService<TelegramBotService>();



            return services;
        }
    }
}