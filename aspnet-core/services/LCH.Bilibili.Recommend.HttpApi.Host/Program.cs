using LCH.Bilibili.Recommend;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .UseAutofac()
    .UseSerilog((context, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("Logs/logs.txt", rollingInterval: RollingInterval.Day);
    });

builder.Services.AddApplication<RecommendHttpApiHostModule>();

var app = builder.Build();

app.InitializeApplication();

app.Run();