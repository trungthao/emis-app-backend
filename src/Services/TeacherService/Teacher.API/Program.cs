using Microsoft.EntityFrameworkCore;
using MediatR;
using Teacher.Application.UseCases.Teachers.Commands.CreateTeacher;
using Teacher.Application.EventHandlers;
using Teacher.Infrastructure.Persistence;
using Teacher.Infrastructure.Repositories;
using Teacher.Domain.Repositories;
using EMIS.Authentication.Extensions;
using EMIS.EventBus.Kafka.Extensions;
using EMIS.Contracts.Events;
using EMIS.Contracts.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
var conn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=localhost;Port=3307;Database=EMIS_TeacherDB;User=emisuser;Password=emispassword;";
builder.Services.AddDbContext<TeacherDbContext>(options =>
    options.UseMySql(conn, new MySqlServerVersion(new Version(8,0,0))));

// Repositories
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// MediatR
builder.Services.AddMediatR(typeof(CreateTeacherCommand).Assembly);

// Add Authentication & Authorization
builder.Services.AddEmisAuthentication(builder.Configuration);
builder.Services.AddEmisAuthorization();

// Add Kafka EventBus (Producer)
builder.Services.AddKafkaEventBus(builder.Configuration);

// Register Event Handlers for Class sync (Consumer)
builder.Services.AddEventHandler<ClassCreatedEvent, ClassCreatedEventHandler>();
builder.Services.AddEventHandler<ClassUpdatedEvent, ClassUpdatedEventHandler>();

// Add Kafka Consumer to subscribe to Class events (for local replica sync)
builder.Services.AddKafkaConsumer(consumer =>
{
    consumer.RegisterEventType<ClassCreatedEvent>(TopicNames.Class.Created);
    consumer.RegisterEventType<ClassUpdatedEvent>(TopicNames.Class.Updated);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
