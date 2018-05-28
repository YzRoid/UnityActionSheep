using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jp.Yzroid.CsgActionSheep
{
    /// <summary>
    /// カメラに対象を追従する機能を実装する
    /// </summary>
    public class FollowCamera : MonoBehaviour
    {

        private Transform mTrans;
        private Transform mTarget; // 追跡するターゲット
        private Vector3 mDistance; // ターゲットとの距離

        void Awake()
        {
            mTrans = GetComponent<Transform>();
        }

        /// <summary>
        /// ターゲット追跡の初期化
        /// </summary>
        /// <param name="trans">ターゲット</param>
        /// <param name="distance">ターゲットとの距離</param>
        public void SetTarget(Transform trans, Vector3 distance, Quaternion rotation)
        {
            mTarget = trans;
            mDistance = distance;
            mTrans.position = mTarget.position + mDistance;
            mTrans.rotation = rotation;
        }

        void LateUpdate()
        {
            if (mTarget == null) return;
            mTrans.position = mTarget.position + mDistance;
        }

    }
}
