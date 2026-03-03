using FSMP.Core.Models;

namespace FSMP.MAUI.Selectors;

public class LibraryItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ArtistTemplate { get; set; }
    public DataTemplate? AlbumTemplate { get; set; }
    public DataTemplate? TrackTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        return item switch
        {
            Artist => ArtistTemplate!,
            Album => AlbumTemplate!,
            Track => TrackTemplate!,
            _ => ArtistTemplate!
        };
    }
}
