using Hangfire;
using HashidsNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SmartLibrary.Web.Consts;
using SmartLibrary.Web.Core.Models;
using SmartLibrary.Web.Helpers;
using SmartLibrary.Web.Mapping;
using SmartLibrary.Web.Services;
using SmartLibrary.Web.Settings;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;
using WhatsAppCloudApi.Extensions;

namespace SmartLibrary.Web.Extensions
{
    public static class DependancyInjection
    {
        public static IServiceCollection AddSmartLibraryServices(this IServiceCollection services, WebApplicationBuilder builder)
        {

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();


            builder.Services.Configure<IdentityOptions>(options =>
            {
                // Default Password settings.
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            });
            builder.Services.AddDataProtection().SetApplicationName(nameof(SmartLibrary));
            builder.Services.AddSingleton<IHashids>(_ => new Hashids("f1nd1ngn3m0", minHashLength: 11));


            builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();

            builder.Services.AddTransient<IImageService, ImageService>();
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();

            builder.Services.Configure<SecurityStampValidatorOptions>(opt => opt.ValidationInterval = TimeSpan.Zero);

            builder.Services.AddControllersWithViews();

            builder.Services.AddAutoMapper(op => op.AddProfile<MappingProfile>());

            builder.Services.AddExpressiveAnnotations();
            builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection(nameof(CloudinarySettings)));
            builder.Services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));
            builder.Services.AddWhatsAppApiClient(builder.Configuration);

            //Hangfire
            builder.Services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
            builder.Services.AddHangfireServer();
            builder.Services.Configure<AuthorizationOptions>(options => options.AddPolicy("AdminsOnly", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(AppRoles.Admin);
            }));

            builder.Services.AddMvc(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

            return services;
        }
    }
}
