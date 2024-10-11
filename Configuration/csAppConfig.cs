#define UseAzureKV          //comment out this line to use user-secrets instead of Azure Key Vault

using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Xml.Linq;

namespace Configuration;

public sealed class csAppConfig
{
    #region Configuration file locations
    public const string Appsettingfile = "appsettings.json";
    public const string UserSecretsProjFile="../Configuration/Configuration.csproj";
    #endregion

    #region Include Azure Key Vault - Step 1: get Vault access parameters during debug with Azure KV usage
    #if DEBUG
        #if UseAzureKV
            public const string UserSecretPath="/Users/Martin/.microsoft/usersecrets/";
            public const string VaultAccessFile="vault-access.json";
        #endif
    #endif
    #endregion
 
    #region Singleton design pattern
    private static readonly object instanceLock = new();
    private static csAppConfig _instance = null;
    #endregion

    #region Configuration data structures
    //read the user secret Id from the project file. Only dduring development
    private string _userSecretId { 
        get {
            string csprojPath = Path.Combine(Directory.GetCurrentDirectory(), UserSecretsProjFile);
            XDocument csproj = XDocument.Load(csprojPath);
            var userSecretsId = csproj.Descendants("UserSecretsId").FirstOrDefault()?.Value;
            return userSecretsId;        
    }}

    private IConfigurationRoot _configuration = null;
    private DbSetDetail _dbSetActive = new DbSetDetail();
    private List<DbSetDetail> _dbSets = new List<DbSetDetail>();
    private PasswordSaltDetails _passwordSaltDetails = new PasswordSaltDetails();
    private JwtConfig _jwtConfig = new JwtConfig();

    #endregion

    private csAppConfig()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        #if UseAzureKV
            #region Include Azure Key Vault - Step 2: Set Key Vault Access parameters as environment variables
        
            //Environment variables are not set. 
            //For Debug: Read and set them from Vault and set Environment variables
            //For Production: Will be set as Environment variables as part of the deployment process
            string _vap = Path.Combine(UserSecretPath, _userSecretId, VaultAccessFile);
            var _vaultAccess = new ConfigurationBuilder()
                                .AddJsonFile(_vap, optional: true, reloadOnChange: true)
                                .Build();


            Environment.SetEnvironmentVariable("AZURE_KeyVaultUri", _vaultAccess["AZURE_KeyVaultUri"]);
            Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", _vaultAccess["AZURE_CLIENT_SECRET"]);
            Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", _vaultAccess["AZURE_CLIENT_ID"]);
            Environment.SetEnvironmentVariable("AZURE_TENANT_ID", _vaultAccess["AZURE_TENANT_ID"]);
            Environment.SetEnvironmentVariable("AZURE_KeyVaultSecret", _vaultAccess["AZURE_KeyVaultSecret"]);
            #endregion
        #endif

#else        
        //Ensure that also docker environment has Development/Production detection
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
#endif
        #if UseAzureKV
            #region Include Azure Key Vault - Step 3: Access the Key Vault and get the KeyVaultSecret
            //Open the AZKV from environment variables
            var _kvuri = Environment.GetEnvironmentVariable("AZURE_KeyVaultUri");
            var _azureKVSecret = Environment.GetEnvironmentVariable("AZURE_KeyVaultSecret");
            var client = new SecretClient(new Uri(_kvuri), new DefaultAzureCredential(
                new DefaultAzureCredentialOptions { AdditionallyAllowedTenants = { "*" } }));

            //Get user-secrets from AZKV and flatten it into a Dictionary<string, string>
            //that can be build into _configuration
            var secret = client.GetSecret(_azureKVSecret);
            var message = secret.Value.Value;
            var userSecretsAKV = JsonFlatToDictionary(message);
            #endregion
        #endif

