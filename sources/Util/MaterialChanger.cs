using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jp.Yzroid.CsgActionSheep
{
    /// <summary>
    /// マテリアルを差し替える
    /// </summary>
    public class MaterialChanger : MonoBehaviour
    {

        [SerializeField]
        private Material[] mMaterialList;
        private int mId;

        /// <summary>
        /// マテリアルを差し替える
        /// </summary>
        /// <param name="i"></param>
        /// <returns>差し替えに成功した場合はtrueを返す</returns>
        public bool ChangeMaterial(int i)
        {
            if (mId == i) return false;
            if (i < mMaterialList.Length)
            {
                mId = i;
                GetComponent<Renderer>().material = mMaterialList[mId];
                return true;
            }
            return false;
        }
    }
}
