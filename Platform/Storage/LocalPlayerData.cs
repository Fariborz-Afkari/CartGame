using UnityEngine;

namespace CardGame.Platform.Storage
{
    public sealed class LocalPlayerData
    {
        private const string CoinsKey = "cardgame.coins";
        private const int DefaultCoins = 5;

        public int LoadCoins()
        {
            return PlayerPrefs.GetInt(CoinsKey, DefaultCoins);
        }

        public void SaveCoins(int coins)
        {
            PlayerPrefs.SetInt(CoinsKey, coins < 0 ? 0 : coins);
            PlayerPrefs.Save();
        }

        public void Reset()
        {
            PlayerPrefs.DeleteKey(CoinsKey);
            PlayerPrefs.Save();
        }
    }
}
