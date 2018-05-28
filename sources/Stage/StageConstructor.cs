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
    /// ステージを生成し、必要な参照を保持しておく
    /// </summary>
    public class StageConstructor : MonoBehaviour
    {

        private Transform mTrans;

        void Awake()
        {
            mTrans = GetComponent<Transform>();
        }

        // ファイルデータ整形用
        private const string WHITE_EXPRESSION = "[ 　\t]"; // ホワイトスペース除去用（半角・全角スペース、タブ文字）
        private readonly Regex REGEX_WHITE = new Regex(WHITE_EXPRESSION);
        private readonly char[] DELIMITER = { ',' };

        // 規格
        private const float BLOCK_SIZE = 0.5f;
        private const float BLOCK_SIZE_HALF = 0.25f;
        private const int ID_BUSH = 1;

        // ステージサイズと構造の配列
        private int mStageX;
        private int mStageY;
        private int[,] mStageMap;

        /// <summary>
        /// 指定IDのファイルを読み込んでステージを生成
        /// </summary>
        /// <param name="stageId"></param>
        public void LoadStage(int stageId)
        {
            // 指定されたステージファイルを読み込んで1行ずつ処理
            string filePath = "stage_" + stageId;
            TextAsset textAsset = Resources.Load(filePath) as TextAsset;
            string text = textAsset.text;
            string line;
            using (TextReader reader = new StringReader(text)) // usingとは、処理終わりにstreamの解放を自動で行う構文（finally句でDisposeを実行するコードと同じ）
            {
                while ((line = reader.ReadLine()) != null)
                {
                    // ステージサイズ
                    if (line.StartsWith("@size"))
                    {
                        line = REGEX_WHITE.Replace(line, "");
                        string[] mapSize = line.Split(DELIMITER, System.StringSplitOptions.RemoveEmptyEntries);
                        mStageX = int.Parse(mapSize[1]);
                        mStageY = int.Parse(mapSize[2]);
                        mStageMap = new int[mStageY, mStageX];
                        InstantiateFloor();
                        continue;
                    }

                    // ステージ構造（ブッシュ）
                    if (line.StartsWith("@bush"))
                    {
                        // ステージ構造は16進数表記の文字列なため、10進数のint配列に変換
                        StringBuilder sbStage = new StringBuilder();
                        for (int y = 0; y < mStageY; y++)
                        {
                            sbStage.Append(reader.ReadLine());
                        }
                        int start = 0;
                        for (int y = 0; y < mStageY; y++)
                        {
                            for (int x = 0; x < mStageX; x++)
                            {
                                mStageMap[y, x] = Convert.ToInt32(sbStage.ToString(start, 2), 16);
                                start += 2;
                            }
                        }
                        // ステージ構造を元にブッシュを生成
                        InstantiateBushs();
                        continue;
                    }

                    // ポップアップポイント
                    if (line.StartsWith("@popup"))
                    {
                        line = REGEX_WHITE.Replace(line, "");
                        string[] strPopupPos = line.Split(DELIMITER, System.StringSplitOptions.RemoveEmptyEntries);
                        InstantiatePopup(int.Parse(strPopupPos[1]), int.Parse(strPopupPos[2]));
                    }

                    // プレイヤー初期位置
                    if (line.StartsWith("@player"))
                    {
                        line = REGEX_WHITE.Replace(line, "");
                        string[] strPlayerPos = line.Split(DELIMITER, System.StringSplitOptions.RemoveEmptyEntries);
                        InstantiatePlayer(int.Parse(strPlayerPos[1]), int.Parse(strPlayerPos[2]));
                        continue;
                    }

                    // チェイサー配置
                    if (line.StartsWith("@chaser"))
                    {
                        line = REGEX_WHITE.Replace(line, "");
                        string[] chaserData = line.Split(DELIMITER, System.StringSplitOptions.RemoveEmptyEntries);
                        InstantiateChaser(chaserData);
                        continue;
                    }
                    
                    // ゴールエリア
                    if (line.StartsWith("@goal"))
                    {
                        line = REGEX_WHITE.Replace(line, "");
                        string[] goalData = line.Split(DELIMITER, System.StringSplitOptions.RemoveEmptyEntries);
                        InstantiateGoalArea(goalData);
                        continue;
                    }
                }
            }
        }

        //----------
        // フロア //
        //---------------------------------------------------------------------------------

        [SerializeField]
        private GameObject prefabFloor;

        /// <summary>
        /// ステージサイズに合わせて床を生成
        /// </summary>
        private void InstantiateFloor()
        {
            float sizeX = mStageX * BLOCK_SIZE;
            float sizeZ = mStageY * BLOCK_SIZE - BLOCK_SIZE_HALF;
            Vector3 pos = new Vector3(sizeX / 2.0f, 0.0f, sizeZ / -2.0f);
            GameObject obj = Instantiate(prefabFloor, pos, Quaternion.identity);
            float scaleX = sizeX / 10.0f;
            float scaleZ = sizeZ / 10.0f;
            obj.transform.localScale = new Vector3(scaleX, 1.0f, scaleZ);
            obj.transform.parent = mTrans;
        }

        //-----------------------
        // ブッシュ（ブロック） //
        //---------------------------------------------------------------------------------

        [SerializeField]
        private GameObject prefabBush;

        /// <summary>
        /// ステージを構成するブッシュを生成
        /// </summary>
        private void InstantiateBushs()
        {
            for (int y = 0; y < mStageY; y++)
            {
                for (int x = 0; x < mStageX; x++)
                {
                    switch (mStageMap[y, x])
                    {
                        case ID_BUSH:
                            InstantiateBush(x, y);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// ブロックを指定されたステージ座標に生成
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void InstantiateBush(int x, int y)
        {
            Vector3 pos = new Vector3(x * BLOCK_SIZE + BLOCK_SIZE_HALF, 0.0f, y * -BLOCK_SIZE - BLOCK_SIZE_HALF);
            GameObject obj = Instantiate(prefabBush, pos, Quaternion.identity);
            obj.transform.parent = mTrans;
        }

        //----------------------
        // ポップアップポイント //
        //---------------------------------------------------------------------------------

        [SerializeField]
        private GameObject prefabPopupPoint;
        private List<PopupPoint> mPopupList = new List<PopupPoint>();

        public List<PopupPoint> GetPopupList()
        {
            return mPopupList;
        }

        /// <summary>
        /// ポップアップポイントを指定されたステージ座標に生成
        /// PopupPointの参照を保持する
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void InstantiatePopup(int x, int y)
        {
            Vector3 pos = new Vector3(x * BLOCK_SIZE + BLOCK_SIZE, BLOCK_SIZE_HALF, y * -BLOCK_SIZE - BLOCK_SIZE);
            GameObject obj = Instantiate(prefabPopupPoint, pos, Quaternion.identity);
            obj.transform.parent = mTrans;
            PopupPoint popupPoint = obj.GetComponent<PopupPoint>();
            mPopupList.Add(popupPoint);
        }

        //-------------
        // プレイヤー //
        //---------------------------------------------------------------------------------

        [SerializeField]
        private GameObject prefabPlayer;
        public GameObject Player { get; private set; }

        /// <summary>
        /// プレイヤーを指定されたステージ座標に生成
        /// Player#GameObjectの参照を保持する
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        private void InstantiatePlayer(int posX, int posY)
        {
            Vector3 pos = new Vector3(posX * BLOCK_SIZE + BLOCK_SIZE, 0.0f, posY * -BLOCK_SIZE - BLOCK_SIZE);
            Player = Instantiate(prefabPlayer, pos, Quaternion.Euler(new Vector3(0.0f, 180.0f, 0.0f)));
            SetCameraTarget();
        }

        public void OnActivePlayer()
        {
            Player.GetComponent<PlayerAction>().OnIdle();
        }

        //-------------
        // チェイサー //
        //---------------------------------------------------------------------------------

        [SerializeField]
        private GameObject prefabChaserBlue;
        [SerializeField]
        private GameObject prefabChaserYellow;
        [SerializeField]
        private GameObject prefabChaserRed;

        private List<ChaserModel> mChaserList = new List<ChaserModel>();

        private void InstantiateChaser(string[] ChaserData)
        {
            int modelId = int.Parse(ChaserData[1]);
            float speed = float.Parse(ChaserData[2]);
            float incSpeed = float.Parse(ChaserData[3]);
            int posX = int.Parse(ChaserData[4]);
            int posY = int.Parse(ChaserData[5]);
            Vector3 pos = new Vector3(posX * BLOCK_SIZE + BLOCK_SIZE, 0.0f, posY * -BLOCK_SIZE - BLOCK_SIZE);
            GameObject obj = null;
            switch (modelId)
            {
                case 1:
                    obj = Instantiate(prefabChaserBlue, pos, Quaternion.identity);
                    break;
                case 2:
                    obj = Instantiate(prefabChaserYellow, pos, Quaternion.identity);
                    break;
                default:
                    obj = Instantiate(prefabChaserRed, pos, Quaternion.identity);
                    break;
            }
            ChaserModel chaser = obj.GetComponent<ChaserModel>();
            chaser.Init(speed, incSpeed);
            mChaserList.Add(chaser);
        }

        public void OnActiveChaser()
        {
            foreach (ChaserModel model in mChaserList)
            {
                model.IsActive = true;
            }
        }

        public void OffActiveChaser()
        {
            foreach (ChaserModel model in mChaserList)
            {
                model.gameObject.SetActive(false);
            }
        }

        //---------------
        // ゴールエリア //
        //---------------------------------------------------------------------------------

        [SerializeField]
        private GameObject prefabGoalArea;

        /// <summary>
        /// ゴールエリアを指定されたステージ座標に生成
        /// </summary>
        /// <param name="goalData">
        /// 0:tag
        /// 1:posX
        /// 2:posZ
        /// 3:scaleX
        /// 4:scaleZ
        /// </param>
        private void InstantiateGoalArea(string[] goalData)
        {
            float scaleY = 1.2f;
            float posY = scaleY / 2.0f;
            float posX = int.Parse(goalData[1]) * BLOCK_SIZE + BLOCK_SIZE_HALF;
            float posZ = int.Parse(goalData[2]) * -BLOCK_SIZE - BLOCK_SIZE_HALF;
            Vector3 pos = new Vector3(posX, posY, posZ);
            GameObject obj = Instantiate(prefabGoalArea, pos, Quaternion.identity);
            obj.transform.parent = mTrans;
            float scaleX = float.Parse(goalData[3]);
            float scaleZ = float.Parse(goalData[4]);
            obj.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        }

        //-------------------
        // カメラの追従設定 //
        //---------------------------------------------------------------------------------

        [SerializeField]
        private FollowCamera mFollowCameraMain;
        [SerializeField]
        private FollowCamera mFollowCameraMinimap;

        private void SetCameraTarget()
        {
            mFollowCameraMain.SetTarget(Player.transform, new Vector3(0.0f, 4.0f, -5.0f), Quaternion.Euler(new Vector3(37.0f, 0.0f, 0.0f)));
            mFollowCameraMinimap.SetTarget(Player.transform, new Vector3(0.0f, 12.0f, 0.0f), Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
        }

    }
}
