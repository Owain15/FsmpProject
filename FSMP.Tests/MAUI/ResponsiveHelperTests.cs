using FluentAssertions;
using FSMP.Core;
using Xunit;

namespace FSMP.Tests.MAUI;

public class ResponsiveHelperTests
{
    [Fact]
    public void PhoneMaxWidth_ShouldBe600()
    {
        ResponsiveHelper.PhoneMaxWidth.Should().Be(600);
    }

    [Fact]
    public void AlbumArtDesktop_ShouldBe200()
    {
        ResponsiveHelper.AlbumArtDesktop.Should().Be(200);
    }

    [Fact]
    public void AlbumArtPhone_ShouldBe120()
    {
        ResponsiveHelper.AlbumArtPhone.Should().Be(120);
    }

    [Theory]
    [InlineData(0, false)]      // Zero width — invalid, not phone
    [InlineData(-1, false)]     // Negative — invalid, not phone
    [InlineData(359, true)]     // Below phone range — still phone
    [InlineData(360, true)]     // Phone lower bound
    [InlineData(400, true)]     // Typical phone width
    [InlineData(430, true)]     // Phone upper bound
    [InlineData(599, true)]     // Just below breakpoint
    [InlineData(600, false)]    // At breakpoint — desktop
    [InlineData(601, false)]    // Just above breakpoint — desktop
    [InlineData(800, false)]    // Typical desktop
    [InlineData(1024, false)]   // Wide desktop
    [InlineData(1920, false)]   // Full HD desktop
    public void IsPhone_ShouldReturnCorrectResult(double width, bool expected)
    {
        ResponsiveHelper.IsPhone(width).Should().Be(expected);
    }
}
