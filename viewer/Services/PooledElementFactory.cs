using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Backpack.Viewer.Services;

/// <summary>
/// 元素池工厂 — 复用 DataTemplate inflate 的 XAML 元素，避免每次滚动重建元素树的开销。
/// 实现 <see cref="IElementFactory"/>，等效于 GridView / ListView 的内置元素回收机制。
/// </summary>
internal sealed class PooledElementFactory : IElementFactory
{
    private readonly DataTemplate     _template;
    private readonly Stack<UIElement>  _pool = new();
    private readonly ConditionalWeakTable<UIElement, object> _owned = new();
    private readonly bool              _clearDataContext;

    /// <param name="clearDataContextOnRecycle">
    /// <c>true</c>（默认）：回收时将 DataContext 清为 null，适合 {Binding} 模板。
    /// <c>false</c>：保留旧 DataContext，适合 {x:Bind} 编译绑定模板（避免 null 导致生成代码抛 NRE）。
    /// </param>
    public PooledElementFactory(DataTemplate template, bool clearDataContextOnRecycle = true)
    {
        _template         = template;
        _clearDataContext = clearDataContextOnRecycle;
    }

    /// <summary>
    /// 从池中取出复用元素，池为空时从 DataTemplate inflate 新元素。
    /// </summary>
    public UIElement GetElement(ElementFactoryGetArgs args)
    {
        if (_pool.Count > 0)
            return _pool.Pop();
        var element = (UIElement)_template.LoadContent();
        _owned.Add(element, this);
        return element;
    }

    /// <summary>
    /// 将元素归还池中；根据构造参数决定是否清空 DataContext。
    /// </summary>
    public void RecycleElement(ElementFactoryRecycleArgs args)
    {
        if (!_owned.TryGetValue(args.Element, out _))
            return;
        if (_clearDataContext && args.Element is FrameworkElement fe)
            fe.DataContext = null;
        _pool.Push(args.Element);
    }
}
