using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ClassLibrary.Common
{
    public class Screen
    {
        public static void MakeResponsive(SizeChangedEventArgs e, int itemWidth,int minItemWidth, GridView gridView)
        {
            ItemsWrapGrid myItemsPanel = (ItemsWrapGrid)gridView.ItemsPanelRoot;
            double screenWidth = e.NewSize.Width;
            int? itemsNumber = gridView.Items?.Count;
            if (itemsNumber > 0)
            {
                if (myItemsPanel != null) myItemsPanel.ItemWidth = (screenWidth / GetNumberOfColumns(screenWidth, itemWidth, minItemWidth));
            }
        }

        private static int GetNumberOfColumns(double screenWidth, int itemWidth, int minItemWidth)
        {
            int w = minItemWidth-1;
            int c = 1;
            while (true)
            {
                if (screenWidth <= w)
                {
                    return c;
                }
                c++;
                w += itemWidth;
            }
        }

        private static int GetNumberOfRows(double screenHeight, int itemWidth, int minItemWidth)
        {
            int w = minItemWidth - 1;
            int c = 1;
            while (true)
            {
                if (screenHeight <= w)
                {
                    return c;
                }
                c++;
                w += itemWidth;
            }
        }

        public static int getLimitItems(double screenHeight, double screenWidth, int itemWidth, int minItemWidth, int itemHeight, int minItemHeight)
        {
            int cols = GetNumberOfColumns(screenWidth, itemWidth, minItemWidth);
            int rows = GetNumberOfRows(screenHeight, itemHeight, minItemHeight);
            return (int)((cols*rows)*1.5);
        }
    }
}
