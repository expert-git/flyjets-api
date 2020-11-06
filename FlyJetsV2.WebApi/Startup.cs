using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlyJetsV2.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FlyJetsV2.Services.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Net;
using System.Net.Mail;
using Stripe;
using Microsoft.EntityFrameworkCore;

namespace FlyJetsV2.WebApi
{
    public class Startup
    {
        [Obsolete]
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            StripeConfiguration.SetApiKey(Configuration.GetSection("Stripe")["SecretKey"]);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie("Cookies", options =>
            {
                options.Cookie.Name = "FJAuthCookie";
                options.Cookie.SameSite = SameSiteMode.None;
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = redirectContext =>
                    {
                        redirectContext.HttpContext.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddMvc(options =>
            {
                options.RespectBrowserAcceptHeader = true;
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
            .AddJsonOptions(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddHttpContextAccessor();

            services.AddCors();

            services.AddSignalR();
            services.AddSingleton<StorageManager>();
            services.AddTransient<NotificationService>();
            services.AddTransient<PaymentService>();
            services.AddTransient<Services.AccountService>(); services.AddTransient<LocationService>();
            services.AddTransient<FeeTypeService>();
            services.AddTransient<TaxTypeService>();
            services.AddTransient<BookingService>();
            services.AddTransient<AircraftService>();
            services.AddTransient<FlightService>();
            services.AddTransient<EmptyLegService>();
            services.AddSingleton<NotificationHub>();
            services.AddTransient<MailerService>();
            services.AddTransient<SearchHistoryService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var cookiePolicyOptions = new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.None,
            };

            app.UseCookiePolicy(cookiePolicyOptions);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseCors(configurePolicy => {
                    configurePolicy.AllowAnyHeader();
                    configurePolicy.AllowAnyMethod();
                    //configurePolicy.WithOrigins("http://localhost:5000", "http://localhost:5001", "https://flyjetswebstg.azurewebsites.net");
                    configurePolicy.AllowAnyOrigin();
                    configurePolicy.AllowCredentials();
                });
            }
            else
            {
                app.UseHsts();
                app.UseHttpsRedirection();

                app.UseCors(configurePolicy => {
                    configurePolicy.AllowAnyHeader();
                    configurePolicy.AllowAnyMethod();
                    configurePolicy.WithOrigins("https://flyjets.com", "https://flyjetswebstg.azurewebsites.net");                    
                    configurePolicy.AllowCredentials();
                });
            }

            app.UseSignalR(routes => {
              routes.MapHub<NotificationHub>("/api/notificationhub");
            });

            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
