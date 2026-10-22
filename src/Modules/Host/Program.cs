using Microsoft.EntityFrameworkCore;
using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.Tasks;
using PeopleOnDuty.src.Modules.TelegramBot;
using PeopleOnDuty.src.Modules.Users;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

var telegramToken = builder.Configuration["Telegram:Token"]
    ?? throw new InvalidOperationException("Telegram token not found in configuration");
    Console.WriteLine($"TOKEN: {(string.IsNullOrEmpty(telegramToken) ? "НЕ НАЙДЕН" : "НАЙДЕН")}");
builder.Services.AddSingleton<IUserStateRepository, InMemoryUserStateRepository>();
builder.Services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(telegramToken));
builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddHostedService<TelegramBotService>();

if (builder.Environment.IsDevelopment())
{
    // Для локальной разработки — файл SQLite
    builder.Services.AddDbContext<TasksContext>(opt =>
        opt.UseSqlite("Data Source=peopleonduty.db"));
    builder.Services.AddDbContext<UserContext>(opt =>
        opt.UseSqlite("Data Source=peopleonduty.db"));
    builder.Services.AddScoped<ITaskRepository, TaskRepository>();
    builder.Services.AddScoped<ITaskService, TaskService>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IUserService, UserService>();
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("Postgres")
?? throw new InvalidOperationException("No such connection in Configuration");
    // Для продакшена — PostgreSQL
    builder.Services.AddTaskModule(connectionString!);
    builder.Services.AddUserModule(connectionString!);
}
builder.Services.AddOpenApi();
builder.Services.AddControllers();



var app = builder.Build();
//app.UseExceptionHandler();  TODO`
//app.UseAuthentication();  TODO
//app.UseAuthorization() TODO
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var tasksDb = scope.ServiceProvider.GetRequiredService<TasksContext>();
    await tasksDb.Database.MigrateAsync();

    var usersDb = scope.ServiceProvider.GetRequiredService<UserContext>();
    await usersDb.Database.MigrateAsync();
}
app.MapOpenApi();


app.Run();

