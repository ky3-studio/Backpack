using Microsoft.Windows.ApplicationModel.Resources;

namespace Backpack.Viewer.Localization;

internal static class Localized
{
    private static readonly ResourceLoader? Loader = TryCreateLoader();

    public static string Get(string key)
    {
        try
        {
            var text = Loader?.GetString(key);
            if (!string.IsNullOrEmpty(text))
                return text;
        }
        catch
        {
        }

        return key;
    }

    private static ResourceLoader? TryCreateLoader()
    {
        try
        {
            return new ResourceLoader();
        }
        catch
        {
            return null;
        }
    }
}
