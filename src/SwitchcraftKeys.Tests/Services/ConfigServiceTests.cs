using FluentAssertions;
using SwitchcraftKeys.Models;
using SwitchcraftKeys.Services;
using Xunit;

namespace SwitchcraftKeys.Tests.Services;

public sealed class ConfigServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly ConfigService _sut;

    public ConfigServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "SwitchcraftKeysTests_" + Guid.NewGuid());
        _sut = new ConfigService(_tempDirectory);
    }

    [Fact]
    public void Load_WhenNoConfigExists_ReturnsDefaultConfig()
    {
        var config = _sut.Load();

        config.Version.Should().Be(1);
        config.Logging.MinimumLevel.Should().Be("Trace");
    }

    [Fact]
    public void SaveThenLoad_RoundTripsValues()
    {
        var config = new AppConfig
        {
            Logging = new LoggingConfig { MinimumLevel = "Warning" },
            Devices =
            {
                ["VID_046D&PID_C31C"] = new DeviceInfo
                {
                    DeviceId = "VID_046D&PID_C31C",
                    Alias = "Logitech",
                    AssignedLayoutKlid = "00000409",
                    RawPath = "not-persisted",
                },
            },
        };

        _sut.Save(config);
        var loaded = _sut.Load();

        loaded.Logging.MinimumLevel.Should().Be("Warning");
        loaded.Devices["VID_046D&PID_C31C"].Alias.Should().Be("Logitech");
        loaded.Devices["VID_046D&PID_C31C"].AssignedLayoutKlid.Should().Be("00000409");
        loaded.Devices["VID_046D&PID_C31C"].RawPath.Should().BeNull();
    }

    [Fact]
    public void EnsureDeviceExists_WhenMissing_AddsDeviceAndSaves()
    {
        _sut.EnsureDeviceExists("BUILTIN");

        var loaded = _sut.Load();

        loaded.Devices.Should().ContainKey("BUILTIN");
        loaded.Devices["BUILTIN"].Alias.Should().Be("BUILTIN");
    }

    [Fact]
    public void AssignLayout_WhenDeviceMissing_AddsDeviceAndPersistsLayout()
    {
        _sut.AssignLayout("VID_046D&PID_C31C", "00000409");

        _sut.GetMappedLayoutKlid("vid_046d&pid_c31c").Should().Be("00000409");
    }

    [Fact]
    public void SetDeviceAlias_WhenBlankAlias_ResetsToDeviceId()
    {
        _sut.SetDeviceAlias("BUILTIN", "Notebook Keyboard");
        _sut.SetDeviceAlias("BUILTIN", " ");

        _sut.Load().Devices["BUILTIN"].Alias.Should().Be("BUILTIN");
    }

    [Fact]
    public void Save_RotatesBackupsAcrossSuccessiveSaves()
    {
        var configPath = Path.Combine(_tempDirectory, "config.json");

        _sut.Save(new AppConfig { Logging = new LoggingConfig { MinimumLevel = "Trace" } });
        _sut.Save(new AppConfig { Logging = new LoggingConfig { MinimumLevel = "Debug" } });
        _sut.Save(new AppConfig { Logging = new LoggingConfig { MinimumLevel = "Information" } });
        _sut.Save(new AppConfig { Logging = new LoggingConfig { MinimumLevel = "Warning" } });

        File.Exists(configPath).Should().BeTrue();
        File.Exists(configPath + ".bak1").Should().BeTrue();
        File.Exists(configPath + ".bak2").Should().BeTrue();
        File.Exists(configPath + ".bak3").Should().BeTrue();

        // Active file holds the most recent save; .bak1 holds the one before it.
        _sut.Load().Logging.MinimumLevel.Should().Be("Warning");
        File.ReadAllText(configPath + ".bak1").Should().Contain("Information");
    }

    [Fact]
    public void Load_WhenPrimaryFileIsCorrupted_FallsBackToBak1()
    {
        _sut.Save(new AppConfig { Logging = new LoggingConfig { MinimumLevel = "Error" } });
        _sut.Save(new AppConfig { Logging = new LoggingConfig { MinimumLevel = "Critical" } });

        var configPath = Path.Combine(_tempDirectory, "config.json");
        File.WriteAllText(configPath, "{ not valid json ");

        var loaded = _sut.Load();

        loaded.Logging.MinimumLevel.Should().Be("Error");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}
