using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jp.Yzroid.CsgActionSheep
{
    public class SceneMain : MonoBehaviour
    {

        private GameController mGame;
        private Scorer mScorer;

        void Awake()
        {
            mGame = GameController.Instance;
            mGame.Init();
            mScorer = mGame.Scorer;
        }

        //--------
        // 状態 //
        //---------------------------------------------------------------------------------

        private enum STATE
        {
            FETCH_RANKER_SCORES = 0,
            LOAD_STAGE,
            WAIT_ENTER_KEY,
            PLAY,
            WAIT_SAVE_SCORE,
            GAME_OVER
        }
        private STATE mState = STATE.FETCH_RANKER_SCORES;

        //--------
        // 更新 //
        //---------------------------------------------------------------------------------

        void Update()
        {
            CheckBackspaceInput();
            switch (mState)
            {
                // 起動時にTOP5のスコアを予め取得
                case STATE.FETCH_RANKER_SCORES:
                    switch (mScorer.AsyncState)
                    {
                        case Scorer.ASYNC_DEFAULT:
                            mScorer.FetchRankerScores();
                            break;
                        case Scorer.ASYNC_DONE:
                            mState = STATE.LOAD_STAGE;
                            break;
                    }
                    break;
                // ステージの生成
                case STATE.LOAD_STAGE:
                    mGame.StageManager.LoadStage(1);
                    mGame.UiManager.LoadUi();
                    mState = STATE.WAIT_ENTER_KEY;
                    break;
                // Enter入力でゲームを開始
                case STATE.WAIT_ENTER_KEY:
                    if (WaitEnter())
                    {
                        mGame.UiManager.HideCenterMsg();
                        mGame.StageManager.StartGame();
                        mState = STATE.PLAY;
                    }
                    break;
                // プレイ中
                case STATE.PLAY:
                    if (mGame.IsGameOver)
                    {
                        mState = STATE.WAIT_SAVE_SCORE;
                    }
                    break;
                case STATE.WAIT_SAVE_SCORE: // スコアの登録について、コールバック待機
                    if (mScorer.HasAsyncError)
                    {
                        // サーバー接続に失敗しているならばゲームオーバーのテキストを表示して終了
                        mGame.UiManager.ShowGameOverText();
                    }
                    else
                    {
                        if (mScorer.AsyncState == Scorer.ASYNC_DONE)
                        {

                            int rank = mScorer.Rank;
                            int[] rankerScoreArray = mScorer.GetRankerScoreArray();
                            mGame.UiManager.PopupRecords(rank, rankerScoreArray);
                            mState = STATE.GAME_OVER;
                        }
                    }
                    break;
            }
        }

        //--------
        // 入力 //
        //---------------------------------------------------------------------------------

        private bool WaitEnter()
        {
            if (Input.GetKeyDown(KeyCode.Return)) return true;
            return false;
        }

        private void CheckBackspaceInput()
        {
            if (Input.GetKeyDown(KeyCode.Backspace)) mGame.OnRestartButton();
        }

    }
}
