using System;

namespace CartaAwsDeploy
{
    public class Environment
    {
        public AccountType AccountType { get; }

        public string ContextPrefix { get; }

        public Environment ()
        {
            string account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT");
            switch (account)
            {
                case "755669635175":
                    AccountType = AccountType.DEVELOPMENT;
                    ContextPrefix = "dev";
                    break;
                case "673623579371":
                    AccountType = AccountType.STAGING;
                    ContextPrefix = "stg";
                    break;
                case "296221556147":
                    AccountType = AccountType.PRODUCTION;
                    ContextPrefix = "prd";
                    break;
                default:
                    throw new ArgumentException($"Unsupported account '{account}'");
            }
        }
    }

    public enum AccountType
    {
        DEVELOPMENT,
        STAGING,
        PRODUCTION
    }
}
