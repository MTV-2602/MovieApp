namespace MovieApp.Group01
{
    public static class SessionContext
    {
        public static int CurrentUserId { get; set; }
        public static string CurrentUsername { get; set; } = string.Empty;
        public static int CurrentRole { get; set; }

        public static void Clear()
        {
            CurrentUserId = 0;
            CurrentUsername = string.Empty;
            CurrentRole = 0;
        }
    }
}
