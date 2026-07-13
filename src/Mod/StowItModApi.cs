using System.Reflection;
using HarmonyLib;
using M00StowIt.Game;

namespace M00StowIt.Mod;

public class StowItModApi : IModApi
{
	public void InitMod(global::Mod modInstance)
	{
		StowItMod.Runtime = ModRuntime.Create(modInstance.Path);
		var harmony = new Harmony("com.m00.stowit");
		harmony.PatchAll(Assembly.GetExecutingAssembly());
	}
}
