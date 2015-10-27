﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ClassLibrary.Models
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
                if (myItemsPanel != null) myItemsPanel.ItemWidth = (screenWidth / Screen.GetNumberOfColumns(screenWidth, itemWidth, minItemWidth));
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
    }
}
