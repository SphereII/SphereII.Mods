
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Video;

    public class TileEntitySCoreSign :  TileEntitySign, ILockable, ITileEntitySignable, ITileEntity {

        private SCoreVideo _scoreVideo;
        public TileEntitySCoreSign(Chunk _chunk) : base(_chunk) {
        }
        public new bool CanRenderString(string _text) {
            
            return smartTextMesh != null && this.smartTextMesh.CanRenderString(_text);
        }
        
        public void SetBlockEntityData(BlockEntityData _blockEntityData) {
            if (_blockEntityData is not { bHasTransform: true } || GameManager.IsDedicatedServer) return;
            textMesh = _blockEntityData.transform.GetComponentInChildren<TextMesh>();
            smartTextMesh = textMesh.transform.gameObject.AddComponent<SmartTextMesh>();
            _scoreVideo = textMesh.transform.gameObject.AddComponent<SCoreVideo>();
            _scoreVideo.Configure(this.signText.Text); 
            var num = (float)_blockEntityData.blockValue.Block.multiBlockPos.dim.x;
            smartTextMesh.MaxWidth = 0.48f * num;
            smartTextMesh.MaxLines = this.lineCount;
            smartTextMesh.ConvertNewLines = true;
            var authoredText = this.signText;
            RefreshTextMesh(string.Empty);
            //RefreshTextMesh((authoredText != null) ? authoredText.Text : null);
        }
    }
