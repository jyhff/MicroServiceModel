using LCH.MicroService.Search;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Volo.Abp;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureDefaults();
builder.Host.UseAutofac();
builder.Services.AddApplication<SearchServiceHostModule>();

var app = builder.Build();

app.InitializeApplication();

app.Run();