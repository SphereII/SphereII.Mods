using HarmonyLib;
using ICSharpCode.WpfDesign.XamlDom;
using System.IO;
using System.Text;
using System.Xml;

namespace Harmony.XUiC
{
    public class TargetHealthBar
    {
        private static readonly string AdvFeatureClass = "AdvancedUI";
        private static readonly string Feature = "ShowTargetHealthBar";

        [HarmonyPatch(typeof(XUiC_TargetBar))]
        [HarmonyPatch("ParseAttribute")]
        public class XUICTargetHealthBar
        {
            public static void Postfix(XUiC_TargetBar __instance, string _name, ref XUiC_TargetBar.EVisibility ___visibility)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;

                if (_name == "visibility")
                    ___visibility = XUiC_TargetBar.EVisibility.Always;

            }
        }
    }
}