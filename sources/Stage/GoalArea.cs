﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jp.Yzroid.CsgActionSheep
{
    public class GoalArea : MonoBehaviour
    {

        private readonly int CENTER = 0, NORTH = 1, EAST = 2, SOUTH = 3, WEST = 4;
        private readonly float THICKNES_HALF = 0.05f; // 四方を囲む壁の厚さの半分

        [SerializeField]
        private LayerMask mLayerMaskPlayer;
        private MaterialChanger mMtlChanger;
        private Vector3[] mCenter = new Vector3[5];
        private Vector3[] mHalfExtents = new Vector3[5];

        void Start()
        {
            mMtlChanger = GetComponent<MaterialChanger>();
            CreateAround();
        }

        /// <summary>
        /// 中心の立方体を囲むように、周囲に厚みのある壁のような領域を生成
        /// </summary>
        private void CreateAround()
        {
            mCenter[CENTER] = transform.localPosition;
            mHalfExtents[CENTER] = transform.localScale / 2.0f;

            float posX = mCenter[CENTER].x;
            float posY = mCenter[CENTER].y;
            float posZ = mCenter[CENTER].z;
            float halfExX = mHalfExtents[CENTER].x;
            float halfExY = mHalfExtents[CENTER].y;
            float halfExZ = mHalfExtents[CENTER].z;

            mCenter[NORTH] = new Vector3(posX, posY, posZ + halfExZ - THICKNES_HALF);
            mHalfExtents[NORTH] = new Vector3(halfExX, halfExY, THICKNES_HALF);
            mCenter[SOUTH] = new Vector3(posX, posY, posZ - halfExZ + THICKNES_HALF);
            mHalfExtents[SOUTH] = new Vector3(halfExX, halfExY, THICKNES_HALF);
            mCenter[EAST] = new Vector3(posX - halfExX + THICKNES_HALF, posY, posZ);
            mHalfExtents[EAST] = new Vector3(THICKNES_HALF, halfExY, halfExZ);
            mCenter[WEST] = new Vector3(posX + halfExX - THICKNES_HALF, posY, posZ);
            mHalfExtents[WEST] = new Vector3(THICKNES_HALF, halfExY, halfExZ);
        }

        /// <summary>
        /// プレイヤーがエリアに内包しているか判定し、結果によってエリアの色を変更する
        /// </summary>
        void Update()
        {
            if (IsHitOverlapBox(CENTER))
            {
                for (int i = 1; i < 5; i++)
                {
                    if (IsHitOverlapBox(i))
                    {
                        mMtlChanger.ChangeMaterial(0);
                        return;
                    }
                }
                mMtlChanger.ChangeMaterial(1);
            }
        }

        private bool IsHitOverlapBox(int i)
        {
            Collider[] hits = Physics.OverlapBox(mCenter[i], mHalfExtents[i], Quaternion.identity, mLayerMaskPlayer);
            if (hits.Length > 0) return true;
            return false;
        }

        /*
        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            for(int i=NORTH; i<mCenter.Length; i++)
            {
                Gizmos.DrawWireCube(mCenter[i], mHalfExtents[i] * 2.0f);
            }
        }
        */

    }
}
