using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Jp.Yzroid.CsgActionSheep
{
    /// <summary>
    /// プレイヤーのアニメーションを管理・制御
    /// </summary>
    public class PlayerAnimation : MonoBehaviour
    {
        public enum ANIM_ID
        {
            IDLE = 0,
            RUN,
            DOWN
        }
        public ANIM_ID AnimId { get; private set; }

        void Awake()
        {
            InitAnim();
        }

        private Animator mAnimator;
        private int mIdIsRun;
        private int mIdDoDown;

        private void InitAnim()
        {
            mAnimator = GetComponent<Animator>();
            mIdIsRun = Animator.StringToHash("IsRun");
            mIdDoDown = Animator.StringToHash("DoDown");
        }

        public void Play(ANIM_ID id)
        {
            AnimId = id;
            switch (AnimId)
            {
                case ANIM_ID.IDLE:
                    mAnimator.SetBool(mIdIsRun, false);
                    break;
                case ANIM_ID.RUN:
                    mAnimator.SetBool(mIdIsRun, true);
                    break;
                case ANIM_ID.DOWN:
                    mAnimator.SetTrigger(mIdDoDown);
                    break;
            }
        }

        //------------------------
        // フェイスアニメーション //
        //---------------------------------------------------------------------------------

        public const string DEFAULT_FACE = "default@sd_hmd";

        [Tooltip("表情のアニメーションクリップを設定")]
        public AnimationClip[] animations;

        //アニメーションイベントから呼び出される表情切り替え用のコールバック
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void OnCallChangeFace(string str)
        {
            int ichecked = 0;
            foreach (var animation in animations)
            {
                if (str == animation.name)
                {
                    ChangeFace(str);
                    break;
                }
                else if (ichecked <= animations.Length)
                {
                    ichecked++;
                }
                else
                {
                    //str指定が間違っている時にはデフォルトの表情に設定
                    ChangeFace(DEFAULT_FACE);
                }
            }
        }

        private void ChangeFace(string str)
        {
            mAnimator.SetLayerWeight(1, 1); // レイヤーウェイト = そのレイヤーのアニメーションをどの程度反映させるかどうか0.0f~1.0f
            mAnimator.CrossFade(str, 0);
        }

    }
}
