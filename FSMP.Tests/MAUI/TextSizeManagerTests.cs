using System.Text.Json;
using FluentAssertions;
using FSMP.Core.Models;

namespace FSMP.Tests.MAUI;

/// <summary>
/// Tests for text-size-related logic. Since TextSizeManager itself uses MAUI types (ResourceDictionary),
/// these tests verify the Configuration model text size properties and data contracts that both
/// platforms rely on.
/// </summary>
public class TextSizeManagerTests
{
    private static readonly string[] AvailableTextSizes = { "Small", "Medium", "Large", "Extra Large" };

    [Fact]
    public void Configuration_TextSize_ShouldDefaultToMedium()
    {
        var config = new Configuration();
        config.TextSize.Should().Be("Medium");
    }

    [Fact]
    public void Configuration_ShouldRoundTrip_AllTextSizeNames()
    {
        foreach (var sizeName in AvailableTextSizes)
        {
            var config = new Configuration { TextSize = sizeName };
            var json = JsonSerializer.Serialize(config);
            var deserialized = JsonSerializer.Deserialize<Configuration>(json);

            deserialized!.TextSize.Should().Be(sizeName);
        }
    }

    [Fact]
    public void Configuration_TextSize_ShouldSerializeWithOtherProperties()
    {
        var config = new Configuration
        {
            Theme = "Dark",
            TextSize = "Large",
            DefaultVolume = 80
        };

        var json = JsonSerializer.Serialize(config);
        var deserialized = JsonSerializer.Deserialize<Configuration>(json);

        deserialized!.Theme.Should().Be("Dark");
        deserialized.TextSize.Should().Be("Large");
        deserialized.DefaultVolume.Should().Be(80);
    }

    [Fact]
    public void AvailableTextSizes_ShouldHaveFourPresets()
    {
        AvailableTextSizes.Should().HaveCount(4);
        AvailableTextSizes.Should().ContainInOrder("Small", "Medium", "Large", "Extra Large");
    }

    [Fact]
    public void Configuration_TextSize_ShouldAcceptAllValidValues()
    {
        foreach (var size in AvailableTextSizes)
        {
            var config = new Configuration { TextSize = size };
            config.TextSize.Should().Be(size);
        }
    }
}
