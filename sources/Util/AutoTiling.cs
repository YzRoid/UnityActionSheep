﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jp.Yzroid.CsgActionSheep
{
    /// <summary>
    /// オブジェクトの大きさに合わせてマテリアルのタイリングを自動調整する
    /// </summary>
    public class AutoTiling : MonoBehaviour
    {

        private enum TILING_TYPE
        {
            XZ = 0
        }
        [SerializeField]
        private TILING_TYPE mTypeId;

        [SerializeField]
        [Tooltip("対応するtransform.scaleが1のときのTiling.x")]
        private int mTilePerScaleX;
        [SerializeField]
        [Tooltip("対応するtransform.scaleが1のときのTiling.y")]
        private int mTilePerScaleY;

        void Start()
        {
            float tilingX = 0;
            float tilingY = 0;
            Vector3 scale = GetComponent<Transform>().localScale;
            Material mtl = GetComponent<Renderer>().material;
            switch (mTypeId)
            {
                case TILING_TYPE.XZ:
                    tilingX = scale.x * mTilePerScaleX;
                    tilingY = scale.z * mTilePerScaleY;
                    break;
            }
            mtl.mainTextureScale = new Vector2(tilingX, tilingY);
        }

    }
}
