namespace Backpack.Viewer.ViewModels.Search;

internal interface ISearchableItem
{
    string Name    { get; }
    int    Rank    { get; }
    Uri?   IconUri { get; }
}
