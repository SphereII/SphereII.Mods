
    using System.Collections.Generic;
    using Platform;
    using UnityEngine;
    using UnityEngine.Video;

    public class TileEntitySCoreSign : TEFeatureSignable, ILockable, ITileEntitySignable, ITileEntity {

        private SCoreVideo _scoreVideo;
        public TileEntitySCoreSign() : base() {
        }

        public new bool CanRenderString(string _text) {
            return smartTextMesh != null && smartTextMesh.Length > 0 && smartTextMesh[0] != null && smartTextMesh[0].CanRenderString(_text);
        }

        public void SetBlockEntityData(BlockEntityData _blockEntityData) {
            if (_blockEntityData is not { bHasTransform: true } || GameManager.IsDedicatedServer) return;
            var textMesh = _blockEntityData.transform.GetComponentInChildren<TextMesh>();
            var mesh = textMesh.transform.gameObject.AddComponent<SmartTextMesh>();
            smartTextMesh = new[] { mesh };
            _scoreVideo = textMesh.transform.gameObject.AddComponent<SCoreVideo>();
            _scoreVideo.Configure(this.signText.Text);
            var num = (float)_blockEntityData.blockValue.Block.multiBlockPos.dim.x;
            mesh.MaxWidth = 0.48f * num;
            mesh.MaxLines = this.lineCount;
            mesh.ConvertNewLines = true;
            RefreshTextMesh(string.Empty);
        }

        public bool IsLocked() => lockFeature?.IsLocked() ?? false;
        public void SetLocked(bool _isLocked) => lockFeature?.SetLocked(_isLocked);
        public PlatformUserIdentifierAbs GetOwner() => lockFeature?.GetOwner();
        public void SetOwner(PlatformUserIdentifierAbs _userIdentifier) => lockFeature?.SetOwner(_userIdentifier);
        public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier) => lockFeature?.IsUserAllowed(_userIdentifier) ?? true;
        public List<PlatformUserIdentifierAbs> GetUsers() => lockFeature?.GetUsers() ?? new List<PlatformUserIdentifierAbs>();
        public bool LocalPlayerIsOwner() => lockFeature?.LocalPlayerIsOwner() ?? true;
        public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier) => lockFeature?.IsOwner(_userIdentifier) ?? false;
        public bool HasPassword() => lockFeature?.HasPassword() ?? false;
        public bool SetPasswordHash(string _passwordHash, PlatformUserIdentifierAbs _userIdentifier) => lockFeature?.SetPasswordHash(_passwordHash, _userIdentifier) ?? false;
        public bool CheckPasswordHash(string _password, PlatformUserIdentifierAbs _userIdentifier) => lockFeature?.CheckPasswordHash(_password, _userIdentifier) ?? true;
        public string GetPasswordHash() => lockFeature?.GetPasswordHash() ?? string.Empty;
    }
