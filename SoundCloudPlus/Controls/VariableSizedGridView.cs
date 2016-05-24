using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using ClassLibrary.Models;

namespace SoundCloudPlus.Controls
{
    public class VariableSizedGridView : GridView
    {
        public static readonly DependencyProperty IsResponsiveProperty = DependencyProperty.Register(
               "IsResponsive", typeof(bool), typeof(VariableSizedGridView), new PropertyMetadata(default(bool), IsResponsiveChangedCallback));

        private static void IsResponsiveChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            bool isResponsive = (bool)dependencyPropertyChangedEventArgs.NewValue;
            VariableSizedGridView thisGridView = (VariableSizedGridView)dependencyObject;
            if (isResponsive)
            {
                thisGridView.SizeChanged += MainPageGridView_SizeChanged;
            }
        }

        private static void MainPageGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MakeResponsive(e, 400, (VariableSizedGridView)sender);
        }

        private static void MakeResponsive(SizeChangedEventArgs e, int minItemWidth, GridView gridView)
        {
            VariableSizedWrapGrid myItemsPanel = (VariableSizedWrapGrid)gridView.ItemsPanelRoot;
            double screenWidth = e.NewSize.Width;
            int? itemsNumber = gridView.Items?.Count;
            if (itemsNumber > 0)
            {
                if (myItemsPanel != null)
                {
                    myItemsPanel.ItemWidth = (screenWidth / GetNumberOfColumns(screenWidth, minItemWidth));
                }
            }
        }

        private static int GetNumberOfColumns(double screenWidth, int minItemWidth)
        {
            int w = minItemWidth - 1;
            int c = 1;
            while (true)
            {
                if (screenWidth <= (w + minItemWidth))
                {
                    return c;
                }
                c++;
                w += minItemWidth;
            }
        }

        public bool IsResponsive
        {
            get { return (bool)GetValue(IsResponsiveProperty); }
            set { SetValue(IsResponsiveProperty, value); }
        }
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            if (item.GetType() == typeof(StreamCollection))
            {
                var collection = item as StreamCollection;
                if (collection != null)
                {
                    StreamCollection obj = collection;
                    element.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, 1);
                    if (obj.Playlist == null)
                    {
                        element.SetValue(VariableSizedWrapGrid.RowSpanProperty, 1);
                    }
                    else if (obj.Track == null)
                    {
                        element.SetValue(VariableSizedWrapGrid.RowSpanProperty, 2);
                    }
                    element.SetValue(VerticalContentAlignmentProperty, VerticalAlignment.Stretch);
                    element.SetValue(HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
                    base.PrepareContainerForItemOverride(element, collection);
                }
            }else if (item.GetType() == typeof(SearchCollection))
            {
                var collection = item as SearchCollection;
                if (collection != null)
                {
                    SearchCollection obj = collection;
                    element.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, 1);
                    if (obj.Playlist != null)
                    {
                        element.SetValue(VariableSizedWrapGrid.RowSpanProperty, 2);
                    }
                    else if (obj.Track != null)
                    {
                        element.SetValue(VariableSizedWrapGrid.RowSpanProperty, 1);
                    }
                    else if (obj.User != null)
                    {
                        element.SetValue(VariableSizedWrapGrid.RowSpanProperty, 1);
                    }
                    element.SetValue(VerticalContentAlignmentProperty, VerticalAlignment.Stretch);
                    element.SetValue(HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
                    base.PrepareContainerForItemOverride(element, collection);
                }
            }
            else if (item.GetType() == typeof(User))
            {
                var collection = item as User;
                if (collection != null)
                {
                    User obj = collection;
                    element.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, 1);
                    element.SetValue(VariableSizedWrapGrid.RowSpanProperty, 1);

                    element.SetValue(VerticalContentAlignmentProperty, VerticalAlignment.Stretch);
                    element.SetValue(HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
                    base.PrepareContainerForItemOverride(element, collection);
                }
            }
        }

        // refresh the variablesizedwrapgrid layout
        public void Update()
        {
            if (!(ItemsPanelRoot is VariableSizedWrapGrid))
                throw new ArgumentException("ItemsPanel is not VariableSizedWrapGrid");

            foreach (var container in ItemsPanelRoot.Children.Cast<GridViewItem>())
            {
                dynamic data = container.Content;
                VariableSizedWrapGrid.SetRowSpan(container, data.RowSpan);
                VariableSizedWrapGrid.SetColumnSpan(container, data.ColSpan);
            }

            ItemsPanelRoot.InvalidateMeasure();
        }
    }
}
