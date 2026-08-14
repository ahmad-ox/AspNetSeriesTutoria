using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;
using Host.Services.OpenApi;
using Host.ContextData;
using Host.Controllers;
using Application.Services;
using Application.Services.Implementations;
using Application.Services.Interfaces;

namespace Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Serialize enums as strings in api responses
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });
            //    .AddNewtonsoftJson(options =>
            //{
            //    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            //    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            //}); ;
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            /*var connectionString = builder.Configuration.GetSection("DatabaseSettings:MssqlConnectionString").Value;
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));*/
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            builder.Services.AddScoped<IDocumentManagementService, DocumentManagementService>();
            builder.Services.AddOptions<DatabaseSettings>()
                       .BindConfiguration(nameof(DatabaseSettings));

            builder.Services.AddDbContext<ApplicationDbContext>((p, m) =>
            {
                var dbSettings = p.GetRequiredService<IOptions<DatabaseSettings>>().Value;
                bool isMySql = dbSettings.IsMySql;
                var connectionString = isMySql ? dbSettings.MySqlConnectionString : dbSettings.MssqlConnectionString;
                _ = isMySql ? m.UseMySQL(connectionString) : m.UseSqlServer(connectionString);
            });

            builder.Services.AddScoped<IServicesTemp, ServicesTemp>();
            builder.Services.AddAzureClients(clientBuilder =>
            {
                // Configure BlobServiceClient with your connection string
                clientBuilder.AddBlobServiceClient("DefaultEndpointsProtocol=https;AccountName=specta;AccountKey=/13e2Ybs7yykZLJ/9nZpDrBieWm60YtcKJOPicB3Cezz+vQIS6uxxQekkENkvTm4XbbHRbDxsfbpdYllnUictQ==12;EndpointSuffix=core.windows.net");
            });


            /*builder.Services
                      .AddOptions<CompanyInfoSettings>()
                      .BindConfiguration(nameof(CompanyInfoSettings));*/

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(option =>
            {
                option.SchemaFilter<EnumDescriptionFilter>();
            });
            builder.Services.AddScoped<ILoggersService, LoggersService>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.DefaultModelsExpandDepth(1);
                options.DocExpansion(DocExpansion.None);
                options.EnableTryItOutByDefault();
            });
            //}

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();

        }
    }
}
