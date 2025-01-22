namespace Api.Extensions.Environment
{
    public class Variables
    {
        /// <summary>
        /// The name of the application.
        /// Default: Polybucket
        /// </summary>
        public static string INSTANCE_NAME => "INSTANCE_NAME";

        /// <summary>
        /// The logging level in which the backend should log at.
        /// Default: Warning
        /// </summary>
        public static string LOG_LEVEL => "LOG_LEVEL";

        /// <summary>
        /// Set
        /// </summary>
        public static string CACHE_RESPONSE => "CACHE_RESPONSE";

        /// <summary>
        /// Address of the database host.
        /// </summary>
        public static string DB_HOST => "DB_HOST";

        /// <summary>
        /// Port of the database host. Default is 3306.
        /// </summary>
        public static string DB_PORT => "DB_PORT";

        /// <summary>
        /// Name of the database. Default is polybucket.
        /// </summary>
        public static string DB_NAME => "DB_NAME";

        /// <summary>
        /// System user in which to connect to the database.
        /// </summary>
        public static string DB_USER => "DB_USER";

        /// <summary>
        /// Password for the DB_USER to connect to the database.
        /// </summary>
        public static string DB_PASS => "DB_PASS";
    }
}