        //Create final ConfigurationRoot, _configuration which includes also AzureKeyVault
        var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory());

        #if UseAzureKV
            #region Include Azure Key Vault - Step 4: Add the KeyVaultSecret to the Configuration builder
            //Instead of loading user-secrets, load the secrets from AZKV
            builder = builder.AddInMemoryCollection(userSecretsAKV);
            #endregion
        #else
            //Load the user secrets file
            builder = builder.AddUserSecrets(_userSecretId, reloadOnChange: true);
        #endif

        //override with any locally set configuration from appsettings.json
        builder = builder.AddJsonFile(Appsettingfile, optional: true, reloadOnChange: true);
        _configuration = builder.Build();

        //get DbSet details, Note: Bind need the NuGet package Microsoft.Extensions.Configuration.Binder
        _configuration.Bind("DbSets", _dbSets); 

        //Set the active db set and fill in location and server into Login Details
        var i = int.Parse(_configuration["DbSetActiveIdx"]);
        _dbSetActive = _dbSets[i];
        _dbSetActive.DbLogins.ForEach(i =>
        {
            i.DbLocation = _dbSetActive.DbLocation;
            i.DbServer = _dbSetActive.DbServer;
        });
        
        //get user password details
        _configuration.Bind("PasswordSaltDetails", _passwordSaltDetails);

        //get jwt configurations
        _configuration.Bind("JwtConfig", _jwtConfig);
    }

    #region Include Azure Key Vault  - Step 5: Helper to convert Azure Keyvault secret to Dictionary
    #if UseAzureKV
        private static Dictionary<string, string> JsonFlatToDictionary(string json)
        {
            IEnumerable<(string Path, JsonProperty P)> GetLeaves(string path, JsonProperty p)
                => p.Value.ValueKind != JsonValueKind.Object
                    ? new[] { (Path: path == null ? p.Name : path + ":" + p.Name, p) }
                    : p.Value.EnumerateObject()
                        .SelectMany(child => GetLeaves(path == null ? p.Name : path + ":" + p.Name, child));

            using (JsonDocument document = JsonDocument.Parse(json)) // Optional JsonDocumentOptions options
                return document.RootElement.EnumerateObject()
                    .SelectMany(p => GetLeaves(null, p))
                    //Clone so that we can use the values outside of using
                    .ToDictionary(k => k.Path, v => v.P.Value.Clone().ToString()); 
        }
    #endif
    #endregion

    #region Singleton design pattern
    private static csAppConfig Instance
    {
        get
        {
            lock (instanceLock)
            {
                if (_instance == null)
                {
                    _instance = new csAppConfig();
                }
                return _instance;
            }
        }
    }
    #endregion
    public static string ASPNETCOREEnvironment
    {
        get
        {
            //Just to ensure environment variable is set, by instance creation
            var _ = Instance;
            
            return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        }
    }
    public static IConfigurationRoot ConfigurationRoot => Instance._configuration;
    public static DbSetDetail DbSetActive => Instance._dbSetActive;
    public static DbLoginDetail DbLoginDetails (string DbLogin)
    {
        if (string.IsNullOrEmpty(DbLogin) || string.IsNullOrWhiteSpace(DbLogin))
            throw new ArgumentNullException();

        var conn = Instance._dbSetActive.DbLogins.First(m => m.DbUserLogin.Trim().ToLower() == DbLogin.Trim().ToLower());
        if (conn == null)
            throw new ArgumentException("Database connection not found");

        return conn;
    }

    #region Include Azure Key Vault  - Step 5: Indicate Secret source as Azure Key vault
    #if UseAzureKV
        public static string SecretSource => 
            $"Azure Keyvault secret: {Environment.GetEnvironmentVariable("AZURE_KeyVaultSecret")}";
    #else
        public static string SecretSource => $"User secret: {((Instance._userSecretId == null) ?Appsettingfile :Instance._userSecretId)}";
    #endif
    #endregion

    public static PasswordSaltDetails PasswordSalt => Instance._passwordSaltDetails;
    public static JwtConfig JwtConfig => Instance._jwtConfig;
}

#region types instaniated with configuration content
public class DbSetDetail
{
    public string DbLocation { get; set; }
    public string DbServer { get; set; }

    public List<DbLoginDetail> DbLogins { get; set; }
}

public class DbLoginDetail
{
    //set after reading in the active DbSet
    
    public string DbLocation { get; set; } = null;
    public string DbServer { get; set; } = null;

    public string DbUserLogin { get; set; }
    public string DbConnection { get; set; }
    public string DbConnectionString => csAppConfig.ConfigurationRoot.GetConnectionString(DbConnection);
}


public class PasswordSaltDetails
{
    public string Salt { get; set; }
    public int Iterations { get; set; }
}

public class JwtConfig
{
    public int LifeTimeMinutes { get; set; }

    public bool ValidateIssuerSigningKey { get; set; }
    public string IssuerSigningKey { get; set; }

    public bool ValidateIssuer { get; set; } = true;
    public string ValidIssuer { get; set; }

    public bool ValidateAudience { get; set; } = true;
    public string ValidAudience { get; set; }

    public bool RequireExpirationTime { get; set; }
    public bool ValidateLifetime { get; set; } = true;
}
#endregion

