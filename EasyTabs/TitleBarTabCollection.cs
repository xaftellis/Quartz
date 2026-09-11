using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTabs
{
    /// <summary>Enforces the pinned boundary for new tabs and cross-window drops.</summary>
    internal sealed class TitleBarTabCollection : ListWithEvents<TitleBarTab>
    {
        public override void Add(TitleBarTab tab) => Insert(Count, tab);

        public override void Insert(int index, TitleBarTab tab)
        {
            if (tab == null) throw new ArgumentNullException(nameof(tab));
            if (index < 0 || index > Count) throw new ArgumentOutOfRangeException(nameof(index));
            int boundary = this.TakeWhile(item => item.IsPinned).Count();
            base.Insert(tab.IsPinned ? Math.Min(index, boundary) : Math.Max(index, boundary), tab);
        }

        public override void InsertRange(int index, IEnumerable<TitleBarTab> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (index < 0 || index > Count) throw new ArgumentOutOfRangeException(nameof(index));
            // Each event carries the actual insertion index, including mixed ranges.
            foreach (TitleBarTab tab in collection.ToArray())
            {
                Insert(index, tab);
                index = IndexOf(tab) + 1;
            }
        }
    }
}
