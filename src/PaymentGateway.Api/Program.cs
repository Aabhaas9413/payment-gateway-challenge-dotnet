using FluentValidation;
using PaymentGateway.Api.Middleware;
using PaymentGateway.Application.Behaviors;
using PaymentGateway.Domain.Interfaces;
using PaymentGateway.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(PaymentGateway.Application.AssemblyReference).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(
    typeof(PaymentGateway.Application.AssemblyReference).Assembly);


builder.Services.AddSingleton<IPaymentRepository, PaymentsRepository>();

builder.Services.AddHttpClient<IBankClient, PaymentGateway.Infrastructure.Clients.BankClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("BankSimulator:BaseUrl") ?? "http://localhost:8080");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
