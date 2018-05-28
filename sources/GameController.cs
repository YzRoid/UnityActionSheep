using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Jp.Yzroid.CsgActionSheep
{
    public class GameController
    {

        // 初期化タイミングでインスタンスを生成
        private static readonly GameController mInstance = new GameController();

        // コンストラクタをprivateにすることによって他クラスからnewできないようにする
        private GameController() { }

        // 他クラスからこのインスタンスを参照する
        public static GameController Instance
        {
            get
            {
                return mInstance;
            }
        }

        public void Init()
        {
            Application.targetFrameRate = 30;
            IsGameOver = false;
            Scorer = new Scorer();
        }

        //----------
        // フラグ //
        //---------------------------------------------------------------------------------

        public bool IsGameOver { get; set; }

        //--------------------------
        // マネージャーの参照を保持 //
        //---------------------------------------------------------------------------------

        public StageManager StageManager { get; set; }
        public UiManager UiManager { get; set; }

        //-------------
        // スコア管理 //
        //---------------------------------------------------------------------------------

        public Scorer Scorer { get; private set; }

        //-------------
        // シーン管理 //
        //---------------------------------------------------------------------------------

        public void OnRestartButton()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

    }
}
