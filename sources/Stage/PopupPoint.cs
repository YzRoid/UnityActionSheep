using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jp.Yzroid.CsgActionSheep
{
    public class PopupPoint : MonoBehaviour
    {

        [SerializeField]
        private GameObject mEffectBox;
        [SerializeField]
        private GameObject prefabFollower;
        private FollowerModel mFollower;

        /// <summary>
        /// フォロワーがポップ中か判定
        /// </summary>
        /// <returns>ポップ中の場合はtrueを返す</returns>
        public bool IsExsistFollower()
        {
            if (mFollower != null)
            {
                if (mFollower.State == FollowerModel.STATE.POPUP) return true;
            }
            return false;
        }

        /// <summary>
        /// フォロワーをポップアップできる状態か判定
        /// 
        /// フォロワーが一度もポップアップしていない or 関連するフォロワーが休眠状態
        /// の場合はポップアップが可能
        /// </summary>
        /// <returns>ポップアップ可能ならばtrueを返す</returns>
        public bool IsReadyPopupFollower()
        {
            if (mFollower == null) return true;
            if (mFollower.State == FollowerModel.STATE.SLEEP) return true;
            return false;
        }

        /// <summary>
        /// フォロワーをポップアップさせる
        /// 同時にエフェクトを発生させる
        /// </summary>
        public void PopupFollower()
        {
            if (mFollower == null)
            {
                GameObject model = Instantiate(prefabFollower, transform.position, Quaternion.identity);
                mFollower = model.GetComponent<FollowerModel>();
            }
            mFollower.Popup();
            mEffectBox.SetActive(true);
        }

        /// <summary>
        /// フォロワーポップ中にプレイヤーが接触した。
        /// エフェクトを非活性にし、ポップ中のフォロワーを返す
        /// </summary>
        /// <returns></returns>
        public FollowerModel DeleverFollower()
        {
            mEffectBox.SetActive(false);
            return mFollower;
        }

    }
}
