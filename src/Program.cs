using MailClientApp;
using MailClientApp.Services;
using MailClientApp.Services.EMailService;
using MailClientApp.Services.SignatureService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.CookiePolicy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(nameof(EmailOptions)));
builder.Services.AddTransient<IMailWorkerService, MailService>();
builder.Services.AddTransient<ISignature, SignatureMessage>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "mail.auth";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.Cookie.MaxAge = TimeSpan.FromDays(1);

        if (!builder.Environment.IsProduction())
        {
            options.Cookie.HttpOnly = false;
            options.Cookie.SameSite = SameSiteMode.None;
        }
        else
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.None;
        }
     
        options.LoginPath = new PathString("/");
    });


builder.Services.AddCors(options =>
{
       options.AddDefaultPolicy(policy =>
       {
           policy
               /*.WithOrigins("http://127.0.0.1:5173")*/
              
               .AllowAnyHeader()
               .AllowAnyMethod()
              .AllowCredentials() .SetIsOriginAllowed(x=>true); //need if using credential: 'include'

       });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseCookiePolicy(new CookiePolicyOptions()
{
    MinimumSameSitePolicy = SameSiteMode.None,
    HttpOnly = HttpOnlyPolicy.None
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();