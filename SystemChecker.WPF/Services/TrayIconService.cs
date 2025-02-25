using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Controls;
using SystemChecker.Core.Interfaces;
using SystemChecker.WPF.ViewModels;

namespace SystemChecker.WPF.Services;

public class TrayIconService : ITrayIconService, IDisposable
{
    private TaskbarIcon _notifyIcon;
    private readonly IServiceProvider _serviceProvider;

    public TrayIconService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Initialize()
    {
        _notifyIcon = new TaskbarIcon
        {
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName),
            ToolTipText = "System Checker"
        };

        // Menu de contexto
        var contextMenu = new ContextMenu();

        var openMenuItem = new MenuItem { Header = "Abrir" };
        openMenuItem.Click += OpenMenuItem_Click;
        contextMenu.Items.Add(openMenuItem);

        var checkNowMenuItem = new MenuItem { Header = "Verificar Agora" };
        checkNowMenuItem.Click += CheckNowMenuItem_Click;
        contextMenu.Items.Add(checkNowMenuItem);

        contextMenu.Items.Add(new Separator());

        var exitMenuItem = new MenuItem { Header = "Sair" };
        exitMenuItem.Click += ExitMenuItem_Click;
        contextMenu.Items.Add(exitMenuItem);

        _notifyIcon.ContextMenu = contextMenu;

        // Duplo clique abre a janela
        _notifyIcon.TrayMouseDoubleClick += NotifyIcon_TrayMouseDoubleClick;
    }

    private void NotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
    {
        ShowMainWindow();
    }

    private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ShowMainWindow();
    }

    private void CheckNowMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
        if (mainViewModel.CheckNowCommand.CanExecute(null))
        {
            mainViewModel.CheckNowCommand.Execute(null);
        }
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        App.IsShuttingDown = true;
        Application.Current.Shutdown();
    }

    private void ShowMainWindow()
    {
        var mainWindow = Application.Current.MainWindow;
        if (mainWindow != null)
        {
            mainWindow.Show();
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Activate();
        }
    }

    public void Dispose()
    {
        _notifyIcon?.Dispose();
    }
}