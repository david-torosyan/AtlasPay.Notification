using Amazon.SimpleEmail;
using AtlasPay.Notification.Application.Abstractions.Delivery;
using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Application.Features.CustomerUserMappings.Commands.RegisterCustomerUser;
using AtlasPay.Notification.Application.Features.DeferredEvents.Commands;
using AtlasPay.Notification.Application.Features.Notifications.Commands;
using AtlasPay.Notification.Infrastructure;
using AtlasPay.Notification.Infrastructure.Delivery;
using AtlasPay.Notification.Infrastructure.Persistence.Repositories;
using AtlasPay.Notification.Worker.Configuration;
using AtlasPay.Notification.Worker.Consumers;
using AtlasPay.Notification.Worker.Delivery;
using AtlasPay.Notification.Worker.Kafka;
using AtlasPay.Notification.Worker.Mappers;
using AtlasPay.Notification.Worker.Processors;
using AtlasPay.Notification.Worker.Publishers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddOptions<KafkaSettings>()
    .Bind(builder.Configuration.GetSection(KafkaSettings.SectionName))
    .Validate(Validations.ValidateKafkaSettings)
    .ValidateOnStart();

builder.Services.AddSingleton<KafkaConsumerFactory>();
builder.Services.AddSingleton<DeadLetterPublisher>();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IEmailProvider, SmtpEmailProvider>();
}
else
{
    builder.Services.AddSingleton<IAmazonSimpleEmailService, AmazonSimpleEmailServiceClient>();
    builder.Services.AddScoped<IEmailProvider, SesEmailProvider>();
}

builder.Services.AddScoped<ProcessFinancialEventHandler>();
builder.Services.AddScoped<DeferFinancialEventHandler>();
builder.Services.AddScoped<FinanceEventMapper>();
builder.Services.AddScoped<FinanceEventProcessor>();

builder.Services.AddScoped<INotificationDeliveryRepository, NotificationDeliveryRepository>();
builder.Services.AddScoped<INotificationSender, EmailNotificationSender>();
builder.Services.AddScoped<ICustomerUserMappingRepository,CustomerUserMappingRepository>();
builder.Services.AddScoped<RegisterCustomerUserHandler>();
builder.Services.AddScoped<NotificationDeliveryProcessor>();

builder.Services.AddHostedService<IdentityEventsConsumer>();
builder.Services.AddHostedService<DeferredEventProcessor>();
builder.Services.AddHostedService<FinanceEventsConsumer>();
builder.Services.AddHostedService<NotificationDeliveryWorker>();

var host = builder.Build();
host.Run();
