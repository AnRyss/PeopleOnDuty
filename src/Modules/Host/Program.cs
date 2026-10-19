using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.Tasks;
using PeopleOnDuty.src.Modules.Users;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Postgres") 
??  throw new InvalidOperationException("No such connection in Configuration");
builder.Services.AddTaskModule(builder.Configuration.GetConnectionString(connectionString)!);
builder.Services.AddOpenApi();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.Run();


