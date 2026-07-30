using CommunityToolkit.Common.Deferred;

namespace Backpack.Viewer.Views.Controls.AutoSuggestBox;

internal sealed class TokenItemRemovingEventArgs : DeferredCancelEventArgs
{
    public TokenItemRemovingEventArgs(object item, AutoSuggestTokenBoxItem token)
    {
        Item = item;
        Token = token;
    }

    public object Item { get; private set; }

    public AutoSuggestTokenBoxItem Token { get; private set; }
}
