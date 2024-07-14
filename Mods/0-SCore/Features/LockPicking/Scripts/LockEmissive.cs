using UnityEngine;

public class LockEmissive : MonoBehaviour
{
    [Header("Behavior")] public bool pulse = true;

    public bool breakpoint;
    public bool success;
    public bool off;

    [Header("Settings")] [Range(0f, 5f)] public float pulseSpeedMod = 0.5f;

    [Header("Colors")] public Color hightlightColor = Color.yellow;

    public Color[] colors = { Color.cyan, Color.red, Color.yellow, Color.green, Color.blue, Color.magenta, Color.white, Color.grey, Color.yellow, Color.blue };

    [Header("Renderers")] [SerializeField] private Renderer[] _renderers;

    public Renderer highlightRenderer;

    // Hidden from Inspector
    [HideInInspector] public float breakpointValue;
    [HideInInspector] public float successValue;
    private int _colorIndex;

    // Private variables
    private float _pulseValue;

    private void Start()
    {
    }

    private void Update()
    {
        if (off)
        {
            SetAllMaterials(0f);
        }
        else if (pulse)
        {
            _pulseValue = Mathf.PingPong(Time.time * pulseSpeedMod, 1); // Pulse the value over time
            SetAllMaterials(_pulseValue); // Set the values
        }
        else if (breakpoint)
        {
            SetAllMaterials(breakpointValue); // Set the values
        }
        else if (success)
        {
            SetAllMaterials(Mathf.Clamp(Mathf.Abs(successValue - 1), 0, 1)); // Set the values
        }

        if (highlightRenderer)
            SetHighlightMaterial();
    }

    private Color _activeColor()
    {
        return colors[_colorIndex];
    }

    public void SetRenders(Renderer[] newRenders)
    {
        _renderers = newRenders;
    }

    public void SetHighlightRenderer(Renderer value)
    {
        highlightRenderer = value;
    }

    private void SetHighlightMaterial()
    {
        // sphereii: disable the high light color 
        //highlightRenderer.material.SetColor("_EmissionColor", hightlightColor);
        highlightRenderer.material.SetColor("_EmissionColor", Color.clear);
    }

    /// <summary>
    ///     Sets all materials attached to the renderers array
    /// </summary>
    /// <param name="value"></param>
    private void SetAllMaterials(float value)
    {
        if (_renderers == null) return;
        for (var i = 0; i < _renderers.Length; i++)
            if (_renderers[i] != null && _renderers[i].gameObject != null)
                if (_renderers[i].gameObject.activeSelf)
                    SetMaterial(_renderers[i].material, value);
    }

    /// <summary>
    ///     Sets a single material
    /// </summary>
    /// <param name="material"></param>
    /// <param name="value"></param>
    private void SetMaterial(Material material, float value)
    {
        var finalColor = _activeColor() * Mathf.LinearToGammaSpace(value);

        //material.SetColor("_EmissionColor", finalColor);

        // sphereii: give it a nice bronze color
        material.SetColor("_EmissionColor", new Color(0.666f, 0.588f, 0.275f, 1f));
    }

    public void NextColor()
    {
        _colorIndex = _colorIndex + 1 >= colors.Length ? 0 : _colorIndex + 1;
    }

    public void PrevColor()
    {
        _colorIndex = _colorIndex - 1 < 0 ? colors.Length - 1 : _colorIndex - 1;
    }

    public void SetPulse()
    {
        pulse = true;
        breakpoint = false;
        success = false;
        off = false;
    }

    public void SetBreakpoint()
    {
        pulse = false;
        breakpoint = true;
        success = false;
        off = false;
    }

    public void SetSuccess()
    {
        pulse = false;
        breakpoint = false;
        success = true;
        off = false;
    }

    public void SetOff()
    {
        pulse = false;
        breakpoint = false;
        success = false;
        off = true;
    }
}