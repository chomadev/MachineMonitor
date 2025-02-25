using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemChecker.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SystemChecker.Core.Models;
using System.Net.Http.Json;
using System.Net.Http;
using SystemChecker.Infrastructure.Settings;
using SystemChecker.WPF.Models;
using System.Threading;

namespace SystemChecker.WPF.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;
    public event EventHandler ConfigurationChanged;
    private readonly ILogger<ConfigurationService> _logger;
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _loadLock = new SemaphoreSlim(1, 1);
    private bool _isLoading = false;
    private DateTime _lastLoadTime = DateTime.MinValue;
    private const int MIN_RELOAD_INTERVAL_MS = 2000; // 2 segundos entre reloads

    public ConfigurationService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<ConfigurationService> logger)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient("API");
        _logger = logger;
    }

    public async Task<MachineConfiguration> LoadConfigurationAsync()
    {
        if (_isLoading)
        {
            _logger.LogDebug("Configuration load already in progress, waiting...");
            await _loadLock.WaitAsync();
            _loadLock.Release();
            return await GetCachedOrLoadConfigurationAsync();
        }

        var timeSinceLastLoad = DateTime.UtcNow - _lastLoadTime;
        if (timeSinceLastLoad.TotalMilliseconds < MIN_RELOAD_INTERVAL_MS)
        {
            _logger.LogDebug("Configuration was loaded recently, using cached version");
            return await GetCachedOrLoadConfigurationAsync();
        }

        return await GetCachedOrLoadConfigurationAsync();
    }

    private async Task<MachineConfiguration> GetCachedOrLoadConfigurationAsync()
    {
        if (!await _loadLock.WaitAsync(0))
        {
            _logger.LogDebug("Another thread is loading configuration, waiting...");
            await _loadLock.WaitAsync();
            _loadLock.Release();
            return _cachedConfiguration ?? new MachineConfiguration();
        }

        try
        {
            _isLoading = true;
            var apiSettings = _configuration.GetSection("ApiSettings");
            var baseUrl = apiSettings["BaseUrl"];
            var apiKey = apiSettings["MachineKey"];
            var url = $"{baseUrl}/api/configuration?apiKey={apiKey}";

            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to load configuration from API. Status: {Status}", response.StatusCode);
                throw new Exception("Failed to load configuration from API");
            }

            var apiConfig = await response.Content.ReadFromJsonAsync<ApiMachineConfigurationDto>();
            if (apiConfig == null)
            {
                throw new Exception("Received null configuration from API");
            }

            var config = apiConfig.ToMachineConfiguration();
            _logger.LogInformation("Configuration loaded from API successfully");

            _cachedConfiguration = config;
            _lastLoadTime = DateTime.UtcNow;
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading configuration");
            throw;
        }
        finally
        {
            _isLoading = false;
            _loadLock.Release();
        }
    }

    private MachineConfiguration _cachedConfiguration;

    public async Task SaveConfigurationAsync(MachineConfiguration config)
    {
        try
        {
            var apiSettings = _configuration.GetSection("ApiSettings");
            var baseUrl = apiSettings["BaseUrl"];
            var apiKey = apiSettings["MachineKey"];
            var url = $"{baseUrl}/api/configuration?apiKey={apiKey}";

            var apiConfig = ApiMachineConfigurationDto.FromMachineConfiguration(config);

            var response = await _httpClient.PutAsJsonAsync(url, apiConfig);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to save configuration to API. Status: {Status}", response.StatusCode);
                throw new Exception("Failed to save configuration to API");
            }

            _cachedConfiguration = config;
            _lastLoadTime = DateTime.UtcNow;

            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving configuration");
            throw;
        }
    }
} 