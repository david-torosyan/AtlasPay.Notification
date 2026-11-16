using AtlasPay.Notification.Application.Features.Notifications.Commands;
using AtlasPay.Notification.Infrastructure;
using AtlasPay.Notification.Worker.Consumers;
using AtlasPay.Notification.Worker.Mappers;
using AtlasPay.Notification.Worker.Publishers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<ProcessFinancialEventHandler>();
builder.Services.AddScoped<FinanceEventMapper>();

builder.Services.AddHostedService<FinanceEventsConsumer>();
builder.Services.AddSingleton<DeadLetterPublisher>();

var host = builder.Build();
host.Run();
