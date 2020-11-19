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
            _entityFrameworkConnectionString = configuration?.GetConnectionString("Authentication");
            _authority = configuration?.GetValue<string>("Authority");
            _audience = configuration?.GetValue<string>("Audience");
            _identityServerOptionsSection = configuration?.GetIdentityServerOptionsSection();
            _facebookAppId = configuration?.GetValue<string>("FacebookAppId");
            _facebookAppSecret = configuration?.GetValue<string>("FacebookAppSecret");
            _serviceBusOptionsSection = configuration?.GetServiceBusOptionsOptionsSection();
            _emailOptionsSection = configuration?.GetSendGridEmailOptionsSection();
            _corsOrigins = configuration?.GetSection("CorsOrigins").Get<string[]>();
            _environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            DefaultInboundClaimTypeMap.Clear();
            DefaultOutboundClaimTypeMap.Clear();
            services
                .AddApplicationInsightsTelemetry(
                    options: options =>
                    {
                    })
                .AddRazorPages(
                    configure: options =>
                    {
                    })
                .AddMvcOptions(
                    setupAction: options =>
                    {
                        options.Filters.Add(new ModelStatePageFilter());
                    })
                .SetCompatibilityVersion(Latest)
                .Services
                .AddCors(
                    setupAction: options =>
                    {
                    })
                .Configure<CookieTempDataProviderOptions>(
                    configureOptions: options =>
                    {
                        options.Cookie.IsEssential = true;
                    })
                .AddEntityFrameworkDataService<IdentityServerDbContext>(
                    configureOptions: options =>
                    {
                    },
                    optionsAction: options =>
                    {
                        options.UseSqlServer(
                            connectionString: _entityFrameworkConnectionString,
                            sqlServerOptionsAction: sqlServerOptions =>
                            {
                                sqlServerOptions.MigrationsAssembly("Authentication.Api");
                            });
                    })
                .AddIdentityServer(
                    config: _identityServerOptionsSection,
                    isDevelopment: _environment.IsDevelopment(),
                    identityOptionsAction: options =>
                    {
                        options.SignIn.RequireConfirmedAccount = true;
                    },
                    identityServerOptionsAction: options =>
                    {
                    },
                    configurationStoreOptionsAction: options =>
                    {
                    },
                    operationalStoreOptionsAction: options =>
                    {
                        options.EnableTokenCleanup = true;
                    },
                    authenticationOptionsAction: options =>
                    {
                    },
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
                    })
                .AddDatabaseDeveloperPageExceptionFilter()
                .AddHealthChecks()
                .AddDbContextCheck<IdentityServerDbContext>()
                .Services
                .AddQueueClient(_serviceBusOptionsSection)
                .AddSendGridEmailService(_emailOptionsSection)
                .AddKendo();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app
                .UseStaticFiles()
                .UseCookiePolicy()
                .UseExceptionHandler(
                    configure: app1 =>
                    {
                        app1.Run(async context => await context.HandleException().ConfigureAwait(false));
                    })
                .UseHttpsRedirection()
                .UseRouting()
                .UseCors(
                    configurePolicy: policy =>
                    {
                        policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(_corsOrigins);
                    })
                .UseHealthChecks("/health")
                .UseAuthentication()
                .UseAuthorization()
                .UseIdentityServer()
                .UseEndpoints(
                    configure: endpoints =>
                    {
                        endpoints.MapRazorPages();
                    });
        }
    }
}
