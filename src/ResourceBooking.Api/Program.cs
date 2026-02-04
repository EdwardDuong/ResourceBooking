using ResourceBooking.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Liveness/readiness probe for container orchestration and CI smoke checks.
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

// Exposed for WebApplicationFactory-based integration tests (added alongside
// the first API endpoints in the next milestone).
public partial class Program { }
