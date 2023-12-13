using BilbolStack.RedisPubSub.Repository;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
builder.Services.AddSingleton<IRedisCacheAdapter, RedisCacheAdapter>();
builder.Services.AddSingleton<IMemoryCacheAdapter, MemoryCacheAdapter>();
builder.Services.AddSingleton<ILayeredCacheAdapter, LayeredCacheAdapter>();
builder.Services.AddOptions<RedisSettings>().BindConfiguration(RedisSettings.ConfigKey);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
