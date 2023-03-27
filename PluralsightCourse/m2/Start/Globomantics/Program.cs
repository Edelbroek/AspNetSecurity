using Globomantics;
using Globomantics.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(o =>
    o.Filters.Add(new AuthorizeFilter()));
builder.Services.AddRazorPages();
builder.Services.AddAuthentication(a =>
{
    a.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    //a.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
    .AddCookie(c =>
    {
        c.Cookie.SameSite = SameSiteMode.Strict;
        // Register OnValidatePrincipal to for instance sign-out a user for whom it is no longer valid to be signed in.
        //c.Events = new CookieAuthenticationEvents
        //{
        //    OnValidatePrincipal = context =>
        //    {
        //        context.RejectPrincipal();
        //        context.HttpContext.SignOutAsync();
        //        return Task.CompletedTask;
        //    }
        //};
    })
    .AddCookie(ExternalAuthenticationDefaults.AuthenticationScheme)
    .AddGoogle(o =>
    {
        o.SignInScheme = ExternalAuthenticationDefaults.AuthenticationScheme;
        o.ClientId = builder.Configuration["GoogleCredentials:ClientId"]
            ?? throw new InvalidOperationException("ClientId from Google is required");
        o.ClientSecret = builder.Configuration["GoogleCredentials:ClientSecret"]
            ?? throw new InvalidOperationException("ClientSecret from Google is required");
    });

builder.Services.AddSingleton<IConferenceRepository, ConferenceRepository>();
builder.Services.AddSingleton<IProposalRepository, ProposalRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Conference}/{action=Index}/{id?}");

app.Run();
