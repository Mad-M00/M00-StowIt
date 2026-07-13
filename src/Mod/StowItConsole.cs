using System;
using System.Collections.Generic;
using System.IO;
using M00StowIt.Core;
using M00StowIt.Game;

namespace M00StowIt.Mod;

// The actions behind the "as <subcommand>" console dispatcher.
internal static class StowItConsole
{
	private const int MaxListedMatches = 60;

	public static void Reload(SdtdConsole console)
	{
		ModRuntime runtime = StowItMod.Runtime;
		runtime.ReloadConfiguration();
		runtime.PrepareAliasRules();
		console.Output($"[StowIt] Config reloaded ({runtime.Aliases.LabelCount} alias label(s)). See log for any warnings.");
	}

	public static void Groups(SdtdConsole console)
	{
		GameItemCatalog catalog = StowItMod.Runtime.Catalog;
		if (catalog == null || catalog.GroupNames.Count == 0)
		{
			console.Output("[StowIt] No item groups available yet (load into a world first).");
			return;
		}
		console.Output($"[StowIt] {catalog.GroupNames.Count} item groups (use these on crate labels / in CrateLabels.txt):");
		foreach (string group in catalog.GroupNames)
		{
			console.Output("  " + group);
		}
	}

	public static void What(SdtdConsole console, List<string> args)
	{
		if (args.Count == 0)
		{
			console.Output("[StowIt] Usage: stow what <crate label>   (e.g. stow what Cans)");
			return;
		}
		ModRuntime runtime = StowItMod.Runtime;
		CategoryRuleResolver resolver = runtime.Resolver;
		if (resolver == null)
		{
			console.Output("[StowIt] Item data not loaded yet (load into a world first).");
			return;
		}
		string label = string.Join(" ", args).Trim().ToLowerInvariant();
		CategoryRule rule = resolver.Resolve(label);
		if (runtime.Aliases.TryGetCanonicalLabel(label, out string canonical))
		{
			console.Output(canonical == label
				? $"[StowIt] '{label}' is an alias from CrateLabels.txt"
				: $"[StowIt] '{label}' fuzzy-matches the alias '{canonical}' from CrateLabels.txt");
		}
		else
		{
			console.Output($"[StowIt] '{label}' has no alias; resolved literally (group, then item name/pattern)");
		}
		console.Output("[StowIt] Resolution: " + rule.Describe());
		List<CatalogItem> matches = resolver.EnumerateMatches(rule);
		console.Output($"[StowIt] A crate with this label receives {matches.Count} item(s):");
		for (int i = 0; i < matches.Count; i++)
		{
			if (i == MaxListedMatches)
			{
				console.Output($"  ... and {matches.Count - MaxListedMatches} more");
				break;
			}
			console.Output("  " + matches[i].ToDisplayString());
		}
	}

	public static void Search(SdtdConsole console, List<string> args)
	{
		if (args.Count == 0)
		{
			console.Output("[StowIt] Usage: stow search <text>   (matches internal and display names, e.g. stow search can)");
			return;
		}
		GameItemCatalog catalog = StowItMod.Runtime.Catalog;
		if (catalog == null)
		{
			console.Output("[StowIt] Item data not loaded yet (load into a world first).");
			return;
		}
		string search = string.Join(" ", args).Trim().ToLowerInvariant();
		var matches = new List<CatalogItem>();
		foreach (CatalogItem item in catalog.Items)
		{
			if (item.NameLower.Contains(search) || item.DisplayName.ToLowerInvariant().Contains(search))
			{
				matches.Add(item);
			}
		}
		matches.Sort((a, b) => string.CompareOrdinal(a.NameLower, b.NameLower));
		console.Output($"[StowIt] {matches.Count} item(s) matching '{search}':");
		foreach (CatalogItem item in matches)
		{
			console.Output("  " + item.ToDisplayString());
		}
	}

	// stow alias ... : edits CrateLabels.txt in place and reloads. Verbs:
	//   set <label> = <items>     create or replace (also the default verb)
	//   add <label> = <items>     append items to an existing label
	//   remove <label> = <items>  take items off a label
	//   delete <label>            remove the label entirely
	//   list                      show every label with its items
	public static void Alias(SdtdConsole console, List<string> args)
	{
		if (args.Count == 0)
		{
			AliasHelp(console);
			return;
		}
		ModRuntime runtime = StowItMod.Runtime;
		string verb = args[0].ToLowerInvariant();
		string remainder = string.Join(" ", args.GetRange(1, args.Count - 1)).Trim();
		switch (verb)
		{
			case "list":
				AliasList(console, runtime);
				return;
			case "delete":
			case "del":
				if (remainder.Length == 0)
				{
					console.Output("[StowIt] Usage: stow alias delete <label>");
					return;
				}
				ApplyEdit(console, runtime, CanonicalOrGiven(runtime, remainder), null,
					(lines, label) => AliasFileEditor.DeleteLabel(lines, label));
				return;
			case "set":
			case "add":
			case "remove":
			case "rem":
				EditWithTokens(console, runtime, verb, remainder);
				return;
			default:
				// No verb: "stow alias Breakfast = foodEgg" means set.
				EditWithTokens(console, runtime, "set", string.Join(" ", args).Trim());
				return;
		}
	}

