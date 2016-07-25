using System;
using System.Collections.Generic;
using System.Linq;
using Regex = System.Text.RegularExpressions.Regex;

/// <summary>
/// Utility class for parsing, scaling, or performing arithmetic operations on values using metric unit prefixes.
/// </summary>
public static class MetricParser
{
	private static PrefixCollection Prefixes = new PrefixCollection();

	/// <summary>
	/// Returns the raw numeric value of the formatted metric value.
	/// </summary>
	/// <param name="value">formatted value</param>
	/// <param name="baseUnit">base unit</param>
	/// <returns>raw numeric value</returns>
	public static double Parse(string value, string baseUnit)
	{
		value = value.Replace(baseUnit, string.Empty);
		double multiplier = 1;
		foreach (var prefix in Prefixes.Prefixes)
		{
			if (prefix == null || prefix.Trim().Equals(string.Empty))
			{
				continue;
			}

			if (value.Contains(prefix))
			{
				value = value.Replace(prefix, string.Empty);
				multiplier = Prefixes[prefix].Value;
				break;
			}
		}

		if (Regex.IsMatch(value, @"[^\d]"))
		{
			throw new ArgumentException(string.Format("Could not parse value '{0}'. Value may contain an invalid metric prefix. " +
				"Please verify that the correct base unit abbreviation was used."));
		}

		return double.Parse(value) * multiplier;
	}

	/// <summary>
	/// Compares two values containing a common base unit
	/// </summary>
	/// <param name="a">value a</param>
	/// <param name="b">value b</param>
	/// <param name="baseUnit">base unit</param>
	/// <returns>If a > b, -1<para />
	/// If a == b, 0<para />
	/// If a < b, 1</returns>
	public static int Compare(string a, string b, string baseUnit)
	{
		return Parse(a, baseUnit).CompareTo(Parse(b, baseUnit));
	}

	/// <summary>
	/// Adds two values containing a common base unit
	/// </summary>
	/// <param name="a">value a</param>
	/// <param name="b">value b</param>
	/// <param name="baseUnit">base unit</param>
	/// <returns>sum of a and b</returns>
	public static double Add(string a, string b, string baseUnit)
	{
		return Parse(a, baseUnit) + Parse(b, baseUnit);
	}

	/// <summary>
	/// Subtracts two values containing a common base unit
	/// </summary>
	/// <param name="a">value a</param>
	/// <param name="b">value b</param>
	/// <param name="baseUnit">base unit</param>
	/// <returns>difference of a and b</returns>
	public static double Subtract(string a, string b, string baseUnit)
	{
		return Parse(a, baseUnit) - Parse(b, baseUnit);
	}

	/// <summary>
	/// Multiplies two values containing a common base unit
	/// </summary>
	/// <param name="a">value a</param>
	/// <param name="b">value b</param>
	/// <param name="baseUnit">base unit</param>
	/// <returns>product of a and b</returns>
	public static double Multiply(string a, string b, string baseUnit)
	{
		return Parse(a, baseUnit) * Parse(b, baseUnit);
	}

	/// <summary>
	/// Divides two values containing a common base unit
	/// </summary>
	/// <param name="a">value a</param>
	/// <param name="b">value b</param>
	/// <param name="baseUnit">base unit</param>
	/// <returns>quotient of a and b</returns>
	public static double Divide(string a, string b, string baseUnit)
	{
		return Parse(a, baseUnit) / Parse(b, baseUnit);
	}

	/// <summary>
	/// Scales metric value to nearest prefix. (Ex: "10000m" -> "10km")
	/// </summary>
	/// <param name="value">value to scale</param>
	/// <param name="baseUnit">base unit</param>
	/// <returns>scaled value</returns>
	public static string AutoScale(string value, string baseUnit)
	{
		return AutoScale(Parse(value, baseUnit), baseUnit);
	}

