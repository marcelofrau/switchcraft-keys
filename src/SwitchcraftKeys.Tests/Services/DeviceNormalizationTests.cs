using FluentAssertions;
using SwitchcraftKeys.Services;
using Xunit;

namespace SwitchcraftKeys.Tests.Services;

public sealed class DeviceNormalizationTests
{
    // -------------------------------------------------------------------------
    // USB / HID paths → VID_XXXX&PID_XXXX
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(@"\\?\HID#VID_046D&PID_C31C&MI_00#7&1234abcd&0&0000#{884b96c3}", "VID_046D&PID_C31C")]
    [InlineData(@"\\?\HID#VID_045E&PID_0750#7&5678efab&0&0000#{abc}", "VID_045E&PID_0750")]
    [InlineData(@"\\?\HID#VID_04D9&PID_A01C&Col01#8&deadbeef&0&0000#{884}", "VID_04D9&PID_A01C")]
    [InlineData(@"\\?\HID#{00001812-0000-1000-8000-00805f9b34fb}_Dev_VID&02046d_PID&b380_REV&0015_d8bf3124f921&Col01#9&376f780e&0&0000#{884b96c3}", "VID_046D&PID_B380")]
    public void Normalize_UsbPath_ReturnsVidPid(string rawPath, string expected)
    {
        var result = DeviceIdNormalizer.Normalize(rawPath);
        result.Should().Be(expected);
    }

    [Fact]
    public void Normalize_UsbPath_LowercaseVidPid_NormalizesToUppercase()
    {
        var path = @"\\?\HID#vid_046d&pid_c31c&MI_00#7&1234abcd&0&0000#{}";
        var result = DeviceIdNormalizer.Normalize(path);
        result.Should().Be("VID_046D&PID_C31C");
    }

    // -------------------------------------------------------------------------
    // BUILTIN paths → "BUILTIN"
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(@"\\?\ACPI#PNP0303#3&13c0b0c5&0#{884b96c3-56ef-11d1-bc8c-00a0c91405dd}")]
    [InlineData(@"\\?\ACPI#PNP0303#3&deadbeef&0#{}")]
    public void Normalize_AcpiPath_ReturnsBuiltin(string rawPath)
    {
        var result = DeviceIdNormalizer.Normalize(rawPath);
        result.Should().Be("BUILTIN");
    }

    [Theory]
    [InlineData(@"\\?\Root\RDP_KBD#0000#{884b96c3-56ef-11d1-bc8c-00a0c91405dd}")]
    [InlineData(@"\\?\ROOT\RDP_KBD#0001#{}")]
    public void Normalize_RdpKeyboard_ReturnsBuiltin(string rawPath)
    {
        var result = DeviceIdNormalizer.Normalize(rawPath);
        result.Should().Be("BUILTIN");
    }

    [Theory]
    [InlineData(@"\\?\ACPI#INT33C6#0#{884b96c3-56ef-11d1-bc8c-00a0c91405dd}")]
    [InlineData(@"\\?\i8042prt\KEYBOARD#0#{abc}")]
    public void Normalize_I8042OrAcpiVariants_ReturnsBuiltin(string rawPath)
    {
        // Paths containing I8042 or ACPI anywhere should map to BUILTIN
        var result = DeviceIdNormalizer.Normalize(rawPath);
        result.Should().Be("BUILTIN");
    }

    // -------------------------------------------------------------------------
    // Edge cases
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Normalize_NullOrEmpty_ReturnsNull(string? rawPath)
    {
        var result = DeviceIdNormalizer.Normalize(rawPath);
        result.Should().BeNull();
    }

    [Fact]
    public void Normalize_UnrecognizedPath_ReturnsNull()
    {
        var result = DeviceIdNormalizer.Normalize(@"\\?\UNKNOWN#DEVICE#0#{}");
        result.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // IsBuiltin helper
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("BUILTIN", true)]
    [InlineData("builtin", true)]
    [InlineData("Builtin", true)]
    [InlineData("VID_046D&PID_C31C", false)]
    [InlineData(null, false)]
    public void IsBuiltin_ReturnsExpected(string? deviceId, bool expected)
    {
        DeviceIdNormalizer.IsBuiltin(deviceId).Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // Stability: same path always produces same ID
    // -------------------------------------------------------------------------

    [Fact]
    public void Normalize_SamePath_AlwaysReturnsSameId()
    {
        var path = @"\\?\HID#VID_046D&PID_C31C&MI_00#7&1234abcd&0&0000#{}";
        var first  = DeviceIdNormalizer.Normalize(path);
        var second = DeviceIdNormalizer.Normalize(path);
        first.Should().Be(second);
    }
}
