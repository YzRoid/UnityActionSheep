﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jp.Yzroid.CsgActionSheep
{
    public class AiWander : AiBase
    {

        private List<Vector3> mPatrolPosList;
        private int mPatrolCount;
        private int mPatrolIndex;

        override protected void Init()
        {
            // リストをシャッフルすれば巡回するポイントの順番をランダムに決定可能
            mPatrolPosList = GameController.Instance.StageManager.GetPatrolPosList();
            mPatrolCount = mPatrolPosList.Count;
            mPatrolIndex = -1;
        }

        private bool newTarget;

        override public void SetDestination()
        {
            if (!IsNavMeshAgentEnable()) return;

            // 活性・非活性の切り替え
            if (mIsExcited)
            {
                // プレイヤーが索敵できなくなった場合は非活性状態へ遷移
                if (!SearchAround(2.0f)) mIsExcited = false;
            }
            else
            {
                // 範囲内にプレイヤーが存在し、視認できる状態ならば活性状態へ遷移
                if (SearchAround(1.0f))
                {
                    if (CatchPlayer()) mIsExcited = true;
                }
            }

            //---------------
            // 目的地を設定 //
            //---------------

            GameObject player = GameController.Instance.StageManager.GetPlayer();
            if (mIsExcited)
            {
                // 活性状態の場合はプレイヤーを追従
                mAgent.SetDestination(player.transform.position);
            }
            else
            {
                // 非活性状態の場合はポップアップポイントを巡回
                if (mPatrolIndex < 0)
                {
                    // 初回は目標地点がないためプレイヤー座標を仮に設定する
                    mPatrolIndex = 0;
                    mAgent.SetDestination(player.transform.position);
                }
                else
                {
                    if (newTarget)
                    {
                        // 新しい目標が決定した直後は3.0f離れるまで到達判定を行わない
                        // これをしないと前回の目標地点が到達判定で使用されて次の目標地点が飛ばされる場合がある
                        if (mAgent.remainingDistance >= 3.0f) newTarget = false;
                    }
                    else
                    {
                        // 目標としているポイントに接近した場合は次のポイントを新しい目標地点に設定する
                        if (mAgent.remainingDistance <= 0.1f)
                        {
                            mPatrolIndex++;
                            if (mPatrolIndex >= mPatrolCount) mPatrolIndex = 0;
                            newTarget = true;
                        }
                    }
                    mAgent.SetDestination(mPatrolPosList[mPatrolIndex]);
                }
            }
        }

    }
}
