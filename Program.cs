using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Version = "v2",
        Title = "APIBASE",
        Description = "Base line for API",
        Contact = new OpenApiContact
        {
            Name = "Ruben Farias",
            Email = "Ruben_Farias@Jabil.com",
        },
        License = new OpenApiLicense
        {
            Name = "Use under LICX",
            Url = new Uri("https://example.com/license"),
        },
        TermsOfService = new Uri("https://example.com/terms"),

    }
    );

    var filePath = Path.Combine(AppContext.BaseDirectory, "APIBASE.xml");
    options.IncludeXmlComments(filePath, includeControllerXmlComments: true);
}
);
var app = builder.Build();
app.UseCors(options => options.AllowAnyOrigin());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "APIBASE v2");
        options.RoutePrefix = string.Empty;
        options.DisplayRequestDuration();
        
    });
}
if(app.Environment.IsProduction()){
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "APIBASE v2");
        options.RoutePrefix = string.Empty;
        options.DisplayRequestDuration();
    });
}
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
