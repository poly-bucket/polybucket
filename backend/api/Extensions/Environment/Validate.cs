using System;

namespace Api.Extensions.Environment
{
    public class Validate
    {
        public static void ValidateEnvironmentVariables()
        {
            ValidateInstanceName();

            ValidateLogLevel();

            ValidateCacheResponse();

            ValidateDatabaseVariables();
        }

        private static void ValidateInstanceName()
        {
            string instanceName = System.Environment.GetEnvironmentVariable(Variables.INSTANCE_NAME);
            if (instanceName.Length < 1)
            {
                System.Console.WriteLine($"Environment variable {Variables.INSTANCE_NAME} must be at least 1 character long.");
                throw new Exception("Environment variable {InstanceName} must be at least 1 character long");
            }
            if (string.IsNullOrEmpty(instanceName) || string.IsNullOrWhiteSpace(instanceName))
            {
                System.Console.WriteLine($"Environment variable {Variables.INSTANCE_NAME} is null or whitespace.");
                throw new Exception("Instance name is either missing or whitespace.");
            }
        }

        private static void ValidateLogLevel()
        {
            if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(Variables.LOG_LEVEL)))
            {
                System.Console.WriteLine($"Environment variable {Variables.LOG_LEVEL} is not set.");
                throw new Exception("Environment variable {InstanceName} is not set");
            }
        }

        private static void ValidateCacheResponse()
        {
            if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(Variables.CACHE_RESPONSE)))
            {
                System.Console.WriteLine($"Environment variable {Variables.CACHE_RESPONSE} is not set.");
                throw new Exception("Environment variable {InstanceName} is not set");
            }
        }

        private static void ValidateDatabaseVariables()
        {
            string dbHost = System.Environment.GetEnvironmentVariable(Variables.DB_HOST);
            string dbPort = System.Environment.GetEnvironmentVariable(Variables.DB_PORT);
            string dbName = System.Environment.GetEnvironmentVariable(Variables.DB_NAME);
            string dbUser = System.Environment.GetEnvironmentVariable(Variables.DB_USER);
            string dbPass = System.Environment.GetEnvironmentVariable(Variables.DB_PASS);

            if (string.IsNullOrEmpty(dbHost))
            {
                throw new Exception($"Environment variable {Variables.DB_HOST} is not set");
            }

            if (string.IsNullOrEmpty(dbPort))
            {
                throw new Exception($"Environment variable {Variables.DB_PORT} is not set");
            }

            if (string.IsNullOrEmpty(dbName))
            {
                throw new Exception($"Environment variable {Variables.DB_NAME} is not set");
            }

            if (string.IsNullOrEmpty(dbUser))
            {
                throw new Exception($"Environment variable {Variables.DB_USER} is not set");
            }

            if (string.IsNullOrEmpty(dbPass))
            {
                throw new Exception($"Environment variable {Variables.DB_PASS} is not set");
            }
        }
    }
}