using System;
using System.Text.Json;
using Amazon.CognitoIdentityProvider;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Carta.Api.Services;
using CartaCore.Persistence;
using CartaWeb.Formatters;
using CartaWeb.Models.Binders;
using CartaWeb.Models.Migration;
using CartaWeb.Models.Options;
using CartaWeb.Services;
using CartaCore.Serialization.Json;

namespace CartaWeb
{
    /// <summary>
    /// Represents the setup class for the web application and web API.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration that is injected into this startup.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Gets the web configuration.
        /// </summary>
        /// <value>The web configuration.</value>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Adds services to the container.
        /// </summary>
        /// <remarks>
        /// Called by the runtime automatically.
        /// </remarks>
        /// <param name="services">The service collection used to add new services to.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // AWS settings
            AwsAccessOptions awsOptions = Configuration
                .GetSection("Deployment:AWS")
                .Get<AwsAccessOptions>();
            AwsDynamoDbOptions awsDynamoDbOptions = Configuration
                .GetSection("Database:DynamoDb:Table")
                .Get<AwsDynamoDbOptions>();
            AwsCognitoOptions awsCognitoOptions = Configuration.
                GetSection("Authentication:Cognito").
                Get<AwsCognitoOptions>();
            AwsCdkOptions awsCdkOptions = Configuration.
                GetSection("CartaAwsDeployStack").
                Get<AwsCdkOptions>();
            if (awsCdkOptions is not null)
            {
                awsOptions.AccessKey = awsCdkOptions.AccessKey;
                awsOptions.SecretKey = awsCdkOptions.SecretKey;
                awsOptions.RegionEndpoint = awsCdkOptions.RegionEndpoint;
                awsDynamoDbOptions.TableName = awsCdkOptions.DynamoDBTable;
                awsCognitoOptions.UserPoolId = awsCdkOptions.UserPoolId;
            }
            services.AddSingleton<OperationJobCollection>(new OperationJobCollection());
            services.AddHostedService<BackgroundOperationService>();
            services.AddSingleton<INoSqlDbContext>(
                new DynamoDbContext
                (
                    awsOptions.AccessKey,
                    awsOptions.SecretKey,
                    Amazon.RegionEndpoint.GetBySystemName(awsOptions.RegionEndpoint),
                    awsDynamoDbOptions.TableName
                )
            );
            if (awsOptions.AccessKey is null)
                services.
                    AddSingleton<IAmazonCognitoIdentityProvider>(
                        new AmazonCognitoIdentityProviderClient
                        (
                            Amazon.RegionEndpoint.GetBySystemName(awsOptions.RegionEndpoint)
                        ));
            else
                services.
                    AddSingleton<IAmazonCognitoIdentityProvider>(
                        new AmazonCognitoIdentityProviderClient
                        (
                            awsOptions.AccessKey,
                            awsOptions.SecretKey,
                            Amazon.RegionEndpoint.GetBySystemName(awsOptions.RegionEndpoint)
                        ));
            if (awsDynamoDbOptions.MigrationSteps is not null)
            {
                services.AddSingleton<INoSqlDbMigrationBuilder>((container) =>
                {
                    ILogger logger =
                        container.GetRequiredService<ILogger<DynamoDbMigrationBuilder>>();
                    return new DynamoDbMigrationBuilder
                        (
                            awsDynamoDbOptions.MigrationSteps,
                            awsOptions.AccessKey,
                            awsOptions.SecretKey,
                            Amazon.RegionEndpoint.GetBySystemName(awsOptions.RegionEndpoint),
                            awsDynamoDbOptions.TableName,
                            logger
                        );
                });
            }
            services.Configure<AwsCognitoOptions>(Configuration.GetSection("Authentication:Cognito"));

