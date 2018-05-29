using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;

namespace PhysikLaborSatellit
{
	internal static class DataExport
	{
		internal static void ExportAsText(Window1 window, string filepath) => new StreamWriter(filepath, false, Encoding.UTF8).Write(
$@"Satelliten-Ausrichtungs-Rechner
Copyright © 2018 Marcel Nowak und Sidney Kuyateh


Ihre eingegebenen Daten:
Antennenstandort: {window.latText.Text}° {((window.longEast.IsChecked == true) ? "E" : "W")}, {window.longText.Text}° {((window.longEast.IsChecked == true) ? "E" : "W")}
Satellitenstandort: {window.longSatText.Text}° {((window.longSatEast.IsChecked == true) ? "E" : "W")}


Berechnete Daten:
Azimutwinkel ψ: {window.azimutText.Text}, Elevationswinkel α: {window.elevationText.Text}, Deklinationswinkel δ: {window.declinationText.Text}

Elevationskurventabelle:
Azimutwinkel        Elevationswinkel    Deklinationswinkel
{string.Concat(window.rows.SelectMany((item) => new string[] { string.Format("{0,-20}{1,-20}{2}\r\n", item.Azimut, item.Elevation, item.Deklination) }))}");

		internal static void ExportAsCSV(Window1 window, string filepath) => new StreamWriter(filepath, false, Encoding.UTF8).Write(
$@"Satelliten-Ausrichtungs-Rechner
Copyright © 2018 Marcel Nowak und Sidney Kuyateh

Eingegebene Daten
Antennenstandort;{window.latText.Text}° {((window.longEast.IsChecked == true) ? "E" : "W")};{window.longText.Text}° {((window.longEast.IsChecked == true) ? "E" : "W")}
Satellitenstandort;{window.longSatText.Text}° {((window.longSatEast.IsChecked == true) ? "E" : "W")}

Berechnete Daten
Azimutwinkel ψ;Elevationswinkel α;Deklinationswinkel δ
{window.azimutText.Text};{window.elevationText.Text};{window.declinationText.Text}

Elevationskurventabelle
Azimutwinkel;Elevationswinkel;Deklinationswinkel
{string.Concat(window.rows.SelectMany((item) => new string[] { string.Format("{0};{1};{2}\r\n", item.Azimut, item.Elevation, item.Deklination) }))}");

		internal static void ExportAsXML(Window1 window, string filepath)
		{
			using (StreamWriter writer = new StreamWriter(filepath, false, Encoding.UTF8))
			{
				writer.Write(new XDocument(new XDeclaration("1.0", "UTF-8", "yes"),
								new XElement("Satellite",
									new XElement("AntennaPosition",
										new XAttribute("Longitude", $"{window.longText.Text}° {((window.longEast.IsChecked == true) ? "E" : "W")}"),
										new XAttribute("Latitude", $"{window.latText.Text}° {((window.longEast.IsChecked == true) ? "E" : "W")}")),
									new XElement("SatellitePosition",
										new XAttribute("Longitude", $"{window.longSatText.Text}° {((window.longSatEast.IsChecked == true) ? "E" : "W")}")),
									new XElement("AntennaDirection",
										new XAttribute("Azimut", window.azimutText.Text),
										new XAttribute("Elevation", window.elevationText.Text),
										new XAttribute("Deklination", window.declinationText.Text)),
									new XElement("ElevationCurve", window.rows.SelectMany((item) => new XElement[] {
														new XElement("Position",
															new XAttribute("Azimut", item.Azimut),
															new XAttribute("Elevation", item.Elevation),
															new XAttribute("Deklination", item.Deklination))
									}).ToArray()))).ToString());
				writer.Flush();
			}
		}

		internal static void ExportAsJSON(Window1 window, string filepath)
		{
			using (StreamWriter writer = new StreamWriter(filepath, false, Encoding.UTF8))
			{
				writer.Write(JObject.FromObject(new
				{
					AntennaPosition = new
					{
						Longitude = $"{window.longText.Text}° {((window.longEast.IsChecked == true) ? "E" : "W")}",
						Latitude = $"{window.latText.Text}° {((window.longEast.IsChecked == true) ? "E" : "W")}"
					},
					SatellitPosition = new
					{
						Longitude = $"{window.longSatText.Text}° {((window.longSatEast.IsChecked == true) ? "E" : "W")}"
					},
					AntennaDirection = new
					{
						Azimut = window.azimutText.Text,
						Elevation = window.elevationText.Text,
						Deklination = window.declinationText.Text
					},
					ElevationCurve = new JArray(window.rows.SelectMany((item) => new JObject[] { JObject.FromObject(new { item.Azimut, item.Deklination, item.Elevation }) }).ToArray())
				}).ToString());
				writer.Flush();
			}
		}

		internal static void ExportPNG(Window1 window, string filepath)
		{
			RenderTargetBitmap targetBitmap = new RenderTargetBitmap((int)window.MainViewport.ActualWidth, (int)window.MainViewport.ActualHeight, 96, 96, PixelFormats.Pbgra32);
			targetBitmap.Render(window.MainViewport);

			using (Stream s = File.Create(filepath))
			{
				(new PngBitmapEncoder()
				{
					Frames =
					{
						BitmapFrame.Create(targetBitmap)
					}
				}).Save(s);
			}
		}
	}
}
