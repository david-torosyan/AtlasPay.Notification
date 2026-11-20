using AtlasPay.Notification.Application.Features.AccountNotificationPreferences.Commands;
using AtlasPay.Notification.Application.Features.AccountNotificationPreferences.Queries;
using AtlasPay.Notification.Application.Features.NotificationChannels.Queries;
using AtlasPay.Notification.Application.Features.NotificationPreferences.Commands;
using AtlasPay.Notification.Infrastructure;
using AtlasPay.Security.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddInfrastructure(
    builder.Configuration);

builder.Services.AddScoped<SetNotificationPreferenceHandler>();
builder.Services.AddScoped<SetAccountNotificationPreferenceHandler>();
builder.Services.AddScoped<GetNotificationChannelsHandler>();
builder.Services.AddScoped<GetNotificationPreferencesHandler>();
builder.Services.AddScoped<GetAccountNotificationPreferencesHandler>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAtlasPayJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("NotificationPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("customer_id");
    });
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
