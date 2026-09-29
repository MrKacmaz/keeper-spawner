namespace KeeperSpawner
{
    internal static class GameState
    {
        /// <summary>
        /// Bir kayıt yüklü ve oyuncu dünyada mı. Ana menüde MainGame.PlayerData eski kayıttan kalmış
        /// olabileceği için gameState ile birlikte kontrol edilir.
        /// </summary>
        public static bool IsInGame
        {
            get
            {
                var game = MainGame.Instance;
                return game != null
                    && game.gameState == MainGame.GameState.InGame
                    && MainGame.PlayerData != null
                    && MainGame.PlayerData.inventory != null;
            }
        }
    }
}
