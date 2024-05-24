using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using FluentValidation;
using WebApp.ModelsValidation;


var builder = WebApplication.CreateBuilder(args);

// create a configuration (app settings) from the appsettings file, depending on the ENV
IConfiguration configuration = builder.Environment.IsDevelopment()
? builder.Configuration.AddJsonFile("appsettings.Development.json").Build()
: builder.Configuration.AddJsonFile("appsettings.json").Build();
// register the DbContext - EF ORM
// this allows the DbContext to be injected
builder.Services.AddDbContext<infsusContext>(options => options.UseSqlServer(configuration.GetConnectionString("InfsusDB")));

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services
            .AddFluentValidationAutoValidation(fv => {
                fv.DisableDataAnnotationsValidation = true;
            })
            .AddFluentValidationClientsideAdapters()
            .AddValidatorsFromAssemblyContaining<MjestoValidator>()
            .AddValidatorsFromAssemblyContaining<SmjestajValidator>()
            .AddValidatorsFromAssemblyContaining<PutovanjeValidator>()
            .AddValidatorsFromAssemblyContaining<RezervacijaValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();