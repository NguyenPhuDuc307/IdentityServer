using vnLab.BackendServer.Data;
using vnLab.BackendServer.Data.Entities;
using vnLab.BackendServer.Helpers;
using vnLab.BackendServer.IdentityServer;
using vnLab.BackendServer.Services;
using vnLab.ViewModel.Email;
using vnLab.ViewModel.Systems.Validators;
using vnLab.ViewModel.reCAPTCHA;

using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Microsoft.AspNetCore.Authentication;
using vnLab.BackendServer.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

var vnLabSpecificOrigins = "vnLabSpecificOrigins";
var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();

builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));
//1. Setup entity framework
services.AddDbContextPool<ApplicationDbContext>(options =>
    options.UseSqlServer(
        configuration.GetConnectionString("DefaultConnection")));
//2. Setup identity
services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.AddServerHeader = false;
});

services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;
})
.AddInMemoryApiResources(Config.Apis)
.AddInMemoryApiScopes(Config.ApiScopes)
.AddInMemoryClients(configuration.GetSection("IdentityServer:Clients"))
.AddInMemoryIdentityResources(Config.Ids)
.AddAspNetIdentity<User>()
.AddProfileService<IdentityProfileService>()
.AddDeveloperSigningCredential();

services.AddCors(options =>
{
    options.AddPolicy(vnLabSpecificOrigins,
    builder =>
    {
        if (allowedOrigins != null)
        {
            builder.WithOrigins(allowedOrigins)
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .AllowCredentials()
                               .SetIsOriginAllowed(origin => true);
        }
    });
});

services.Configure<IdentityOptions>(options =>
{
    // Default Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.SignIn.RequireConfirmedAccount = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.User.RequireUniqueEmail = true;
});
Log.Logger = new LoggerConfiguration()
.Enrich.FromLogContext()
.WriteTo.Console()
.CreateLogger();

services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// Add services to the container.
services.AddHttpClient();
services.AddControllersWithViews();
services.AddSignalR();

// Add validator to the service collection
services.AddFluentValidationAutoValidation();
services.AddFluentValidationClientsideAdapters();
services.AddValidatorsFromAssemblyContaining<RoleCreateRequestValidator>();

services.AddAuthentication()
.AddLocalApi("Bearer", option =>
{
    option.ExpectedScope = "api.vnlab";
});

services.AddAuthorization(options =>
{
    options.AddPolicy("Bearer", policy =>
    {
        policy.AddAuthenticationSchemes("Bearer");
        policy.RequireAuthenticatedUser();
    });
});

services.AddRazorPages(options =>
{
    options.Conventions.AddAreaFolderRouteModelConvention("Identity", "/Account/", model =>
    {
        foreach (var selector in model.Selectors)
        {
            var attributeRouteModel = selector.AttributeRouteModel;
            if (attributeRouteModel != null)
            {
                attributeRouteModel.Order = -1;
                if (attributeRouteModel.Template != null)
                    attributeRouteModel.Template = attributeRouteModel.Template.Remove(0, "Identity".Length);
            }
        }
    });
});

services.AddAuthentication()
.AddGoogle(googleOptions =>
{
    var googleClientId = configuration.GetSection("Authentication:Google:ClientId").Value;
    var googleClientSecret = configuration.GetSection("Authentication:Google:ClientSecret").Value;

    if (googleClientId != null && googleClientSecret != null)
    {
        googleOptions.ClientId = googleClientId;
        googleOptions.ClientSecret = googleClientSecret;
        googleOptions.ClaimActions.MapJsonKey("image", "picture");
        googleOptions.Scope.Add("email");
        googleOptions.SaveTokens = true;

        googleOptions.Events.OnRedirectToAuthorizationEndpoint = context =>
        {
            context.Response.Redirect(context.RedirectUri + "&prompt=select_account");
            return Task.CompletedTask;
        };
    }
})
.AddFacebook(facebookOptions =>
{
    var facebookAppId = configuration.GetSection("Authentication:Facebook:AppId").Value;
    var facebookAppSecret = configuration.GetSection("Authentication:Facebook:AppSecret").Value;

    if (facebookAppId != null && facebookAppSecret != null)
    {
        facebookOptions.AppId = facebookAppId;
        facebookOptions.AppSecret = facebookAppSecret;
    }
})
// .AddMicrosoftAccount(microsoftOptions =>
// {
//     var microsoftClientId = configuration.GetSection("Authentication:Microsoft:ClientId").Value;
//     var microsoftClientSecret = configuration.GetSection("Authentication:Microsoft:ClientSecret").Value;

//     if (microsoftClientId != null && microsoftClientSecret != null)
//     {
//         microsoftOptions.ClientId = microsoftClientId;
//         microsoftOptions.ClientSecret = microsoftClientSecret;
//     }
// })
;

services.AddTransient<DbInitializer>();
services.AddTransient<IEmailSender, EmailSenderService>();
services.AddTransient<IStorageService, FileStorageService>();
services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
services.Configure<reCAPTCHASettings>(configuration.GetSection("reCAPTCHA"));
services.AddTransient<IViewRenderService, ViewRenderService>();
services.AddTransient<ICacheService, DistributedCacheService>();
services.AddTransient<IFileValidator, FileValidator>();
services.AddTransient<ICloudStorageService, CloudStorageService>();

services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "vnLab API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://localhost:5000/connect/authorize"),
                Scopes = new Dictionary<string, string> { { "api.vnlab", "vnLab API" } }
            },
        },
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new List<string>{ "api.vnlab" }
        }
    });
});

services.AddDistributedSqlServerCache(o =>
{
    o.ConnectionString = configuration.GetConnectionString("DefaultConnection");
    o.SchemaName = "dbo";
    o.TableName = "CacheTable";
});

services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts(hsts => hsts.MaxAge(365).IncludeSubdomains().Preload());
    app.UseXContentTypeOptions();
    app.UseReferrerPolicy(opts => opts.NoReferrer());
    app.UseXXssProtection(options => options.EnabledWithBlockMode());
    app.UseXfo(options => options.Deny());
}

app.UseErrorWrapping();

//app.UseCsp(opts => opts
//        .BlockAllMixedContent()
//        .StyleSources(s => s.Self())
//        .StyleSources(s => s.UnsafeInline())
//        .FontSources(s => s.Self())
//        .FormActions(s => s.Self())
//        .FrameAncestors(s => s.Self())
//        .ImageSources(s => s.Self())
//        .ScriptSources(s => s.Self())
//    );

app.UseStaticFiles();

app.UseIdentityServer();

app.UseAuthentication();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseCors(vnLabSpecificOrigins);

app.MapDefaultControllerRoute();

app.MapRazorPages();

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.OAuthClientId("swagger");
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "vnLab API");
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    var serviceProvider = scope.ServiceProvider;
    try
    {
        Log.Information("Seeding data...");
        var dbInitializer = serviceProvider.GetService<DbInitializer>();
        if (dbInitializer != null)
            dbInitializer.Seed()
                         .Wait();
    }
    catch (Exception ex)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
