using NCMB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jp.Yzroid.CsgActionSheep
{
    public class Scorer : MonoBehaviour
    {
        public int Score { get; private set; }

        public void AddScore(int score)
        {
            Score += score;
        }

        //------------------
        // NCMBランキング //
        //---------------------------------------------------------------------------------

        private readonly string CLASS_NAME = "HIGH_SCORE";
        private readonly string KEY_SCORE = "score";

        // 非同期処理の進行状況
        public const int ASYNC_DEFAULT = 0, ASYNC_DOING = 1, ASYNC_DONE = 2;
        public int AsyncState { get; private set; }
        public bool HasAsyncError { get; private set; }

        // 結果を格納
        public int Rank { get; private set; }
        private int[] mRankerScoreArray;

        public int[] GetRankerScoreArray()
        {
            return mRankerScoreArray;
        }

        /// <summary>
        /// 今回のスコアを保持しているTOP5のランキングスコアに反映する
        /// </summary>
        private void RenewRankerScoreArray()
        {
            int length = mRankerScoreArray.Length;
            int[] tempArray = new int[length];
            tempArray[Rank - 1] = Score;
            int tempIndex = 0;

            for (int i = 0; i < length; i++)
            {
                if (tempArray[i] == 0)
                {
                    tempArray[i] = mRankerScoreArray[tempIndex];
                    tempIndex++;
                }
            }
            mRankerScoreArray = tempArray;
        }

        /// <summary>
        /// サーバーからTOP5までのスコアを取得
        /// </summary>
        public void FetchRankerScores()
        {
            AsyncState = ASYNC_DOING;
            NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>(CLASS_NAME);
            query.OrderByDescending(KEY_SCORE);
            query.Limit = 5;
            query.FindAsync((List<NCMBObject> records, NCMBException e) =>
            {
                if (e == null)
                {
                    int count = records.Count;
                    mRankerScoreArray = new int[count];
                    for (int i = 0; i < count; i++)
                    {
                        mRankerScoreArray[i] = System.Convert.ToInt32(records[i][KEY_SCORE]);
                    }
                }
                else
                {
                    Debug.LogError(e);
                    // エラーが出た場合はデフォルトのスコアを設定してしまう
                    mRankerScoreArray = new int[5];
                    mRankerScoreArray[0] = 3000;
                    mRankerScoreArray[1] = 2000;
                    mRankerScoreArray[2] = 1500;
                    mRankerScoreArray[3] = 1000;
                    mRankerScoreArray[4] = 500;
                    HasAsyncError = true;
                }
                AsyncState = ASYNC_DONE;
            });
        }

        /// <summary>
        /// 今回のスコアについて判定を行い、TOP5に入っている場合はサーバーに保存すると同時に順位を保持する
        /// </summary>
        public void SaveScore()
        {
            if (HasAsyncError) return;

            AsyncState = ASYNC_DOING;
            // 今回のスコアがTOP5に入っているか判定
            Rank = 100;
            int length = mRankerScoreArray.Length;
            for (int i = 0; i < length; i++)
            {
                if (Score >= mRankerScoreArray[i])
                {
                    Rank = i + 1;
                    break;
                }
            }

            // TOP5に入った場合はスコアをサーバーに保存
            if (Rank > 5)
            {
                AsyncState = ASYNC_DONE;
                return;
            }
            else
            {
                NCMBObject obj = new NCMBObject(CLASS_NAME);
                obj[KEY_SCORE] = Score;
                obj.SaveAsync((NCMBException e) =>
                {
                    if (e != null)
                    {
                        Debug.LogError(e);
                        HasAsyncError = true;
                    }
                    else
                    {
                        // 保存に成功した場合は保持しているTOP5のスコアを新しい状態に差し替える
                        RenewRankerScoreArray();
                    }
                    AsyncState = ASYNC_DONE;
                });
            }
        }

    }
}
