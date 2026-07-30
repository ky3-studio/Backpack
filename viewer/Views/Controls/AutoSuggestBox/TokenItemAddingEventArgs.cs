using CommunityToolkit.Common.Deferred;

namespace Backpack.Viewer.Views.Controls.AutoSuggestBox;

internal sealed class TokenItemAddingEventArgs : DeferredCancelEventArgs
{
    public TokenItemAddingEventArgs(string token)
    {
        TokenText = token;
    }

    public string TokenText { get; private set; }

    public object? Item { get; set; }
}
