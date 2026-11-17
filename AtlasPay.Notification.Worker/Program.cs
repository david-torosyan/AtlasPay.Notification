using Amazon.SimpleEmail;
using AtlasPay.Notification.Application.Abstractions.Delivery;
using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Application.Features.DeferredEvents.Commands;
using AtlasPay.Notification.Application.Features.Notifications.Commands;
using AtlasPay.Notification.Infrastructure;
using AtlasPay.Notification.Infrastructure.Delivery;
using AtlasPay.Notification.Infrastructure.Persistence.Repositories;
using AtlasPay.Notification.Worker.Consumers;
using AtlasPay.Notification.Worker.Delivery;
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

builder.Services.AddSingleton<IAmazonSimpleEmailService, AmazonSimpleEmailServiceClient>();
builder.Services.AddScoped<INotificationDeliveryRepository, NotificationDeliveryRepository>();
builder.Services.AddScoped<INotificationSender, EmailNotificationSender>();
builder.Services.AddScoped<IEmailProvider, SesEmailProvider>();

builder.Services.AddHostedService<DeferredEventProcessor>();
builder.Services.AddHostedService<FinanceEventsConsumer>();
builder.Services.AddScoped<NotificationDeliveryProcessor>();
builder.Services.AddHostedService<NotificationDeliveryWorker>();

var host = builder.Build();
host.Run();
