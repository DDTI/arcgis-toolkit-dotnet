using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Toolkit.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace ArcGISRuntime.Toolkit.Samples.Desktop.Legend
{
    /// <summary>
    /// Demonstrates how to show layers in a treeview and show legend for each layer on the map view.
    /// </summary>
    /// <title>Legends in Treeview</title>
    /// <category>Toolkit</category>
    /// <subcategory>Legend</subcategory>
    /// <usesoffline>false</usesoffline>
    /// <usesonline>true</usesonline>
    public partial class LegendsInTreeViewSample : UserControl
    {
        private ObservableCollection<int> _visibleLayers = new ObservableCollection<int> { 0, 1, 2 };

        public LegendsInTreeViewSample()
        {
            InitializeComponent();
        }

        private void MyMapView_OnLayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            // Zoom to water network
            var layer = e.Layer as ArcGISDynamicMapServiceLayer;
            if (layer != null)
            {
                var extent = layer.ServiceInfo.InitialExtent ?? layer.ServiceInfo.InitialExtent;
                if (extent != null)
                {
                    // If extent is not in the same spatial reference than map, reproject it
                    if (!SpatialReference.Equals(extent.SpatialReference, MyMapView.SpatialReference))
                        extent = GeometryEngine.Project(extent, MyMapView.SpatialReference) as Envelope;
                    if (extent != null)
                    {
                        extent = extent.Expand(0.5);
                        MyMapView.SetView(extent);
                    }
                }
            }
        }

        private void CheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var chk = sender as CheckBox;
            var ctx = chk.DataContext as LayerItemViewModel;
            var id = ctx.SubLayerID;
            var lyr = MyMapView.Map.Layers[1] as ArcGISDynamicMapServiceLayer;
            
            if (chk.IsChecked == true)
            {
                _visibleLayers.Add(id);
            }
            else
            {
                _visibleLayers.Remove(id);
            }

            lyr.VisibleLayers = _visibleLayers;
        }

        private void LayerExpander_Toggled(object sender, System.Windows.RoutedEventArgs e)
        {
            var layerExpander = sender as Expander;
            var item = layerExpander.DataContext as LayerItemViewModel;
            var subItems = item.LayerItemsSource;
            foreach(var i in subItems)
            {
                var label = i.Label;
                var subItemElement = FindChild<Button>(this, label);
                if(subItemElement != null)
                {
                    subItemElement.Visibility = layerExpander.IsExpanded ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            

        }

        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childTag">x:Tag (string) associated with child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public static T FindChild<T>(DependencyObject parent, string childTag)
           where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childTag);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childTag))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's tag is set for search
                    if (frameworkElement != null && frameworkElement.Tag.ToString() == childTag)
                    {
                        // if the child's tag is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
    }


    public class EnumeratorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Esri.ArcGISRuntime.Layers.Layer && targetType == typeof(IEnumerable<Esri.ArcGISRuntime.Layers.Layer>))
            {
                return new Esri.ArcGISRuntime.Layers.Layer[] { (Esri.ArcGISRuntime.Layers.Layer)value };
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
