using System;
using UnityEngine;

public class XUiC_RH_GamePrefSelector : XUiController
{
	private XUiV_Label controlLabel;

	private XUiC_ComboBoxList<XUiC_GamePrefSelector.GameOptionValue> controlCombo;

	private XUiC_TextInput controlText;

	private Color enabledColor;

	private Color disabledColor = new Color(0.625f, 0.625f, 0.625f);

	private int[] valuesFromXml;

	private string[] namesFromXml;

	private string valueLocalizationPrefixFromXml;

	private bool isTextInput;

	private GamePrefs.EnumType valueType;

	private Func<int, int> valuePreDisplayModifierFunc;

	public Action<XUiC_RH_GamePrefSelector, EnumGamePrefs> OnValueChanged;

	private bool enabled = true;

	public XUiC_TextInput ControlText => controlText;

	public bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			if (enabled != value)
			{
				enabled = value;
				controlCombo.Enabled = value;
				controlText.Enabled = value;
				controlLabel.Color = (value ? enabledColor : disabledColor);
			}
		}
	}

	public override void Init()
	{
		base.Init();
		controlLabel = (XUiV_Label)GetChildById("ControlLabel").ViewComponent;
		enabledColor = controlLabel.Color;
		controlCombo = GetChildById("ControlCombo").GetChildByType<XUiC_ComboBoxList<XUiC_GamePrefSelector.GameOptionValue>>();
		controlCombo.OnValueChanged += ControlCombo_OnValueChanged;
		controlText = GetChildById("ControlText").GetChildByType<XUiC_TextInput>();
		controlText.OnChangeHandler += ControlText_OnChangeHandler;
		if (!isTextInput)
		{
			SetupOptions();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		controlCombo.ViewComponent.IsVisible = !isTextInput;
		controlText.ViewComponent.IsVisible = isTextInput;
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		switch (_name)
		{
		case "is_textinput":
			isTextInput = StringParsers.ParseBool(_value);
			return true;
		case "value_type":
			valueType = EnumUtils.Parse<GamePrefs.EnumType>(_value, _ignoreCase: true);
			return true;
		case "values":
			if (_value.Length > 0)
			{
				string[] array = _value.Split(',');
				valuesFromXml = new int[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					valuesFromXml[i] = StringParsers.ParseSInt32(array[i]);
				}
			}
			return true;
		case "display_names":
			if (_value.Length > 0)
			{
				namesFromXml = _value.Split(',');
				for (int j = 0; j < namesFromXml.Length; j++)
				{
					namesFromXml[j] = namesFromXml[j].Trim();
				}
			}
			return true;
		default:
			return base.ParseAttribute(_name, _value, _parent);
		case "value_localization_prefix":
			if (_value.Length > 0)
			{
				valueLocalizationPrefixFromXml = _value.Trim();
			}
			return true;
		}
	}

	private void SetupOptions()
	{
		string[] array = null;
		if (valueType == GamePrefs.EnumType.Int)
		{
			if (valuesFromXml == null)
			{
				valuesFromXml = new int[namesFromXml.Length];
				for (int i = 0; i < valuesFromXml.Length; i++)
				{
					valuesFromXml[i] = i;
				}
			}
			array = new string[valuesFromXml.Length];
			if (namesFromXml == null || namesFromXml.Length != valuesFromXml.Length)
			{
				for (int j = 0; j < valuesFromXml.Length; j++)
				{
					if (namesFromXml != null && j < namesFromXml.Length)
					{
						array[j] = Localization.Get(namesFromXml[j]);
						continue;
					}
					int num = valuesFromXml[j];
					if (valuePreDisplayModifierFunc != null)
					{
						num = valuePreDisplayModifierFunc(num);
					}
					array[j] = string.Format(Localization.Get(valueLocalizationPrefixFromXml + ((num == 1) ? "" : "s")), num);
				}
			}
			else
			{
				for (int k = 0; k < namesFromXml.Length; k++)
				{
					array[k] = Localization.Get(namesFromXml[k]);
				}
			}
		}
		else if (valueType == GamePrefs.EnumType.Bool)
		{
			valuesFromXml = new int[2] { 0, 1 };
			array = new string[2]
			{
				Localization.Get("goOff"),
				Localization.Get("goOn")
			};
		}
		controlCombo.Elements.Clear();
		for (int l = 0; l < valuesFromXml.Length; l++)
		{
			XUiC_GamePrefSelector.GameOptionValue item = new XUiC_GamePrefSelector.GameOptionValue(valuesFromXml[l], array[l]);
			controlCombo.Elements.Add(item);
		}
	}

	private void ControlText_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
	}

	private void ControlCombo_OnValueChanged(XUiController _sender, XUiC_GamePrefSelector.GameOptionValue _oldValue, XUiC_GamePrefSelector.GameOptionValue _newValue)
	{
		switch (viewComponent.ID)
		{
		case "RH_CityZombieMultiplier":
			RH_Options.GameOptions.CityZombieMultiplier = _newValue.Value;
			break;
		case "RH_HeadShotOnly":
			RH_Options.GameOptions.HeadShotOnly = _newValue.Value == 1;
			break;
		case "RH_ShowFetchItemOnCompass":
			RH_Options.GameOptions.ShowFetchItemOnCompass = _newValue.Value == 1;
			break;
		case "RH_ShowFetchDistanceIndicator":
			RH_Options.GameOptions.ShowFetchDistanceIndicator = _newValue.Value == 1;
			break;
		case "RH_ZombieRage":
			RH_Options.GameOptions.ZombieRage = _newValue.Value == 1;
			break;
		case "RH_SightRange":
			RH_Options.GameOptions.SightRange = _newValue.Value;
			break;
		case "RH_WanderingHordeFrequency":
			RH_Options.GameOptions.WanderingHordeFrequency = _newValue.Value;
			break;
		case "RH_WanderingHordeMultiplier":
			RH_Options.GameOptions.WanderingHordeMultiplier = _newValue.Value;
			break;
		}
		CheckDefaultValue();
	}

	public void SetCurrentValue()
	{
		try
		{
			switch (viewComponent.ID)
			{
			case "RH_CityZombieMultiplier":
				controlCombo.SelectedIndex = RH_Options.GameOptions.CityZombieMultiplier - 1;
				break;
			case "RH_HeadShotOnly":
				controlCombo.SelectedIndex = (RH_Options.GameOptions.HeadShotOnly ? 1 : 0);
				break;
			case "RH_ShowFetchItemOnCompass":
				controlCombo.SelectedIndex = (RH_Options.GameOptions.ShowFetchItemOnCompass ? 1 : 0);
				break;
			case "RH_ShowFetchDistanceIndicator":
				controlCombo.SelectedIndex = (RH_Options.GameOptions.ShowFetchDistanceIndicator ? 1 : 0);
				break;
			case "RH_ZombieRage":
				controlCombo.SelectedIndex = (RH_Options.GameOptions.ZombieRage ? 1 : 0);
				break;
			case "RH_SightRange":
				controlCombo.SelectedIndex = RH_Options.ConvertSightRangeToIndex();
				break;
			case "RH_WanderingHordeFrequency":
				controlCombo.SelectedIndex = RH_Options.GameOptions.WanderingHordeFrequency;
				break;
			case "RH_WanderingHordeMultiplier":
				controlCombo.SelectedIndex = RH_Options.GameOptions.WanderingHordeMultiplier - 1;
				break;
			}
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		CheckDefaultValue();
	}

	private void CheckDefaultValue()
	{
		bool flag = IsDefaultValueForGameMode();
		controlText.ActiveTextColor = (flag ? Color.white : Color.yellow);
		controlCombo.TextColor = (flag ? Color.white : Color.yellow);
	}

	private bool IsDefaultValueForGameMode()
	{
		return viewComponent.ID switch
		{
			"RH_CityZombieMultiplier" => controlCombo.Value.Value == 1, 
			"RH_HeadShotOnly" => controlCombo.Value.Value != 1, 
			"RH_ShowFetchItemOnCompass" => controlCombo.Value.Value == 1, 
			"RH_ShowFetchDistanceIndicator" => controlCombo.Value.Value == 1, 
			"RH_ZombieRage" => controlCombo.Value.Value == 1, 
			"RH_SightRange" => controlCombo.Value.Value == 0, 
			"RH_WanderingHordeFrequency" => controlCombo.Value.Value == 0, 
			"RH_WanderingHordeMultiplier" => controlCombo.Value.Value == 1, 
			_ => false, 
		};
	}

	private void SetVisible(bool _visible)
	{
		viewComponent.IsVisible = _visible;
	}
}
