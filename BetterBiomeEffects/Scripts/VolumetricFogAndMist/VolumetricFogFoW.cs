//------------------------------------------------------------------------------------------------------------------
// Volumetric Fog & Mist
// Created by Ramiro Oliva (Kronnect)
//------------------------------------------------------------------------------------------------------------------
using UnityEngine;
using System.Collections.Generic;


namespace VolumetricFogAndMist {


    public enum MASK_TEXTURE_BRUSH_MODE {
        AddFog = 0,
        RemoveFog = 1
    }


    public partial class VolumetricFog : MonoBehaviour {

        const int MAX_SIMULTANEOUS_TRANSITIONS = 10000;

        #region Fog of War settings

        [SerializeField]
        bool _fogOfWarEnabled;

        public bool fogOfWarEnabled {
            get { return _fogOfWarEnabled; }
            set {
                if (value != _fogOfWarEnabled) {
                    _fogOfWarEnabled = value;
                    FogOfWarInit();
                    UpdateMaterialProperties();
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        Vector3 _fogOfWarCenter;

        public Vector3 fogOfWarCenter {
            get { return _fogOfWarCenter; }
            set {
                if (value != _fogOfWarCenter) {
                    _fogOfWarCenter = value;
                    UpdateMaterialProperties();
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        Vector3 _fogOfWarSize = new Vector3(1024, 0, 1024);

        public Vector3 fogOfWarSize {
            get { return _fogOfWarSize; }
            set {
                if (value != _fogOfWarSize) {
                    if (value.x > 0 && value.z > 0) {
                        _fogOfWarSize = value;
                        UpdateMaterialProperties();
                        isDirty = true;
                    }
                }
            }
        }

        [SerializeField, Range(32, 2048)]
        int _fogOfWarTextureSize = 256;

        public int fogOfWarTextureSize {
            get { return _fogOfWarTextureSize; }
            set {
                if (value != _fogOfWarTextureSize) {
                    if (value > 16) {
                        _fogOfWarTextureSize = value;
                        FogOfWarCheckTexture();
                        UpdateMaterialProperties();
                        isDirty = true;
                    }
                }
            }
        }

        [SerializeField, Range(0, 100)]
        float _fogOfWarRestoreDelay = 0;

        public float fogOfWarRestoreDelay {
            get { return _fogOfWarRestoreDelay; }
            set {
                if (value != _fogOfWarRestoreDelay) {
                    _fogOfWarRestoreDelay = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField, Range(0, 25)]
        float _fogOfWarRestoreDuration = 2f;

        public float fogOfWarRestoreDuration {
            get { return _fogOfWarRestoreDuration; }
            set {
                if (value != _fogOfWarRestoreDuration) {
                    _fogOfWarRestoreDuration = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField, Range(0, 1)]
        float _fogOfWarSmoothness = 1f;

        public float fogOfWarSmoothness {
            get { return _fogOfWarSmoothness; }
            set {
                if (value != _fogOfWarSmoothness) {
                    _fogOfWarSmoothness = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        bool _fogOfWarBlur;

        public bool fogOfWarBlur {
            get { return _fogOfWarBlur; }
            set {
                if (value != _fogOfWarBlur) {
                    _fogOfWarBlur = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        bool _fogOfWarReuseOther;

        public bool fogOfWarReuseOther {
            get { return _fogOfWarReuseOther; }
            set {
                if (value != _fogOfWarReuseOther) {
                    _fogOfWarReuseOther = value;
                    if (!_fogOfWarReuseOther) {
                        _fogOfWarOther = null;
                    }
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        VolumetricFog _fogOfWarOther;

        public VolumetricFog fogOfWarOther {
            get { return _fogOfWarOther; }
            set {
                if (value != _fogOfWarOther) {
                    _fogOfWarOther = value;
                    isDirty = true;
                }
            }
        }




        #endregion


        #region In-Editor fog of war painter

        [SerializeField]
        bool _maskEditorEnabled;

        public bool maskEditorEnabled {
            get { return _maskEditorEnabled; }
            set {
                if (value != _maskEditorEnabled) {
                    _maskEditorEnabled = value;
                }
            }
        }

        [SerializeField]
        MASK_TEXTURE_BRUSH_MODE _maskBrushMode = MASK_TEXTURE_BRUSH_MODE.RemoveFog;

        public MASK_TEXTURE_BRUSH_MODE maskBrushMode {
            get { return _maskBrushMode; }
            set {
                if (value != _maskBrushMode) {
                    _maskBrushMode = value;
                }
            }
        }


        [SerializeField, Range(1, 128)]
        int _maskBrushWidth = 20;

        public int maskBrushWidth {
            get { return _maskBrushWidth; }
            set {
                if (value != _maskBrushWidth) {
                    _maskBrushWidth = value;
                }
            }
        }

        [SerializeField, Range(0, 1)]
        float _maskBrushFuzziness = 0.5f;

        public float maskBrushFuzziness {
            get { return _maskBrushFuzziness; }
            set {
                if (value != _maskBrushFuzziness) {
                    _maskBrushFuzziness = value;
                }
            }
        }

        [SerializeField, Range(0, 1)]
        float _maskBrushOpacity = 0.15f;

        public float maskBrushOpacity {
            get { return _maskBrushOpacity; }
            set {
                if (value != _maskBrushOpacity) {
                    _maskBrushOpacity = value;
                }
            }
        }

        #endregion

        bool canDestroyFOWTexture;

        [SerializeField]
        Texture2D _fogOfWarTexture;

        public Texture2D fogOfWarTexture {
            get { return _fogOfWarTexture; }
            set {
                if (_fogOfWarTexture != value) {
                    if (value != null) {
                        if (value.width != value.height) {
                            Debug.LogError("Fog of war texture must be square.");
                        } else {
                            _fogOfWarTexture = value;
                            canDestroyFOWTexture = false;
                            ReloadFogOfWarTexture();
                        }
                    }
                }
            }
        }


        Color32[] fogOfWarColorBuffer;

        struct FogOfWarTransition {
            public bool enabled;
            public int x, y;
            public float startTime, startDelay;
            public float duration;
            public int initialAlpha;
            public int targetAlpha;
        }

        FogOfWarTransition[] fowTransitionList;
        int lastTransitionPos;
        Dictionary<int, int> fowTransitionIndices;
        bool requiresTextureUpload;
        Material fowBlurMat;
        RenderTexture fowBlur1, fowBlur2;

        bool isUsingFoWFromOther {
            get {
                // safety check
                if (_fogOfWarReuseOther) {
                    if (_fogOfWarOther != null && (!_fogOfWarOther.fogOfWarEnabled || _fogOfWarOther.fogOfWarReuseOther)) {
                        _fogOfWarOther = null;
                    }
                    if (_fogOfWarOther == null) {
                        // look for another fog instance that uses fow
                        int instancesCount = allFogInstances.Count;
                        for (int k = 0; k < instancesCount; k++) {
                            VolumetricFog fog = allFogInstances[k];
                            if (fog != null && fog != this && fog.fogOfWarEnabled && !fog.fogOfWarReuseOther) {
                                _fogOfWarOther = fog;
                                break;
                            }
                        }
                    }
                }
                return _fogOfWarReuseOther && _fogOfWarOther != null;
            }
        }

        #region Fog Of War

        void FogOfWarInit() {

            if (fowTransitionList == null || fowTransitionList.Length != MAX_SIMULTANEOUS_TRANSITIONS) {
                fowTransitionList = new FogOfWarTransition[MAX_SIMULTANEOUS_TRANSITIONS];
            }
            if (fowTransitionIndices == null) {
                fowTransitionIndices = new Dictionary<int, int>(MAX_SIMULTANEOUS_TRANSITIONS);
            } else {
                fowTransitionIndices.Clear();
            }
            lastTransitionPos = -1;

            if (isUsingFoWFromOther) return;

            if (_fogOfWarTexture == null) {
                FogOfWarCheckTexture();
            } else if (_fogOfWarEnabled && (fogOfWarColorBuffer == null || fogOfWarColorBuffer.Length == 0)) {
                ReloadFogOfWarTexture();
            }
        }

        void FogOfWarDestroy() {
            if (canDestroyFOWTexture && _fogOfWarTexture != null) {
                DestroyImmediate(_fogOfWarTexture);
            }
            if (fowBlur1 != null) {
                fowBlur1.Release();
            }
            if (fowBlur2 != null) {
                fowBlur2.Release();
            }
        }

        /// <summary>
        /// Reloads the current contents of the fog of war texture
        /// </summary>
        public void ReloadFogOfWarTexture() {
            if (_fogOfWarTexture == null) return;
            _fogOfWarTextureSize = _fogOfWarTexture.width;
            fogOfWarColorBuffer = _fogOfWarTexture.GetPixels32();
            lastTransitionPos = -1;
            if (fowTransitionIndices != null) {
                fowTransitionIndices.Clear();
            }
            isDirty = true;
            fogOfWarEnabled = true;
        }


        void FogOfWarCheckTexture() {
            if (!_fogOfWarEnabled || !Application.isPlaying || isUsingFoWFromOther)
                return;
            int size = GetScaledSize(_fogOfWarTextureSize, 1.0f);
            if (_fogOfWarTexture == null || _fogOfWarTexture.width != size || _fogOfWarTexture.height != size) {
                _fogOfWarTexture = new Texture2D(size, size, TextureFormat.Alpha8, false);
                _fogOfWarTexture.hideFlags = HideFlags.DontSave;
                _fogOfWarTexture.filterMode = FilterMode.Bilinear;
                _fogOfWarTexture.wrapMode = TextureWrapMode.Clamp;
                canDestroyFOWTexture = true;
                ResetFogOfWar();
            }
        }


        /// <summary>
        /// Updates fog of war transitions and uploads texture changes to GPU if required
        /// </summary>
        public void UpdateFogOfWar(bool forceUpload = false) {
            if (!_fogOfWarEnabled || _fogOfWarTexture == null || isUsingFoWFromOther)
                return;

            if (forceUpload) {
                requiresTextureUpload = true;
            }

            int tw = _fogOfWarTexture.width;
            for (int k = 0; k <= lastTransitionPos; k++) {
                FogOfWarTransition fw = fowTransitionList[k];
                if (!fw.enabled)
                    continue;
                float elapsed = Time.time - fw.startTime - fw.startDelay;
                if (elapsed > 0) {
                    float t = fw.duration <= 0 ? 1 : elapsed / fw.duration;
                    if (t < 0) t = 0; else if (t > 1f) t = 1f;
                    int alpha = (int)(fw.initialAlpha + (fw.targetAlpha - fw.initialAlpha) * t);
                    int colorPos = fw.y * tw + fw.x;
                    fogOfWarColorBuffer[colorPos].a = (byte)alpha;
                    requiresTextureUpload = true;
                    if (t >= 1f) {
                        fowTransitionList[k].enabled = false;
                        // Add refill slot if needed
                        if (fw.targetAlpha < 255 && _fogOfWarRestoreDelay > 0) {
                            AddFogOfWarTransitionSlot(fw.x, fw.y, (byte)fw.targetAlpha, 255, _fogOfWarRestoreDelay, _fogOfWarRestoreDuration);
                        }
                    }
                }
            }
            if (requiresTextureUpload) {
                requiresTextureUpload = false;
                UploadFogOfWarTextureEditsToGPU();
            }
        }

        void UploadFogOfWarTextureEditsToGPU() {
            _fogOfWarTexture.SetPixels32(fogOfWarColorBuffer);
            _fogOfWarTexture.Apply();

            // Smooth texture
            if (_fogOfWarBlur) {
                BlurFoWTexture();
            }
            SetFogOfWarMaterialProperties();

#if UNITY_EDITOR
            if (!Application.isPlaying) {
                UnityEditor.EditorUtility.SetDirty(_fogOfWarTexture);
            }
#endif
        }

        Texture GetGPUFogOfWarTexture() {
            if (_fogOfWarBlur) {
                if (fowBlur2 == null) {
                    BlurFoWTexture();
                }
                return fowBlur2;
            } else {
                return _fogOfWarTexture;
            }
        }

        void SetFogOfWarMaterialProperties() {
            if (isUsingFoWFromOther) {
                SetFogOfWarMaterialProperties(_fogOfWarOther);
            } else {
                SetFogOfWarMaterialProperties(this);
                // Notify other fog instances
                int instancesCount = allFogInstances.Count;
                for (int k = 0; k < instancesCount; k++) {
                    VolumetricFog fog = allFogInstances[k];
                    if (fog != null && fog.fogOfWarEnabled && fog.fogOfWarReuseOther == this) {
                        fog.SetFogOfWarMaterialProperties(this);
                    }
                }
            }
        }

        void SetFogOfWarMaterialProperties(VolumetricFog fog) {
            if (fogMat == null) return;
            fogMat.SetTexture(ShaderParams.FogOfWarTexture, fog.GetGPUFogOfWarTexture());
            fogMat.SetVector(ShaderParams.FogOfWarCenter, fog.fogOfWarCenter);
            fogMat.SetVector(ShaderParams.FogOfWarSize, fog.fogOfWarSize);
            Vector3 ca = fog.fogOfWarCenter - 0.5f * fog.fogOfWarSize;
            if (_useXYPlane) {
                fogMat.SetVector(ShaderParams.FogOfWarCenterAdjusted, new Vector4(ca.x / fog.fogOfWarSize.x, ca.y / (fog.fogOfWarSize.y + 0.0001f), 1f));
            } else {
                fogMat.SetVector(ShaderParams.FogOfWarCenterAdjusted, new Vector4(ca.x / fog.fogOfWarSize.x, 1f, ca.z / (fog.fogOfWarSize.z + 0.0001f)));
            }
        }

        void BlurFoWTexture() {
            if (_fogOfWarTexture == null || isUsingFoWFromOther) return;

            if (fowBlurMat == null) {
                fowBlurMat = new Material(FogUtils.GetShader("FoWBlur"));
                fowBlurMat.hideFlags = HideFlags.DontSave;
            }
            if (fowBlurMat == null)
                return;

            if (fowBlur1 == null || fowBlur1.width != _fogOfWarTexture.width || fowBlur2 == null || fowBlur2.width != _fogOfWarTexture.width) {
                CreateFoWBlurRTs();
            }
            fowBlur1.DiscardContents();
            Graphics.Blit(_fogOfWarTexture, fowBlur1, fowBlurMat, 0);
            fowBlur2.DiscardContents();
            Graphics.Blit(fowBlur1, fowBlur2, fowBlurMat, 1);
        }


        void CreateFoWBlurRTs() {
            if (fowBlur1 != null) {
                fowBlur1.Release();
            }
            if (fowBlur2 != null) {
                fowBlur2.Release();
            }
            RenderTextureDescriptor desc = new RenderTextureDescriptor(_fogOfWarTexture.width, _fogOfWarTexture.height, RenderTextureFormat.ARGB32, 0);
            fowBlur1 = new RenderTexture(desc);
            fowBlur2 = new RenderTexture(desc);
        }

        /// <summary>
        /// Instantly changes the alpha value of the fog of war at world position. It takes into account FogOfWarCenter and FogOfWarSize.
        /// Note that only x and z coordinates are used. Y (vertical) coordinate is ignored.
        /// </summary>
        /// <param name="worldPosition">in world space coordinates.</param>
        /// <param name="radius">radius of application in world units.</param>
        public void SetFogOfWarAlpha(Vector3 worldPosition, float radius, float fogNewAlpha) {
            SetFogOfWarAlpha(worldPosition, radius, fogNewAlpha, 1f);
        }


        /// <summary>
        /// Changes the alpha value of the fog of war at world position creating a transition from current alpha value to specified target alpha. It takes into account FogOfWarCenter and FogOfWarSize.
        /// Note that only x and z coordinates are used. Y (vertical) coordinate is ignored.
        /// </summary>
        /// <param name="worldPosition">in world space coordinates.</param>
        /// <param name="radius">radius of application in world units.</param>
        /// <param name="fogNewAlpha">target alpha value.</param>
        /// <param name="duration">duration of transition in seconds (0 = apply fogNewAlpha instantly).</param>
        public void SetFogOfWarAlpha(Vector3 worldPosition, float radius, float fogNewAlpha, float duration) {
            SetFogOfWarAlpha(worldPosition, radius, fogNewAlpha, true, duration, _fogOfWarSmoothness, _fogOfWarRestoreDelay, _fogOfWarRestoreDuration);
        }


        /// <summary>
        /// Changes the alpha value of the fog of war at world position creating a transition from current alpha value to specified target alpha. It takes into account FogOfWarCenter and FogOfWarSize.
        /// Note that only x and z coordinates are used. Y (vertical) coordinate is ignored.
        /// </summary>
        /// <param name="worldPosition">in world space coordinates.</param>
        /// <param name="radius">radius of application in world units.</param>
        /// <param name="fogNewAlpha">target alpha value.</param>
        /// <param name="duration">duration of transition in seconds (0 = apply fogNewAlpha instantly).</param>
        /// <param name="smoothness">border smoothness.</param>
        public void SetFogOfWarAlpha(Vector3 worldPosition, float radius, float fogNewAlpha, float duration, float smoothness) {
            SetFogOfWarAlpha(worldPosition, radius, fogNewAlpha, true, duration, smoothness, _fogOfWarRestoreDelay, _fogOfWarRestoreDuration);
        }

        /// <summary>
        /// Changes the alpha value of the fog of war at world position creating a transition from current alpha value to specified target alpha. It takes into account FogOfWarCenter and FogOfWarSize.
        /// Note that only x and z coordinates are used. Y (vertical) coordinate is ignored.
        /// </summary>
        /// <param name="worldPosition">in world space coordinates.</param>
        /// <param name="radius">radius of application in world units.</param>
        /// <param name="fogNewAlpha">target alpha value.</param>
        /// <param name="blendAlpha">if new alpha is combined with preexisting alpha value or replaced.</param>
        /// <param name="duration">duration of transition in seconds (0 = apply fogNewAlpha instantly).</param>
        /// <param name="smoothness">border smoothness.</param>
        /// <param name="restoreDelay">delay before the fog alpha is restored. Pass 0 to keep change forever.</param>
        /// <param name="restoreDuration">restore duration in seconds.</param>
        public void SetFogOfWarAlpha(Vector3 worldPosition, float radius, float fogNewAlpha, bool blendAlpha, float duration, float smoothness, float restoreDelay, float restoreDuration) {
            if (_fogOfWarTexture == null || fogOfWarColorBuffer == null || fogOfWarColorBuffer.Length == 0 || isUsingFoWFromOther)
                return;

            float tx = (worldPosition.x - _fogOfWarCenter.x) / _fogOfWarSize.x + 0.5f;
            if (tx < 0 || tx > 1f)
                return;
            float tz = (worldPosition.z - _fogOfWarCenter.z) / _fogOfWarSize.z + 0.5f;
            if (tz < 0 || tz > 1f)
                return;

            int tw = _fogOfWarTexture.width;
            int th = _fogOfWarTexture.height;
            int px = (int)(tx * tw);
            int pz = (int)(tz * th);
            float sm = 0.0001f + smoothness;
            byte newAlpha8 = (byte)(fogNewAlpha * 255);
            float tr = radius / _fogOfWarSize.z;
            int delta = (int)(th * tr);
            int deltaSqr = delta * delta;
            for (int r = pz - delta; r <= pz + delta; r++) {
                if (r > 0 && r < th - 1) {
                    for (int c = px - delta; c <= px + delta; c++) {
                        if (c > 0 && c < tw - 1) {
                            int distanceSqr = (pz - r) * (pz - r) + (px - c) * (px - c);
                            if (distanceSqr <= deltaSqr) {
                                int colorBufferPos = r * tw + c;
                                Color32 colorBuffer = fogOfWarColorBuffer[colorBufferPos];
                                if (!blendAlpha) {
                                    colorBuffer.a = 255;
                                }
                                distanceSqr = deltaSqr - distanceSqr;
                                float t = (float)distanceSqr / (deltaSqr * sm);
                                t = 1f - t;
                                if (t < 0) {
                                    t = 0;
                                } else if (t > 1f) {
                                    t = 1f;
                                }
                                byte targetAlpha = (byte)(newAlpha8 + (colorBuffer.a - newAlpha8) * t);
                                if (targetAlpha < 255) {
                                    if (duration > 0) {
                                        AddFogOfWarTransitionSlot(c, r, colorBuffer.a, targetAlpha, 0, duration);
                                    } else {
                                        colorBuffer.a = targetAlpha;
                                        fogOfWarColorBuffer[colorBufferPos] = colorBuffer;
                                        //fogOfWarTexture.SetPixel(c, r, colorBuffer);
                                        requiresTextureUpload = true;
                                        if (restoreDelay > 0) {
                                            AddFogOfWarTransitionSlot(c, r, targetAlpha, 255, restoreDelay, restoreDuration);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Changes the alpha value of the fog of war within bounds creating a transition from current alpha value to specified target alpha. It takes into account FogOfWarCenter and FogOfWarSize.
        /// Note that only x and z coordinates are used. Y (vertical) coordinate is ignored.
        /// </summary>
        /// <param name="bounds">in world space coordinates.</param>
        /// <param name="fogNewAlpha">target alpha value.</param>
        /// <param name="duration">duration of transition in seconds (0 = apply fogNewAlpha instantly).</param>
        public void SetFogOfWarAlpha(Bounds bounds, float fogNewAlpha, float duration) {
            SetFogOfWarAlpha(bounds, fogNewAlpha, true, duration, _fogOfWarSmoothness, _fogOfWarRestoreDelay, _fogOfWarRestoreDuration);
        }

        /// <summary>
        /// Changes the alpha value of the fog of war within bounds creating a transition from current alpha value to specified target alpha. It takes into account FogOfWarCenter and FogOfWarSize.
        /// Note that only x and z coordinates are used. Y (vertical) coordinate is ignored.
        /// </summary>
        /// <param name="bounds">in world space coordinates.</param>
        /// <param name="fogNewAlpha">target alpha value.</param>
        /// <param name="duration">duration of transition in seconds (0 = apply fogNewAlpha instantly).</param>
        /// <param name="smoothness">border smoothness.</param>
        public void SetFogOfWarAlpha(Bounds bounds, float fogNewAlpha, float duration, float smoothness) {
            SetFogOfWarAlpha(bounds, fogNewAlpha, true, duration, smoothness, _fogOfWarRestoreDelay, _fogOfWarRestoreDuration);
        }


        /// <summary>
        /// Changes the alpha value of the fog of war within bounds creating a transition from current alpha value to specified target alpha. It takes into account FogOfWarCenter and FogOfWarSize.
        /// Note that only x and z coordinates are used. Y (vertical) coordinate is ignored.
        /// </summary>
        /// <param name="bounds">in world space coordinates.</param>
        /// <param name="fogNewAlpha">target alpha value.</param>
        /// <param name="blendAlpha">if new alpha is combined with preexisting alpha value or replaced.</param>
        /// <param name="duration">duration of transition in seconds (0 = apply fogNewAlpha instantly).</param>
        /// <param name="smoothness">border smoothness.</param>
        /// <param name="fuzzyness">randomization of border noise.</param>
        /// <param name="restoreDelay">delay before the fog alpha is restored. Pass 0 to keep change forever.</param>
        /// <param name="restoreDuration">restore duration in seconds.</param>
        public void SetFogOfWarAlpha(Bounds bounds, float fogNewAlpha, bool blendAlpha, float duration, float smoothness, float restoreDelay = 0, float restoreDuration = 0) {
            if (_fogOfWarTexture == null || fogOfWarColorBuffer == null || fogOfWarColorBuffer.Length == 0 || isUsingFoWFromOther)
                return;

            Vector3 worldPosition = bounds.center;
            float tx = (worldPosition.x - _fogOfWarCenter.x) / _fogOfWarSize.x + 0.5f;
            if (tx < 0 || tx > 1f)
                return;
            float tz = (worldPosition.z - _fogOfWarCenter.z) / _fogOfWarSize.z + 0.5f;
            if (tz < 0 || tz > 1f)
                return;

            int tw = _fogOfWarTexture.width;
            int th = _fogOfWarTexture.height;
            int px = (int)(tx * tw);
            int pz = (int)(tz * th);
            byte newAlpha8 = (byte)(fogNewAlpha * 255);
            float trz = bounds.extents.z / _fogOfWarSize.z;
            float trx = bounds.extents.x / _fogOfWarSize.x;
            float aspect1 = trx > trz ? 1f : trz / trx;
            float aspect2 = trx > trz ? trx / trz : 1f;
            int deltaz = (int)(th * trz);
            int deltazSqr = deltaz * deltaz;
            int deltax = (int)(tw * trx);
            int deltaxSqr = deltax * deltax;
            float sm = 0.0001f + smoothness;

            int r0 = pz - deltaz;
            if (r0 < 1) r0 = 1;
            int r1 = pz + deltaz;
            if (r1 > th - 2) r1 = th - 2;
            int c0 = px - deltax;
            if (c0 < 1) c0 = 1;
            int c1 = px + deltax;
            if (c1 > tw - 2) c1 = tw - 2;

            for (int r = r0; r <= r1; r++) {
                int distancezSqr = (pz - r) * (pz - r);
                distancezSqr = deltazSqr - distancezSqr;
                float t1 = (float)distancezSqr * aspect1 / (deltazSqr * sm);
                for (int c = c0; c <= c1; c++) {
                    int distancexSqr = (px - c) * (px - c);
                    int colorBufferPos = r * tw + c;
                    Color32 colorBuffer = fogOfWarColorBuffer[colorBufferPos];
                    if (!blendAlpha) colorBuffer.a = 255;
                    distancexSqr = deltaxSqr - distancexSqr;
                    float t2 = (float)distancexSqr * aspect2 / (deltaxSqr * sm);
                    float t = t1 < t2 ? t1 : t2;
                    t = 1f - t;
                    if (t < 0) t = 0; else if (t > 1f) t = 1f;
                    byte targetAlpha = (byte)(newAlpha8 + (colorBuffer.a - newAlpha8) * t); // Mathf.Lerp(newAlpha8, colorBuffer.a, t);
                    if (targetAlpha < 255) {
                        if (duration > 0) {
                            AddFogOfWarTransitionSlot(c, r, colorBuffer.a, targetAlpha, 0, duration);
                        } else {
                            colorBuffer.a = targetAlpha;
                            fogOfWarColorBuffer[colorBufferPos] = colorBuffer;
                            requiresTextureUpload = true;
                            if (restoreDelay > 0) {
                                AddFogOfWarTransitionSlot(c, r, targetAlpha, 255, restoreDelay, restoreDuration);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Changes the alpha value of the fog of war within bounds creating a transition from current alpha value to specified target alpha. It takes into account FogOfWarCenter and FogOfWarSize.
        /// Note that only x and z coordinates are used. Y (vertical) coordinate is ignored.
        /// </summary>
        /// <param name="collider">collider used to define the shape of the area where fog of war alpha will be set. Collider must be convex.</param>
        /// <param name="fogNewAlpha">target alpha value.</param>
        /// <param name="blendAlpha">if new alpha is combined with preexisting alpha value or replaced.</param>
        /// <param name="duration">duration of transition in seconds (0 = apply fogNewAlpha instantly).</param>
        /// <param name="smoothness">border smoothness.</param>
        /// <param name="restoreDelay">delay before the fog alpha is restored. Pass 0 to keep change forever.</param>
        /// <param name="restoreDuration">restore duration in seconds.</param>
        public void SetFogOfWarAlpha(Collider collider, float fogNewAlpha, bool blendAlpha, float duration, float smoothness, float restoreDelay = 0, float restoreDuration = 0) {
            if (_fogOfWarTexture == null || fogOfWarColorBuffer == null || fogOfWarColorBuffer.Length == 0 || isUsingFoWFromOther)
                return;

            Bounds bounds = collider.bounds;

            Vector3 worldPosition = bounds.center;
            float tx = (worldPosition.x - _fogOfWarCenter.x) / _fogOfWarSize.x + 0.5f;
            if (tx < 0 || tx > 1f)
                return;
            float tz = (worldPosition.z - _fogOfWarCenter.z) / _fogOfWarSize.z + 0.5f;
            if (tz < 0 || tz > 1f)
                return;

            int tw = _fogOfWarTexture.width;
            int th = _fogOfWarTexture.height;
            int px = (int)(tx * tw);
            int pz = (int)(tz * th);
            byte newAlpha8 = (byte)(fogNewAlpha * 255);
            float trz = bounds.extents.z / fogOfWarSize.z;
            float trx = bounds.extents.x / fogOfWarSize.x;
            float aspect1 = trx > trz ? 1f : trz / trx;
            float aspect2 = trx > trz ? trx / trz : 1f;
            int deltaz = (int)(th * trz);
            int deltazSqr = deltaz * deltaz;
            int deltax = (int)(tw * trx);
            int deltaxSqr = deltax * deltax;
            float sm = 0.0001f + smoothness;
            Vector3 wpos = bounds.min;
            wpos.y = bounds.center.y;

            for (int rr = 0; rr <= deltaz * 2; rr++) {
                int r = pz - deltaz + rr;
                if (r > 0 && r < th - 1) {
                    int distancezSqr = (pz - r) * (pz - r);
                    distancezSqr = deltazSqr - distancezSqr;
                    float t1 = (float)distancezSqr * aspect1 / (deltazSqr * sm);
                    wpos.z = bounds.min.z + bounds.size.z * rr / (deltaz * 2f);
                    for (int cc = 0; cc <= deltax * 2; cc++) {
                        int c = px - deltax + cc;
                        if (c > 0 && c < tw - 1) {
                            wpos.x = bounds.min.x + bounds.size.x * cc / (deltax * 2f);
                            Vector3 colliderPos = collider.ClosestPoint(wpos);
                            if (colliderPos != wpos) continue; // point is outside collider

                            int distancexSqr = (px - c) * (px - c);
                            int colorBufferPos = r * tw + c;
                            Color32 colorBuffer = fogOfWarColorBuffer[colorBufferPos];
                            if (!blendAlpha) colorBuffer.a = 255;
                            distancexSqr = deltaxSqr - distancexSqr;
                            float t2 = (float)distancexSqr * aspect2 / (deltaxSqr * sm);
                            float t = t1 < t2 ? t1 : t2;
                            t = 1f - t;
                            if (t < 0) t = 0; else if (t > 1f) t = 1f;
                            byte targetAlpha = (byte)(newAlpha8 + (colorBuffer.a - newAlpha8) * t);
                            if (targetAlpha < 255) {
                                if (duration > 0) {
                                    AddFogOfWarTransitionSlot(c, r, colorBuffer.a, targetAlpha, 0, duration);
                                } else {
                                    colorBuffer.a = targetAlpha;
                                    fogOfWarColorBuffer[colorBufferPos] = colorBuffer;
                                    requiresTextureUpload = true;
                                    if (restoreDelay > 0) {
                                        AddFogOfWarTransitionSlot(c, r, targetAlpha, 255, restoreDelay, restoreDuration);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        readonly List<ushort> indices = new List<ushort>();
        readonly List<Vector3> vertices = new List<Vector3>();
        Mesh lastMesh;

        /// <summary>
        /// Changes the alpha value of the fog of war within bounds creating a transition from current alpha value to specified target alpha. It takes into account FogOfWarCenter and FogOfWarSize.
        /// Note that only x and z coordinates are used. Y (vertical) coordinate is ignored.
        /// </summary>
        /// <param name="go">gameobject used to define the shape of the area where fog of war alpha will be set. The gameobject must have a mesh associated.</param>
        /// <param name="fogNewAlpha">target alpha value.</param>
        /// <param name="duration">duration of transition in seconds (0 = apply fogNewAlpha instantly).</param>
        /// <param name="restoreDelay">delay before the fog alpha is restored. Pass 0 to keep change forever.</param>
        /// <param name="restoreDuration">restore duration in seconds.</param>
        public void SetFogOfWarAlpha(GameObject go, float fogNewAlpha, float duration = 0, float restoreDelay = 0, float restoreDuration = 0) {
            if (_fogOfWarTexture == null || fogOfWarColorBuffer == null || fogOfWarColorBuffer.Length == 0 || isUsingFoWFromOther)
                return;

            if (go == null) return;

            MeshRenderer meshRenderer = go.GetComponentInChildren<MeshRenderer>();
            if (meshRenderer == null) {
                Debug.LogError("No MeshRenderer found on this object.");
                return;
            }
            Bounds bounds = meshRenderer.bounds;

            MeshFilter mf = meshRenderer.GetComponent<MeshFilter>();
            if (mf == null) {
                Debug.LogError("No MeshFilter found on this object.");
                return;
            }
            Mesh mesh = mf.sharedMesh;
            if (mesh == null) {
                Debug.LogError("No Mesh found on this object.");
                return;
            }
            if (mesh.GetTopology(0) != MeshTopology.Triangles) {
                Debug.LogError("Only triangle topology is supported by this tool.");
                return;
            }
            if (lastMesh != mesh) {
                mesh.GetTriangles(indices, 0);
                mesh.GetVertices(vertices);
                lastMesh = mesh;
            }

            Vector3 worldPosition = bounds.center;
            float tx = (worldPosition.x - _fogOfWarCenter.x) / _fogOfWarSize.x + 0.5f;
            if (tx < 0 || tx > 1f)
                return;
            float tz = (worldPosition.z - _fogOfWarCenter.z) / _fogOfWarSize.z + 0.5f;
            if (tz < 0 || tz > 1f)
                return;

            int tw = _fogOfWarTexture.width;
            int th = _fogOfWarTexture.height;
            byte newAlpha8 = (byte)(fogNewAlpha * 255);

            // Get triangle info
            int indicesLength = indices.Count;
            Transform t = meshRenderer.transform;
            Vector2[] triangles = new Vector2[indicesLength];
            Vector2 v0, v1, v2;
            for (int k = 0; k < indicesLength; k += 3) {
                Vector3 w0 = t.TransformPoint(vertices[indices[k]]);
                Vector3 w1 = t.TransformPoint(vertices[indices[k + 1]]);
                Vector3 w2 = t.TransformPoint(vertices[indices[k + 2]]);
                v0.x = w0.x; v0.y = w0.z;
                v1.x = w1.x; v1.y = w1.z;
                v2.x = w2.x; v2.y = w2.z;
                triangles[k] = v0;
                triangles[k + 1] = v1;
                triangles[k + 2] = v2;
            }
            int index = 0;

            int px = (int)(tx * tw);
            int pz = (int)(tz * th);
            float trz = bounds.extents.z / _fogOfWarSize.z;
            float trx = bounds.extents.x / _fogOfWarSize.x;
            int deltaz = (int)(th * trz);
            int deltax = (int)(tw * trx);
            int r0 = pz - deltaz;
            if (r0 < 1) r0 = 1;
            int r1 = pz + deltaz;
            if (r1 > th - 2) r1 = th - 2;
            int c0 = px - deltax;
            if (c0 < 1) c0 = 1;
            int c1 = px + deltax;
            if (c1 > tw - 2) c1 = tw - 2;

            v0 = triangles[index];
            v1 = triangles[index + 1];
            v2 = triangles[index + 2];

            for (int r = r0; r <= r1; r++) {
                int rr = r * tw;
                float wz = (((r + 0.5f) / th) - 0.5f) * _fogOfWarSize.z + _fogOfWarCenter.z;
                for (int c = c0; c <= c1; c++) {
                    float wx = (((c + 0.5f) / tw) - 0.5f) * _fogOfWarSize.x + _fogOfWarCenter.x;
                    // Check if any triangle contains this position
                    for (int i = 0; i < indicesLength; i += 3) {
                        if (PointInTriangle(wx, wz, v0.x, v0.y, v1.x, v1.y, v2.x, v2.y)) {
                            int colorBufferPos = rr + c;
                            Color32 colorBuffer = fogOfWarColorBuffer[colorBufferPos];
                            if (duration > 0) {
                                AddFogOfWarTransitionSlot(c, r, colorBuffer.a, newAlpha8, 0, duration);
                            } else {
                                colorBuffer.a = newAlpha8;
                                fogOfWarColorBuffer[colorBufferPos] = colorBuffer;
                                requiresTextureUpload = true;
                                if (restoreDelay > 0) {
                                    AddFogOfWarTransitionSlot(c, r, newAlpha8, 255, restoreDelay, restoreDuration);
                                }
                            }
                            break;
                        } else {
                            index += 3;
                            index %= indicesLength;
                            v0 = triangles[index];
                            v1 = triangles[index + 1];
                            v2 = triangles[index + 2];
                        }
                    }
                }
            }
        }


        float Sign(float p1x, float p1z, float p2x, float p2z, float p3x, float p3z) {
            return (p1x - p3x) * (p2z - p3z) - (p2x - p3x) * (p1z - p3z);
        }

        bool PointInTriangle(float x, float z, float v1x, float v1z, float v2x, float v2z, float v3x, float v3z) {
            float d1 = Sign(x, z, v1x, v1z, v2x, v2z);
            float d2 = Sign(x, z, v2x, v2z, v3x, v3z);
            float d3 = Sign(x, z, v3x, v3z, v1x, v1z);

            bool has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }


        /// <summary>
        /// Restores fog of war to full opacity
        /// </summary>
        /// <param name="worldPosition">World position.</param>
        /// <param name="radius">Radius.</param>
        public void ResetFogOfWarAlpha(Vector3 worldPosition, float radius) {
            if (_fogOfWarTexture == null || fogOfWarColorBuffer == null || fogOfWarColorBuffer.Length == 0 || isUsingFoWFromOther)
                return;

            float tx = (worldPosition.x - _fogOfWarCenter.x) / _fogOfWarSize.x + 0.5f;
            if (tx < 0 || tx > 1f)
                return;
            float tz = (worldPosition.z - _fogOfWarCenter.z) / _fogOfWarSize.z + 0.5f;
            if (tz < 0 || tz > 1f)
                return;

            int tw = _fogOfWarTexture.width;
            int th = _fogOfWarTexture.height;
            int px = (int)(tx * tw);
            int pz = (int)(tz * th);
            float tr = radius / _fogOfWarSize.z;
            int delta = (int)(th * tr);
            int deltaSqr = delta * delta;

            int r0 = pz - delta;
            if (r0 < 1) r0 = 1;
            int r1 = pz + delta;
            if (r1 > th - 2) r1 = th - 2;
            int c0 = px - delta;
            if (c0 < 1) c0 = 1;
            int c1 = px + delta;
            if (c1 > tw - 2) c1 = tw - 2;


            for (int r = r0; r <= r1; r++) {
                for (int c = c0; c <= c1; c++) {
                    int distanceSqr = (pz - r) * (pz - r) + (px - c) * (px - c);
                    if (distanceSqr <= deltaSqr) {
                        int colorBufferPos = r * tw + c;
                        Color32 colorBuffer = fogOfWarColorBuffer[colorBufferPos];
                        colorBuffer.a = 255;
                        fogOfWarColorBuffer[colorBufferPos] = colorBuffer;
                    }
                }
            }
            requiresTextureUpload = true;
        }



        /// <summary>
        /// Restores fog of war to full opacity
        /// </summary>
        public void ResetFogOfWarAlpha(Bounds bounds) {
            ResetFogOfWarAlpha(bounds.center, bounds.extents.x, bounds.extents.z);
        }

        /// <summary>
        /// Restores fog of war to full opacity
        /// </summary>
        public void ResetFogOfWarAlpha(Vector3 position, Vector3 size) {
            ResetFogOfWarAlpha(position, size.x * 0.5f, size.z * 0.5f);
        }

        /// <summary>
        /// Restores fog of war to full opacity
        /// </summary>
        /// <param name="position">Position in world space.</param>
        /// <param name="extentsX">Half of the length of the rectangle in X-Axis.</param>
        /// <param name="extentsZ">Half of the length of the rectangle in Z-Axis.</param>
        public void ResetFogOfWarAlpha(Vector3 position, float extentsX, float extentsZ) {
            if (_fogOfWarTexture == null || fogOfWarColorBuffer == null || fogOfWarColorBuffer.Length == 0 || isUsingFoWFromOther)
                return;

            float tx = (position.x - _fogOfWarCenter.x) / _fogOfWarSize.x + 0.5f;
            if (tx < 0 || tx > 1f)
                return;
            float tz = (position.z - _fogOfWarCenter.z) / _fogOfWarSize.z + 0.5f;
            if (tz < 0 || tz > 1f)
                return;

            int tw = _fogOfWarTexture.width;
            int th = _fogOfWarTexture.height;
            int px = (int)(tx * tw);
            int pz = (int)(tz * th);
            float trz = extentsZ / _fogOfWarSize.z;
            float trx = extentsX / _fogOfWarSize.x;
            int deltaz = (int)(th * trz);
            int deltax = (int)(tw * trx);
            int r0 = pz - deltaz;
            if (r0 < 1) r0 = 1;
            int r1 = pz + deltaz;
            if (r1 > th - 2) r1 = th - 2;
            int c0 = px - deltax;
            if (c0 < 1) c0 = 1;
            int c1 = px + deltax;
            if (c1 > tw - 2) c1 = tw - 2;

            for (int r = r0; r <= r1; r++) {
                for (int c = c0; c <= c1; c++) {
                    int colorBufferPos = r * tw + c;
                    Color32 colorBuffer = fogOfWarColorBuffer[colorBufferPos];
                    colorBuffer.a = 255;
                    fogOfWarColorBuffer[colorBufferPos] = colorBuffer;
                }
            }
            requiresTextureUpload = true;
        }


        public void ResetFogOfWar(byte alpha = 255) {
            if (_fogOfWarTexture == null || !isPartOfScene || isUsingFoWFromOther)
                return;
            int h = _fogOfWarTexture.height;
            int w = _fogOfWarTexture.width;
            int newLength = h * w;
            if (fogOfWarColorBuffer == null || fogOfWarColorBuffer.Length != newLength) {
                fogOfWarColorBuffer = new Color32[newLength];
            }
            Color32 opaque = new Color32(alpha, alpha, alpha, alpha);
            for (int k = 0; k < newLength; k++) {
                fogOfWarColorBuffer[k] = opaque;
            }
            UploadFogOfWarTextureEditsToGPU();
            lastTransitionPos = -1;
            fowTransitionIndices.Clear();
            isDirty = true;
        }

        /// <summary>
        /// Gets or set fog of war state as a Color32 buffer. The alpha channel stores the transparency of the fog at that position (0 = no fog, 1 = opaque).
        /// </summary>
        public Color32[] fogOfWarTextureData {
            get {
                return fogOfWarColorBuffer;
            }
            set {
                fogOfWarEnabled = true;
                fogOfWarColorBuffer = value;
                if (value == null || _fogOfWarTexture == null || isUsingFoWFromOther)
                    return;
                if (value.Length != _fogOfWarTexture.width * _fogOfWarTexture.height)
                    return;
                UploadFogOfWarTextureEditsToGPU();
            }
        }

        void AddFogOfWarTransitionSlot(int x, int y, byte initialAlpha, byte targetAlpha, float delay, float duration) {

            // Check if this slot exists
            int index;
            int key = y * 64000 + x;

            if (!fowTransitionIndices.TryGetValue(key, out index)) {
                index = -1;
                for (int k = 0; k <= lastTransitionPos; k++) {
                    if (!fowTransitionList[k].enabled) {
                        index = k;
                        fowTransitionIndices[key] = index;
                        break;
                    }
                }
            }
            if (index >= 0) {
                if (fowTransitionList[index].enabled && (fowTransitionList[index].x != x || fowTransitionList[index].y != y)) {
                    index = -1;
                }
            }

            if (index < 0) {
                if (lastTransitionPos >= MAX_SIMULTANEOUS_TRANSITIONS - 1)
                    return;
                index = ++lastTransitionPos;
                fowTransitionIndices[key] = index;
            } else if (fowTransitionList[index].enabled) return; // ongoing transition

            fowTransitionList[index].x = x;
            fowTransitionList[index].y = y;
            fowTransitionList[index].duration = duration;
            fowTransitionList[index].startTime = Time.time;
            fowTransitionList[index].startDelay = delay;
            fowTransitionList[index].initialAlpha = initialAlpha;
            fowTransitionList[index].targetAlpha = targetAlpha;
            fowTransitionList[index].enabled = true;
        }


        /// <summary>
        /// Gets the current alpha value of the Fog of War at a given world position
        /// </summary>
        /// <returns>The fog of war alpha.</returns>
        /// <param name="worldPosition">World position.</param>
        public float GetFogOfWarAlpha(Vector3 worldPosition) {
            if (fogOfWarColorBuffer == null || fogOfWarColorBuffer.Length == 0 || _fogOfWarTexture == null || isUsingFoWFromOther)
                return 1f;

            float tx = (worldPosition.x - _fogOfWarCenter.x) / _fogOfWarSize.x + 0.5f;
            if (tx < 0 || tx > 1f)
                return 1f;
            float tz = (worldPosition.z - _fogOfWarCenter.z) / _fogOfWarSize.z + 0.5f;
            if (tz < 0 || tz > 1f)
                return 1f;

            int tw = _fogOfWarTexture.width;
            int th = _fogOfWarTexture.height;
            int px = (int)(tx * tw);
            int pz = (int)(tz * th);
            int colorBufferPos = pz * tw + px;
            if (colorBufferPos < 0 || colorBufferPos >= fogOfWarColorBuffer.Length)
                return 1f;
            return fogOfWarColorBuffer[colorBufferPos].a / 255f;
        }


        #region Gizmos

        void ShowFoWGizmo() {
            if (_maskEditorEnabled && _fogOfWarEnabled && !Application.isPlaying) {
                Vector3 pos = _fogOfWarCenter;
                pos.y = -10;
                pos.y += _baselineHeight + _height * 0.5f;
                Vector3 size = new Vector3(_fogOfWarSize.x, 0.1f, _fogOfWarSize.z);
                for (int k = 0; k < 5; k++) {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(pos, size);
                    pos.y += 0.5f;
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireCube(pos, size);
                    pos.y += 0.5f;
                }
            }
        }

        #endregion


        #endregion
    }


}