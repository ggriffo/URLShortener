using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Cosmos;
using URLShortner.Model.Data;
using Microsoft.EntityFrameworkCore.Cosmos;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    public IConfigurationRoot Configuration { get; set; }
    // The container we will create.
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();
        builder.Services.AddDbContext<DataBaseContext>(x => x.UseCosmos(
            connectionString: builder.Configuration.GetSection("CosmosDB").GetValue<string>("ConnectionString"),
            databaseName: builder.Configuration.GetSection("CosmosDB").GetValue<string>("DatabaseName")
        ));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.MapControllers();

        app.Run();
    }
}