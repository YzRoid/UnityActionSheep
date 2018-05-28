using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System;
using UnityEngine;

namespace Jp.Yzroid.CsgActionSheep
{
    /// <summary>
    /// ステージ全体を管理・制御
    /// </summary>
    public class StageManager : MonoBehaviour
    {

        private StageConstructor mConstructor;

        void Start()
        {
            GameController.Instance.StageManager = this;
            mConstructor = GetComponent<StageConstructor>();
        }

        //---------------------
        // ゲームの開始と終了 //
        //---------------------------------------------------------------------------------

        public void LoadStage(int stageId)
        {
            mConstructor.LoadStage(stageId);
        }

        public void StartGame()
        {
            // 制限時間を設定
            mRestTime = 100;

            // プレイヤーとチェイサーの行動開始
            mConstructor.OnActivePlayer();
            mConstructor.OnActiveChaser();

            // コルーチンを開始
            StartCoroutine("PopupFollower");
            StartCoroutine("CountDown");
        }

        public void EndGame()
        {
            // チェイサーを除去
            mConstructor.OffActiveChaser();

            // コルーチン終了
            StopAllCoroutines();

            // フラグを立てる
            GameController.Instance.IsGameOver = true;
        }

        //------------
        // ゲッター //
        //---------------------------------------------------------------------------------

        public GameObject GetPlayer()
        {
            return mConstructor.Player;
        }

        /// <summary>
        /// 全てのポップアップポイントのpositionをリストとして返す
        /// </summary>
        /// <returns></returns>
        public List<Vector3> GetPatrolPosList()
        {
            List<Vector3> result = new List<Vector3>();
            foreach (PopupPoint point in mConstructor.GetPopupList())
            {
                result.Add(point.transform.position);
            }
            return result;
        }

        //------------
        // 制限時間 //
        //---------------------------------------------------------------------------------

        private int mRestTime;

        private IEnumerator CountDown()
        {
            while (true)
            {
                yield return new WaitForSeconds(1.0f);

                // 制限時間終了でプレイヤーをダウン状態へ遷移 → それによってEndGameが呼ばれる
                mRestTime--;
                if (mRestTime <= 0)
                {
                    mRestTime = 0;
                    GameController.Instance.UiManager.RenewTime(mRestTime);
                    mConstructor.Player.GetComponent<PlayerAction>().OnDown();
                    yield break;
                }

                // 残り時間10秒になったときにテキストの色を赤に変更する
                if (mRestTime == 10) GameController.Instance.UiManager.ChangeTimeTextColorIntoRed();
                GameController.Instance.UiManager.RenewTime(mRestTime);
            }
        }

        //-------------------
        // フォロワーの管理 //
        //---------------------------------------------------------------------------------

        private readonly float POPUP_DURATION = 5.0f;
        private readonly int MAX_FOLLOWER_COUNT = 4; // アクティブ状態のフォロワー最大数
        private int mFollowerCount; // アクティブ状態のフォロワー数

        /// <summary>
        /// ステージ内に存在するフォロワーの数が最大に達していない場合は
        /// ポップアップ可能なポイントからランダムに1つ選択してフォロワーをポップアップする
        /// </summary>
        /// <returns></returns>
        private IEnumerator PopupFollower()
        {
            while (true)
            {
                yield return new WaitForSeconds(POPUP_DURATION);
                if (mFollowerCount < MAX_FOLLOWER_COUNT)
                {
                    var newList = mConstructor.GetPopupList().Where(i => i.IsReadyPopupFollower());
                    if (newList.Any())
                    {
                        var popupPoint = newList.OrderBy(i => Guid.NewGuid()).Take(1).ToList();
                        popupPoint[0].PopupFollower();
                        mFollowerCount++;
                    }
                }
            }
        }

        public void OnSleepFollower()
        {
            mFollowerCount--;
        }

    }
}
