using System.ComponentModel;
using System.Windows;
using SystemChecker.Core.Interfaces;
using SystemChecker.WPF.ViewModels;

namespace SystemChecker.WPF.Views;

public partial class MainWindow : Window
{
    private readonly ITrayIconService _trayIconService;

    public MainWindow(MainViewModel viewModel, ITrayIconService trayIconService)
    {
        InitializeComponent();
        DataContext = viewModel;
        _trayIconService = trayIconService;

        // Hide the window when minimized
        StateChanged += MainWindow_StateChanged;
        // Prevent the window from being closed directly
        Closing += MainWindow_Closing;
    }

    private void MainWindow_StateChanged(object sender, System.EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
            _trayIconService.ShowNotification("System Checker", "Application continues running in the background.");
        }
    }

    private void MainWindow_Closing(object sender, CancelEventArgs e)
    {
        // If the user tries to close the window, only minimize
        if (!App.IsShuttingDown)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
        }
    }
}