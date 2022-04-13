using MongoRepository.Sample.Repositories;
using MongoRepository.Sample.Services;

namespace MongoRepository.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            

            #region Basic setup

            builder.Services.Configure<SampleOptions>(builder.Configuration.GetSection("SampleOptions"));

            // Enable debug mode if you need:
            var options = builder.Configuration.GetSection("SampleOptions").Get<SampleOptions>();
            var debugSetting = new MongoTelemetrySettings() { EnableDebugMode = options.EnableDebugMode };

            builder.Services.AddMongoClientFactory(debugSetting);
            builder.Services.AddScoped<ISampleReadWriteRepository, SampleRepository>();

            #endregion

            #region Multi-Tenant

            builder.Services.AddHttpContextAccessor(); // this httpContextAccessor just example how to map your auth.
            builder.Services.AddScoped<IMultiTenantIdentificationService, MultiTenantIdentificationService>();
            builder.Services.AddScoped<IMultiTenantRepository, MultiTenantRepository>();

            #endregion

            #region Index Sample

            builder.Services.AddScoped<IIndexSampleRepository, IndexSampleRepository>();

            #endregion

            #region Audit Sample


            builder.Services.AddScoped<IAuditSampleRepository, AuditSampleRepository>();
            builder.Services.AddScoped<IAuditService, AuditService>();

            #endregion

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
