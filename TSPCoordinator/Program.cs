using System.Reflection;
using System.Text.Json.Serialization;
using Dahomey.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using TspCoordinator.Data;
using TspCoordinator.Data.TspApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    var converters = TspCoordinator.Data.TspApi.JsonConverters.Converters;

    foreach (var c in converters)
    {
        options.JsonSerializerOptions.Converters.Add(c);
    }
    options.JsonSerializerOptions.SetupExtensions();

});
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.ToString());
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
});
builder.Services.AddLocalization(options => { options.ResourcesPath = "Resources"; });
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ConfigurationService>();
builder.Services.AddSingleton<TspInstancesService>();
builder.Services.AddSingleton<JobService>();
builder.Services.AddSingleton<JobStatusReportingService>();
builder.Services.AddHostedService<ApplicationPartsLogger>();


var app = builder.Build();

// "Warm up" the job service to restore the job queue on startup (rather than on first request)
app.Services.GetService<JobService>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

var supportedCultures = new[] { "en-US", "ru-RU", "et-EE", "pl-PL" };

app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures));

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

// we need this declaration for running integration tests
public partial class Program
{

}