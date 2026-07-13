using System.Collections.Generic;

namespace M00StowIt.Mod;

// The "as <subcommand>" console entry point: stow reload | groups | what | search | alias.
public class ConsoleCmdStowIt : ConsoleCmdAbstract
{
	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		SdtdConsole console = SingletonMonoBehaviour<SdtdConsole>.Instance;
		if (_params.Count == 0)
		{
			StowItConsole.Help(console);
			return;
		}
		string subcommand = _params[0].ToLowerInvariant();
		List<string> args = _params.GetRange(1, _params.Count - 1);
		switch (subcommand)
		{
			case "reload":
				StowItConsole.Reload(console);
				break;
			case "groups":
				StowItConsole.Groups(console);
				break;
			case "what":
				StowItConsole.What(console, args);
				break;
			case "search":
			case "items":
				StowItConsole.Search(console, args);
				break;
			case "alias":
				StowItConsole.Alias(console, args);
				break;
			case "help":
				StowItConsole.Help(console);
				break;
			default:
				console.Output($"[StowIt] Unknown subcommand '{subcommand}'.");
				StowItConsole.Help(console);
				break;
		}
	}

	public override string[] getCommands()
	{
		return new string[2] { "stow", "stowit" };
	}

	public override string getDescription()
	{
		return "StowIt: stow reload | groups | what <label> | search <text> | alias ...";
	}
}
