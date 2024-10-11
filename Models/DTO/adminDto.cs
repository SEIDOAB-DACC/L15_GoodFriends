using System;
using Configuration;

namespace Models.DTO
{
    public class adminInfoDbDto
    {
        public string appEnvironment => csAppConfig.ASPNETCOREEnvironment;
        public string dbConnection => csAppConfig.DbLoginDetails("sysadmin").DbConnection;
        public string secretSource => csAppConfig.SecretSource;

    }
}