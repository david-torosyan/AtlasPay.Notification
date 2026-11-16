using AtlasPay.Notification.Application.Features.Notifications.Commands;
using AtlasPay.Notification.Infrastructure;
using AtlasPay.Notification.Worker;
using AtlasPay.Notification.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<ProcessFinancialEventHandler>();

builder.Services.AddHostedService<FinanceEventsConsumer>();

var host = builder.Build();
host.Run();
