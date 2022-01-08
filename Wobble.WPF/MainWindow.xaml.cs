using System;
using System.ComponentModel;
using System.Windows;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Wobble.Models;
using Wobble.ViewModels;
using Wobble.WPF.Windows;

namespace Wobble.WPF
{
    public partial class MainWindow : Window
    {
        private static readonly ILog s_log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly IServiceProvider _serviceProvider;

        private BotManager VM => DataContext as BotManager;

        public MainWindow(IServiceProvider serviceProvider, BotSettings botSettings)
        {
            InitializeComponent();

            s_log.Info("Wobble started");

            _serviceProvider = serviceProvider;

            DataContext = new BotManager(botSettings);

            Closing += OnClosing;
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Help_OnClick(object sender, RoutedEventArgs e)
        {
            Help help = _serviceProvider.GetRequiredService<Help>();
            help.Owner = this;
            help.Show();
        }

        private void About_OnClick(object sender, RoutedEventArgs e)
        {
            About about = _serviceProvider.GetRequiredService<About>();
            about.Owner = this;
            about.ShowDialog();
        }

        private void DisplayCommands_OnClick(object sender, RoutedEventArgs e)
        {
            VM.DisplayCommands();
        }

        private void ClearChat_OnClick(object sender, RoutedEventArgs e)
        {
            VM.ClearChat();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            s_log.Info("Wobble stopped");

            VM.Disconnect();
        }
    }
}