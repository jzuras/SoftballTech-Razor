﻿using Microsoft.EntityFrameworkCore;
using Sbt.Data;

namespace Sbt
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            //builder.Services.AddDbContext<DemoContext>(options =>
                //options.UseSqlServer(builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING") ?? throw new InvalidOperationException("Connection string 'DemoContext' not found.")));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            var app = builder.Build();

            // As of Dec 2023 I am disabling most of this site, except the home page (it is now OBE).
            // This middleware allows the home page to still work and all others to be not found,
            // since i don't want to delete all the pages themselves at this point.
            app.UseMiddleware<DisablePagesMiddleware>();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            else
            {
                app.UseMigrationsEndPoint();
            }

            //using (var scope = app.Services.CreateScope())
            //{
            //    var services = scope.ServiceProvider;

            //    var context = services.GetRequiredService<DemoContext>();
            //    context.Database.EnsureCreated();
            //}

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}