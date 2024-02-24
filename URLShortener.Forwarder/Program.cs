using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using URLShortener;
using URLShortner;
using URLShortner.Model.Data;
internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddDbContext<DataBaseContext>(x => x.UseCosmos(
            connectionString: builder.Configuration["CosmosDb:ConnectionString"],
            databaseName: builder.Configuration["CosmosDb:DataBaseName"]
        ));

        builder.Services.AddTransient<URLShortner.URLController>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.MapGet("/{path}", async (
            [FromRoute]string path,
            URLController shortUrlRepository
        ) =>
        {
            var shortUrl = await shortUrlRepository.GetURL(path);
            if (shortUrl == null || string.IsNullOrEmpty(shortUrl.Value?.FullURL))
                return Results.NotFound();

            return Results.Redirect(shortUrl.Value.FullURL);
        });

        app.Run();
    }
}