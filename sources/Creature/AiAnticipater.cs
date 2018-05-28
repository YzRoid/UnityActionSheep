using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jp.Yzroid.CsgActionSheep
{
    public class AiAnticipater : AiBase
    {

        override protected void Init() { }

        override public void SetDestination()
        {
            if (!IsNavMeshAgentEnable()) return;

            // 興奮状態の切り替え
            if (mIsExcited)
            {
                // プレイヤーが索敵できなくなった場合は興奮状態を解除
                if (!SearchAround(1.5f)) mIsExcited = false;
            }
            else
            {
                // 範囲内にプレイヤーが存在し、視認できる状態ならば興奮状態へ遷移
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
                // 興奮状態の場合はプレイヤーを追従
                mAgent.SetDestination(player.transform.position);
            }
            else
            {
                // 非興奮状態の場合はプレイヤー進行方向へ先回り
                Vector3? temp = player.GetComponent<PlayerRaycastPoint>().GetDestinationPoint();
                if (temp == null)
                {
                    mAgent.SetDestination(player.transform.position);
                }
                else
                {
                    mAgent.SetDestination((Vector3)temp);
                }
            }
        }

    }
}
