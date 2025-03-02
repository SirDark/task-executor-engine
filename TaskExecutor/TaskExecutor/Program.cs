using TaskExecutor;
using TaskExecutor.Options;
using TaskExecutor.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<IRabbitMessager, RabbitMessager>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();
