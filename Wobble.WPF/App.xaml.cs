using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wobble.Models;
using Wobble.WPF.Windows;

namespace Wobble.WPF
{
    public partial class App : Application
    {
        private BotSettings _twitchBotSettings;

        private IServiceProvider ServiceProvider { get; set; }
        private IConfiguration Configuration { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddUserSecrets<MainWindow>();

            Configuration = builder.Build();

            _twitchBotSettings = new BotSettings(Configuration.AsEnumerable());

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            // Startup window
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.ShowDialog();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(s => new MainWindow(ServiceProvider, _twitchBotSettings));
            services.AddTransient(typeof(Help));
            services.AddTransient(typeof(About));
        }
    }
}