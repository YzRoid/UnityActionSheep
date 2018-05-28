using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Jp.Yzroid.CsgActionSheep
{
    public class FollowerModel : CustomMonoBehaviour
    {

        private Rigidbody mRigid;
        private Vector3 mHomePos;

        void Awake()
        {
            mRigid = GetComponent<Rigidbody>();
            mHomePos = GetComponent<Transform>().position;
            mAgent = GetComponent<NavMeshAgent>();
            InitAnim();
        }

        void Update()
        {
            switch (State)
            {
                case STATE.FOLLOW:
                    UpdateFollow();
                    break;
            }
        }

        //-----------------
        // アニメーション //
        //---------------------------------------------------------------------------------

        private Animator mAnimator;
        private int mIdIsRun;

        private void InitAnim()
        {
            mAnimator = GetComponent<Animator>();
            mIdIsRun = Animator.StringToHash("IsRun");
        }

        //--------
        // 状態 //
        //---------------------------------------------------------------------------------

        public enum STATE
        {
            DEFAULT = 0,
            POPUP,
            FOLLOW,
            BLOW_OFF,
            SLEEP
        }
        public STATE State { get; private set; }

        //----------------
        // NavMeshAgent //
        //---------------------------------------------------------------------------------

        private readonly float START_DISTANCE = 1.0f;
        private readonly float STOP_DISTANCE = 0.8f;

        private NavMeshAgent mAgent;

        public void SetSpeed(float spd)
        {
            mAgent.speed = spd;
            mAgent.acceleration = spd * 4.0f;
        }

        private void UpdateFollow()
        {
            mAgent.SetDestination(mTarget.position);

            float remainingDistance = mAgent.remainingDistance;
            if (remainingDistance > START_DISTANCE)
            {
                mAgent.isStopped = false;
                mAnimator.SetBool(mIdIsRun, true);
            }
            else if (remainingDistance <= STOP_DISTANCE)
            {
                mAgent.velocity = Vector3.zero; // 減速ではなくピタっと止める
                mAgent.isStopped = true;
                mAnimator.SetBool(mIdIsRun, false);
            }
        }

        //-------------
        // アクション //
        //---------------------------------------------------------------------------------

        /// <summary>
        /// ポップアップ
        /// </summary>
        public void Popup()
        {
            State = STATE.POPUP;
            gameObject.SetActive(true);
        }

        private Transform mTarget; // 追従する対象
        private int mOrder; // フォロワーの並び順

        /// <summary>
        /// 指定された対象の追従を開始
        /// </summary>
        public void Follow(Transform target, int order)
        {
            mTarget = target;
            mOrder = order;
            State = STATE.FOLLOW;
        }

        /// <summary>
        /// 吹き飛ばされ、一定時間後に休眠状態へ遷移する
        /// </summary>
        public void BlowOff()
        {
            State = STATE.BLOW_OFF;
            mAgent.velocity = Vector3.zero;
            mAgent.isStopped = true;
            mAgent.updatePosition = false;
            mRigid.isKinematic = false; // 物理挙動を一時的に有効にする
            mRigid.AddForce(new Vector3(200.0f, 200.0f, 0.0f));
            StartCoroutine(DelayMethod(3.0f, () => Sleep()));
        }

        /// <summary>
        /// 次のリポップに備えて休眠状態へ遷移
        /// </summary>
        public void Sleep()
        {
            // 休眠状態
            State = STATE.SLEEP;
            mTarget = null;
            gameObject.SetActive(false);

            // ポジションと物理挙動を初期化
            mAgent.Warp(mHomePos);
            mAgent.updatePosition = true;
            mRigid.isKinematic = true;

            // マネージャーに通知
            GameController.Instance.StageManager.OnSleepFollower();
        }

        //-------------
        // 当たり判定 //
        //---------------------------------------------------------------------------------

        private readonly string TAG_CHASER = "Chaser";

        void OnTriggerEnter(Collider other)
        {
            // 追従中に敵と接触した場合はプレイヤーに通知
            if (other.tag == TAG_CHASER)
            {
                if (State == STATE.FOLLOW)
                {
                    PlayerAction pa = GameController.Instance.StageManager.GetPlayer().GetComponent<PlayerAction>();
                    pa.OnContactFollower(mOrder);
                }
            }
        }
    }
}
