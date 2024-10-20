using MudBlazor.Services;
using MyApp.Shared;
using MyApp.Shared.Interfaces;
using MyApp.Web.Components;
using MyApp.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add device specific services used by Razor Class Library (MyApp.Shared)
builder.Services.AddScoped<IFormFactor, FormFactor>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddMudServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MyApp.Shared._Imports).Assembly); 

app.Run();
