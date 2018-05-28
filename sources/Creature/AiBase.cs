using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Jp.Yzroid.CsgActionSheep
{
    public abstract class AiBase : MonoBehaviour
    {

        private Transform mTrans;
        protected NavMeshAgent mAgent;

        void Awake()
        {
            mTrans = GetComponent<Transform>();
            mAgent = GetComponent<NavMeshAgent>();
            Init();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected abstract void Init();

        //----------------
        // NavMeshAgent //
        //---------------------------------------------------------------------------------

        /// <summary>
        /// NavMeshAgentが有効か判定
        /// </summary>
        /// <returns>有効な場合はtrueを返す</returns>
        protected bool IsNavMeshAgentEnable()
        {
            if (mAgent.pathStatus == NavMeshPathStatus.PathInvalid) return false;
            return true;
        }

        /// <summary>
        /// NavMeshAgentの目標地点を決定する
        /// </summary>
        /// <returns></returns>
        public abstract void SetDestination();

        /// <summary>
        /// NavMeshAgentの速度を設定する
        /// </summary>
        /// <param name="speed"></param>
        public void ApplySpeed(float speed)
        {
            mAgent.speed = speed;
        }

        //--------
        // 索敵 //
        //---------------------------------------------------------------------------------

        private readonly string TAG_PLAYER = "Player";

        [SerializeField]
        private LayerMask layerMaskPlayer;
        [SerializeField]
        private LayerMask layerMaskBush;
        [SerializeField]
        protected float mSearchRange;
        protected bool mIsExcited; // 興奮状態

        /// <summary>
        /// 周囲を索敵する
        /// </summary>
        /// <param name="rangeScale">索敵範囲についての係数</param>
        /// <returns>プレイヤーを発見した場合はtrueを返す</returns>
        protected bool SearchAround(float rangeScale)
        {
            if (Physics.CheckSphere(mTrans.position, mSearchRange * rangeScale, layerMaskPlayer)) return true;
            return false;
        }

        /// <summary>
        /// Playerを視認する
        /// BlockとPlayerに衝突するRayをPlayerに向かって飛ばし、Blockに衝突しなければ視認したと判定
        /// </summary>
        /// <returns>視認できた場合はtrueを返す</returns>
        protected bool CatchPlayer()
        {
            Vector3 direction = (GameController.Instance.StageManager.GetPlayer().transform.position - mTrans.position).normalized;
            Ray ray = new Ray(mTrans.position, direction);
            RaycastHit hit;
            int mask = layerMaskPlayer + layerMaskBush;
            if (Physics.Raycast(ray, out hit, mSearchRange, mask))
            {
                if (hit.collider.tag == TAG_PLAYER) return true;
            }
            return false;
        }

        /*
        private void OnDrawGizmos()
        {
            Vector3 direction = (GameController.Instance.StageManager.GetPlayer().transform.position - mTrans.position).normalized;
            Ray ray = new Ray(mTrans.position, direction);
            RaycastHit hit;
            int mask = layerMaskPlayer + layerMaskBush;
            if (Physics.Raycast(ray, out hit, mSearchRange, mask))
            {
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.black, 1.0f, false);
            }
        }
        */

    }
}
