using Auth.Application.EventHandlers;
using Auth.Application.UseCases.Login;
using Auth.Infrastructure.Extensions;
using EMIS.Authentication.Extensions;
using EMIS.Contracts.Constants;
using EMIS.Contracts.Events;
using EMIS.EventBus.Kafka.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Database
builder.Services.AddAuthDatabase(builder.Configuration);

// Add MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(LoginCommandHandler).Assembly));

// Add Authentication & Authorization
builder.Services.AddEmisAuthentication(builder.Configuration);
builder.Services.AddEmisAuthorization();

// Add EventBus (Kafka)
builder.Services.AddKafkaEventBus(builder.Configuration);

// Register Event Handlers
builder.Services.AddEventHandler<TeacherCreatedEvent, TeacherCreatedEventHandler>();

// Add Kafka Consumer with topic subscriptions
builder.Services.AddKafkaConsumer(consumer =>
{
    // Subscribe to Teacher domain events
    consumer.RegisterEventType<TeacherCreatedEvent>(TopicNames.Teacher.Created);

    // Subscribe to Student domain events (when implemented)
    // consumer.RegisterEventType<StudentCreatedEvent>(TopicNames.Student.Created);
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
