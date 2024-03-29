﻿using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace PhysikLaborSatellit
{
	public class LongitudeRule : ValidationRule
	{
		public double Min => 0;
		public double Max => 180;
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			string v = (string)value;
			double longitude = 0;
			try
			{
				if (v.Length > 0)
				{
					longitude = double.Parse(v, NumberStyles.Float, CultureInfo.CurrentCulture);
				}
			}
			catch (Exception)
			{
				return new ValidationResult(false, "Keine Zahl eingegeben");
			}
			if (longitude < Min || longitude > Max)
			{
				return new ValidationResult(false, "Außerhalb des zulässigen Bereichs");
			}
			return ValidationResult.ValidResult;
		}
	}

	public class LatitudeRule : ValidationRule
	{
		public double Min => 0;
		public double Max => 90;
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			string v = (string)value;
			double latitude = 0;
			try
			{
				if (v.Length > 0)
				{
					latitude = double.Parse(v, CultureInfo.CurrentCulture);
				}
			}
			catch (Exception)
			{
				return new ValidationResult(false, "Keine Zahl eingegeben");
			}
			if (latitude < Min || latitude > Max)
			{
				return new ValidationResult(false, "Außerhalb des zulässigen Bereichs");
			}
			return ValidationResult.ValidResult;
		}
	}

	public class MyDataSource
	{
		public double Longitude { get; set; }
		public double Latitude { get; set; }
		public double LongitudeSat { get; set; }

		public MyDataSource() => Longitude = Latitude = LongitudeSat = 0;
	}

	public class DoubleToPersistantStringConverter : IValueConverter
	{
		private string lastConvertBackString;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is double))
			{
				return null;
			}

			string stringValue = lastConvertBackString ?? value.ToString();
			lastConvertBackString = null;

			return stringValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is string))
			{
				return null;
			}

			if (double.TryParse((string)value, out double result))
			{
				lastConvertBackString = (string)value;
				return result;
			}

			return null;
		}
	}

	public class ElevationCurveRow
	{
		public string Azimut { get; set; }
		public string Elevation { get; set; }
		public string Deklination { get; set; }
	}
}
