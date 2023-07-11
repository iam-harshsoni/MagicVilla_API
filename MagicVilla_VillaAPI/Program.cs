using Serilog;

var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.File("log/villalog.txt",rollingInterval:RollingInterval.Day).CreateLogger();

//this will notify the application that it has to use serilog instead of the built-in one
builder.Host.UseSerilog();

/*
 * 1. Add services to the container.
 * 2. Added newtonsoftjson support to the service
 * 3. "options => options.ReturnHttpNotAcceptable = true " this code will throw exception if the application is   other than Json
 * 4. to support XML formatting we have to add this code 'AddXmlDataContractSerializerFormatters()' */

builder.Services.AddControllers(options => options.ReturnHttpNotAcceptable = true).AddNewtonsoftJson().AddXmlDataContractSerializerFormatters();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
