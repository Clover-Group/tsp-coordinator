using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using TspCoordinator.Data;
using TspCoordinator.Data.TspApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    var converters = new JsonConverter[] 
    {
        new TypeMappingConverter<TspCoordinator.Data.TspApi.V1.IInputConf, TspCoordinator.Data.TspApi.V1.JdbcInputConf>(),
        new TypeMappingConverter<TspCoordinator.Data.TspApi.V1.IInputConf, TspCoordinator.Data.TspApi.V1.KafkaInputConf>(),
        new TypeMappingConverter<TspCoordinator.Data.TspApi.V1.IOutputConf, TspCoordinator.Data.TspApi.V1.JdbcOutputConf>(),
        new TypeMappingConverter<TspCoordinator.Data.TspApi.V1.IOutputConf, TspCoordinator.Data.TspApi.V1.KafkaOutputConf>(),

        new TypeMappingConverter<TspCoordinator.Data.TspApi.V2.IInputConf, TspCoordinator.Data.TspApi.V2.JdbcInputConf>(),
        new TypeMappingConverter<TspCoordinator.Data.TspApi.V2.IInputConf, TspCoordinator.Data.TspApi.V2.KafkaInputConf>(),
        new TypeMappingConverter<TspCoordinator.Data.TspApi.V2.IOutputConf, TspCoordinator.Data.TspApi.V2.JdbcOutputConf>(),
        new TypeMappingConverter<TspCoordinator.Data.TspApi.V2.IOutputConf, TspCoordinator.Data.TspApi.V2.KafkaOutputConf>(),

        new TypeMappingConverter<TspCoordinator.Data.TspApi.V3.IInputConf, TspCoordinator.Data.TspApi.V3.JdbcInputConf>(),
        new TypeMappingConverter<TspCoordinator.Data.TspApi.V3.IInputConf, TspCoordinator.Data.TspApi.V3.KafkaInputConf>(),
        new TypeMappingConverter<TspCoordinator.Data.TspApi.V3.IOutputConf, TspCoordinator.Data.TspApi.V3.JdbcOutputConf>(),
        new TypeMappingConverter<TspCoordinator.Data.TspApi.V3.IOutputConf, TspCoordinator.Data.TspApi.V3.KafkaOutputConf>(),
        new EventSchemaValueConverter()
    };

    foreach (var c in converters)
    {
        options.JsonSerializerOptions.Converters.Add(c);
    }

});
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<TspInstancesService>();
builder.Services.AddSingleton<JobService>();
builder.Services.AddHostedService<ApplicationPartsLogger>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
