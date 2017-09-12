using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Demo.Core.DataContext;
using Demo.Web.Hubs;
using Demo.Web.Middlewares;
using Demo.Web.Services;
using Demo.Web.WebSocket;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Demo.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private readonly string SECRET_KEY = "mysupersecret_secretkey!123";

        public static SecurityKey SecurityKey;

        public static TokenProviderOptions TokenProviderOptions;

        public static int TOKEN_EXPIRES = 24; // 24h;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region Database And Migration
           
            services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("ChatDb"));

            #endregion

            #region Identity

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<DataContext>()
                .AddDefaultTokenProviders();

            #endregion

            #region JWT

            SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SECRET_KEY));

            TokenProviderOptions = new TokenProviderOptions
            {
                Expiration = TimeSpan.FromHours(TOKEN_EXPIRES),
                SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256),
            };

            services.AddAuthentication()
                .AddJwtBearer(o =>
                {
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = SecurityKey,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    };
                });


            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build();
            });

            #endregion

            // Enable Cors
            services.AddCors();


            services.AddOptions();

            services.AddMvc();

            services.AddWebSocketManager();


            #region Extra configurations

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

            });

            #endregion


            services.AddScoped<IDbInitializer, DbInitializer>();
            services.AddTransient<IIdentityService, IdentityService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IDbInitializer dbInitializer)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            #region Logging

            if (env.IsDevelopment())
            {
                loggerFactory.AddConsole(Configuration.GetSection("Logging"));
                loggerFactory.AddDebug();
            }

            #endregion

            #region Cors

            app.UseCors(builder =>
                builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            #endregion

            #region Seed Data For Development

            dbInitializer.Initialize();

            #endregion

            #region JWT Authentication

            app.UseAuthentication();

            app.UseMiddleware<TokenProviderMiddleware>(Options.Create(TokenProviderOptions));

            #endregion

            app.UseMvc();

            app.UseWebSockets();

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                app.MapWebSocketManager("/chat", serviceScope.ServiceProvider.GetService<ChatHub>());
            }

            app.UseDefaultFiles();

            app.UseStaticFiles();

            app.UseMvc();

        }
    }
}
