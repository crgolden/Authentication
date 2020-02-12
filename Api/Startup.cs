namespace Authentication
{
    using Core;
    using Core.Filters;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using static System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler;
    using static Microsoft.AspNetCore.Mvc.CompatibilityVersion;

    public class Startup
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _entityFrameworkConnectionString;
        private readonly string _authority;
        private readonly string _audience;
        private readonly IConfigurationSection _identityServerOptionsSection;
        private readonly string _facebookAppId;
        private readonly string _facebookAppSecret;
        private readonly IConfigurationSection _serviceBusOptionsSection;
        private readonly IConfigurationSection _emailOptionsSection;
        private readonly string[] _corsOrigins;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _environment = environment;
            _entityFrameworkConnectionString = configuration.GetConnectionString("Authentication");
            _authority = configuration.GetValue<string>("Authority");
            _audience = configuration.GetValue<string>("Audience");
            _identityServerOptionsSection = configuration.GetIdentityServerOptionsSection();
            _facebookAppId = configuration.GetValue<string>("FacebookAppId");
            _facebookAppSecret = configuration.GetValue<string>("FacebookAppSecret");
            _serviceBusOptionsSection = configuration.GetServiceBusOptionsOptionsSection();
            _emailOptionsSection = configuration.GetSendGridEmailOptionsSection();
            _corsOrigins = configuration.GetSection("CorsOrigins").Get<string[]>();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            DefaultInboundClaimTypeMap.Clear();
            DefaultOutboundClaimTypeMap.Clear();
            services.AddApplicationInsightsTelemetry(options =>
            {
            });
            services.AddRazorPages(options =>
            {
            }).AddMvcOptions(options =>
            {
                options.Filters.Add(new ModelStatePageFilter());
            }).SetCompatibilityVersion(Latest);
            services.AddCors(options =>
            {
            });
            services.Configure<CookieTempDataProviderOptions>(options => options.Cookie.IsEssential = true);
            services.AddEntityFrameworkDataService<IdentityServerDbContext>(
                options =>
                {
                },
                options =>
                {
                    options.UseSqlServer(_entityFrameworkConnectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.MigrationsAssembly("Authentication.Api");
                    });
                });
            services.AddIdentityServer(
                _identityServerOptionsSection,
                _environment.IsDevelopment(),
                options => options.SignIn.RequireConfirmedEmail = true,
                operationalStoreOptionsAction: options => options.EnableTokenCleanup = true,
                identityServerAuthenticationOptionsAction: options =>
                {
                    options.Authority = _authority;
                    options.ApiName = _audience;
                },
                facebookOptionsAction: options =>
                {
                    options.AppId = _facebookAppId;
                    options.AppSecret = _facebookAppSecret;
                    options.Events.OnRemoteFailure = context => context.HandleRemoteFailure();
                });
            services.AddHealthChecks().AddDbContextCheck<IdentityServerDbContext>();
            services.AddQueueClient(_serviceBusOptionsSection);
            services.AddSendGridEmailService(_emailOptionsSection);
            services.AddKendo();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseExceptionHandler(app1 => app1.Run(async context => await context.HandleException().ConfigureAwait(false)));
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(_corsOrigins));
            app.UseHealthChecks("/health");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseIdentityServer();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
