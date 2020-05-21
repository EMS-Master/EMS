﻿using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Wires;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using UI.ViewModel;
using Xceed.Wpf.Toolkit;

namespace UI.View
{
    /// <summary>
    /// Interaction logic for MapView.xaml
    /// </summary>
    public partial class MapView : UserControl
    {
        public List<BatteryStorage> BatteryStorageList = new List<BatteryStorage>();
        public List<Generator> GeneratorList = new List<Generator>();

        public MapView()
        {
            DataContext = new MapViewModel(this);
            InitializeComponent();
            LoadMap();
            LoadXML();

        }

        private void LoadMap()
        {
            GMapProvider.WebProxy = WebRequest.GetSystemWebProxy();
            GMapProvider.WebProxy.Credentials = CredentialCache.DefaultNetworkCredentials;

            mapa.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;

            double blX = 45.2325;
            double blY = 19.793909;
            double trX = 45.277031;
            double trY = 19.894459;
            mapa.Position = new PointLatLng((blX + trX) / 2, (blY + trY) / 2);

            mapa.ShowCenter = false;
            mapa.MinZoom = 2;
            mapa.MaxZoom = 18;
            mapa.Zoom = 13;
        }

        public void LoadXML()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("C:/Users/ASUS/Desktop/New folder (2)/EMS/ModelLabs/GMap/Geographic.xml");

            XmlNodeList BatteryStorageNode = doc.GetElementsByTagName("BatteryStorage");
            foreach (XmlNode item in BatteryStorageNode)
            {
                
                float maxPower = float.Parse(item["MaxPower"].InnerText);
                float minCapacity = float.Parse(item["MinCapacity"].InnerText);
                float x = float.Parse(item["X"].InnerText);
                float y = float.Parse(item["Y"].InnerText);

                BatteryStorage bs = new BatteryStorage(maxPower, minCapacity, x, y);

                BatteryStorageList.Add(bs);
            }
            XmlNodeList GeneratorNode = doc.GetElementsByTagName("Generator");
            foreach (XmlNode item in GeneratorNode)
            {

                float maxPower = float.Parse(item["MaxQ"].InnerText);
                float minPower = float.Parse(item["MinQ"].InnerText);
                GeneratorType generatorType;
                Enum.TryParse(item["GeneratorType"].InnerText, out generatorType);
                float x = float.Parse(item["X"].InnerText);
                float y = float.Parse(item["Y"].InnerText);

                Generator g = new Generator(0, minPower, maxPower, generatorType, x, y);

                GeneratorList.Add(g);
            }

        }

        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }

        private void LoadBatteryStorage()
        {
            foreach (var item in BatteryStorageList)
            {
                double X;
                double Y;
                ToLatLon(item.X, item.Y, 34, out X, out Y);

                GMapOverlay markersOverlay = new GMapOverlay("markersGreen");
                GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(X, Y), GMarkerGoogleType.green);
                marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                marker.ToolTipText = "maxPower: " + item.MaxPower + " minCapacity: " + item.MinCapacity;

                markersOverlay.Markers.Add(marker);
                mapa.Overlays.Add(markersOverlay);
            }
        }
        private void LoadGenerators()
        {
            foreach (var item in GeneratorList)
            {
                double X;
                double Y;
                ToLatLon(item.X, item.Y, 34, out X, out Y);

                GMapOverlay markersOverlay = new GMapOverlay("markersBlue");
                GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(X, Y), GMarkerGoogleType.blue);
                marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                marker.ToolTipText = "maxPower: " + item.MaxQ + " minPower: " + item.MinQ + "Type" + item.GeneratorType;

                markersOverlay.Markers.Add(marker);
                mapa.Overlays.Add(markersOverlay);
            }
        }

        private void CheckBox1_Checked(object sender, RoutedEventArgs e)
        {
            LoadBatteryStorage();
        }
        private void CheckBox2_Checked(object sender, RoutedEventArgs e)
        {
            LoadGenerators();
        }


        private void CheckBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            GMap.NET.ObjectModel.ObservableCollectionThreadSafe<GMapOverlay> list = new GMap.NET.ObjectModel.ObservableCollectionThreadSafe<GMapOverlay>();

            foreach (var item in mapa.Overlays)
            {
                if (item.Id == "markersGreen")
                    list.Add(item);
            }

            for (int i = 0; i < list.Count; i++)
            {
                mapa.Overlays.Remove(list[i]);
            }
        }
        private void CheckBox2_Unchecked(object sender, RoutedEventArgs e)
        {
            GMap.NET.ObjectModel.ObservableCollectionThreadSafe<GMapOverlay> list = new GMap.NET.ObjectModel.ObservableCollectionThreadSafe<GMapOverlay>();

            foreach (var item in mapa.Overlays)
            {
                if (item.Id == "markersBlue")
                    list.Add(item);
            }

            for (int i = 0; i < list.Count; i++)
            {
                mapa.Overlays.Remove(list[i]);
            }
        }
    }
}
