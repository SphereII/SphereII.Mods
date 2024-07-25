using HarmonyLib;
using ICSharpCode.WpfDesign.XamlDom;
using System.IO;
using System.Text;
using System.Xml;

namespace Harmony.XUiC
{
    public class XUiControllerPatches
    {

        [HarmonyPatch(typeof(XUiController))]
        [HarmonyPatch("GetBindingValue")]
        public class XUiControllerGetBindingValue
        {
            public static bool Postfix(bool __result, ref string _value, string _bindingName)
            {
                if (_bindingName == null) return false;

                // modinfo=0-SCore:Version
                // modinfo=0-SCore:Description
                if ( _bindingName.StartsWith("modinfo="))
                {
                    _value = "N/A";

                    var filter = _bindingName.Replace("modinfo=", "").Split(':');
                    var modletName = "";
                    var info = "";

                    // Check if filer has the name and filter.
                    if (filter.Length > 0)
                        modletName = filter[0];
                    if (filter.Length > 1)
                        info = filter[1].ToLower();

                    // If we don't have a mod name, then don't do anything.
                    if (string.IsNullOrEmpty(modletName)) return false;

                    // Default to version if not specified.
                    if (string.IsNullOrEmpty(info)) info = "version";

                    var mod = ModManager.GetMod(modletName);
                    if (mod != null)
                    {
                        if (info == "version")
                            _value = mod.Version.ToString();
                        if (info == "author")
                            _value = mod.Author;
                        if (info == "description")
                            _value = mod.Description;
                    }

                    return true;
                }

                return __result;

            }
        }
    }
}