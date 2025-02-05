using Moq;
using SystemChecker.Core.Interfaces;
using SystemChecker.WPF.Commands;
using SystemChecker.WPF.ViewModels;
using Xunit;

namespace SystemChecker.Tests.ViewModels;

public class ConfigurationViewModelTests : TestBase
{
    private readonly Mock<IConfigurationService> _configServiceMock;
    private readonly ConfigurationViewModel _viewModel;

    public ConfigurationViewModelTests()
    {
        _configServiceMock = new Mock<IConfigurationService>();
        _configServiceMock.Setup(x => x.GetCurrentSchedule())
            .Returns("*/15 * * * *");
        _configServiceMock.Setup(x => x.GetMonitoredServices())
            .Returns(new List<string> { "TestService1", "TestService2" });

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
        // Act
        await _viewModel.LoadInitialConfiguration();

        // Assert
        Assert.Equal("*/15 * * * *", _viewModel.CronExpression);
        Assert.Equal(2, _viewModel.MonitoredServices.Count);
        Assert.Contains("TestService1", _viewModel.MonitoredServices);
        Assert.Contains("TestService2", _viewModel.MonitoredServices);
        Assert.Contains("successfully", _viewModel.ValidationMessage);
    }

    [Fact]
    public void ValidateCronExpression_ShouldShowValidationMessage()
    {
        // Arrange
        _viewModel.CronExpression = "*/15 * * * *";

        // Act
        _viewModel.ValidateCommand.Execute(null);

        // Assert
        Assert.Contains("Valid", _viewModel.ValidationMessage);
        Assert.Contains("Next execution", _viewModel.ValidationMessage);
    }

    [Fact]
    public async Task SaveConfiguration_ShouldUpdateConfigurationService()
    {
        // Arrange
        var savedCronExpression = string.Empty;
        var savedServices = new List<string>();

        _configServiceMock.Setup(x => x.UpdateConfiguration(
            It.IsAny<string>(),
            It.IsAny<List<string>>()))
            .Callback<string, List<string>>((cron, services) =>
            {
                savedCronExpression = cron;
                savedServices = services;
            })
            .Returns(Task.CompletedTask);

        _viewModel.CronExpression = "*/30 * * * *";
        _viewModel.MonitoredServices.Add("NewService");

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal("*/30 * * * *", savedCronExpression);
        Assert.Contains("NewService", savedServices);
        Assert.Contains("successfully", _viewModel.ValidationMessage);
    }
}