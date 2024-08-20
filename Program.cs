using Camping_retake.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Camping_retake
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add LiteDbContext service to the container
            builder.Services.AddSingleton<LiteDbContext>(new LiteDbContext("Filename=CampingDB_Retake.db;Connection=shared;"));

            // Add session services
            builder.Services.AddDistributedMemoryCache(); // Adds a default in-memory implementation of IDistributedCache
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
                options.Cookie.HttpOnly = true; // Make the session cookie accessible only by the server
                options.Cookie.IsEssential = true; // Ensure the session cookie is always sent, even if non-essential cookies are rejected
            });

            // Add controllers to the container
            builder.Services.AddControllers();

            // Configure CORS to allow requests from the frontend
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    policyBuilder =>
                    {
                        policyBuilder.WithOrigins("http://localhost:8080") // Allow requests from your frontend
                                     .AllowAnyHeader()
                                     .AllowAnyMethod();
                    });
            });

            // Add Swagger for API documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Enable CORS
            app.UseCors("AllowSpecificOrigin");

            app.UseHttpsRedirection();

            // Add session middleware - should be placed before authentication/authorization
            app.UseSession();

            // Add authentication middleware (only if you're using authentication)
            app.UseAuthentication();

            // Add authorization middleware
            app.UseAuthorization();

            // Map controllers to endpoints
            app.MapControllers();

            app.Run();
        }
    }
}
