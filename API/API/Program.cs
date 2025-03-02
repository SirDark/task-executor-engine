using API.Data;
using API.Options;
using API.Repositories;
using API.Services.HostedService;
using API.Services.RabbitMQ;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection("RabbitMQ"));

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DbConnection"));
});

builder.Services.AddHostedService<UpdateTaskService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

int migration_retry_counter = 0;//in some cases the postgres container did not set itself up when the migrations are applied that is why this is here
int max_try = int.Parse(builder.Configuration.GetValue<string>("MIGRATION_MAX_RETRY")!);
while (migration_retry_counter < max_try)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var dbContext = services.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();  // This applies any pending migrations
            break;
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while applying migrations. retrying...");
            migration_retry_counter++;
            Thread.Sleep(10);
        }
    }
}

app.UseCors("AllowAll");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
