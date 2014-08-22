using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Codeplex.Reactive;
using Codeplex.Reactive.Extensions;
using MonadicIT.Visual.Backbone;

namespace MonadicIT.Visual.ViewModels
{
    public class SelectorViewModel<TSelectable> where TSelectable : ISelectable
    {
        private readonly IList<TSelectable> _items;

        public IList<TSelectable> Items { get { return _items; } }
        public ReactiveProperty<TSelectable> SelectedItem { get; private set; }

        public SelectorViewModel(IEnumerable<TSelectable> viewModels)
        {
            _items = viewModels.ToList();

            var activeItemStreams = from dvm in _items
                                    let selectedItem = from b in dvm.ObserveProperty(x => x.IsSelected)
                                                       where b
                                                       select dvm
                                    select selectedItem;
            SelectedItem = activeItemStreams.Merge().ToReactiveProperty();

            _items.First().IsSelected = true;        
        }
    }
}