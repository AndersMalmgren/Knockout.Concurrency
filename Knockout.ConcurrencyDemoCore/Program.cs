using Knockout.ConcurrencyDemoCore.Events;
using SignalR.EventAggregatorProxy.AspNetCore.Middlewares;
using SignalR.EventAggregatorProxy.Boostrap;
using SignalR.EventAggregatorProxy.Event;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddSignalREventAggregator()
    .AddSingleton<IEventAggregator, EventAggregator>()
    .AddSingleton<SignalR.EventAggregatorProxy.EventAggregation.IEventAggregator>(p => p.GetRequiredService<IEventAggregator>())
    .AddSingleton<IEventTypeFinder, EventTypeFinder>()
    .AddTransient<MessageConstraintHandler>()
    .AddRazorPages();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapDefaultControllerRoute();

app
    .UseEventProxy()
    .UseSignalREventAggregator();

app.Run();
