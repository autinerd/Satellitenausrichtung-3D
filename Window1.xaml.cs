//#define DEBUG_AXIS

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Microsoft.Win32;

namespace PhysikLaborSatellit
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		internal ObservableCollection<ElevationCurveRow> rows = new ObservableCollection<ElevationCurveRow>();

		public Window1()
		{
			SelectedCamera = 0;
			InitializeComponent();
			longText.Focus();
		}

		public int SelectedCamera { get; set; }
		// The main object model group.
		private Model3DGroup MainModel3Dgroup = new Model3DGroup();

		// The camera.
		private PerspectiveCamera TheCamera = new PerspectiveCamera();

		// The camera's current location.
		private double CameraR = r_e * 1.3 / radius_earth; // a bit away from the satellite, so it can be seen
		private double CameraPhi = 0;       // 0 degrees
		private double CameraTheta = 0;     // 0 degrees

		// The change in CameraPhi when you press the up and down arrows.
		private const double CameraDPhi = Math.PI / 180;
		// The change in CameraTheta when you press the left and right arrows.
		private const double CameraDTheta = Math.PI / 180;
		// The change in CameraR when you press + or -.
		private const double CameraDR = 0.05 * r_e;

		const double r_e = 5000;
		// radius of the earth to distance of the satellite
		const double radius_earth = 0.151;

		// Create the scene.
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			// Give the camera its initial position.
			TheCamera = new PerspectiveCamera
			{
				FieldOfView = 60
			};
			MainViewport.Camera = TheCamera;
			SetSun();
			PositionCamera();

			// Add the group of models to a ModelVisual3D.
			ModelVisual3D model_visual = new ModelVisual3D
			{
				Content = MainModel3Dgroup
			};
			// Display the main visual to the viewport.
			MainViewport.Children.Add(model_visual);
		}

		/// <summary>
		/// Set the sun's position.
		/// </summary>
		private void SetSun()
		{
			SunCalculation.GetSunCalculation(-9, 48, out double azimut, out double h);
			Vector3D v = SphericalCoordinatesToCartesic(1, 9, 48);
			Vector3D sun_vec = v + SphericalCoordinatesToCartesic(23481.4, -azimut, h);

			MainModel3Dgroup.Children.Add(new GeometryModel3D(AddSmoothSphere(new Point3D(sun_vec.X, sun_vec.Y, sun_vec.Z), 109.16, 180, 360), new DiffuseMaterial(Brushes.Yellow)));

			PointLight pointLight = new PointLight(Colors.LightYellow, new Point3D(sun_vec.X, sun_vec.Y, sun_vec.Z))
			{
				Range = 50000
			};
			MainModel3Dgroup.Children.Add(pointLight);
			SpotLight spotLight = new SpotLight(Colors.LightYellow, new Point3D(v.X, v.Y, v.Z), SphericalCoordinatesToCartesic(23481.4, -azimut, h), 50, 50);
			MainModel3Dgroup.Children.Add(spotLight);
		}

		/// <summary>
		/// Set the sun's position.
		/// </summary>
		/// <param name="longitude"></param>
		/// <param name="latitude"></param>
		private void SetSun(double longitude, double latitude)
		{
			SunCalculation.GetSunCalculation(longitude, latitude, out double azimut, out double h);
			Vector3D v = SphericalCoordinatesToCartesic(r_e, -longitude, latitude);
			Vector3D sun_vec = v + SphericalCoordinatesToCartesic(23481.4 * r_e, -azimut, h);
			MainModel3Dgroup.Children.Add(new GeometryModel3D(AddSmoothSphere(new Point3D(sun_vec.X, sun_vec.Y, sun_vec.Z), 109.16 * r_e, 180, 360), new EmissiveMaterial(Brushes.LightYellow)));

			PointLight pointLight = new PointLight(Colors.LightYellow, new Point3D(sun_vec.X, sun_vec.Y, sun_vec.Z))
			{
				Range = 50000 * r_e
			};
			MainModel3Dgroup.Children.Add(pointLight);

			SpotLight spotLight = new SpotLight(Colors.LightYellow, new Point3D(v.X, v.Y, v.Z), SphericalCoordinatesToCartesic(23481.4, -azimut, h), 50, 50)
			{
				Range = r_e * 50000
			};
			MainModel3Dgroup.Children.Add(spotLight);
		}

		// Add the model to the Model3DGroup.
		private void DefineModel(double longitude, double latitude, double longitudeSat)
		{
			// Clear everthing
			MainModel3Dgroup.Children.Clear();
			Panel3D.Background = Brushes.SkyBlue;
			SetSun(longitude, latitude);

			// Make earth

			MainModel3Dgroup.Children.Add(new GeometryModel3D(AddSmoothSphere(new Point3D(0, 0, 0), r_e, 360, 180), new DiffuseMaterial(Brushes.Aqua)));

			// Make satellite cylinder
			Vector3D sat_vector = SphericalCoordinatesToCartesic(r_e / radius_earth, -SatelliteAntennaCalculator.DegToRad(longitudeSat), 0);
			MainModel3Dgroup.Children.Add(new GeometryModel3D(AddSmoothCylinder(new Point3D(sat_vector.X, sat_vector.Y, sat_vector.Z),
				Vector3D.Multiply(sat_vector, 0.005), 200, 50), new DiffuseMaterial(Brushes.Silver)));

			// Make antenna cylinder
			Vector3D person_vec = SphericalCoordinatesToCartesic(r_e, -SatelliteAntennaCalculator.DegToRad(longitude), SatelliteAntennaCalculator.DegToRad(latitude)) * 1.005;
			MainModel3Dgroup.Children.Add(new GeometryModel3D(AddSmoothCylinder(new Point3D(person_vec.X, person_vec.Y, person_vec.Z), Vector3D.Subtract(sat_vector, person_vec) * 0.00005 * r_e, 0.003 * r_e, 50), new DiffuseMaterial(Brushes.Gray)));

			// Add line between person and satellite
			SolidColorBrush brush6;
			if (SatelliteAntennaCalculator.GetElevationAngle(longitude, latitude, longitudeSat) < 0)
			{
				brush6 = new SolidColorBrush(Color.FromArgb(0x80, 0xff, 0, 0));
			}
			else
			{
				brush6 = new SolidColorBrush(Color.FromArgb(0x80, 0x01, 0x32, 0x20));
			}
			GeometryModel3D model6 = new GeometryModel3D(AddSmoothCylinder(new Point3D(person_vec.X, person_vec.Y, person_vec.Z), Vector3D.Subtract(sat_vector, person_vec), 0.003 * r_e, 50), new DiffuseMaterial(brush6));
			MainModel3Dgroup.Children.Add(model6);
		}

		// Add a triangle to the indicated mesh.
		// Reuse points so triangles share normals.
		private static void AddSmoothTriangle(MeshGeometry3D mesh, Dictionary<Point3D, int> dict, Point3D point1, Point3D point2, Point3D point3)
		{
			int index1, index2, index3;

			// Find or create the points.
			if (dict.ContainsKey(point1))
			{
				index1 = dict[point1];
			}
			else
			{
				index1 = mesh.Positions.Count;
				mesh.Positions.Add(point1);
				dict.Add(point1, index1);
			}

			if (dict.ContainsKey(point2))
			{
				index2 = dict[point2];
			}
			else
			{
				index2 = mesh.Positions.Count;
				mesh.Positions.Add(point2);
				dict.Add(point2, index2);
			}

			if (dict.ContainsKey(point3))
			{
				index3 = dict[point3];
			}
			else
			{
				index3 = mesh.Positions.Count;
				mesh.Positions.Add(point3);
				dict.Add(point3, index3);
			}

			// If two or more of the points are
			// the same, it's not a triangle.
			if ((index1 == index2) ||
				(index2 == index3) ||
				(index3 == index1))
			{
				return;
			}

			// Create the triangle.
			mesh.TriangleIndices.Add(index1);
			mesh.TriangleIndices.Add(index2);
			mesh.TriangleIndices.Add(index3);
		}

		// Adjust the camera's position.
		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Up:
				case Key.W:
					switch (SelectedCamera)
					{
						case 0:
							CameraTheta += CameraDTheta;
							if (CameraTheta > Math.PI / 2.0)
							{
								CameraTheta = Math.PI / 2.0;
							}

							break;
						default:
							ChangeCamera2View(new Vector3D(0, 0, 0.3 * r_e));
							break;
					}
					break;
				case Key.Down:
				case Key.S:
					switch (SelectedCamera)
					{
						case 0:
							CameraTheta -= CameraDTheta;
							if (CameraTheta < -Math.PI / 2.0)
							{
								CameraTheta = -Math.PI / 2.0;
							}

							break;
						default:
							ChangeCamera2View(new Vector3D(0, 0, -0.3 * r_e));
							break;
					}
					break;
				case Key.Left:
				case Key.A:
					switch (SelectedCamera)
					{
						case 0:
							CameraPhi += CameraDPhi;
							break;
						default:
							ChangeCamera2View(new Vector3D(0, 0.3 * r_e, 0));
							break;
					}
					break;
				case Key.Right:
				case Key.D:
					switch (SelectedCamera)
					{
						case 0:
							CameraPhi -= CameraDPhi;
							break;
						default:
							ChangeCamera2View(new Vector3D(0, -0.3 * r_e, 0));
							break;
					}
					break;
				case Key.Add:
				case Key.OemPlus:
					if (SelectedCamera == 0)
					{
						CameraR -= CameraDR;
						if (CameraR < CameraDR)
						{
							CameraR = CameraDR;
						}
					}
					break;
				case Key.Subtract:
				case Key.OemMinus:
					if (SelectedCamera == 0)
					{
						CameraR += CameraDR;
					}
					break;
				default:
					return;
			}
			e.Handled = true;
			// Update the camera's position.
			if (SelectedCamera == 0)
			{
				PositionCamera();
			}
		}

		// Position the satellite camera.
		private void PositionCamera()
		{
			// Calculate the camera's position in Cartesian coordinates.
			Vector3D v = SphericalCoordinatesToCartesic(CameraR, CameraPhi, CameraTheta);
			TheCamera.Position = new Point3D(v.X, v.Y, v.Z);

			// Look toward the origin.
			TheCamera.LookDirection = -v;

			// Set the Up direction.
			TheCamera.UpDirection = new Vector3D(0, 0, 1);
		}

		// Position the antenna camera.
		private void PositionCamera2(double longitude, double latitude, double longitudeSat)
		{
			// Calculate the camera's position in Cartesian coordinates.
			Vector3D v = SphericalCoordinatesToCartesic(r_e, -SatelliteAntennaCalculator.DegToRad(longitude), SatelliteAntennaCalculator.DegToRad(latitude)) * 1.005;
			TheCamera.Position = new Point3D(v.X, v.Y, v.Z);

			// Look toward the satellite
			Vector3D sat_vector = SphericalCoordinatesToCartesic(r_e / radius_earth, -SatelliteAntennaCalculator.DegToRad(longitudeSat), 0);

			TheCamera.LookDirection = Vector3D.Subtract(sat_vector, v);

			// Set the Up direction.
			TheCamera.UpDirection = new Vector3D(0, 0, 1);
		}

		private void ChangeCamera2View(Vector3D v) => TheCamera.LookDirection += v;

		#region add shapes

		// Add a cylinder with smooth sides.
		private MeshGeometry3D AddSmoothCylinder(Point3D end_point, Vector3D axis, double radius, int num_sides)
		{
			MeshGeometry3D mesh = new MeshGeometry3D();
			// Get two vectors perpendicular to the axis.
			Vector3D v1;
			if ((axis.Z < -0.01) || (axis.Z > 0.01))
			{
				v1 = new Vector3D(axis.Z, axis.Z, -axis.X - axis.Y);
			}
			else
			{
				v1 = new Vector3D(-axis.Y - axis.Z, axis.X, axis.X);
			}

			Vector3D v2 = Vector3D.CrossProduct(v1, axis);

			// Make the vectors have length radius.
			v1 *= (radius / v1.Length);
			v2 *= (radius / v2.Length);

			// Make the top end cap.
			// Make the end point.
			int pt0 = mesh.Positions.Count; // Index of end_point.
			mesh.Positions.Add(end_point);

			// Make the top points.
			double theta = 0;
			double dtheta = 2 * Math.PI / num_sides;
			for (int i = 0; i < num_sides; i++)
			{
				mesh.Positions.Add(end_point +
					Math.Cos(theta) * v1 +
					Math.Sin(theta) * v2);
				theta += dtheta;
			}

			// Make the top triangles.
			int pt1 = mesh.Positions.Count - 1; // Index of last point.
			int pt2 = pt0 + 1;                  // Index of first point in this cap.
			for (int i = 0; i < num_sides; i++)
			{
				mesh.TriangleIndices.Add(pt0);
				mesh.TriangleIndices.Add(pt1);
				mesh.TriangleIndices.Add(pt2);
				pt1 = pt2++;
			}

			// Make the bottom end cap.
			// Make the end point.
			pt0 = mesh.Positions.Count; // Index of end_point2.
			Point3D end_point2 = end_point + axis;
			mesh.Positions.Add(end_point2);

			// Make the bottom points.
			theta = 0;
			for (int i = 0; i < num_sides; i++)
			{
				mesh.Positions.Add(end_point2 +
					Math.Cos(theta) * v1 +
					Math.Sin(theta) * v2);
				theta += dtheta;
			}

			// Make the bottom triangles.
			theta = 0;
			pt1 = mesh.Positions.Count - 1; // Index of last point.
			pt2 = pt0 + 1;                  // Index of first point in this cap.
			for (int i = 0; i < num_sides; i++)
			{
				mesh.TriangleIndices.Add(num_sides + 1);    // end_point2
				mesh.TriangleIndices.Add(pt2);
				mesh.TriangleIndices.Add(pt1);
				pt1 = pt2++;
			}

			// Make the sides.
			// Add the points to the mesh.
			int first_side_point = mesh.Positions.Count;
			theta = 0;
			for (int i = 0; i < num_sides; i++)
			{
				Point3D p1 = end_point +
					Math.Cos(theta) * v1 +
					Math.Sin(theta) * v2;
				mesh.Positions.Add(p1);
				Point3D p2 = p1 + axis;
				mesh.Positions.Add(p2);
				theta += dtheta;
			}

			// Make the side triangles.
			pt1 = mesh.Positions.Count - 2;
			pt2 = pt1 + 1;
			int pt3 = first_side_point;
			int pt4 = pt3 + 1;
			for (int i = 0; i < num_sides; i++)
			{
				mesh.TriangleIndices.Add(pt1);
				mesh.TriangleIndices.Add(pt2);
				mesh.TriangleIndices.Add(pt4);

				mesh.TriangleIndices.Add(pt1);
				mesh.TriangleIndices.Add(pt4);
				mesh.TriangleIndices.Add(pt3);

				pt1 = pt3;
				pt3 += 2;
				pt2 = pt4;
				pt4 += 2;
			}
			return mesh;
		}

		// Add a sphere.
		private MeshGeometry3D AddSmoothSphere(Point3D center, double radius, int num_phi, int num_theta)
		{
			MeshGeometry3D mesh = new MeshGeometry3D();
			// Make a dictionary to track the sphere's points.
			Dictionary<Point3D, int> dict = new Dictionary<Point3D, int>();

			double phi0, theta0;
			double dphi = Math.PI / num_phi;
			double dtheta = 2 * Math.PI / num_theta;

			phi0 = 0;
			double y0 = radius * Math.Cos(phi0);
			double r0 = radius * Math.Sin(phi0);
			for (int i = 0; i < num_phi; i++)
			{
				double phi1 = phi0 + dphi;
				double y1 = radius * Math.Cos(phi1);
				double r1 = radius * Math.Sin(phi1);

				// Point ptAB has phi value A and theta value B.
				// For example, pt01 has phi = phi0 and theta = theta1.
				// Find the points with theta = theta0.
				theta0 = 0;
				Point3D pt00 = new Point3D(
					center.X + r0 * Math.Cos(theta0),
					center.Y + y0,
					center.Z + r0 * Math.Sin(theta0));
				Point3D pt10 = new Point3D(
					center.X + r1 * Math.Cos(theta0),
					center.Y + y1,
					center.Z + r1 * Math.Sin(theta0));
				for (int j = 0; j < num_theta; j++)
				{
					// Find the points with theta = theta1.
					double theta1 = theta0 + dtheta;
					Point3D pt01 = new Point3D(
						center.X + r0 * Math.Cos(theta1),
						center.Y + y0,
						center.Z + r0 * Math.Sin(theta1));
					Point3D pt11 = new Point3D(
						center.X + r1 * Math.Cos(theta1),
						center.Y + y1,
						center.Z + r1 * Math.Sin(theta1));
					// Create the triangles.
					AddSmoothTriangle(mesh, dict, pt00, pt11, pt10);
					AddSmoothTriangle(mesh, dict, pt00, pt01, pt11);

					// Move to the next value of theta.
					theta0 = theta1;
					pt00 = pt01;
					pt10 = pt11;
				}

				// Move to the next value of phi.
				phi0 = phi1;
				y0 = y1;
				r0 = r1;
			}
			return mesh;
		}
		#endregion

		private static Vector3D SphericalCoordinatesToCartesic(double radius, double phi, double lambda) => new Vector3D(radius * Math.Cos(lambda) * Math.Cos(phi), radius * Math.Cos(lambda) * Math.Sin(phi), radius * Math.Sin(lambda));

		// Calculate button click
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (Validation.GetHasError(longText) || Validation.GetHasError(latText) || Validation.GetHasError(longSatText))
			{
				return;
			}

			double longitude = double.Parse(longText.Text, CultureInfo.CurrentCulture) * ((longEast.IsChecked == true) ? -1 : 1);
			double latitude = double.Parse(latText.Text, CultureInfo.CurrentCulture) * ((latSouth.IsChecked == true) ? -1 : 1);
			double longitudeSat = double.Parse(longSatText.Text, CultureInfo.CurrentCulture) * ((longSatEast.IsChecked == true) ? -1 : 1);

			MainViewport.Focus();

			double elevation = SatelliteAntennaCalculator.GetElevationAngle(longitude, latitude, longitudeSat);
			if (elevation > 0)
			{
				azimutText.Text = SatelliteAntennaCalculator.GetAzimutAngle(longitude, latitude, longitudeSat).ToString("0#.#0°", CultureInfo.CurrentCulture);
				elevationText.Text = elevation.ToString("0#.#0°", CultureInfo.CurrentCulture);
				declinationText.Text = SatelliteAntennaCalculator.GetDeclinationAngle(longitude, latitude, longitudeSat).ToString("0#.#0°", CultureInfo.CurrentCulture);
				azimutText.Background = elevationText.Background = declinationText.Background = Brushes.White;
				azimutText.Foreground = declinationText.Foreground = elevationText.Foreground = Brushes.Black;
			}
			else
			{
				SolidColorBrush b = new SolidColorBrush(Color.FromArgb(0x99, 0xFF, 00, 00));
				azimutText.Background = elevationText.Background = declinationText.Background = b;
				azimutText.Foreground = declinationText.Foreground = elevationText.Foreground = Brushes.White;
				azimutText.Text = declinationText.Text = elevationText.Text = "Nicht empfangbar!";
			}

			DefineModel(longitude, latitude, longitudeSat);
			CameraPhi = -SatelliteAntennaCalculator.DegToRad(longitudeSat);
			CameraTheta = 0;
			switch (SelectedCamera)
			{
				case 1:
					PositionCamera2(longitude, latitude, longitudeSat);
					break;
				default:
					PositionCamera();
					break;
			}

			FillElevationCurveRows(latitude, longitude, longitudeSat);
			angleGrid.ItemsSource = rows;

			ExportTab.IsEnabled = true;
		}

		// Zoom with the Mouse wheel
		private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (SelectedCamera == 0)
			{
				if (e.Delta > 0)
				{
					CameraR -= CameraDR;
					if (CameraR < CameraDR)
					{
						CameraR = CameraDR;
					}
				}
				else
				{
					CameraR += CameraDR;
				}

				PositionCamera();
			}


		}

		void FillElevationCurveRows(double latitude, double longitude, double longitudeSat)
		{
			rows.Clear();
			int begin = ((latitude > 0) ? 90 : 270);
			for (int i = begin; i != (begin + 180 + 5) % 360; i = (i + 5) % 360)
			{
				double alpha = SatelliteAntennaCalculator.GetElevationCurve(i, latitude);
				if (alpha < 0)
				{
					rows.Add(new ElevationCurveRow { Azimut = i.ToString("0°", CultureInfo.CurrentCulture), Elevation = "nicht empfangbar", Deklination = "nicht empfangbar" });
					continue;
				}
				else
				{
					double delta = SatelliteAntennaCalculator.GetDeclinationAngle(i, latitude);
					rows.Add(new ElevationCurveRow { Azimut = i.ToString("0°", CultureInfo.CurrentCulture), Elevation = alpha.ToString("0#.#0°", CultureInfo.CurrentCulture), Deklination = delta.ToString("0#.#0°", CultureInfo.CurrentCulture) });
				}
			}
		}

		// Keybord focus selects all text of textbox
		private void TextBox_GotFocus(object sender, RoutedEventArgs e) => ((TextBox)sender).SelectAll();

		/// <summary>
		/// Click the "Search folder..." button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			SaveFileDialog sfd = new SaveFileDialog()
			{
				AddExtension = true,
				ValidateNames = true,
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
				Filter = "Text-Datei|*.txt|Komma-separierte Werte|*.csv|XML-Datei|*.xml|JSON-Datei|*.json|PNG-Bild|*.png",
				Title = "Datei exportieren"
			};
			if (sfd.ShowDialog() == true)
			{
				fileNameBox.Text = sfd.FileName;
			}
		}

		/// <summary>
		/// Click the "Export" button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			if (fileNameBox.Text.Length > 0)
			{
				if (ExportText.IsChecked == true)
				{
					DataExport.ExportAsText(this, fileNameBox.Text);
				}
				if (ExportCSV.IsChecked == true)
				{
					DataExport.ExportAsCSV(this, fileNameBox.Text);
				}
				if (ExportXML.IsChecked == true)
				{
					DataExport.ExportAsXML(this, fileNameBox.Text);
				}
				if (ExportJSON.IsChecked == true)
				{
					DataExport.ExportAsJSON(this, fileNameBox.Text);
				}
			}
			MessageBox.Show("Erfolgreich exportiert!", "Export erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.None);
		}
		/// <summary>
		/// Click the "Save PNG" button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button_Click_3(object sender, RoutedEventArgs e)
		{
			if (fileNameBox.Text.Length > 0)
			{
				DataExport.ExportPNG(this, fileNameBox.Text);
			}
			MessageBox.Show("Erfolgreich exportiert!", "Export erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.None);
		}

		private void currentCamera_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (SelectedCamera == 0)
			{
				PositionCamera();
			} else
			{
				PositionCamera2(double.Parse(longText.Text) * ((longEast.IsChecked == true) ? -1 : 1), double.Parse(latText.Text) * ((latSouth.IsChecked == true) ? -1 : 1), double.Parse(longSatText.Text) * ((longSatEast.IsChecked == true) ? -1 : 1));
			}
			MainViewport.Focus();
		}
	}
}
