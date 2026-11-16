using AtlasPay.Notification.Application.Features.DeferredEvents.Commands;
using AtlasPay.Notification.Application.Features.Notifications.Commands;
using AtlasPay.Notification.Infrastructure;
using AtlasPay.Notification.Worker.Consumers;
using AtlasPay.Notification.Worker.Mappers;
using AtlasPay.Notification.Worker.Processors;
using AtlasPay.Notification.Worker.Publishers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSingleton<DeadLetterPublisher>();

builder.Services.AddScoped<ProcessFinancialEventHandler>();
builder.Services.AddScoped<DeferFinancialEventHandler>();
builder.Services.AddScoped<FinanceEventMapper>();
builder.Services.AddScoped<FinanceEventProcessor>();

builder.Services.AddHostedService<DeferredEventProcessor>();
builder.Services.AddHostedService<FinanceEventsConsumer>();

var host = builder.Build();
host.Run();
