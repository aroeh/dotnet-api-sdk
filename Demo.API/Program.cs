using Demo.API.Core.Extensions;
using Demo.API.Infrastructure.MongoDb.Extensions;
using Dotnet.API.SDK.Extensions;
using Dotnet.API.MongoDb.SDK.Extensions;
using Microsoft.AspNetCore.Http.Json;
using System.Reflection;
using System.Text.Json.Serialization;
using Demo.API.Infrastructure.MongoDb.Documents;

namespace Demo.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");

        /*
         * Reading configuration can be done 3 ways through the api sdk
         * 1. Read app settings and environment variables - builder.Configuration.AddConfigSettings<Program>()
         * 2. Read settings including user secrets with an assembly class - builder.Configuration.AddConfigSettings<Program>()
         * 3. read settings including user secrets with an assembly - builder.Configuration.AddConfigSettings(Assembly.GetExecutingAssembly())
         */
        builder.Configuration.AddConfigSettings(Assembly.GetExecutingAssembly());

        // get additional app settings from Azure App Configuration
        builder.Configuration.AddAzureAppConfigSettings([builder.Environment.EnvironmentName]);

        // Add services to the container.

        // Add API versioning to the controllers.  By default it is configured for query string versioning
        // and requires the query string param api-version on calls to the API.
        builder.Services.AddVersioning();

        builder.Services.AddControllers()
            .AddJsonOptions(options => // handle enum json serialization for incoming requests
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        // handle enum json serialization for responses
        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // register default exception handler from the SDK
        builder.Services.AddExceptionHandlerAndProblemDetails();

        // register a custom exception handler from the API
        //builder.Services.AddExceptionHandlerAndProblemDetails<CustomExceptionHandler>();

        // register dependencies for core and infrastructure repos
        builder.Services.AddCoreOrchestrations();
        builder.Services.AddInfrastructureRepos();

        // add SDK dependencies
        builder.Services.ConfigureMongoDbOptions(builder.Configuration, "RestuarantApi:Settings:MongoDb");
        builder.Services.AddScopedMongoDbRepo<RestuarantDocument>("restuarants");

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        // use the exception handler
        app.UseExceptionHandler();

        app.UseHttpsRedirection();


        // map the health check endpoint and setup a custom writer

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
