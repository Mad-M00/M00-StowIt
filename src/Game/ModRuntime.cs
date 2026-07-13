using M00StowIt.Core;

namespace M00StowIt.Game;

// Composition root. Built once in InitMod; Harmony patches and console
// commands reach the object graph through StowItMod.Runtime (patches are
// static entry points, so one static bridge is unavoidable — everything
// behind it is instance-based and testable).
internal sealed class ModRuntime
{
	private readonly string configPath;
	private readonly string aliasPath;
	private GameItemCatalog catalog;
	private CategoryRuleResolver resolver;
	private bool aliasRulesPrepared;

	public string AliasFilePath => aliasPath;

	private ModRuntime(string modPath)
	{
		Log = new UnitySorterLog();
		configPath = modPath + "/StowItConfig.xml";
		aliasPath = modPath + "/CrateLabels.txt";
		LoadConfigurationFiles();
		Operations = new OperationTracker();
		Ui = new PlayerUiContext();
		Stash = new SortingOperations(
			Ui,
			Operations,
			new ContainerScanner(() => Settings, Log),
			new ActionRepeatTracker(windowSeconds: 2f),
			Log,
			() => Settings,
			() => Aliases,
			() => Resolver,
			() => Catalog,
			PrepareAliasRules);
	}

	public static ModRuntime Create(string modPath) => new(modPath);

	public ISorterLog Log { get; }

	public PlayerUiContext Ui { get; }

	public OperationTracker Operations { get; }

	public SortingOperations Stash { get; }

	public StowItSettings Settings { get; private set; }

	public AliasConfiguration Aliases { get; private set; }

	// Null until a world is loaded and item data exists.
	public GameItemCatalog Catalog => catalog ??= GameItemCatalog.TryCreate(Log);

	public CategoryRuleResolver Resolver
	{
		get
		{
			GameItemCatalog current = Catalog;
			if (current == null)
			{
				return null;
			}
			return resolver ??= new CategoryRuleResolver(current, Log, Aliases);
		}
	}

	public void ReloadConfiguration()
	{
		LoadConfigurationFiles();
		catalog = null;
		resolver = null;
		aliasRulesPrepared = false;
	}

	// Called when the player leaves the world. The runtime is static-rooted,
	// so anything it references would survive the world teardown: the old UI
	// controller graph and the item catalog (which wraps every ItemClass of
	// the unloaded world). Dropping them here frees that memory and prevents
	// stale item data from leaking into the next world the player loads.
	public void HandleWorldUnloaded()
	{
		Ui.Clear();
		Operations.Current = SortOperation.None;
		catalog = null;
		resolver = null;
		aliasRulesPrepared = false;
	}

	// Resolves every alias label once (logging what each matched) so config
	// problems surface in the log before the first quick stack.
	public void PrepareAliasRules()
	{
		if (aliasRulesPrepared)
		{
			return;
		}
		CategoryRuleResolver current = Resolver;
		if (current == null)
		{
			return;
		}
		current.ResolveAllAliasLabels();
		aliasRulesPrepared = true;
	}

	private void LoadConfigurationFiles()
	{
		Settings = ConfigurationLoader.LoadSettings(configPath, Log);
		Aliases = ConfigurationLoader.LoadAliases(aliasPath, Log);
	}
}

internal static class StowItMod
{
	public static ModRuntime Runtime;
}
