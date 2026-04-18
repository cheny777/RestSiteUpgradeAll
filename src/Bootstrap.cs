using System.Reflection;
using System.Threading;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace RestSiteUpgradeAll;

[ModInitializer(nameof(Initialize))]
internal static class Bootstrap
{
    private static int _initialized;

    internal static void Initialize()
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1)
        {
            return;
        }

        Log.Info("Initializing.");
        var harmony = new Harmony("rest_site_upgrade_all");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        Log.Info("Applied Harmony patches.");
    }
}
