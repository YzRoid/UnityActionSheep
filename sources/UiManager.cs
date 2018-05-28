using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Jp.Yzroid.CsgActionSheep
{
    public class UiManager : MonoBehaviour {

        void Start()
        {
            GameController.Instance.UiManager = this;
            DOTween.Init();
        }

        /// <summary>
        /// ゲーム起動のタイミングで必要なUIを表示したりtweenアニメーションを開始する
        /// </summary>
        public void LoadUi()
        {
            mBottomPanel.gameObject.SetActive(true);
            mCenterPanel.gameObject.SetActive(true);
            mCenterText.DOFade(0.1f, 1.2f).SetEase(Ease.InCubic).SetLoops(-1, LoopType.Yoyo);
            mTimeText.gameObject.SetActive(true);
            mScoreText.gameObject.SetActive(true);
        }

        //-------------------
        // センターテキスト //
        //---------------------------------------------------------------------------------

        [SerializeField]
        private Image mCenterPanel;
        [SerializeField]
        private Text mCenterText;

        public void HideCenterMsg()
        {
            mCenterPanel.gameObject.SetActive(false);
            mCenterText.DOKill(false); // 注：killしないとオブジェクトが非アクティブでも実行され続ける！
        }

        //-----------------
        // ボトムテキスト //
        //---------------------------------------------------------------------------------

        [SerializeField]
        private Image mBottomPanel;

        //-----------------
        // タイムテキスト //
        //---------------------------------------------------------------------------------

        [SerializeField]
        private Text mTimeText;

        public void RenewTime(int i)
        {
            mTimeText.text = "TIME: " + i;
        }

        public void ChangeTimeTextColorIntoRed()
        {
            mTimeText.color = new Color(1.0f, 10.0f / 255.0f, 0.0f, 1.0f);
        }

        //-----------------
        // スコアテキスト //
        //---------------------------------------------------------------------------------

        [SerializeField]
        private Text mScoreText;

        public void renewScore(int i)
        {
            mScoreText.text = "SCORE: " + i;
        }

        //------------------------
        // ゲームオーバーテキスト //
        //---------------------------------------------------------------------------------

        [SerializeField]
        private Text mGameOverText;

        public void ShowGameOverText()
        {
            mGameOverText.gameObject.SetActive(true);
        }

        //------------------------------------
        // オンラインランキングのポップアップ //
        //---------------------------------------------------------------------------------

        [SerializeField]
        private RectTransform mRecordWindow;
        [SerializeField]
        private Image[] mRecordRow;
        [SerializeField]
        private Text[] mRecordPointText;

        public void PopupRecords(int rank, int[] rankerScoreArray)
        {
            // テキストにTOP5スコアを反映
            int length = mRecordPointText.Length;
            for (int i = 0; i < length; i++)
            {
                mRecordPointText[i].text = rankerScoreArray[i] + " Points";
            }

            // ランクインした場合は該当行の背景色を赤に変更
            if (rank <= 5)
            {
                mRecordRow[rank - 1].color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
            }

            // ポップアップ開始
            Sequence seq = DOTween.Sequence();
            seq.OnStart(() =>
            {
                mRecordWindow.localScale = Vector3.zero;
                mRecordWindow.gameObject.SetActive(true);
            });
            seq.Append(mRecordWindow.DOScale(1.2f, 0.3f));
            seq.Append(mRecordWindow.DOScale(1.0f, 0.1f));
            //seq.OnComplete(() => ProcessComplated());
            seq.Play();
        }

    }

}
