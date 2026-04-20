using CassMach.API.Extensions;
using CassMach.Application;
using CassMach.Infrastructure;
using CassMach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Application services (MediatR, AutoMapper, FluentValidation)
builder.Services.AddApplication();

// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApiServices(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CassMach.Infrastructure.Persistence.ApplicationDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add CORS middleware (must be before authentication)
app.UseCors("DefaultCorsPolicy");
 
// Authentication and Authorization (order is important)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed data only if database is empty
await SeedData.SeedAsyncIfEmpty(app.Services);

// Her başlatmada eksik permission'ları ekle; rollere yeni izinleri bağla (ör. Errors.*)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CassMach.Infrastructure.Persistence.ApplicationDbContext>();
    await CassMach.Infrastructure.Persistence.SeedData.SeedPermissionsAsync(context);
    await CassMach.Infrastructure.Persistence.SeedData.SyncRolePermissionsAsync(context);
}

app.Run();
