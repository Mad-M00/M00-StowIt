using System;
using System.Xml;

namespace M00StowIt.Core;

// Parses StowItConfig.xml. Resilience contract: any structural problem —
// malformed XML, a missing required tag, an unparsable number — falls back to
// COMPLETE defaults rather than a half-applied config, so a broken edit can
// never leave the mod in a mixed state. Routing mode and fallback-crate name
// are optional and default individually.
public static class StowItSettingsParser
{
	public static StowItSettings Parse(string xmlText, ISorterLog log)
	{
		try
		{
			var document = new XmlDocument();
			document.LoadXml(xmlText);
			var settings = StowItSettings.Defaults();
			XmlNodeList routingNodes = document.GetElementsByTagName("SortingMode");
			if (routingNodes.Count > 0
				&& Enum.TryParse(routingNodes[0].InnerText.Trim(), ignoreCase: true, out RoutingMode routing))
			{
				settings.Routing = routing;
			}
			XmlNodeList fallbackNodes = document.GetElementsByTagName("FallbackCrateName");
			if (fallbackNodes.Count > 0)
			{
				settings.FallbackCrateName = fallbackNodes[0].InnerText.Trim();
			}
			settings.SortKeyCodes = ParseKeyCodes(document, "SortButtons");
			settings.RestockKeyCodes = ParseKeyCodes(document, "RestockButtons");
			settings.SortRadius = ParseRadius(document);
			return settings;
		}
		catch (Exception e)
		{
			log.Warning("Failed to load or parse config; using defaults");
			log.Warning(e.Message);
			return StowItSettings.Defaults();
		}
	}

	private static string RequiredText(XmlDocument document, string tagName)
	{
		XmlNodeList nodes = document.GetElementsByTagName(tagName);
		if (nodes.Count == 0)
		{
			throw new Exception($"Missing required tag {tagName}");
		}
		return nodes[0].InnerText.Trim();
	}

	private static int[] ParseKeyCodes(XmlDocument document, string tagName)
	{
		string[] values = RequiredText(document, tagName).Split(' ');
		if (values.Length == 0)
		{
			throw new Exception($"Must have at least one value for tag {tagName}");
		}
		var keyCodes = new int[values.Length];
		for (int i = 0; i < values.Length; i++)
		{
			keyCodes[i] = int.Parse(values[i]);
		}
		return keyCodes;
	}

	private static GridRadius ParseRadius(XmlDocument document)
	{
		string[] values = RequiredText(document, "SortDistance").Split(' ');
		if (values.Length != 3)
		{
			throw new Exception("Must have exactly three values for tag SortDistance");
		}
		return new GridRadius(ClampRadius(values[0]), ClampRadius(values[1]), ClampRadius(values[2]));
	}

	private static int ClampRadius(string value)
	{
		return Math.Min(Math.Max(int.Parse(value), 0), 127);
	}
}
