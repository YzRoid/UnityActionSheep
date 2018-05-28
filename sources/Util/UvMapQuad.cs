using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jp.Yzroid.CsgActionSheep
{
    /// <summary>
    /// Quadのメッシュに対してUVマップを設定する
    /// 頂点の順番は
    /// 0:左下
    /// 1:右上
    /// 2:右下
    /// 3:左上
    /// </summary>
    public class UvMapQuad : MonoBehaviour
    {

        [SerializeField]
        private float leftBottomU, leftBottomV, rightUpU, rightUpV, rightBottomU, rightBottomV, leftUpU, leftUpV;

        private void Awake()
        {
            Vector2[] newUv =
            {
                new Vector2(leftBottomU, leftBottomV),
                new Vector2(rightUpU, rightUpV),
                new Vector2(rightBottomU, rightBottomV),
                new Vector2(leftUpU, leftUpV)
            };
            Mesh mesh = GetComponent<MeshFilter>().mesh;
            mesh.uv = newUv;
        }

    }

}
