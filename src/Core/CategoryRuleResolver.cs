using System;
using System.Collections.Generic;

namespace M00StowIt.Core;

// Turns crate labels into routing rules via a fixed cascade, per token:
//   1. "@Label"                  -> inherit that alias label's full rule
//   2. known game group          -> group rule
//   3. another alias label       -> inherit its full rule
//   4. item wildcard pattern     -> item rule
//   5. exact internal item name  -> item rule
//   6. exact item display name   -> item rule
//   7. nothing                   -> warn (if enabled) and ignore
// Groups outrank bare label references so a token like "Resources" keeps
// meaning the game group even when a "Resources" crate label exists; "@"
// forces the label (which is how translations reference group-named crates).
// "-token" entries are exclusions resolved with the same cascade. Labels with
// an alias resolve through their alias tokens; any other label resolves
// literally, so a crate signed "foodCan*" works without configuration.
// Results are cached; the resolver is rebuilt when configuration reloads.
public sealed class CategoryRuleResolver
{
	private readonly IItemCatalog catalog;
	private readonly ISorterLog log;
	private readonly AliasConfiguration aliases;
	private readonly Dictionary<string, CategoryRule> cache = new(StringComparer.Ordinal);
	private readonly HashSet<string> resolvingLabels = new(StringComparer.Ordinal);

	public CategoryRuleResolver(IItemCatalog catalog, ISorterLog log, AliasConfiguration aliases)
	{
		this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
		this.log = log ?? throw new ArgumentNullException(nameof(log));
		this.aliases = aliases ?? throw new ArgumentNullException(nameof(aliases));
	}

	public CategoryRule Resolve(string label)
	{
		string key = label.Trim().ToLowerInvariant();
		if (cache.TryGetValue(key, out var cached))
		{
			return cached;
		}
		if (!resolvingLabels.Add(key))
		{
			log.Warning($"Label '{key}' is part of a circular alias reference; the loop is ignored");
			return new CategoryRule(key, new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), int.MaxValue);
		}
		try
		{
			IReadOnlyList<string> tokens;
			int priority;
			if (aliases.TryGetCanonicalLabel(key, out string canonicalLabel))
			{
				tokens = aliases.TokensByLabel[canonicalLabel];
				priority = aliases.GetPriority(canonicalLabel);
			}
			else
			{
				tokens = new[] { key };
				priority = int.MaxValue;
			}
			CategoryRule rule = BuildRule(key, tokens, priority);
			cache[key] = rule;
			log.Info($"Label '{key}' resolved: {rule.Describe()}");
			return rule;
		}
		finally
		{
			resolvingLabels.Remove(key);
		}
	}

	// Rules for the labels that actually route something; labels resolving to
	// nothing are skipped (their crates then only receive top-ups).
	public IReadOnlyList<CategoryRule> ResolveMany(IEnumerable<string> labels)
	{
		var rules = new List<CategoryRule>();
		foreach (string label in labels)
		{
			CategoryRule rule = Resolve(label);
			if (!rule.IsEmpty)
			{
				rules.Add(rule);
			}
		}
		return rules;
	}

	public void ResolveAllAliasLabels()
	{
		foreach (string label in aliases.TokensByLabel.Keys)
		{
			Resolve(label);
		}
	}

	// Every item the rule routes, sorted by internal name. For stow what.
	public List<CatalogItem> EnumerateMatches(CategoryRule rule)
	{
		var matches = new List<CatalogItem>();
		foreach (CatalogItem item in catalog.Items)
		{
			if (rule.Matches(item, MatchTier.Item) || rule.Matches(item, MatchTier.Group))
			{
				matches.Add(item);
			}
		}
		matches.Sort((a, b) => string.CompareOrdinal(a.NameLower, b.NameLower));
		return matches;
	}

	private CategoryRule BuildRule(string label, IReadOnlyList<string> tokens, int priority)
	{
		var groups = new HashSet<string>(StringComparer.Ordinal);
		var includeItems = new HashSet<string>(StringComparer.Ordinal);
		var excludeItems = new HashSet<string>(StringComparer.Ordinal);
		foreach (string rawToken in tokens)
		{
			string token = rawToken.Trim();
			bool exclude = token.StartsWith("-");
			if (exclude)
			{
				token = token.Substring(1).Trim();
			}
			bool explicitReference = token.StartsWith("@");
			if (explicitReference)
			{
				token = token.Substring(1).Trim();
			}
			if (token.Length == 0)
			{
				continue;
			}
			string tokenLower = token.ToLowerInvariant();
			if (!explicitReference && catalog.IsKnownGroup(tokenLower))
			{
				if (exclude)
				{
					AddGroupMembers(tokenLower, excludeItems);
				}
				else
				{
					groups.Add(tokenLower);
				}
				continue;
			}
			if (TryMergeLabelReference(label, tokenLower, exclude, groups, includeItems, excludeItems))
			{
				continue;
			}
			if (explicitReference)
			{
				if (aliases.WarnUnknownGroups)
				{
					log.Warning($"Label '{label}': '@{token}' matches no alias label; ignored.");
				}
				continue;
			}
			HashSet<string> target = exclude ? excludeItems : includeItems;
			if (!AddItemMatches(tokenLower, target) && aliases.WarnUnknownGroups)
			{
				log.Warning(
					$"Label '{label}': '{token}' matched no game group, item name or pattern; ignored. " +
					$"Use 'stow what {token}' or 'stow search <search>' to investigate, or set WarnUnknownGroups = 0 to silence this.");
			}
		}
		return new CategoryRule(label, groups, includeItems, excludeItems, priority);
	}

	// "Dosen = Cans" (or "@Cans"): inherit the referenced label's full rule,
	// exclusions included, so translated crates behave exactly like the
	// original. Excluding a reference subtracts everything it matches.
	private bool TryMergeLabelReference(
		string currentLabel,
		string tokenLower,
		bool exclude,
		HashSet<string> groups,
		HashSet<string> includeItems,
		HashSet<string> excludeItems)
	{
		if (!aliases.TryGetCanonicalLabel(tokenLower, out string referencedLabel)
			|| referencedLabel == currentLabel)
		{
			return false;
		}
		CategoryRule referenced = Resolve(referencedLabel);
		if (exclude)
		{
			excludeItems.UnionWith(referenced.IncludeItemsView);
			foreach (string group in referenced.Groups)
			{
				AddGroupMembers(group, excludeItems);
			}
		}
		else
		{
			referenced.CopyMatchersInto(groups, includeItems, excludeItems);
		}
		return true;
	}

	private void AddGroupMembers(string groupLower, HashSet<string> target)
	{
		foreach (CatalogItem item in catalog.Items)
		{
			if (item.IsInGroup(groupLower))
			{
				target.Add(item.NameLower);
			}
		}
	}

	private bool AddItemMatches(string tokenLower, HashSet<string> target)
	{
		int before = target.Count;
		if (WildcardPattern.IsPattern(tokenLower))
		{
			foreach (CatalogItem item in catalog.Items)
			{
				if (WildcardPattern.Matches(item.NameLower, tokenLower))
				{
					target.Add(item.NameLower);
				}
			}
		}
		else if (catalog.FindByInternalName(tokenLower) != null)
		{
			target.Add(tokenLower);
		}
		else
		{
			IReadOnlyList<CatalogItem> byDisplay = catalog.FindByDisplayName(tokenLower);
			if (byDisplay != null)
			{
				foreach (CatalogItem item in byDisplay)
				{
					target.Add(item.NameLower);
				}
			}
		}
		return target.Count > before;
	}
}
