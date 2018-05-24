using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhysikLaborSatellit
{
	internal static class SatelliteAntennaCalculator
	{
		const double k = 0.151;
		/// <summary>
		/// Gibt den Azimutwinkel zurück
		/// </summary>
		/// <param name="lambda">Längengraddifferenz im Bogenmaß</param>
		/// <param name="beta">Breitengrad im Bogenmaß</param>
		/// <returns>Azimutwinkel im Bogenmaß</returns>
		internal static double GetAzimutAngle(double lambda, double beta) => Math.Atan(Math.Tan(lambda) / Math.Sin(beta)) + ((beta > 0) ? 180 : 0);

		/// <summary>
		/// Gibt den Elevationswinkel zurück
		/// </summary>
		/// <param name="lambda">Längengraddifferenz im Bogenmaß</param>
		/// <param name="beta">Breitengrad im Bogenmaß</param>
		/// <returns>Elevationswinkel im Bogengrad</returns>
		internal static double GetElevationAngle(double lambda, double beta)
		{
			double alpha = (Math.Cos(lambda) * Math.Cos(beta) - k) / (Math.Sqrt(1 - Math.Pow(Math.Cos(lambda), 2) * Math.Pow(Math.Cos(beta), 2)));
			if ((lambda > 0 && beta < 0) || (lambda < 0 && beta > 0))
				alpha *= -1;
			return alpha;
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
