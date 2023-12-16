//------------------------------------------------------------------------------------------------------------------
// Volumetric Fog & Mist 2
// Created by Kronnect
//------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace VolumetricFogAndMist {

    public partial class VolumetricFog : MonoBehaviour {

        [Tooltip("Fits the fog altitude to the terrain heightmap")]
        public bool _terrainFit;

        [NonSerialized]
        public bool forceTerrainCaptureUpdate;

        LayerMask lastTerrainLayerMask;


        public bool terrainFit {
            get { return _terrainFit; }
            set { if (_terrainFit != value) {
                    _terrainFit = value;
                    UpdateMaterialProperties();
                    isDirty = true;
                }
            }
        }

        public bool isTerrainFitActive {
            get { return _terrainFit && isFogAreaActive; }
        }

        public HeightmapCaptureResolution _terrainFitResolution = HeightmapCaptureResolution._128;

        public HeightmapCaptureResolution terrainFitResolution {
            get { return _terrainFitResolution; }
            set {
                if (_terrainFitResolution != value) {
                    _terrainFitResolution = value;
                    UpdateMaterialProperties();
                    isDirty = true;
                }
            }
        }

        [Tooltip("Which objects will be included in the heightmap capture. By default all objects are included but you may want to restrict this to just the terrain.")]
        public LayerMask _terrainLayerMask = -1;

        public LayerMask terrainLayerMask {
            get { return _terrainLayerMask; }
            set {
                if (_terrainLayerMask != value) {
                    _terrainLayerMask = value;
                    UpdateMaterialProperties();
                    isDirty = true;
                }
            }
        }

        [Tooltip("The height of fog above terrain surface.")]
        public float _terrainFogHeight = 25f;

        public float terrainFogHeight {
            get { return _terrainFogHeight; }
            set {
                if (_terrainFogHeight != value) {
                    _terrainFogHeight = value;
                    UpdateMaterialProperties();
                    isDirty = true;
                }
            }
        }

        public float _terrainFogMinAltitude = 0f;

        public float terrainFogMinAltitude {
            get { return _terrainFogMinAltitude; }
            set {
                if (_terrainFogMinAltitude != value) {
                    _terrainFogMinAltitude = value;
                    UpdateMaterialProperties();
                    isDirty = true;
                }
            }
        }

        public float _terrainFogMaxAltitude = 150f;

        public float terrainFogMaxAltitude {
            get { return _terrainFogMaxAltitude; }
            set {
                if (_terrainFogMaxAltitude != value) {
                    _terrainFogMaxAltitude = value;
                    UpdateMaterialProperties();
                    isDirty = true;
                }
            }
        }


        const string SURFACE_CAM_NAME = "SurfaceCam";

        public enum HeightmapCaptureResolution {
            _64 = 64,
            _128 = 128,
            _256 = 256,
            _512 = 512,
            _1024 = 1024
        }

        RenderTexture rt;

        Camera surfaceCam;
        int camStartFrameCount;
        Vector3 lastSurfaceCamPos;

        void DisposeSurfaceCapture() {
            DisableSurfaceCapture();
            if (rt != null) {
                rt.Release();
                DestroyImmediate(rt);
            }
        }

        void CheckSurfaceCapture() {
            if (surfaceCam == null) {
                Transform childCam = transform.Find(SURFACE_CAM_NAME);
                if (childCam != null) {
                    surfaceCam = childCam.GetComponent<Camera>();
                    if (surfaceCam == null) {
                        DestroyImmediate(childCam.gameObject);
                    }
                }
            }
        }

        void DisableSurfaceCapture() {
            if (surfaceCam != null) {
                surfaceCam.enabled = false;
            }
        }


        void SurfaceCaptureSupportCheck() {

            if (!isTerrainFitActive) {
                DisposeSurfaceCapture();
                return;
            }

            Transform childCam = transform.Find(SURFACE_CAM_NAME);
            if (childCam != null) {
                surfaceCam = childCam.GetComponent<Camera>();
            }

            bool needsSurfaceCapture = forceTerrainCaptureUpdate;
            if (surfaceCam == null) {
                if (childCam != null) {
                    DestroyImmediate(childCam.gameObject);
                }
                if (surfaceCam == null) {
                    GameObject camObj = new GameObject(SURFACE_CAM_NAME, typeof(Camera));
                    camObj.transform.SetParent(transform, false);
                    surfaceCam = camObj.GetComponent<Camera>();
                    surfaceCam.depthTextureMode = DepthTextureMode.None;
                    surfaceCam.clearFlags = CameraClearFlags.Depth;
                    surfaceCam.allowHDR = false;
                    surfaceCam.allowMSAA = false;
                }
                needsSurfaceCapture = true;
                surfaceCam.stereoTargetEye = StereoTargetEyeMask.None;
                surfaceCam.orthographic = true;
                surfaceCam.nearClipPlane = 1f;
            }

            if (rt != null && rt.width != (int)_terrainFitResolution) {
                if (surfaceCam.targetTexture == rt) {
                    surfaceCam.targetTexture = null;
                }
                rt.Release();
                DestroyImmediate(rt);
            }

            if (rt == null) {
                rt = new RenderTexture((int)_terrainFitResolution, (int)_terrainFitResolution, 24, RenderTextureFormat.Depth);
                rt.antiAliasing = 1;
                needsSurfaceCapture = true;
            }

            int thisLayer = 1 << gameObject.layer;
            if ((_terrainLayerMask & thisLayer) != 0) {
                _terrainLayerMask &= ~thisLayer; // exclude fog layer
            }

            surfaceCam.cullingMask = _terrainLayerMask;
            surfaceCam.targetTexture = rt;

            if (_terrainLayerMask != lastTerrainLayerMask) {
                lastTerrainLayerMask = _terrainLayerMask;
                needsSurfaceCapture = true;
            }

            if (isTerrainFitActive && needsSurfaceCapture) {
                PerformHeightmapCapture();
            } else {
                surfaceCam.enabled = false;
            }
        }

        /// <summary>
        /// Updates shadows on this volumetric fog
        /// </summary>
        public void PerformHeightmapCapture() {
            if (surfaceCam != null) {
                surfaceCam.enabled = true;
                camStartFrameCount = Time.frameCount;
                if (fogMat != null && !fogMat.IsKeywordEnabled(SKW_SURFACE)) {
                    fogMat.EnableKeyword(SKW_SURFACE);
                }
                forceTerrainCaptureUpdate = false;
            }
        }


        void SetupCameraCaptureMatrix() {

            Bounds fogBounds = GetBounds();
            Vector3 camPos = fogBounds.center;
            camPos.y = fogBounds.max.y;
            surfaceCam.farClipPlane = 10000;
            surfaceCam.transform.position = camPos;
            surfaceCam.transform.eulerAngles = new Vector3(90, 0, 0);
            Vector3 size = fogBounds.size;
            surfaceCam.orthographicSize = Mathf.Max(size.x * 0.5f, size.z * 0.5f);

            if (fogMat != null) {
                fogMat.SetMatrix(ShaderParams.SurfaceCaptureMatrix, surfaceCam.projectionMatrix * surfaceCam.worldToCameraMatrix);
                fogMat.SetTexture(ShaderParams.SurfaceDepthTexture, surfaceCam.targetTexture);
                fogMat.SetVector(ShaderParams.SurfaceData, new Vector4(camPos.y, _terrainFogHeight, _terrainFogMinAltitude, _terrainFogMaxAltitude));
            }
        }

        void SurfaceCaptureUpdate() {

            if (!isTerrainFitActive) return;

            if (surfaceCam == null) return;


            SetupCameraCaptureMatrix();

            if (!surfaceCam.enabled && lastSurfaceCamPos != surfaceCam.transform.position) {
                lastSurfaceCamPos = surfaceCam.transform.position;
                PerformHeightmapCapture();
            } else if (Time.frameCount > camStartFrameCount + 1 && Application.isPlaying) {
                surfaceCam.enabled = false;
            }
        }


    }


}