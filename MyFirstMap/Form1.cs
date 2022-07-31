using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThinkGeo.Core;

namespace MyFirstMap
{
    public partial class Form1 : Form
    {
        InMemoryFeatureLayer inMemoryFeatureLayer;
        PopupOverlay popupOverlay;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            ProjectionConverter projectionConverter = new ProjectionConverter(4326, 900913);
            projectionConverter.Open();


            mapView.CurrentExtent = MaxExtents.BingMaps;
            BingMapsOverlay bingMapsOverlay = new BingMapsOverlay("AnJQit0BPR7vlanLKgsmLy43yj_WxNMtyQJJJF7QD2OJ_STvHNy4w1CaGr3ZeI3w", BingMapsMapType.AerialWithLabels);
            mapView.Overlays.Add(bingMapsOverlay);


            LayerOverlay layerOverlay = new LayerOverlay();
            inMemoryFeatureLayer = new InMemoryFeatureLayer();
            inMemoryFeatureLayer.ZoomLevelSet.ZoomLevel01.DefaultPointStyle = new PointStyle(PointSymbolType.Circle, 12, GeoBrushes.Red, GeoPens.White);
            inMemoryFeatureLayer.ZoomLevelSet.ZoomLevel01.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;

            layerOverlay.Layers.Add(inMemoryFeatureLayer);
            mapView.Overlays.Add(layerOverlay);


            popupOverlay = new PopupOverlay();
            mapView.Overlays.Add(popupOverlay);




            string[] lines = System.IO.File.ReadAllLines("sample.csv");
            for (int i = 1; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(new char[] { ',' });
                double longitude = double.Parse(parts[0]);
                double latitude = double.Parse(parts[1]);

                Dictionary<string, string> items = new Dictionary<string, string>();
                items.Add("Name", parts[2]);
                items.Add("Address", parts[3]);
                Feature feature = new Feature(longitude, latitude, i.ToString(),  items);
                inMemoryFeatureLayer.InternalFeatures.Add(projectionConverter.ConvertToExternalProjection(feature));
            }
            inMemoryFeatureLayer.Open();
            mapView.CurrentExtent = inMemoryFeatureLayer.GetBoundingBox();

        }

        private void mapView_MapClick(object sender, MapClickMapViewEventArgs e)
        {
           popupOverlay.Popups.Clear();
           Collection<Feature> features = inMemoryFeatureLayer.QueryTools.GetFeaturesNearestTo(e.WorldLocation, GeographyUnit.Meter, 1, ReturningColumnsType.AllColumns);


            if (features.Count > 0)
            {
                var distance = MapUtil.GetScreenDistanceBetweenTwoWorldPoints(mapView.CurrentExtent, new Feature(e.WorldLocation), features[0], mapView.Width, mapView.Height);

                if (distance < 15)
                {
                    var pop = new Popup((PointShape)features[0].GetShape())
                    {
                        Content =  $"Name: {features[0].ColumnValues["name"].ToString()}{Environment.NewLine}Address:{features[0].ColumnValues["address"]}"
                    };
                    popupOverlay.Popups.Add(pop);
                }
            }

            popupOverlay.Refresh();
        }
    }
}
