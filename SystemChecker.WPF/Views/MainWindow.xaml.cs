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

        // Esconde a janela ao minimizar
        StateChanged += MainWindow_StateChanged;
        // Impede o fechamento direto da janela
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
        // Se o usuário tentar fechar a janela, apenas minimiza
        if (!App.IsShuttingDown)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
        }
    }
}