using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SwitchcraftKeys.Services;
using Xunit;

namespace SwitchcraftKeys.Tests.Services;

public sealed class LayoutServiceTests
{
    [Fact]
    public void GetAvailableLayouts_ReturnsOnlyLoadedLayouts()
    {
        var sut = CreateService(
            readLayouts: () => new Dictionary<string, string>
            {
                ["00000409"] = "US",
                ["00000416"] = "Portuguese (Brazil ABNT)",
            },
            getAllLayoutHandles: () => [new IntPtr(0x00000409)]);

        var layouts = sut.GetAvailableLayouts();

        layouts.Should().HaveCount(1);
        layouts.Should().Contain(layout => layout.Klid == "00000409" && layout.Hkl == new IntPtr(0x00000409));
        layouts.Should().NotContain(layout => layout.Klid == "00000416");
    }

    [Fact]
    public void GetAvailableLayouts_WhenLoadedHklHasVariantId_UsesBaseKlidDisplayName()
    {
        var sut = CreateService(
            readLayouts: () => new Dictionary<string, string>
            {
                ["00000C0A"] = "Spanish",
            },
            getAllLayoutHandles: () => [new IntPtr(0x040A0C0A)]);

        var layouts = sut.GetAvailableLayouts();

        layouts.Should().ContainSingle();
        layouts[0].DisplayName.Should().Be("Spanish");
        layouts[0].Klid.Should().Be("040A0C0A");
    }

    [Fact]
    public async Task SwitchLayoutAsync_WhenVerificationSucceeds_ReturnsTrue()
    {
        var activeHkl = IntPtr.Zero;
        var targetHkl = new IntPtr(0x00000409);
        var sut = CreateService(
            loadKeyboardLayout: (_, _) => targetHkl,
            activateKeyboardLayout: (hkl, _) =>
            {
                activeHkl = hkl;
                return new IntPtr(0x00000416);
            },
            getKeyboardLayout: _ => activeHkl);

        var switched = await sut.SwitchLayoutAsync("00000409");

        switched.Should().BeTrue();
    }

    [Fact]
    public async Task SwitchLayoutAsync_WhenLoadFails_ReturnsFalse()
    {
        var sut = CreateService(loadKeyboardLayout: (_, _) => IntPtr.Zero);

        var switched = await sut.SwitchLayoutAsync("00000409");

        switched.Should().BeFalse();
    }

    private static LayoutService CreateService(
        Func<Dictionary<string, string>>? readLayouts = null,
        Func<IntPtr[]>? getAllLayoutHandles = null,
        Func<uint, IntPtr>? getKeyboardLayout = null,
        Func<string, uint, IntPtr>? loadKeyboardLayout = null,
        Func<IntPtr, uint, IntPtr>? activateKeyboardLayout = null)
    {
        return new LayoutService(
            NullLogger<LayoutService>.Instance,
            readLayouts ?? (() => []),
            getAllLayoutHandles ?? (() => []),
            getKeyboardLayout ?? (_ => IntPtr.Zero),
            loadKeyboardLayout ?? ((_, _) => new IntPtr(0x00000409)),
            activateKeyboardLayout ?? ((_, _) => new IntPtr(0x00000416)));
    }
}
