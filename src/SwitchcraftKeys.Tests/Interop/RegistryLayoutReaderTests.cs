using FluentAssertions;
using SwitchcraftKeys.Interop;
using Xunit;

namespace SwitchcraftKeys.Tests.Interop;

/// <summary>
/// Tests for RegistryLayoutReader.
/// These tests read the real registry — no Win32 mock needed since
/// RegistryLayoutReader uses Microsoft.Win32.Registry (managed), not P/Invoke.
/// Every Windows machine has at least one layout installed, so the assertions
/// are safe on any developer or CI machine.
/// </summary>
public sealed class RegistryLayoutReaderTests
{
    // -------------------------------------------------------------------------
    // ReadAll
    // -------------------------------------------------------------------------

    [Fact]
    public void ReadAll_ReturnsAtLeastOneLayout()
    {
        var layouts = RegistryLayoutReader.ReadAll();
        layouts.Should().NotBeEmpty("every Windows machine has at least one keyboard layout");
    }

    [Fact]
    public void ReadAll_AllKeysAre8CharUppercaseHex()
    {
        var layouts = RegistryLayoutReader.ReadAll();

        foreach (var klid in layouts.Keys)
        {
            klid.Should().HaveLength(8, because: "KLIDs are always 8-char hex strings");
            klid.Should().MatchRegex("^[0-9A-F]{8}$", because: "KLIDs must be uppercase hex");
        }
    }

    [Fact]
    public void ReadAll_AllDisplayNamesAreNonEmpty()
    {
        var layouts = RegistryLayoutReader.ReadAll();

        foreach (var (klid, name) in layouts)
        {
            name.Should().NotBeNullOrWhiteSpace(
                because: $"layout {klid} must have a non-empty display name");
        }
    }

    [Fact]
    public void ReadAll_IsCaseInsensitiveOnKey()
    {
        // Dictionary uses OrdinalIgnoreCase comparer
        var layouts = RegistryLayoutReader.ReadAll();
        if (layouts.Count == 0) return;

        var firstKey = layouts.Keys.First();
        layouts.Should().ContainKey(firstKey.ToLowerInvariant());
        layouts.Should().ContainKey(firstKey.ToUpperInvariant());
    }

    // -------------------------------------------------------------------------
    // GetDisplayName
    // -------------------------------------------------------------------------

    [Fact]
    public void GetDisplayName_InstalledKlid_ReturnsNonNullName()
    {
        var all = RegistryLayoutReader.ReadAll();
        all.Should().NotBeEmpty("every Windows machine has at least one keyboard layout");

        var installedKlid = all.Keys.First();
        var name = RegistryLayoutReader.GetDisplayName(installedKlid);
        name.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]        // too short
    [InlineData("000000000")]  // too long
    public void GetDisplayName_InvalidKlid_ReturnsNull(string? klid)
    {
        var name = RegistryLayoutReader.GetDisplayName(klid!);
        name.Should().BeNull();
    }

    [Fact]
    public void GetDisplayName_NonExistentKlid_ReturnsNull()
    {
        // DEADBEEF is not a real layout KLID
        var name = RegistryLayoutReader.GetDisplayName("DEADBEEF");
        name.Should().BeNull();
    }

    [Fact]
    public void GetDisplayName_MatchesReadAllForSameKlid()
    {
        var all = RegistryLayoutReader.ReadAll();
        if (all.Count == 0) return;

        var (klid, expectedName) = all.First();
        var name = RegistryLayoutReader.GetDisplayName(klid);
        name.Should().Be(expectedName);
    }
}
