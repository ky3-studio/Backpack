namespace Backpack.Viewer.Views.Controls.AutoSuggestBox;

internal interface ITokenStringContainer
{
    string? Text { get; set; }

    bool IsLast { get; }
}
