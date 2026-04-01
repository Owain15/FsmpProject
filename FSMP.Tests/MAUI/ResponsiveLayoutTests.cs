using FluentAssertions;
using FSMP.Core;
using Xunit;

namespace FSMP.Tests.MAUI;

/// <summary>
/// Tests that verify responsive layout logic for phone vs desktop form factors.
/// These test the decision logic (which elements should be visible/hidden at which widths)
/// rather than pixel-perfect rendering (which would require Appium).
/// </summary>
public class ResponsiveLayoutTests
{
    // --- Phone layout expectations (400dp typical phone) ---

    [Fact]
    public void PhoneWidth_ShouldBeDetectedAsPhone()
    {
        ResponsiveHelper.IsPhone(400).Should().BeTrue();
    }

    [Fact]
    public void PhoneWidth_ShouldUseCompactAlbumArt()
    {
        // On phone, album art should be 120dp
        var artSize = ResponsiveHelper.IsPhone(400) ? ResponsiveHelper.AlbumArtPhone : ResponsiveHelper.AlbumArtDesktop;
        artSize.Should().Be(120);
    }

    [Fact]
    public void PhoneWidth_ShouldHideSidebarToggles()
    {
        // On phone, sidebar toggle buttons should not be visible
        var showSidebarToggles = !ResponsiveHelper.IsPhone(400);
        showSidebarToggles.Should().BeFalse();
    }

    [Fact]
    public void PhoneWidth_NavMenu_ShouldBeFullWidth()
    {
        // On phone, nav menu should use full width (WidthRequest = -1)
        var isPhone = ResponsiveHelper.IsPhone(400);
        var menuWidth = isPhone ? -1.0 : 220.0;
        menuWidth.Should().Be(-1);
    }

    // --- Desktop layout expectations (1024dp typical desktop) ---

    [Fact]
    public void DesktopWidth_ShouldNotBeDetectedAsPhone()
    {
        ResponsiveHelper.IsPhone(1024).Should().BeFalse();
    }

    [Fact]
    public void DesktopWidth_ShouldUseStandardAlbumArt()
    {
        var artSize = ResponsiveHelper.IsPhone(1024) ? ResponsiveHelper.AlbumArtPhone : ResponsiveHelper.AlbumArtDesktop;
        artSize.Should().Be(200);
    }

    [Fact]
    public void DesktopWidth_ShouldShowSidebarToggles()
    {
        var showSidebarToggles = !ResponsiveHelper.IsPhone(1024);
        showSidebarToggles.Should().BeTrue();
    }

    [Fact]
    public void DesktopWidth_NavMenu_ShouldBeFixedWidth()
    {
        var isPhone = ResponsiveHelper.IsPhone(1024);
        var menuWidth = isPhone ? -1.0 : 220.0;
        menuWidth.Should().Be(220);
    }

    // --- Breakpoint boundary tests ---

    [Fact]
    public void AtBreakpoint600_ShouldBeDesktop()
    {
        ResponsiveHelper.IsPhone(600).Should().BeFalse();
    }

    [Fact]
    public void JustBelowBreakpoint_ShouldBePhone()
    {
        ResponsiveHelper.IsPhone(599).Should().BeTrue();
    }

    // --- Album art size constants ---

    [Fact]
    public void AlbumArtPhone_ShouldBeSmallerThanDesktop()
    {
        ResponsiveHelper.AlbumArtPhone.Should().BeLessThan(ResponsiveHelper.AlbumArtDesktop);
    }
}
