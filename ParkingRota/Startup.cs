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
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Middleware;
    using NodaTime;

    public class Startup
    {
        public Startup(IConfiguration configuration) => this.Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
                options.Secure = CookieSecurePolicy.SameAsRequest;
            });

            var connectionString =
                Environment.GetEnvironmentVariable("ParkingRotaConnectionString") ??
                this.Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

            services.AddAuthorization(options =>
            {
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
                .AddDefaultUI()
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

            var mapperConfiguration = new MapperConfiguration(c =>
            {
                c.CreateMap<Data.BankHoliday, Business.Model.BankHoliday>();
                c.CreateMap<Data.RegistrationToken, Business.Model.RegistrationToken>();
                c.CreateMap<Data.Request, Business.Model.Request>();
                c.CreateMap<Data.Reservation, Business.Model.Reservation>();
                c.CreateMap<Data.SystemParameterList, Business.Model.SystemParameterList>();
            });

            services.AddSingleton<IMapper>(new Mapper(mapperConfiguration));

            services.AddMvc()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizePage("/EditReservations", UserRole.TeamLeader);
                    options.Conventions.AuthorizePage("/OverrideRequests", UserRole.TeamLeader);

                    options.Conventions.AuthorizeFolder("/");

                    options.Conventions.AllowAnonymousToPage("/Index");
                    options.Conventions.AllowAnonymousToPage("/Error");
                    options.Conventions.AllowAnonymousToPage("/Privacy");
                    options.Conventions.AllowAnonymousToPage("/RegisterSuccess");

                    options.Conventions.AddPageRoute("/OverrideRequests", "OverrideRequests/{id?}");
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddHttpContextAccessor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseResponseHeadersMiddleware(
                    new ResponseHeadersBuilder()
                        .AddResponseHeaders());

                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            var isElasticBeanstalkEnvironmentVariable = Environment.GetEnvironmentVariable("IsElasticBeanstalk");

            if (bool.TryParse(isElasticBeanstalkEnvironmentVariable, out var isElasticBeanstalk) && isElasticBeanstalk)
            {
                loggerFactory.AddAWSProvider(this.Configuration.GetAWSLoggingConfigSection());
            }
            else
            {
                loggerFactory.AddConsole(LogLevel.Debug);
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
