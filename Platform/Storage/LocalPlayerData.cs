using UnityEngine;

namespace CardGame.Platform.Storage
{
    public sealed class LocalPlayerData
    {
        private const string CoinsKey = "CardGame.Coins";

        public int LoadCoins()
        {
            return PlayerPrefs.GetInt(
                CoinsKey,
                10);
        }

        public void SaveCoins(int coins)
        {
            PlayerPrefs.SetInt(
                CoinsKey,
                coins);

            PlayerPrefs.Save();
        }

        public void Clear()
        {
            PlayerPrefs.DeleteKey(
                CoinsKey);

            PlayerPrefs.Save();
        }
    }
}
