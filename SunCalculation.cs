using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysikLaborSatellit
{
	internal static class SunCalculation
	{
		/// <summary>
		/// Konvertiert Gradmaß in Bogenmaß
		/// </summary>
		/// <param name="deg">Winkel im Gradmaß</param>
		/// <returns>Winkel im Bogenmaß</returns>
		private static double DegToRad(double deg) => deg * Math.PI / 180;

		/// <summary>
		/// Konvertiert Bogenmaß in Gradmaß
		/// </summary>
		/// <param name="rad">Winkel im Bogenmaß</param>
		/// <returns>Winkel im Gradmaß</returns>
		private static double RadToDeg(double rad) => rad * 180 / Math.PI;

		private static double GetJD()
		{
			DateTime date = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			DateTime date2 = DateTime.Now;
			return 2451544.5 + (date2.ToUniversalTime().Ticks / 10000 - date.ToUniversalTime().Ticks / 10000) / 86400000;
		}

		internal static void GetSunCalculation(double longitude, double latitude, out double azimut, out double h)
		{
			double lambda = DegToRad(-longitude);
			double phi = DegToRad(latitude);

			double n = GetJD() - 2451545;
			double L = DegToRad(280.46) + DegToRad(0.9856474) * n;
			double g = DegToRad(357.528) + DegToRad(0.9856003) * n;
			double Lambda = L + DegToRad(1.915) * Math.Sin(g) + DegToRad(0.01997) * Math.Sin(2 * g);

			double epsilon = DegToRad(23.439) - DegToRad(0.0000004) * n;
			double alpha = (Math.Cos(Lambda) > 0) ? Math.Atan(Math.Cos(epsilon) * Math.Tan(Lambda)) : Math.Atan(Math.Cos(epsilon) * Math.Tan(Lambda)) + 4 * Math.Atan(1);
			double delta = Math.Asin(Math.Sin(epsilon) * Math.Sin(Lambda));

			double T0 = ((int)GetJD() + 0.5 - 2451545) / 36525;
			double Phi_hG = (6.697376 + 2400.05134 * T0 + 1.002738 * new TimeSpan(DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second).TotalHours) % 24;
			double Phi_G = Phi_hG * DegToRad(15);
			double Phi = Phi_G + lambda;
			double tau = Phi - alpha;
			azimut = Math.Atan((Math.Sin(tau)) / (Math.Cos(tau) * Math.Sin(phi) - Math.Tan(delta) * Math.Cos(phi)));
			h = Math.Asin(Math.Cos(delta) * Math.Cos(tau) * Math.Cos(phi) + Math.Sin(delta) * Math.Sin(phi));
		}
	}
}
