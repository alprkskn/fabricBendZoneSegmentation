﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
public class PProcessScript : MonoBehaviour
{
    enum DownSampleMode { Off, Half, Quarter }


    private Camera _camera;
    private Material _edgeDetectMaterial;
    private Material _gaussianBlurMaterial;
    private Material _blendMaterial;

    public Shader DepthNormalsShader;
    public int DepthNormalDownsampleCount = 3;
    private Material DepthNormalsMaterial;

    #region EdgeDetection
    public float angleThreshold = 80, depthWeight = 300;
    [SerializeField, Range(0, 8)] private int _kernelRadius = 1;
    [SerializeField, Range(0.5f, 2)] private float _texelSizeDivider = 2;
    [SerializeField] private Shader _edgeDetectShader;
    [SerializeField] private Shader _blendShader;
    [SerializeField] private Color _edgeColor;
    #endregion

    #region GaussianBlur
    [SerializeField] private Shader _blurShader;
    [SerializeField] private DownSampleMode _downSampleMode = DownSampleMode.Quarter;
    [SerializeField, Range(0, 8)] private int _iteration = 4;
    #endregion

    #region UI Controllers
    [SerializeField] private Slider _gaussSlider;
    [SerializeField] private Slider _kernelSlider;
    [SerializeField] private Text _kernelText;
    [SerializeField] private Text _gaussText;
    #endregion

    // Creates a private material used to the effect
    void Awake()
    {
        _camera = GetComponent<Camera>();
        _edgeDetectMaterial = new Material(_edgeDetectShader);
        _gaussianBlurMaterial = new Material(_blurShader);
        _blendMaterial = new Material(_blendShader);
        _camera.depthTextureMode = DepthTextureMode.DepthNormals;

        _gaussSlider.value = _iteration;
        _gaussText.text = _iteration.ToString();
        _gaussSlider.onValueChanged.AddListener((x) =>
        {
            _iteration = (int)x;
            _gaussText.text = _iteration.ToString();
        });

        _kernelSlider.value = _kernelRadius;
        _kernelText.text = _kernelRadius.ToString();
        _kernelSlider.onValueChanged.AddListener((x) =>
        {
            _kernelRadius = (int)x;
            _kernelText.text = _kernelRadius.ToString();
        });

        DepthNormalsMaterial = new Material(DepthNormalsShader);
    }

    // Postprocess the image
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderTexture rt1 = RenderTexture.GetTemporary(source.width, source.height);
        //RenderTexture rt2 = RenderTexture.GetTemporary(source.width / DepthNormalDownsampleCount
        //    , source.height / DepthNormalDownsampleCount);
        
        //Graphics.Blit(source, rt2, DepthNormalsMaterial);
        //_edgeDetectMaterial.SetTexture("_DepthNormalTex", rt2);
        //_edgeDetectMaterial.SetInt("_DownSample", DepthNormalDownsampleCount);

        EdgeDetectionPass(source, rt1);
        BlurPass(rt1, destination);

        Graphics.Blit(source, destination, _blendMaterial);
        Graphics.Blit(rt1, destination, _blendMaterial);

        RenderTexture.ReleaseTemporary(rt1);
        //RenderTexture.ReleaseTemporary(rt2);

    }

    void EdgeDetectionPass(RenderTexture source, RenderTexture destination)
    {
        _edgeDetectMaterial.SetColor("_EdgeColor", _edgeColor);
        _edgeDetectMaterial.SetFloat("_angleThreshold", angleThreshold);
        _edgeDetectMaterial.SetFloat("_depthWeight", depthWeight);
        _edgeDetectMaterial.SetInt("_kernelRadius", _kernelRadius);
        _edgeDetectMaterial.SetFloat("_texelSizeDivider", _texelSizeDivider);
        Graphics.Blit(source, destination, _edgeDetectMaterial);
    }

    void BlurPass(RenderTexture source, RenderTexture destination)
    {
        RenderTexture rt1, rt2;

        if (_downSampleMode == DownSampleMode.Half)
        {
            rt1 = RenderTexture.GetTemporary(source.width / 2, source.height / 2);
            rt2 = RenderTexture.GetTemporary(source.width / 2, source.height / 2);
            Graphics.Blit(source, rt1);
        }
        else if (_downSampleMode == DownSampleMode.Quarter)
        {
            rt1 = RenderTexture.GetTemporary(source.width / 4, source.height / 4);
            rt2 = RenderTexture.GetTemporary(source.width / 4, source.height / 4);
            Graphics.Blit(source, rt1, _gaussianBlurMaterial, 0);
        }
        else
        {
            rt1 = RenderTexture.GetTemporary(source.width, source.height);
            rt2 = RenderTexture.GetTemporary(source.width, source.height);
            Graphics.Blit(source, rt1);
        }

        for (var i = 0; i < _iteration; i++)
        {
            Graphics.Blit(rt1, rt2, _gaussianBlurMaterial, 1);
            Graphics.Blit(rt2, rt1, _gaussianBlurMaterial, 2);
        }

        Graphics.Blit(rt1, destination);

        RenderTexture.ReleaseTemporary(rt1);
        RenderTexture.ReleaseTemporary(rt2);
    }
}