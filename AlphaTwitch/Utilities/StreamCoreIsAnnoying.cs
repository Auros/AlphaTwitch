using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamCore;

namespace AlphaTwitch.Utilities
{
    [HarmonyPatch(typeof(StreamCore.Plugin))]
    [HarmonyPatch("Log")]
    class StreamCoreIsAnnoying
    {
        static bool Prefix()
        {
            return false;
        }
    }
}
