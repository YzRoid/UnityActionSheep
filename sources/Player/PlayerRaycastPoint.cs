using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jp.Yzroid.CsgActionSheep
{
    public class PlayerRaycastPoint : MonoBehaviour
    {

        [SerializeField]
        private Transform mRaycastPoint;
        [SerializeField]
        private LayerMask layerMaskBush;
        private Transform mTrans;

        void Awake()
        {
            mTrans = GetComponent<Transform>();
        }

        /// <summary>
        /// プレイヤーの進行方向にRayを飛ばし、ブロック（ブッシュ）に当たった場所から1ブロック手前のポジションを取得。
        /// </summary>
        /// <returns></returns>
        public Vector3? GetDestinationPoint()
        {
            Ray ray = new Ray(mRaycastPoint.position, mTrans.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 20.0f, layerMaskBush))
            {
                Vector3 result = mTrans.position + ray.direction * (hit.distance - 0.5f);
                return result;
            }
            return null;
        }

        /*
        private void OnDrawGizmos()
        {
            Ray ray = new Ray(mRaycastPoint.position, mTrans.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 20.0f, layerMaskBush))
            {
                Debug.DrawRay(ray.origin, ray.direction * (hit.distance - 0.5f), Color.red, 1.0f, false);
            }
        }
        */

    }
}
