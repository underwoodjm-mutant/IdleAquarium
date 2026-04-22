using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]

#if UNITY_EDITOR

#endif

public class ObjectGlow : MonoBehaviour
{
    SkinnedMeshRenderer _myMeshRenderer;

    bool _isGlowing = false;
    Color _startingEmissionColor;

    [SerializeField, Range(0,150)]
    private float _intensity = 30f;
    private float _currentIntensity;

    //Point Light Variables
    GameObject _lightObject;
    Light _myLight;
    float _lightRange = 25;

    public float Intensity { get => _intensity; set => _intensity = value; }
    public bool IsGlowing { get => _isGlowing; }
    public float LightRange { get => _lightRange; set => _lightRange = value; }

    [Header("Debug")]
    [SerializeField]
    bool _glowOn;

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("Start Function on: " + gameObject.name);

        _myMeshRenderer = GetComponent<SkinnedMeshRenderer>();

        _startingEmissionColor = _myMeshRenderer.material.GetColor("_EmissionColor");

        if (Application.isPlaying)
        {
            _lightObject = new GameObject("Light Object");
            _lightObject.transform.position = transform.position + (Vector3.up * 2);
            _lightObject.transform.SetParent(transform);
            _lightObject.SetActive(false);
            _myLight = _lightObject.AddComponent<Light>();
            _myLight.type = LightType.Point;
            _myLight.range = _lightRange;
            _myLight.shadows = LightShadows.Soft;
            _myLight.color = _startingEmissionColor * _intensity / 2;
        }
    }

    SkinnedMeshRenderer GetRenderer()
    {
        if (!_myMeshRenderer)
        {
            _myMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            return _myMeshRenderer;
        }
        else
        {
            return _myMeshRenderer;
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (_glowOn)
        {
            GetRenderer().material.SetColor("_EmissionColor", _startingEmissionColor * _intensity);
        }
        else
        {
            GetRenderer().material.SetColor("_EmissionColor", _startingEmissionColor);
        }
    }
#endif

    public void GlowOn()
    {
        GetRenderer().sharedMaterial.SetColor("_EmissionColor", _startingEmissionColor * _intensity);
        _currentIntensity = _intensity;
        _myLight.color = _startingEmissionColor * _intensity / 2;
        _lightObject.SetActive(true);
        _isGlowing = true;
    }

    public void UpdateGlow()
    {
        if (_isGlowing && _intensity != _currentIntensity)
        {
            GetRenderer().sharedMaterial.SetColor("_EmissionColor", _startingEmissionColor * _intensity);
            _myLight.color = _startingEmissionColor * _intensity / 2;
            _currentIntensity = _intensity;
        }
    }

    public void GlowOff()
    {
        GetRenderer().sharedMaterial.SetColor("_EmissionColor", _startingEmissionColor);
        _lightObject.SetActive(false);
        _isGlowing = false;
    }
}
