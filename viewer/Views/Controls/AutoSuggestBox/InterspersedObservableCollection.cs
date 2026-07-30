using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Backpack.Viewer.Views.Controls.AutoSuggestBox;

internal sealed partial class InterspersedObservableCollection : IList, IEnumerable<object>, INotifyCollectionChanged
{
    private readonly Dictionary<int, object> interspersedObjects = [];
    private bool isInsertingOriginal;

    public InterspersedObservableCollection()
        : this(new ObservableCollection<object>())
    {
    }

    public InterspersedObservableCollection(object itemsSource)
    {
        if (itemsSource is not IList list)
        {
            throw new ArgumentException("The input items source must implements System.Collections.IList");
        }

        ItemsSource = list;

        if (ItemsSource is not INotifyCollectionChanged incc)
        {
            throw new ArgumentException("The input items source must implements System.Collections.Specialized.INotifyCollectionChanged");
        }

        incc.CollectionChanged += OnCollectionChanged;
    }

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public IList ItemsSource { get; }

    public bool IsFixedSize
    {
        get => false;
    }

    public bool IsReadOnly
    {
        get => false;
    }

    public int Count
    {
        get => ItemsSource.Count + interspersedObjects.Count;
    }

    public bool IsSynchronized
    {
        get => false;
    }

    public object SyncRoot
    {
        get => new();
    }

    public object? this[int index]
    {
        get => interspersedObjects.TryGetValue(index, out object? value) ? value : ItemsSource[ToInnerIndex(index)];
        set => throw new NotImplementedException();
    }

    public void Insert(int index, object? obj)
    {
        MoveKeysForward(index, 1);

        ArgumentNullException.ThrowIfNull(obj);
        interspersedObjects[index] = obj;

        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, obj, index));
    }

    public void InsertAt(int outerIndex, object obj)
    {
        int index = outerIndex - interspersedObjects.Keys.Count(key => key < outerIndex);

        if (index != outerIndex)
        {
            MoveKeysForward(outerIndex, 1);

            isInsertingOriginal = true;
        }

        ItemsSource.Insert(index, obj);
    }

    public IEnumerator<object> GetEnumerator()
    {
        int i = 0;
        int count = 0;
        int realized = 0;

        foreach (object element in ItemsSource)
        {
            while (interspersedObjects.TryGetValue(i++, out object? obj))
            {
                realized++;

                yield return obj;
            }

            count++;

            yield return element;
        }

        if (realized < interspersedObjects.Count)
        {
            foreach ((int _, object value) in interspersedObjects.Where(kvp => kvp.Key >= i).OrderBy(kvp => kvp.Key))
            {
                yield return value;
            }
        }
    }

    public int Add(object? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        int index = ItemsSource.Add(value);
        return ToOuterIndex(index);
    }

    public void Clear()
    {
        ItemsSource.Clear();
        interspersedObjects.Clear();
    }

    public bool Contains(object? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return interspersedObjects.ContainsValue(value) || ItemsSource.Contains(value);
    }

    public int IndexOf(object? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (ItemKeySearch(value, out int key))
        {
            return key;
        }

        int index = ItemsSource.IndexOf(value);

        return index == -1 ? -1 : ToOuterIndex(index);
    }

    public void Remove(object? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (ItemKeySearch(value, out int key))
        {
            interspersedObjects.Remove(key);

            MoveKeysBackward(key, 1);

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value, key));

            return;
        }

        ItemsSource.Remove(value);
    }

    public void RemoveAt(int index)
    {
        throw new NotSupportedException();
    }

    public void CopyTo(Array array, int index)
    {
        throw new NotSupportedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void OnCollectionChanged(object? source, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
                ArgumentNullException.ThrowIfNull(eventArgs.NewItems);
                int count = eventArgs.NewItems.Count;

                if (count > 0)
                {
                    if (!isInsertingOriginal)
                    {
                        MoveKeysForward(eventArgs.NewStartingIndex, count);
                    }

                    isInsertingOriginal = false;

                    CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, eventArgs.NewItems, ToOuterIndex(eventArgs.NewStartingIndex)));
                }

                break;
            case NotifyCollectionChangedAction.Remove:
                ArgumentNullException.ThrowIfNull(eventArgs.OldItems);
                count = eventArgs.OldItems.Count;

                if (count > 0)
                {
                    int outerIndex = ToOuterIndexAfterRemoval(eventArgs.OldStartingIndex);

                    MoveKeysBackward(outerIndex, count);

                    CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, eventArgs.OldItems, outerIndex));
                }

                break;
            case NotifyCollectionChangedAction.Reset:

                ReadjustKeys();

                CollectionChanged?.Invoke(this, eventArgs);
                break;
        }
    }

    private void MoveKeysForward(int pivot, int amount)
    {
        foreach (int key in interspersedObjects.Keys.OrderByDescending(v => v))
        {
            if (key < pivot)
            {
                break;
            }

            interspersedObjects[key + amount] = interspersedObjects[key];
            interspersedObjects.Remove(key);
        }
    }

    private void MoveKeysBackward(int pivot, int amount)
    {
        foreach (int key in interspersedObjects.Keys.OrderBy(v => v))
        {
            if (key <= pivot)
            {
                continue;
            }

            interspersedObjects[key - amount] = interspersedObjects[key];
            interspersedObjects.Remove(key);
        }
    }

    private void ReadjustKeys()
    {
        int count = ItemsSource.Count;
        int existing = 0;

        foreach (int key in interspersedObjects.Keys.OrderBy(v => v))
        {
            if (key <= count)
            {
                existing++;
                continue;
            }

            interspersedObjects[count + existing++] = interspersedObjects[key];
            interspersedObjects.Remove(key);
        }
    }

    private int ToInnerIndex(int outerIndex)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(outerIndex, Count);

        if (interspersedObjects.ContainsKey(outerIndex))
        {
            throw new ArgumentException("The outer index can't be inserted as a key to the original collection.");
        }

        return outerIndex - interspersedObjects.Keys.Count(key => key <= outerIndex);
    }

    private int ToOuterIndex(int innerIndex)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(innerIndex, ItemsSource.Count);

        foreach ((int key, object _) in interspersedObjects.OrderBy(v => v.Key))
        {
            if (innerIndex >= key)
            {
                innerIndex++;
                continue;
            }

            break;
        }

        return innerIndex;
    }

    private int ToOuterIndexAfterRemoval(int innerIndexToProject)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(innerIndexToProject, ItemsSource.Count + 1);

        foreach ((int key, object _) in interspersedObjects.OrderBy(v => v.Key))
        {
            if (innerIndexToProject >= key)
            {
                innerIndexToProject++;
                continue;
            }

            break;
        }

        return innerIndexToProject;
    }

    private bool ItemKeySearch(object value, out int key)
    {
        if (interspersedObjects.ContainsValue(value))
        {
            key = value is null
                ? interspersedObjects.First(kvp => kvp.Value is null).Key
                : interspersedObjects.First(kvp => kvp.Value.Equals(value)).Key;

            return true;
        }

        key = default;
        return false;
    }
}
