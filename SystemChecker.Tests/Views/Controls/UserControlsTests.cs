using System.Windows;
using SystemChecker.Core.Models;
using SystemChecker.WPF.ViewModels;
using SystemChecker.WPF.Views.Controls;
using Xunit;

namespace SystemChecker.Tests.Views.Controls;

public class UserControlsTests
{
    public UserControlsTests()
    {
        // Necessário para testes WPF
        if (Application.Current == null)
        {
            new Application();
        }
    }

    [WpfFact]
    public void ServicesTabControl_ShouldLoadAndBindToViewModel()
    {
        // Arrange
        var viewModel = new MainViewModel(null, null);
        var control = new ServicesTabControl { DataContext = viewModel };

        // Act
        control.ApplyTemplate();

        // Assert
        Assert.NotNull(control);
        Assert.Equal(viewModel, control.DataContext);
    }

    [WpfFact]
    public void SystemResourcesTabControl_ShouldLoadAndBindToViewModel()
    {
        // Arrange
        var viewModel = new MainViewModel(null, null);
        var control = new SystemResourcesTabControl { DataContext = viewModel };

        // Act
        control.ApplyTemplate();

        // Assert
        Assert.NotNull(control);
        Assert.Equal(viewModel, control.DataContext);
    }

    [WpfFact]
    public void ConfigurationTabControl_ShouldLoadAndBindToViewModel()
    {
        // Arrange
        var viewModel = new MainViewModel(null, null);
        var control = new ConfigurationTabControl { DataContext = viewModel };

        // Act
        control.ApplyTemplate();

        // Assert
        Assert.NotNull(control);
        Assert.Equal(viewModel, control.DataContext);
    }
} 