using System;
using System.Data.SqlClient;
using Core.Common.Auth;
using Dapper;
using Infrastructure.Repositories.SQLServer;

namespace Infrastructure.Auth
{
    public class UserAuthenticationDataRepository : IUserAuthenticationDataRepository
    {
        private readonly UserAuthDbContextOptions _connectionSettings;

        public UserAuthenticationDataRepository(UserAuthDbContextOptions connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }

        public UserAuthenticationData FindUserAuthById(Guid id)
        {
            var sql = "SELECT UserId, Username, Password, Email FROM dbo.AuthData WHERE UserId = @UserId";

            using (var connection = new SqlConnection(_connectionSettings.ConnectionString))
            {
                connection.Open();
                var authData = connection.QueryFirstOrDefault<UserAuthenticationData>(sql, new {UserId = id});
                return authData;
            }
        }

        public UserAuthenticationData FindUserAuth(string userName)
        {
            var sql = "SELECT UserId, Username, Password, Email FROM dbo.AuthData WHERE Username = @Username";

            using (var connection = new SqlConnection(_connectionSettings.ConnectionString))
            {
                connection.Open();
                var authData = connection.QueryFirstOrDefault<UserAuthenticationData>(sql, new { Username = userName});
                return authData;
            }
        }

        public UserAuthenticationData AddUserAuth(UserAuthenticationData userAuthenticationData)
        {
            var sql = "INSERT INTO dbo.AuthData (UserId, Username, Password, Email) VALUES (@UserId, @Username, @Password, @Email)";

            using (var connection = new SqlConnection(_connectionSettings.ConnectionString))
            {
                connection.Open();
                var affected = connection.Execute(sql, new
                {
                    Username = userAuthenticationData.UserName,
                    Password = userAuthenticationData.Password,
                    UserId = userAuthenticationData.UserId,
                    Email = userAuthenticationData.Email
                });
                if (affected <= 0)
                {
                    throw new Exception();
                }
                return userAuthenticationData;
            }
        }

        public void SaveUserAuth(UserAuthenticationData userAuthenticationData)
        {
            var sql = "INSERT INTO dbo.AuthData (UserId, Username, Password, Email) VALUES (@UserId, @Username, @Password, @Email)";

            using (var connection = new SqlConnection(_connectionSettings.ConnectionString))
            {
                connection.Open();
                var affected = connection.Execute(sql, new
                {
                    Username = userAuthenticationData.UserName,
                    Password = userAuthenticationData.Password,
                    UserId = userAuthenticationData.UserId,
                    Email = userAuthenticationData.Email
                });
                if (affected <= 0)
                {
                    throw new Exception();
                }
            }
        }
    }
}