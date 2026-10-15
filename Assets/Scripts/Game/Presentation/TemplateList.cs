using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>
    /// Rows cloned from an inactive template beside it, kept between updates so a list that changes size reuses its
    /// rows. The template's parent usually has a layout group that places them.
    /// </summary>
    public sealed class TemplateList<T> where T : Component
    {
        private readonly T _template;
        private readonly List<T> _items = new List<T>();

        public TemplateList(T template)
        {
            _template = template != null ? template : throw new ArgumentNullException(nameof(template));
            _template.gameObject.SetActive(false);
        }

        /// <summary>Makes exactly <paramref name="count"/> rows, in order after the template.</summary>
        public IReadOnlyList<T> Show(int count)
        {
            while (_items.Count > count)
            {
                Object.Destroy(_items[_items.Count - 1].gameObject);
                _items.RemoveAt(_items.Count - 1);
            }

            while (_items.Count < count)
            {
                T item = Object.Instantiate(_template, _template.transform.parent);
                item.gameObject.SetActive(true);
                _items.Add(item);
            }

            return _items;
        }
    }
}