	/// <summary>
	/// Scales raw value to nearest metric prefix.
	/// </summary>
	/// <param name="value">value to scale</param>
	/// <param name="baseUnit">base unit</param>
	/// <returns>scaled value</returns>
	public static string AutoScale(double value, string baseUnit)
	{
		var result = value.ToString();
		var prefix = string.Empty;
		foreach (var p in Prefixes.Prefixes)
		{
			if (value > 2 * Prefixes[p].Value)
			{
				result = (value / Prefixes[p].Value).ToString();
				prefix = p;
				break;
			}
		}

		return result + prefix + baseUnit;
	}

	/// <summary>
	/// Scales metric value to target prefix. If target prefix is empty, scales to base unit value.
	/// </summary>
	/// <param name="value">value to scale</param>
	/// <param name="baseUnit">base unit</param>
	/// <param name="targetPrefix">target prefix to scale to</param>
	/// <returns>scaled value</returns>
	public static string Scale(string value, string baseUnit, string targetPrefix = "")
	{
		return Scale(Parse(value, baseUnit), baseUnit, targetPrefix);
	}

	/// <summary>
	/// Scales raw value to target prefix.
	/// </summary>
	/// <param name="value">value to scale</param>
	/// <param name="baseUnit">base unit</param>
	/// <param name="targetPrefix">target prefix to scale to</param>
	/// <returns>scaled value</returns>
	public static string Scale(double value, string baseUnit, string targetPrefix)
	{
		if (!Prefixes.SupportsPrefix(targetPrefix))
		{
			throw new ArgumentException(string.Format("Invalid metric prefix '{0}'.", targetPrefix));
		}

		return (value / Prefixes[targetPrefix].Value).ToString() + targetPrefix + baseUnit;
	}

	public class PrefixCollection
	{
		private Dictionary<string, Prefix> dict = new List<Prefix>()
			{
				new Prefix("yotta", "Y", Math.Pow(1000, 8)),
				new Prefix("zetta", "Z", Math.Pow(1000, 7)),
				new Prefix("exa", "E", Math.Pow(1000, 6)),
				new Prefix("peta", "P", Math.Pow(1000, 5)),
				new Prefix("tera", "T", Math.Pow(1000, 4)),
				new Prefix("giga", "G", Math.Pow(1000, 3)),
				new Prefix("mega", "M", Math.Pow(1000, 2)),
				new Prefix("kilo", "k", 1000),
				new Prefix("hecto", "h", 100),
				new Prefix("deca", "da", 10),
				new Prefix(string.Empty, string.Empty, 1),
				new Prefix("deci", "d", 0.1),
				new Prefix("centi", "c", 0.01),
				new Prefix("milli", "m", 0.001),
				new Prefix("micro", "μ", Math.Pow(1000, -2)),
				new Prefix("nano", "n", Math.Pow(1000, -3)),
				new Prefix("pico", "p", Math.Pow(1000, -4)),
				new Prefix("femto", "f", Math.Pow(1000, -5)),
				new Prefix("atto", "a", Math.Pow(1000, -6)),
				new Prefix("zepto", "z", Math.Pow(1000, -7)),
				new Prefix("yocto", "y", Math.Pow(1000, -8)),
			}.ToDictionary(p => p.Symbol, p => p);

		internal PrefixCollection()
		{
		}

		public Prefix this[string key]
		{
			get
			{
				return dict.ContainsKey(key) ? dict[key] : null;
			}
		}

		public IEnumerable<string> Prefixes
		{
			get
			{
				return dict.Keys;
			}
		}

		public IEnumerable<string> Names
		{
			get
			{
				return from p in dict.Values
					   select p.Name;
			}
		}

		public bool SupportsPrefix(string prefix)
		{
			return dict.ContainsKey(prefix);
		}

		public bool SupportsName(string name)
		{
			return Names.Contains(name);
		}
	}

	public class Prefix
	{
		public string Name { get; private set; }
		public string Symbol { get; private set; }
		public double Value { get; private set; }

		public Prefix(string name, string symbol, double value)
		{
			Name = name;
			Symbol = symbol;
			Value = value;
		}
	}
}