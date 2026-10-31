using Microsoft.EntityFrameworkCore;
using PeopleOnDuty.src.Modules.Archive;
using PeopleOnDuty.src.Modules.Host;
using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.Tasks;
using PeopleOnDuty.src.Modules.TelegramBot;
using PeopleOnDuty.src.Modules.Users;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);
var telegramToken = builder.Configuration["Telegram:Token"]
    ?? throw new InvalidOperationException("Telegram token not found in configuration");
    Console.WriteLine($"TOKEN: {(string.IsNullOrEmpty(telegramToken) ? "НЕ НАЙДЕН" : "НАЙДЕН")}");
builder.Services.AddTelegrammModule(telegramToken);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddLocalContext();
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("Postgres")
?? throw new InvalidOperationException("No such connection in Configuration");
    // Для продакшена — PostgreSQL
    builder.Services.AddTaskModule(connectionString!);
    builder.Services.AddUserModule(connectionString!);
    builder.Services.AddArchiveModule(connectionString!);
}
builder.Services.AddOpenApi();
builder.Services.AddControllers();



var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var tasksDb = scope.ServiceProvider.GetRequiredService<TasksContext>();
    await tasksDb.Database.MigrateAsync();

    var usersDb = scope.ServiceProvider.GetRequiredService<UserContext>();
    await usersDb.Database.MigrateAsync();
    
    var archiveDb = scope.ServiceProvider.GetRequiredService<ArchiveTaskContext>();
    await archiveDb.Database.MigrateAsync();
}
//app.UseExceptionHandler();  TODO`
//app.UseAuthentication();  TODO
//app.UseAuthorization() TODO
app.MapControllers();

app.MapOpenApi();


app.Run();

