using Configuration;
using Models;
using Models.DTO;
using DbModels;
using DbContext;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;

namespace DbRepos
{
    public class csLoginDbRepos
    {
        #region soon to be used, commented out to reduce warnings
        private ILogger<csLoginDbRepos> _logger = null;
        private string _dbseed = "sysadmin"; 
        private string _dblogin = "gstusr";
        #endregion

        #region contructors
        public csLoginDbRepos(ILogger<csLoginDbRepos> logger)
        {
            _logger = logger;
        }
        public csLoginDbRepos() { }
        /*
        public csLoginDbRepos(ILogger<csLoginDbRepos> logger)
        {
            _logger = logger;
        }
        */
        #endregion

        public async Task<usrInfoDto> SeedAsync(int nrOfUsers, int nrOfSuperUsers)
        {
            using (var db = csMainDbContext.DbContext(_dbseed))
            {
                _logger.LogInformation($"Seeding {nrOfUsers} users and {nrOfSuperUsers} superusers");
                
                //First delete all existing users
                foreach (var u in db.Users)
                    db.Users.Remove(u);

                //add users
                for (int i = 1; i <= nrOfUsers; i++)
                {
                    db.Users.Add(new csUserDbM
                    {
                        UserId = Guid.NewGuid(),
                        UserName = $"user{i}",
                        Email = $"user{i}@gmail.com",
                        Password = EncryptPassword($"user{i}"),
                        Role = "usr"
                    });
                }

                //add super user
                for (int i = 1; i <= nrOfSuperUsers; i++)
                {
                    db.Users.Add(new csUserDbM
                    {
                        UserId = Guid.NewGuid(),
                        UserName = $"superuser{i}",
                        Email = $"superuser{i}@gmail.com",
                        Password = EncryptPassword($"superuser{i}"),
                        Role = "supusr"
                    });
                }
                await db.SaveChangesAsync();

                var _info = new usrInfoDto
                {
                    NrUsers = await db.Users.CountAsync(i => i.Role == "usr"),
                    NrSuperUsers = await db.Users.CountAsync(i => i.Role == "supusr")
                };

                return _info;
            }
        }


        public async Task<loginUserSessionDto> LoginUserAsync(loginCredentialsDto usrCreds)
        {
            using (var db = csMainDbContext.DbContext(_dblogin))
            using (var cmd1 = db.Database.GetDbConnection().CreateCommand())
            {
                //Notice how I use the efc Command to call sp as I do not return any dataset, only output parameters
                //Notice also how I encrypt the password, no coms to database with open password
                cmd1.CommandType = CommandType.StoredProcedure;
                cmd1.CommandText = "gstusr.spLogin";
                cmd1.Parameters.Add(new SqlParameter("UserNameOrEmail", usrCreds.UserNameOrEmail));
                cmd1.Parameters.Add(new SqlParameter("Password", EncryptPassword(usrCreds.Password)));

                int _usrIdIdx = cmd1.Parameters.Add(new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                int _usrIdx = cmd1.Parameters.Add(new SqlParameter("UserName", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output });
                int _roleIdx = cmd1.Parameters.Add(new SqlParameter("Role", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output });


                db.Database.OpenConnection();
                await cmd1.ExecuteScalarAsync();

                var _info = new loginUserSessionDto
                {
                    //Notice the soft cast conversion 'as' it will be null if cast cannot be made
                    UserId = cmd1.Parameters[_usrIdIdx].Value as Guid?,
                    UserName = cmd1.Parameters[_usrIdx].Value as string,
                    UserRole = cmd1.Parameters[_roleIdx].Value as string
                };

                return _info;
            }
        }


        //Industry strength encryption of passwords
        //Notice how I read the Salt and the Iterations from csAppConfig, which is stored in user secrets
        private static string EncryptPassword(string _password)
        {
            //Hash a password using salt and streching
            byte[] encrypted = KeyDerivation.Pbkdf2(
                password: _password,
                salt: Encoding.UTF8.GetBytes(csAppConfig.PasswordSalt.Salt),
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: csAppConfig.PasswordSalt.Iterations,
                numBytesRequested: 64);

            return Convert.ToBase64String(encrypted);
        }
    }
}

