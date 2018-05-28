using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jp.Yzroid.CsgActionSheep
{
    public class AiNormal : AiBase
    {

        override protected void Init() { }

        override public void SetDestination()
        {
            if (!IsNavMeshAgentEnable()) return;
            mAgent.SetDestination(GameController.Instance.StageManager.GetPlayer().transform.position);
        }

    }
}
