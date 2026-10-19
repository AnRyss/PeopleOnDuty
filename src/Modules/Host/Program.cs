using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.Tasks;
using PeopleOnDuty.src.Modules.Users;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres")
?? throw new InvalidOperationException("No such connection in Configuration");
builder.Services.AddTaskModule(connectionString);
builder.Services.AddUserModule(connectionString);
builder.Services.AddOpenApi();

var app = builder.Build();
//app.UseExceptionHandler();  TODO
//app.UseAuthentication();  TODO
//app.UseAuthorization() TODO
app.MapControllers();

app.MapOpenApi();
app.UseSwaggerUI(options =>
   {
       options.SwaggerEndpoint("/openapi/v1.json", "My API V1");
   });


app.Run();


