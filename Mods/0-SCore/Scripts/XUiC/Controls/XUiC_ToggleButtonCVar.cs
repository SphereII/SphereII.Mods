//	<togglebuttonCVar name="toggleLockPick" cvar="LegacyLockPick" caption_key="xuiLockPicking" tooltip_key="xuiLockPickingToolTip" width="290" height="32" depth="3" font_size="24" crispness="OnDesktop" effect="Outline8" effect_distance="1,1" effect_color="20,20,20,230" />

using UnityEngine;

public class XUiC_ToggleButtonCVar : XUiC_ToggleButton {
    public string CVarName;

    public delegate void XUiEvent_ToggleButtonCVarValueChanged(XUiC_ToggleButtonCVar _sender, bool _newValue);
    public event XUiEvent_ToggleButtonCVarValueChanged OnValueChanged;

    public override bool ParseAttribute(string name, string value, XUiController _parent) {
        var flag = base.ParseAttribute(name, value, _parent);
        if (flag) return true;

        if (name != "cvar") return false;
        CVarName = value;
        return true;
    }
}