using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PunchPal.WPF.Converters
{
    public static class ListScrollHelper
    {
        public static readonly DependencyProperty ScrollToItemProperty =
            DependencyProperty.RegisterAttached(
                "ScrollToItem",
                typeof(bool),
                typeof(ListScrollHelper),
                new PropertyMetadata(false, OnScrollToItemChanged));

        public static bool GetScrollToItem(DependencyObject obj)
        {
            return (bool)obj.GetValue(ScrollToItemProperty);
        }

        public static void SetScrollToItem(DependencyObject obj, bool value)
        {
            obj.SetValue(ScrollToItemProperty, value);
        }

        private static void OnScrollToItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListBox listView)
            {
                if ((bool)e.NewValue)
                {
                    listView.SelectionChanged += ListView_SelectionChanged;
                }
                else
                {
                    listView.SelectionChanged -= ListView_SelectionChanged;
                }
            }
        }

        private static void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem != null)
            {
                listBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    listBox.UpdateLayout();
                    listBox.ScrollIntoView(listBox.SelectedItem);

                    // 可选：更强制地确保项滚入视图
                    var container = listBox.ItemContainerGenerator.ContainerFromItem(listBox.SelectedItem) as FrameworkElement;
                    container?.BringIntoView();
                }), DispatcherPriority.Background);
            }
        }
    }
}
