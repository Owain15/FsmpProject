namespace FSMP.Core;

/// <summary>
/// Provides responsive layout constants and helpers for phone vs desktop form factors.
/// Phone: &lt;600dp (portrait only, Android). Desktop: ≥600dp (any orientation, Windows).
/// </summary>
public static class ResponsiveHelper
{
    /// <summary>
    /// Width breakpoint in dp. Below this is phone layout, at or above is desktop.
    /// </summary>
    public const double PhoneMaxWidth = 600;

    /// <summary>
    /// Standard album art size for desktop layout.
    /// </summary>
    public const double AlbumArtDesktop = 200;

    /// <summary>
    /// Compact album art size for phone layout.
    /// </summary>
    public const double AlbumArtPhone = 120;

    /// <summary>
    /// Returns true if the given width indicates a phone-sized screen.
    /// </summary>
    public static bool IsPhone(double width) => width > 0 && width < PhoneMaxWidth;
}
