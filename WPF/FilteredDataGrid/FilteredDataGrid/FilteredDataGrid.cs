using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace FilteredDataGridProject
{
    public class FilteredDataGridProject : DataGrid
    {
        private List<string> _keys = new List<string>();
        private readonly Dictionary<string, string> _columnFilters;
        private readonly Dictionary<string, Binding> _columnBindings;
        private readonly Dictionary<string, TextBox> _columnFilterTextBox;

        /// <summary>
        /// Cached die Properties: Performancesteigerung
        /// </summary>
        private readonly Dictionary<string, PropertyInfo> _propertyCache;

        private ICollectionView _collectionView;

        public static DependencyProperty IsResetFilterProperty = DependencyProperty.Register("IsResetFilter", typeof(bool), typeof(FilteredDataGridProject), new PropertyMetadata(false, IsResetFilterCallback));

        private static void IsResetFilterCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as FilteredDataGridProject;
            if (grid != null && ((bool)e.NewValue))
            {
                grid.resetFilter();
                grid.IsResetFilter = false;
            }
        }

        public bool IsResetFilter
        {
            get { return (bool)(GetValue(IsResetFilterProperty)); }
            set { SetValue(IsResetFilterProperty, value); }
        }

        /// <summary>
        /// Case Sensitive Filterung
        /// </summary>
        public static DependencyProperty IsFilteringCaseSensitiveProperty = DependencyProperty.Register("IsFilteringCaseSensitive", typeof(bool), typeof(FilteredDataGridProject), new PropertyMetadata(true));

        public bool IsFilteringCaseSensitive
        {
            get { return (bool)(GetValue(IsFilteringCaseSensitiveProperty)); }
            set { SetValue(IsFilteringCaseSensitiveProperty, value); }
        }

        /// <summary>
        /// Gefilterte ItemsSource
        /// </summary>
        public static DependencyProperty FilteredItemsSourceProperty
             = DependencyProperty.Register("FilteredItemsSource", typeof(List<object>), typeof(FilteredDataGridProject),
                 new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

        public List<object> FilteredItemsSource
        {
            get { return (List<object>)GetValue(FilteredItemsSourceProperty); }
            set { SetValue(FilteredItemsSourceProperty, value); }
        }

        private void updateFilteredCollection()
        {
            FilteredItemsSource = _collectionView?.Cast<object>().ToList();
        }

        public FilteredDataGridProject()
        {
            _columnFilters = new Dictionary<string, string>();
            _columnBindings = new Dictionary<string, Binding>();
            _propertyCache = new Dictionary<string, PropertyInfo>();
            _columnFilterTextBox = new Dictionary<string, TextBox>();

            AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(OnTextChanged), true);
            AddHandler(TextBoxBase.KeyDownEvent, new KeyEventHandler(OnKeyDownTextBox), true);

            DataContextChanged += new DependencyPropertyChangedEventHandler(filteringDataGridDataContextChanged);
        }

        private void resetFilter()
        {
            foreach (var key in _keys)
            {
                _columnFilterTextBox[key].Text = string.Empty;
            }

            _keys.Clear();
            _columnFilterTextBox.Clear();
            _columnFilters.Clear();
            _columnBindings.Clear();
            _propertyCache.Clear();

            applyFilters();
            scrollIntoView(CurrentColumn);
        }

        private void filteringDataGridDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _propertyCache.Clear();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            scrollIntoView();
            focusCellBySelectedItem();
        }

        private void OnKeyDownTextBox(object sender, KeyEventArgs e)
        {
            TextBox filterTextBox = e.OriginalSource as TextBox;
            DataGridColumnHeader header = tryFindParent<DataGridColumnHeader>(filterTextBox);

            if (header != null && e.Key == Key.Down)
            {
                if (Items.Count > 0)
                {
                    focusSelectedItem();
                    focusCellBySelectedItem(header.DisplayIndex);

                    if (SelectedItem == null)
                    {
                        var iterator = ItemsSource.GetEnumerator();
                        iterator.MoveNext();

                        if (iterator.Current != null)
                        {
                            SelectedIndex = 0;

                            focusSelectedItem();
                            focusCellBySelectedItem(header.DisplayIndex);
                        }
                    }
                }
            }
            else if (e.Key == Key.Tab && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                if (header != null && header.DisplayIndex > 0 && Columns.Count() > header.DisplayIndex)
                {
                    scrollIntoView(header.DisplayIndex - 1);
                }
                else if (CurrentCell != null && CurrentCell.Column?.DisplayIndex == 0)
                {
                    scrollIntoView(Columns.Count() - 1);
                }
            }
            else if (e.Key == Key.Tab)
            {
                if (header != null && header.DisplayIndex >= 0 && Columns.Count() > header.DisplayIndex + 1)
                {
                    scrollIntoView(header.DisplayIndex + 1);
                }
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox filterTextBox = e.OriginalSource as TextBox;
            DataGridColumnHeader header = tryFindParent<DataGridColumnHeader>(filterTextBox);

            if (header != null)
            {
                updateFilter(filterTextBox, header);
                applyFilters();
            }
        }

        /// <summary>
        /// Wird aufgerufen, sobald ein Item hinzugefügt oder entfernt wird.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (ItemsSource != null)
            {
                _collectionView = CollectionViewSource.GetDefaultView(ItemsSource);
            }

            updateFilteredCollection();
            scrollIntoView(CurrentColumn);
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            applyFilters();
            scrollIntoView(CurrentColumn);
        }

        /// <summary>
        /// Aktualisiert den aktuellen internen Filter
        /// </summary>
        private void updateFilter(TextBox textBox, DataGridColumnHeader header)
        {
            string columnBindingPath = header.Column.SortMemberPath ?? string.Empty;

            if (!string.IsNullOrEmpty(columnBindingPath))
            {
                _columnFilterTextBox[columnBindingPath] = textBox;
                _columnFilters[columnBindingPath] = textBox.Text;
                _columnBindings[columnBindingPath] = (header.Column as DataGridBoundColumn)?.Binding as Binding;
            }

            _keys = new List<string>(_columnFilterTextBox.Keys);
        }

        /// <summary>
        /// Filter wird angewendet
        /// </summary>
        private void applyFilters()
        {
            _collectionView = CollectionViewSource.GetDefaultView(ItemsSource);
            if (_collectionView != null)
            {
                _collectionView.Filter = delegate (object item)
                {
                    bool show = true;

                    foreach (KeyValuePair<string, string> filter in _columnFilters)
                    {
                        var propertyValue = getPropertyValue(item, filter.Key);
                        var binding = _columnBindings[filter.Key];
                        if (binding?.Converter != null)
                        {
                            propertyValue = binding.Converter.Convert(propertyValue, typeof(string), binding.ConverterParameter, System.Globalization.CultureInfo.CurrentCulture);
                        }

                        bool hasFilterInput = !string.IsNullOrWhiteSpace(filter.Value);

                        if (hasFilterInput)
                        {
                            var containsFilter = IsTextSearchCaseSensitive ?
                                !string.IsNullOrWhiteSpace(propertyValue?.ToString()) && propertyValue.ToString().Contains(filter.Value) :
                                !string.IsNullOrWhiteSpace(propertyValue?.ToString()) && propertyValue.ToString().ToLower().Contains(filter.Value.ToLower());

                            if (!containsFilter)
                            {
                                show = false;
                                break;
                            }
                        }
                    }

                    return show;
                };
            }
        }

        private void scrollIntoView(DataGridColumn column = null)
        {
            if (SelectedItem != null)
            {
                DataGridRow row = (DataGridRow)ItemContainerGenerator.ContainerFromIndex(SelectedIndex);

                UpdateLayout();
                ScrollIntoView(SelectedItem, column);
            }
        }

        private void scrollIntoView(int columnIndex)
        {
            if (SelectedItem != null)
            {
                if (columnIndex >= 0)
                {
                    var cellcontent = Columns[columnIndex]?.GetCellContent(SelectedItem);
                    var cell = cellcontent?.Parent as DataGridCell;
                    if (cell != null)
                    {
                        scrollIntoView(cell.Column);
                    }
                }
            }
        }

        private void focusSelectedItem()
        {
            if (SelectedItem != null)
            {
                DataGridRow row = (DataGridRow)ItemContainerGenerator.ContainerFromIndex(SelectedIndex);

                if (row == null)
                {
                    row = ItemContainerGenerator.ContainerFromIndex(SelectedIndex) as DataGridRow;
                }

                row?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void focusCellBySelectedItem()
        {
            if (CurrentColumn == null && SelectedItem != null)
            {
                Columns[0].DisplayIndex = 0;
                focusCellBySelectedItem(0);
            }
            else if (CurrentColumn != null && CurrentColumn.DisplayIndex >= 0)
            {
                focusCellBySelectedItem(CurrentColumn.DisplayIndex);
            }
        }

        private void focusCellBySelectedItem(int columnIndex = -1)
        {
            if (SelectedItem != null)
            {
                if (columnIndex >= 0)
                {
                    var cellcontent = Columns[columnIndex]?.GetCellContent(SelectedItem);
                    var cell = cellcontent?.Parent as DataGridCell;
                    if (cell != null)
                    {
                        cell.Focus();

                        scrollIntoView(CurrentColumn);
                    }
                }
            }
        }

        private object getPropertyValue(object item, string propertyPath)
        {
            if (string.IsNullOrEmpty(propertyPath))
                return null;

            string[] Splitter = { "." };
            string[] SourceProperties = propertyPath.Split(Splitter, StringSplitOptions.None);
            var propertyInfo = item?.GetType()?.GetProperty(SourceProperties[0]);
            var propertyValue = propertyInfo?.GetValue(item, null);

            var propertyType = propertyInfo?.PropertyType;
            for (int x = 1; x < SourceProperties.Length; ++x)
            {
                propertyInfo = propertyType?.GetProperty(SourceProperties[x]);
                propertyType = propertyInfo?.PropertyType;
                propertyValue = propertyInfo?.GetValue(propertyValue, null);
            }

            return propertyValue;
        }

        private static T tryFindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = getParentObject(child);

            if (parentObject == null)
                return null;

            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                return tryFindParent<T>(parentObject);
            }
        }

        private static DependencyObject getParentObject(DependencyObject child)
        {
            if (child == null) return null;
            ContentElement contentElement = child as ContentElement;

            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce?.Parent;
            }

            return VisualTreeHelper.GetParent(child);
        }
    }
}
