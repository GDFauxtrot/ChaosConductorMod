namespace ChaosConductor
{
    public static class ModLogger
    {
        /// <summary>
        /// A helper function designed to log verbose statements similarly to Unity's "Debug.Log".
        /// More versatile however, as statements can also be viewed in an attached console as well
        /// as the game's "Player.log" file, and always comes timestamped.
        /// </summary>
        public static void Log(object input)
        {
            Main.Log.LogInfo(System.DateTime.Now.ToString("HH:mm:ss:fff | ") + (input?.ToString() ?? "null"));
        }
    }
}
