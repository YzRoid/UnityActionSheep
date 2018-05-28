using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jp.Yzroid.CsgActionSheep
{
    public class ChaserModel : MonoBehaviour
    {

        private AiBase mAi;
        private float mSpeed;
        private float mIncSpeed; // 速度の上昇度（ゴールポイントにフォロワーが届けられるたびにスピードアップ）
        public bool IsActive { get; set; }

        void Awake()
        {
            mAi = GetComponent<AiBase>();
        }

        public void Init(float speed, float incSpeed)
        {
            mSpeed = speed;
            mIncSpeed = incSpeed;
            mAi.ApplySpeed(mSpeed);
        }

        void Update()
        {
            if (!IsActive) return;
            mAi.SetDestination();
        }

    }
}
