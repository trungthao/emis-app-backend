using EMIS.Contracts.Events;
using EMIS.EventBus.Kafka.Extensions;
using FluentValidation;
using Message.API.EventHandlers;
using Message.API.Hubs;
using Message.Application.Commands;
using Message.Application.EventHandlers;
using Message.Domain.Repositories;
using Message.Infrastructure.Configuration;
using Message.Infrastructure.Persistence;
using Message.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// MongoDB Configuration
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

// Add MongoDB Context
builder.Services.AddSingleton<MessageDbContext>();

// Add Repositories
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();

// Add MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateConversationCommand).Assembly);
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateConversationCommandValidator>();

// 🔥 Add Kafka EventBus
builder.Services.AddKafkaEventBus(builder.Configuration);

// 🔥 Register Event Handlers
builder.Services.AddEventHandler<SendMessageRequestedEvent, MessagePersistenceHandler>(); // Write-Behind Pattern
builder.Services.AddEventHandler<MessageSentEvent, MessageSentEventHandler>(); // SignalR Broadcast
builder.Services.AddEventHandler<StudentAssignedToClassEvent, StudentAssignedToClassEventHandler>();
builder.Services.AddEventHandler<TeacherAssignedToClassEvent, TeacherAssignedToClassEventHandler>();

// 🔥 Add Kafka Consumer as Hosted Service
builder.Services.AddKafkaConsumer(consumer =>
{
    // Register event type mappings (Topic name -> Event Type)
    consumer.RegisterEventType<SendMessageRequestedEvent>(nameof(SendMessageRequestedEvent)); // NEW: Write-Behind
    consumer.RegisterEventType<MessageSentEvent>(nameof(MessageSentEvent));
    consumer.RegisterEventType<StudentAssignedToClassEvent>(nameof(StudentAssignedToClassEvent));
    consumer.RegisterEventType<TeacherAssignedToClassEvent>(nameof(TeacherAssignedToClassEvent));
});

// Add Controllers
builder.Services.AddControllers();

// Add SignalR
builder.Services.AddSignalR();

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

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Message API",
        Version = "v1",
        Description = "API for messaging and real-time chat (Event-Driven Architecture)"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Message API v1"));
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Map SignalR Hub
app.MapHub<ChatHub>("/chathub");

app.Run();
