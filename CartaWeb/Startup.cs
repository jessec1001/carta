using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;

using CartaCore.Serialization.Json;
using CartaWeb.Formatters;
using CartaWeb.Models.Binders;

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
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                    options.JsonSerializerOptions.Converters.Insert(0, new JsonDiscriminantConverter());
                    options.JsonSerializerOptions.Converters.Insert(1, new JsonObjectEnumerableConverter());
                    options.JsonSerializerOptions.Converters.Insert(2, new JsonObjectConverter());
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
                options => Configuration
                    .GetSection("Authentication:OpenIdConnect")
                    .Bind(options)
            );

            // In production, the React files will be served from this directory.
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

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
