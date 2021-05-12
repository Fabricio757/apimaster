namespace WebApi.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; }

    }
    public class ConnectionString
    {
        public string UsersDB { get; set; }
        public string TestDB { get; set; }
        public string AnimalsDB { get; set; }
    }
}