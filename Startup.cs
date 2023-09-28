
using Microsoft.OpenApi.Models;

namespace EnotPayment
{
    public class Startup { 
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCors();
            services.AddMemoryCache();
            services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.IgnoreNullValues = true);
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddSwaggerGen();

            // configure strongly typed settings object
            services.Configure<EnotPayment.AppSettings>(Configuration.GetSection("AppSettings"));           
          
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        
            services.AddScoped<PaymentService>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PaymentService", Version = "v1" });
            });
        }

        // configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // migrate database changes on startup (includes initial db creation)
           // context.Database.Migrate();

            // generated swagger json and swagger ui middleware
            app.UseSwagger();

            app.UseRouting();

            // global cors policy
            app.UseCors(x => x
                .SetIsOriginAllowed(origin => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
          
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerUI(c =>
                {                 
                    app.UseSwagger();
                });
                             
            }
            app.UseEndpoints(x => x.MapControllers());
      
        }

    }
}
