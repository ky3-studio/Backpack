using Microsoft.UI.Xaml;

namespace Backpack.Viewer.Views.Controls.AutoSuggestBox;

[DependencyProperty<string>("Text")]
internal sealed partial class PreTokenStringContainer : DependencyObject, ITokenStringContainer
{
    public PreTokenStringContainer(bool isLast = false)
    {
        IsLast = isLast;
    }

    public PreTokenStringContainer(string text)
    {
        Text = text;
    }

    public bool IsLast { get; }

    public override string ToString()
    {
        return Text!;
    }
}
