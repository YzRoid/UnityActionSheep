using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jp.Yzroid.CsgActionSheep
{
    /// <summary>
    /// プレイヤーの入力受付と行動制御
    /// 当たり判定もここに実装
    /// </summary>
    public class PlayerAction : CustomMonoBehaviour
    {

        private Transform mTrans;
        private Rigidbody mRigid;
        private PlayerAnimation mAnim;

        void Start()
        {
            mTrans = GetComponent<Transform>();
            mRigid = GetComponent<Rigidbody>();
            mAnim = GetComponent<PlayerAnimation>();
            State = STATE.DEFAULT;
        }

        void Update()
        {
            CheckInputMouse();

            switch (State)
            {
                case STATE.IDLE:
                case STATE.RUN:
                    CheckInputMove();
                    break;
            }
        }

        void FixedUpdate()
        {
            switch (State)
            {
                case STATE.IDLE:
                case STATE.RUN:
                    UpdateMove();
                    break;
                case STATE.ROLL:
                    UpdateRoll();
                    break;
            }
        }

        //--------
        // 状態 //
        //---------------------------------------------------------------------------------

        public enum STATE
        {
            DEFAULT = 0,
            IDLE,
            RUN,
            ROLL,
            DOWN
        }
        public STATE State { get; private set; }

        public void OnIdle()
        {
            State = STATE.IDLE;
        }

        //-------------
        // 入力の監視 //
        //---------------------------------------------------------------------------------

        /// <summary>
        /// wasdの入力監視
        /// 8方向移動
        /// </summary>
        private void CheckInputMove()
        {
            Vector3 velocity = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) velocity.z += 1.0f;
            if (Input.GetKey(KeyCode.A)) velocity.x -= 1.0f;
            if (Input.GetKey(KeyCode.S)) velocity.z -= 1.0f;
            if (Input.GetKey(KeyCode.D)) velocity.x += 1.0f;

            // 移動力が0でない場合は移動の初期化
            if (velocity.magnitude > 0.0f)
            {
                OnMove(velocity);
                return;
            }

            // 移動力が0の場合は移動停止の初期化
            OffMove();
        }

        private void CheckInputMouse()
        {
            if (Input.GetMouseButtonDown(1)) OnDown();
        }

        //--------
        // 移動 //
        //---------------------------------------------------------------------------------

        private const float MAX_SPEED = 4.0f;
        private const float DEC_SPEED_VALUE = 0.4f;
        private const float ROTATE_SPEED = 0.2f;

        private float mSpeed = MAX_SPEED; // 現在の速度
        private Vector3 mMoveVelocity; // 入力によって決まる移動力を一時的に保持

        /// <summary>
        /// 移動の初期化
        /// 
        /// アニメーションを開始
        /// 状態を遷移
        /// 移動力を保持
        /// </summary>
        /// <param name="velocity"></param>
        private void OnMove(Vector3 velocity)
        {
            mAnim.Play(PlayerAnimation.ANIM_ID.RUN);
            State = STATE.RUN;
            mMoveVelocity = velocity.normalized * mSpeed;
        }

        /// <summary>
        /// 移動停止の初期化
        /// 
        /// アニメーションを開始
        /// 状態を遷移
        /// 移動力を保持
        /// </summary>
        private void OffMove()
        {
            mAnim.Play(PlayerAnimation.ANIM_ID.IDLE);
            State = STATE.IDLE;
            mMoveVelocity = Vector3.zero;
        }

        /// <summary>
        /// 実際の移動処理
        /// 
        /// Rigidbodyに保持している移動力を反映する
        /// </summary>
        private void UpdateMove()
        {
            mRigid.velocity = mMoveVelocity;

            // 移動力が0でない場合はその方向に向きを変える（スムーズに）
            if (mMoveVelocity.magnitude > 0.0f)
            {
                mRigid.angularVelocity = Vector3.zero; // （重要）これが無いと壁と接触しながら移動する際に回転力が相殺される
                mRigid.rotation = Quaternion.Slerp(mRigid.rotation, Quaternion.LookRotation(mMoveVelocity), ROTATE_SPEED);
            }
            else
            {
                // 移動終了後は地面との摩擦で回転する力がかかってしまう場合があるので、それをゼロにする
                mRigid.angularVelocity = Vector3.zero;
            }
        }

        //---------------
        // 回転・ダウン //
        //---------------------------------------------------------------------------------

        /// <summary>
        /// その場で回転を開始し、一定秒後にダウン状態へ遷移する
        /// </summary>
        public void OnRoll()
        {
            if (State == STATE.ROLL || State == STATE.DOWN) return;
            State = STATE.ROLL;
            mAnim.Play(PlayerAnimation.ANIM_ID.IDLE);
            mRigid.velocity = Vector3.zero;
            mRigid.angularVelocity = Vector3.zero;
            StartCoroutine(DelayMethod(1.0f, () => OnDown()));
        }

        private void UpdateRoll()
        {
            Vector3 angle = new Vector3(0.0f, 540.0f, 0.0f) * Time.deltaTime;
            mTrans.Rotate(angle);
        }

        /// <summary>
        /// ダウンアニメーションの初期化
        /// このタイミングでオンラインランキングの非同期通信を開始
        /// </summary>
        public void OnDown()
        {
            State = STATE.DOWN;
            mRigid.velocity = Vector3.zero;
            mRigid.angularVelocity = Vector3.zero;
            mAnim.Play(PlayerAnimation.ANIM_ID.DOWN);

            // 通信開始
            GameController.Instance.Scorer.SaveScore();
        }

        /// <summary>
        /// ダウンアニメーションの終了時にAnimationEventによって呼び出される
        /// ゲームの終了処理を呼び出す
        /// </summary>
        public void EndDownAnimation()
        {
            GameController.Instance.StageManager.EndGame();
        }

        //-------------
        // 当たり判定 //
        //---------------------------------------------------------------------------------

        private readonly string TAG_POPUP_POINT = "PopupPoint";
        private readonly string TAG_GOAL_AREA = "GoalArea";
        private readonly string TAG_CHASER = "Chaser";

        void OnTriggerEnter(Collider other)
        {
            if (State != STATE.IDLE && State != STATE.RUN) return;

            // ポップアップポイントに接触した
            if (other.tag == TAG_POPUP_POINT) TakeFollower(other.GetComponent<PopupPoint>());

            // ゴールエリアに到達した
            if (other.tag == TAG_GOAL_AREA) ReleaseFollower();

            // 敵と接触した
            if (other.tag == TAG_CHASER) OnRoll();
        }

        //-------------------
        // フォロワーの追従 //
        //---------------------------------------------------------------------------------

        private List<FollowerModel> mFollowerList = new List<FollowerModel>();

        /// <summary>
        /// 連れているフォロワーの数によって移動速度を再設定
        /// </summary>
        private void SetSpeed()
        {
            int followerCount = mFollowerList.Count;
            mSpeed = MAX_SPEED - DEC_SPEED_VALUE * followerCount;
        }

        /// <summary>
        /// 接触したポップアップポイントに待機中のフォロワーが存在する場合はそのフォロワーを連れていく（追従させる）
        /// </summary>
        /// <param name="popup"></param>
        private void TakeFollower(PopupPoint popup)
        {
            if (!popup.IsExsistFollower()) return;

            // フォロワーに追従させる対象を決定する（すでにフォロワーが追従している場合は一番後ろのフォロワーを対象にする）
            Transform target = mTrans;
            int followerCount = mFollowerList.Count;
            if (followerCount > 0) target = mFollowerList.Last().gameObject.GetComponent<Transform>();

            // フォロワーの追従開始
            FollowerModel follower = popup.DeleverFollower();
            follower.Follow(target, followerCount);

            // フォロワーをリストに追加し、プレイヤーとフォロワーの移動速度を再設定
            mFollowerList.Add(follower);
            SetSpeed();
            foreach (FollowerModel model in mFollowerList)
            {
                model.SetSpeed(mSpeed);
            }
        }

        public int Score { get; private set; }

        /// <summary>
        /// ゴールエリアに到達。
        /// 引き連れているフォロワー数によって点数を獲得。
        /// 
        /// 1匹が100点、2匹以上の場合はそれぞれ100点のボーナスが加算されていく
        /// 1匹目:100, 2匹目:200, 3匹目:300, 4匹目:400 = 最大で1000
        /// </summary>
        private void ReleaseFollower()
        {
            if (mFollowerList.Count <= 0) return;

            int score = 0;
            int index = 0;
            foreach (FollowerModel model in mFollowerList)
            {
                // スコア計算
                score += index * 100 + 100;
                index++;

                // フォロワーを休眠状態へ
                model.Sleep();
            }

            // スコア加算
            GameController gameCon = GameController.Instance;
            Scorer scorer = gameCon.Scorer;
            scorer.AddScore(score);
            gameCon.UiManager.renewScore(scorer.Score);

            // 速度設定
            mFollowerList.Clear();
            mSpeed = MAX_SPEED;
        }

        /// <summary>
        /// フォロワーが敵と接触した際に、接触したフォロワーから呼び出される。
        /// 接触したフォロワーとそれより後ろに追従していたフォロワーは吹き飛ばされ、追従が解消される。
        /// </summary>
        /// <param name="order">接触したフォロワーの並び番号</param>
        public void OnContactFollower(int order)
        {
            // C#でリスト列挙中に要素を除去したい場合はindex番号を使って末尾から除去していくみたい？（javaでいうIteratorが使えない）
            int count = mFollowerList.Count;
            for (int i = count - 1; i >= order; i--)
            {
                mFollowerList[i].BlowOff();
                mFollowerList.RemoveAt(i);
            }

            // 速度を再設定
            SetSpeed();
            foreach (FollowerModel model in mFollowerList)
            {
                model.SetSpeed(mSpeed);
            }
        }

    }
}
