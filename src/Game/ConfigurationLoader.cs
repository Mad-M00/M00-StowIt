using System;
using System.Collections.Generic;
using System.IO;
using M00StowIt.Core;

namespace M00StowIt.Game;

// File access for the two config files; parsing lives in Core. Missing or
// broken files degrade to defaults, never to a crash.
internal static class ConfigurationLoader
{
	public static StowItSettings LoadSettings(string path, ISorterLog log)
	{
		try
		{
			if (!File.Exists(path))
			{
				log.Warning("Unable to find config at: " + path + "; using defaults");
				return StowItSettings.Defaults();
			}
			return StowItSettingsParser.Parse(File.ReadAllText(path), log);
		}
		catch (Exception e)
		{
			log.Warning("Failed to read config; using defaults");
			log.Warning(e.Message);
			return StowItSettings.Defaults();
		}
	}

	// Language files (CrateLabels.<culture>.txt, e.g. CrateLabels.de.txt)
	// load FIRST, alphabetically, so a label redefined in the main file (by
	// hand or via "stow alias") always wins over a translation. Deleting a
	// language file simply deactivates its labels.
	public static AliasConfiguration LoadAliases(string path, ISorterLog log)
	{
		try
		{
			var lines = new List<string>();
			int languageFiles = 0;
			foreach (string languagePath in FindLanguageFiles(path))
			{
				lines.AddRange(File.ReadAllLines(languagePath));
				languageFiles++;
			}
			if (!string.IsNullOrEmpty(path) && File.Exists(path))
			{
				lines.AddRange(File.ReadAllLines(path));
			}
			else
			{
				log.Info("No CrateLabels.txt found; sign labels are resolved directly (group, then item name/pattern).");
			}
			if (lines.Count == 0)
			{
				return AliasConfiguration.Empty;
			}
			AliasConfiguration aliases = AliasFileParser.Parse(lines);
			log.Info($"Loaded {aliases.LabelCount} category alias label(s)"
				+ (languageFiles > 0 ? $" (including {languageFiles} language file(s))" : ""));
			return aliases;
		}
		catch (Exception e)
		{
			log.Warning("Failed to load CrateLabels.txt; crate labels disabled");
			log.Warning(e.Message);
			return AliasConfiguration.Empty;
		}
	}

	private static List<string> FindLanguageFiles(string mainPath)
	{
		var languageFiles = new List<string>();
		string directory = string.IsNullOrEmpty(mainPath) ? null : Path.GetDirectoryName(mainPath);
		if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
		{
			return languageFiles;
		}
		string mainFileName = Path.GetFileName(mainPath);
		foreach (string candidate in Directory.GetFiles(directory, "CrateLabels.*.txt"))
		{
			string fileName = Path.GetFileName(candidate);
			if (!fileName.Equals(mainFileName, StringComparison.OrdinalIgnoreCase)
				&& fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
			{
				languageFiles.Add(candidate);
			}
		}
		languageFiles.Sort(StringComparer.OrdinalIgnoreCase);
		return languageFiles;
	}
}
