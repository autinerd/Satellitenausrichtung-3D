using System;

namespace PhysikLaborSatellit
{
	internal static class SatelliteAntennaCalculator
	{
		const double k = 0.151;

		/// <summary>
		/// Konvertiert Gradmaß in Bogenmaß
		/// </summary>
		/// <param name="deg">Winkel im Gradmaß</param>
		/// <returns>Winkel im Bogenmaß</returns>
		internal static double DegToRad(double deg) => deg * Math.PI / 180;

		/// <summary>
		/// Konvertiert Bogenmaß in Gradmaß
		/// </summary>
		/// <param name="rad">Winkel im Bogenmaß</param>
		/// <returns>Winkel im Gradmaß</returns>
		internal static double RadToDeg(double rad) => rad * 180 / Math.PI;

		/// <summary>
		/// Gibt den Azimutwinkel zurück
		/// </summary>
		/// <param name="longitude">Längengrad der Antenne</param>
		/// <param name="latitude">Breitengrad der Antenne</param>
		/// <param name="longitudeSat">Längengrad des Satelliten</param>
		/// <returns>Azimutwinkel</returns>
		internal static double GetAzimutAngle(double longitude, double latitude, double longitudeSat)
		{
			if (latitude == 0)
				return ((longitudeSat < 0) ? 90 : 270);
			else if (latitude == 180)
				return 90;
			double lambda = DegToRad(longitudeSat - longitude);
			double beta = DegToRad(latitude);
			double psi = Math.Atan(Math.Tan(lambda) / Math.Sin(beta)) + ((latitude > 0) ? Math.PI : 0);
			if (psi < 0)
				psi += 2 * Math.PI;
			return RadToDeg(psi);
		}

		/// <summary>
		/// Gibt den Elevationswinkel zurück
		/// </summary>
		/// <param name="longitude">Längengrad der Antenne</param>
		/// <param name="latitude">Breitengrad der Antenne</param>
		/// <param name="longitudeSat">Längengrad des Satelliten</param>
		/// <returns>Elevationswinkel</returns>
		internal static double GetElevationAngle(double longitude, double latitude, double longitudeSat)
		{
			double lambda = DegToRad(longitudeSat - longitude);
			double beta = DegToRad(latitude);
			double alpha = Math.Atan((Math.Cos(lambda) * Math.Cos(beta) - k)
				/ Math.Sqrt(1 - Math.Pow(Math.Cos(lambda), 2) * Math.Pow(Math.Cos(beta), 2)));
			return RadToDeg(alpha);
		}

		/// <summary>
		/// Gibt den Elevationswinkel abhängig von Azimutwinkel und Breitengrad aus.
		/// </summary>
		/// <param name="psi">Azimutwinkel im Bogenmaß</param>
		/// <param name="beta">Breitengrad im Bogenmaß</param>
		/// <returns>Elevationswinkel im Bogenmaß</returns>
		internal static double GetElevationCurve(double psi, double beta)
		{
			double test = Math.Cos(psi) / Math.Tan(beta);
			double ergebnis = test - k * (Math.Sqrt(Math.Pow(test, 2) + 1));
			if (beta < 0)
				ergebnis *= -1;
			return ergebnis;
		}


	}
}
