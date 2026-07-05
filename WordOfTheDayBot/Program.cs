global using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using WordOfTheDayBot;
using WordOfTheDayBot.Database;

// Just so that I don't forget where this is: https://netcord.dev/docs and https://netcord.dev/guides/getting-started/installation.html
// TODO improve the editorconfig

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services
	.AddDiscordGateway()
	.AddApplicationCommands()
	.AddGatewayHandlers(typeof(Program).Assembly);

// For some reason adding user secrets isn't done by default for HostApplicationBuilder. Also for some reason
// my builder.Environment turned into Production, for some reason. Idk why, but it doesn't really matter to me.
#if DEBUG
	builder.Configuration.AddUserSecrets<Program>();
#endif

// I figure the different scopes would be each individual handlker
builder.Services.AddScoped<MainLoop>();
builder.Services.AddScoped<WordManager>();
builder.Services.AddScoped<DatabaseInterface>();
builder.Services.AddScoped<MessageSender>();
builder.Services.AddScoped<UnexpectedErrorHandler>(); // Most services depend on this

builder.Services.AddHttpClient<DictionaryApiInterface>();

// The factory always needs to be initialized before normal dbcontext to prevent error.
builder.Services.AddDbContextFactory<AppDbContext>(options => {
	string supabaseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("DatabaseConnectionString is null");
	DbContextOptionsBuilder? dbContextOptions = options.UseNpgsql(supabaseConnectionString);
	if (builder.Environment.IsDevelopment()) {
		dbContextOptions.EnableSensitiveDataLogging()
		.LogTo(
			Console.WriteLine,
			[DbLoggerCategory.Database.Command.Name],
			LogLevel.Information
		);
	}
});

IHost host = builder.Build();

// Add commands from modules
host.AddModules(typeof(Program).Assembly);

await host.RunAsync();
