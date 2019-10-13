namespace ParkingRota
{
    using System;
    using AutoMapper;
    using Business;
    using Business.Model;
    using Data;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.UI;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NodaTime;

    public class Startup
    {
        public Startup(IConfiguration configuration) => this.Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
                options.Secure = CookieSecurePolicy.SameAsRequest;
            });

            var connectionString =
                Environment.GetEnvironmentVariable("ParkingRotaConnectionString") ??
                this.Configuration.GetValue<string>("ParkingRotaConnectionString") ??
                this.Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

            services.AddAuthorization(options =>
            {
                options.AddPolicy(UserRole.SiteAdmin, policy => policy.RequireRole(UserRole.SiteAdmin));
                options.AddPolicy(UserRole.TeamLeader, policy => policy.RequireRole(UserRole.TeamLeader));
            });

            services.AddIdentity<ApplicationUser, IdentityRole>(c =>
                {
                    c.Password.RequireDigit = false;
                    c.Password.RequireLowercase = false;
                    c.Password.RequireNonAlphanumeric = false;
                    c.Password.RequireUppercase = false;
                    c.Password.RequiredLength = 10;
                    c.Password.RequiredUniqueChars = 5;

                    c.SignIn.RequireConfirmedEmail = true;

                    c.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddDefaultTokenProviders();

            services.AddSingleton<IClock>(SystemClock.Instance);

            services.AddScoped<IAllocationRepository, AllocationRepository>();
            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
            services.AddScoped<IBankHolidayRepository, BankHolidayRepository>();
            services.AddScoped<IDateCalculator, DateCalculator>();
            services.AddScoped<IEmailRepository, EmailRepository>();
            services.AddHttpClient<IPasswordBreachChecker, PasswordBreachChecker>();
            services.AddScoped<IRegistrationTokenRepository, RegistrationTokenRepository>();
            services.AddScoped<IRegistrationTokenValidator, RegistrationTokenValidator>();
            services.AddScoped<IRequestRepository, RequestRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<ISystemParameterListRepository, SystemParameterListRepository>();

            services.AddSingleton<IMapper>(MapperBuilder.Build());

            services.AddMvc()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizePage("/EditReservations", UserRole.TeamLeader);
                    options.Conventions.AuthorizePage("/OverrideRequests", UserRole.TeamLeader);

                    options.Conventions.AuthorizeFolder("/Users", UserRole.SiteAdmin);

                    options.Conventions.AuthorizeFolder("/");

                    options.Conventions.AllowAnonymousToPage("/Index");
                    options.Conventions.AllowAnonymousToPage("/Error");
                    options.Conventions.AllowAnonymousToPage("/Privacy");
                    options.Conventions.AllowAnonymousToPage("/RegisterSuccess");

                    options.Conventions.AddPageRoute("/OverrideRequests", "OverrideRequests/{id?}");
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddHttpContextAccessor();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            if (Helpers.IsElasticBeanstalk())
            {
                loggerFactory.AddAWSProvider(this.Configuration.GetAWSLoggingConfigSection());
            }

            app.UseHttpsRedirection();

            var provider = new FileExtensionContentTypeProvider
            {
                Mappings = {[".webmanifest"] = "application/manifest+json"}
            };

            app.UseStaticFiles(new StaticFileOptions {ContentTypeProvider = provider});

            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