	private static void EditWithTokens(SdtdConsole console, ModRuntime runtime, string verb, string expression)
	{
		int equalsIndex = expression.IndexOf('=');
		if (equalsIndex <= 0 || equalsIndex == expression.Length - 1)
		{
			console.Output($"[StowIt] Usage: stow alias {verb} <label> = <items>   (items separated by commas)");
			return;
		}
		string labelPart = expression.Substring(0, equalsIndex).Trim();
		var tokens = new List<string>();
		foreach (string part in expression.Substring(equalsIndex + 1)
			.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
		{
			string token = part.Trim();
			if (token.Length > 0)
			{
				tokens.Add(token);
			}
		}
		if (labelPart.Length == 0 || tokens.Count == 0)
		{
			console.Output($"[StowIt] Usage: stow alias {verb} <label> = <items>");
			return;
		}
		string label = CanonicalOrGiven(runtime, labelPart);
		switch (verb)
		{
			case "set":
				ApplyEdit(console, runtime, label, tokens,
					(lines, l) => AliasFileEditor.SetTokens(lines, l, tokens));
				break;
			case "add":
				ApplyEdit(console, runtime, label, tokens,
					(lines, l) => AliasFileEditor.AddTokens(lines, l, tokens));
				break;
			default:
				ApplyEdit(console, runtime, label, tokens,
					(lines, l) => AliasFileEditor.RemoveTokens(lines, l, tokens));
				break;
		}
	}

	// Fuzzy: "stow alias add mod tools = ..." finds the "Mod Tools" line even
	// though the file spells it differently. Unknown labels pass through
	// unchanged so "set" can create them with the player's spelling.
	private static string CanonicalOrGiven(ModRuntime runtime, string labelPart)
	{
		return runtime.Aliases.TryGetCanonicalLabel(labelPart, out string canonical) ? canonical : labelPart;
	}

	private static void ApplyEdit(
		SdtdConsole console,
		ModRuntime runtime,
		string label,
		List<string> tokens,
		Func<IReadOnlyList<string>, string, AliasFileEditor.Result> edit)
	{
		string path = runtime.AliasFilePath;
		string[] lines;
		try
		{
			lines = File.Exists(path) ? File.ReadAllLines(path) : Array.Empty<string>();
		}
		catch (Exception e)
		{
			console.Output("[StowIt] Could not read CrateLabels.txt: " + e.Message);
			return;
		}
		AliasFileEditor.Result result = edit(lines, label);
		console.Output("[StowIt] " + result.Message);
		if (!result.Changed)
		{
			return;
		}
		try
		{
			if (File.Exists(path))
			{
				File.Copy(path, path + ".bak", overwrite: true);
			}
			File.WriteAllLines(path, new List<string>(result.Lines));
		}
		catch (Exception e)
		{
			console.Output("[StowIt] Could not save CrateLabels.txt: " + e.Message);
			return;
		}
		runtime.ReloadConfiguration();
		runtime.PrepareAliasRules();
		CategoryRuleResolver resolver = runtime.Resolver;
		if (resolver != null && runtime.Aliases.TryGetCanonicalLabel(label, out string canonical))
		{
			CategoryRule rule = resolver.Resolve(canonical);
			console.Output($"[StowIt] '{canonical}' now receives {resolver.EnumerateMatches(rule).Count} item(s): {rule.Describe()}");
		}
		console.Output("[StowIt] Saved (previous version kept as CrateLabels.txt.bak) and reloaded.");
	}

	private static void AliasList(SdtdConsole console, ModRuntime runtime)
	{
		var labels = new List<string>(runtime.Aliases.TokensByLabel.Keys);
		labels.Sort((a, b) =>
		{
			int byPriority = runtime.Aliases.GetPriority(a).CompareTo(runtime.Aliases.GetPriority(b));
			return byPriority != 0 ? byPriority : string.CompareOrdinal(a, b);
		});
		console.Output($"[StowIt] {labels.Count} alias label(s), highest priority first:");
		foreach (string label in labels)
		{
			console.Output($"  {label} = {string.Join(", ", runtime.Aliases.TokensByLabel[label])}");
		}
	}

	private static void AliasHelp(SdtdConsole console)
	{
		console.Output("[StowIt] Edit crate rules from in-game (saves to CrateLabels.txt):");
		console.Output("  stow alias <label> = <items>          create a label or replace its items");
		console.Output("  stow alias add <label> = <items>      add items to a label");
		console.Output("  stow alias remove <label> = <items>   take items off a label");
		console.Output("  stow alias delete <label>             remove the label completely");
		console.Output("  stow alias list                       show all labels and their items");
	}

	public static void Help(SdtdConsole console)
	{
		console.Output("[StowIt] Subcommands:");
		console.Output("  stow reload          - reload StowItConfig.xml and CrateLabels.txt");
		console.Output("  stow groups          - list all game item groups");
		console.Output("  stow what <label>    - show how a crate label resolves and what it receives");
		console.Output("  stow search <text>   - find items by internal or display name");
		console.Output("  stow alias ...       - add/change/delete crate rules from in-game (stow alias for details)");
	}
}
