using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AMNet.Test;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);
        
        builder.WebHost.UseUrls("http://+:6070");

        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(o =>
        {
            o.SingleLine = true;
            o.IncludeScopes = false;
        });

        builder.Logging.SetMinimumLevel(LogLevel.Information);
        builder.Logging.AddFilter("Microsoft.AspNetCore.Http.Result", LogLevel.None);
        builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.AspNetCore.Routing.EndpointMiddleware", LogLevel.Error);
        builder.Logging.AddFilter("Microsoft.AspNetCore.Cors.Infrastructure.CorsService", LogLevel.Warning);
        
        builder.Services.AddCors(cors =>
        {
            cors.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin();
                policy.AllowAnyHeader();
                policy.WithMethods("GET", "POST");
                policy.SetPreflightMaxAge(TimeSpan.FromHours(6));
            });
        });

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, SerializerContext.Default);
        });

        var app = builder.Build();
        var startedAt = Environment.TickCount64;

        app.UseCors();
        
        var apiRoot = app.MapGroup("/amnet");
        apiRoot.MapGet("/info", () => Results.Ok(new SystemState(1, null, "AMNet-Test", Environment.TickCount64 - startedAt, 1000)));
        apiRoot.MapPost("/signin", async ctx =>
        {
            try
            {
                var body = await JsonSerializer.DeserializeAsync<CardReadRequest>(ctx.Request.Body, SerializerContext.Default.CardReadRequest);

                if (body?.MatrixCode.Count(char.IsAsciiDigit) == 20)
                {
                    ctx.Response.StatusCode = 202;
                    app.Services.GetRequiredService<ILogger<Program>>().LogInformation("Card request received: {card}", body.MatrixCode);
                }
                else
                {
                    ctx.Response.StatusCode = 400;
                    app.Services.GetRequiredService<ILogger<Program>>().LogWarning("Invalid card request received: {card}", body?.MatrixCode);
                }
            }
            catch
            {
                ctx.Response.StatusCode = 400;
            }
        });

        app.Run();
    }
}

public record SystemState(
    [property: JsonPropertyName("apiVersion")] int ApiVersion,
    [property: JsonPropertyName("gameId")] string GameId,
    [property: JsonPropertyName("serverName")] string ServerName,
    [property: JsonPropertyName("sessionUptime")] long SessionUptime,
    [property: JsonPropertyName("timeSinceLastPoll")] long? TimeSinceLastPoll);

public record CardReadRequest(
    [property: JsonPropertyName("cardId")] string MatrixCode);

[JsonSerializable(typeof(SystemState))]
[JsonSerializable(typeof(CardReadRequest))]
[JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
internal partial class SerializerContext : JsonSerializerContext;