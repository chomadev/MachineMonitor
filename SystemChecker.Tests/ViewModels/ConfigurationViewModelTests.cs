using Moq;
using SystemChecker.Core.Interfaces;
using SystemChecker.WPF.Commands;
using SystemChecker.WPF.ViewModels;
using Xunit;

namespace SystemChecker.Tests.ViewModels;

public class ConfigurationViewModelTests
{
    private readonly Mock<IConfigurationService> _configServiceMock;
    private readonly ConfigurationViewModel _viewModel;

    public ConfigurationViewModelTests()
    {
        _configServiceMock = new Mock<IConfigurationService>();
        _configServiceMock.Setup(x => x.GetCurrentSchedule())
            .Returns("*/15 * * * *");

        _viewModel = new ConfigurationViewModel(_configServiceMock.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithCurrentSchedule()
    {
        // Assert
        Assert.Equal("*/15 * * * *", _viewModel.CronExpression);
    }

    [Theory]
    [InlineData("*/15 * * * *", true)]  // Válido
    [InlineData("invalid", false)]       // Inválido
    [InlineData("", false)]              // Vazio
    public void CanSaveConfiguration_ShouldValidateCronExpression(string cronExpression, bool expectedResult)
    {
        // Arrange
        _viewModel.CronExpression = cronExpression;

        // Act
        var canExecute = (_viewModel.SaveCommand as AsyncRelayCommand)?.CanExecute(null);

        // Assert
        Assert.Equal(expectedResult, canExecute);
    }

    [Fact]
    public void ValidateExpression_ShouldShowNextExecution()
    {
        // Arrange
        _viewModel.CronExpression = "0 12 * * *";

        // Act
        (_viewModel.ValidateCommand as RelayCommand).Execute(null);

        // Assert
        Assert.Contains("Válido", _viewModel.ValidationMessage);
        Assert.Contains("12:00", _viewModel.ValidationMessage);
    }

    [Fact]
    public void ValidateExpression_ShouldShowErrorForInvalidExpression()
    {
        // Arrange
        _viewModel.CronExpression = "invalid";

        // Act
        (_viewModel.ValidateCommand as RelayCommand).Execute(null);

        // Assert
        Assert.Contains("inválida", _viewModel.ValidationMessage);
    }

    [Fact]
    public async Task LoadInitialConfiguration_ShouldLoadCorrectly()
    {
        // Arrange
        var configService = new Mock<IConfigurationService>();
        configService.Setup(x => x.GetCurrentSchedule())
            .Returns("*/15 * * * *");
        configService.Setup(x => x.GetMonitoredServices())
            .Returns(new List<string> { "Service1", "Service2" });

        var viewModel = new ConfigurationViewModel(configService.Object);

        // Act
        await viewModel.LoadInitialConfiguration();

        // Assert
        Assert.Equal("*/15 * * * *", viewModel.CronExpression);
        Assert.Equal(2, viewModel.MonitoredServices.Count);
        Assert.Contains("Service1", viewModel.MonitoredServices);
        Assert.Contains("Service2", viewModel.MonitoredServices);
        Assert.Contains("sucesso", viewModel.ValidationMessage);
    }
}