            // Formatting settings.
            services
                .AddControllersWithViews(options =>
                {
                    options.RespectBrowserAcceptHeader = true;
                    options.ReturnHttpNotAcceptable = true;

                    // Add our custom model binder provider.
                    options.ModelBinderProviders.Insert(0, new CustomModelBinderProvider());

                    // Our custom formatting middleware needs to come before other formatters.
                    options.InputFormatters.Insert(0, new GraphInputFormatter());
                    options.OutputFormatters.Insert(0, new GraphOutputFormatter());
                }).AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                    options.JsonSerializerOptions.Converters.Add(new JsonObjectConverter());
                });

            // Important: this solves a deployment only issue.
            // Configures X-Forwarded-* headers to be used when determining scheme and remote IP.
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                // These are the headers we need.
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto;

                // Only loopback proxies are allowed by default.
                // Clear that restriction because forwarders are enabled by explicit 
                // configuration.
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // Authentication settings.
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie
            (
                options => Configuration
                    .GetSection("Authentication:Cookies")
                    .Bind(options)
            )
            .AddOpenIdConnect
            (
                options =>
                {
                    Configuration
                        .GetSection("Authentication:OpenIdConnect")
                        .Bind(options);
                    if (awsCdkOptions is not null)
                    {
                        options.ClientId = awsCdkOptions.UserPoolClientId;
                        string serverUrl = $"https://cognito-idp.{awsOptions.RegionEndpoint}.amazonaws.com/" +
                            $"{awsCognitoOptions.UserPoolId}";
                        options.Authority = serverUrl;
                        options.MetadataAddress = $"{serverUrl}/.well-known/openid-configuration";
                    }
                }
            );

            // In production, the React files will be served from this directory.
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            // Important: this solves a deployment-only issue.
            // Kestrel HTTPS redirection workaround.
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                // These are the headers we need to forward.
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto;

                // Only loopback proxies are allowed by default.
                // Clear that restriction because forwarders are enabled by explicit configuration.
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
        }

        /// <summary>
        /// Configures the HTTP request pipeline.
        /// </summary>
        /// <remarks>
        /// Called by the runtime automatically.
        /// </remarks>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The web host environment.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            // Add AWS cloud watch logging for production
            if (env.IsProduction())
                loggerFactory.AddAWSProvider(Configuration.GetAWSLoggingConfigSection());

            // Perform database migrations
            // INoSqlDbMigrationBuilder noSqlDbMigrationBuilder =
            //     app.ApplicationServices.GetService<INoSqlDbMigrationBuilder>();
            // if (noSqlDbMigrationBuilder is not null) noSqlDbMigrationBuilder.PerformMigrations();

            // Important: this solves a deployment-only issue.
            // Forwards headers from load balancers and proxy servers that terminate SSL.
            app.UseForwardedHeaders();

            // Development settings.
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

            // HTTPS and SPA settings.
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = (options) =>
                {
                    // Get the headers so we can modify the cache-control header.
                    ResponseHeaders headers = options.Context.Response.GetTypedHeaders();

                    // Static resources are cached for 1 year.
                    if (options.Context.Request.Path.StartsWithSegments("/static"))
                    {
                        headers.CacheControl = new CacheControlHeaderValue
                        {
                            Public = true,
                            MaxAge = TimeSpan.FromDays(365)
                        };
                    }
                    // Non-static resources are not ever cached.
                    else
                    {
                        headers.CacheControl = new CacheControlHeaderValue
                        {
                            Public = true,
                            MaxAge = TimeSpan.FromDays(0)
                        };
                    }
                }
            });

            // Routing settings and Auth and AuthZ.
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
                spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions()
                {
                    OnPrepareResponse = (options) =>
                    {
                        // Get the headers so we can modify the cache-control header.
                        ResponseHeaders headers = options.Context.Response.GetTypedHeaders();
                        headers.CacheControl = new CacheControlHeaderValue
                        {
                            Public = true,
                            MaxAge = TimeSpan.FromDays(0)
                        };
                    }
                };

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
