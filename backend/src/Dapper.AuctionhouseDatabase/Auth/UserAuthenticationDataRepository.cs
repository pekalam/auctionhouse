using Core.DomainFramework;
using Dapper;
using Microsoft.Data.SqlClient;
using Users.Domain.Auth;
using Users.Domain.Repositories;

namespace Adapter.Dapper.AuctionhouseDatabase
{
    internal class UserAuthenticationDataRepository : IUserAuthenticationDataRepository
    {
        private readonly MsSqlConnectionSettings _connectionSettings;

        public UserAuthenticationDataRepository(MsSqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }

        public UserAuthenticationData FindUserAuthById(Guid id)
        {
            var sql = "SELECT UserId, Username, Password, Email FROM dbo.AuthData WHERE UserId = @UserId";

            using var connection = new SqlConnection(_connectionSettings.ConnectionString);
            connection.Open();
            var authData = connection.QueryFirstOrDefault<UserAuthenticationData>(sql, new { UserId = id });
            return authData;
        }

        public UserAuthenticationData FindUserAuth(string userName)
        {
            var sql = "SELECT UserId, Username, Password, Email FROM dbo.AuthData WHERE Username = @Username";

            using var connection = new SqlConnection(_connectionSettings.ConnectionString);
            connection.Open();
            var authData = connection.QueryFirstOrDefault<UserAuthenticationData>(sql, new { Username = userName });
            return authData;
        }

        public UserAuthenticationData AddUserAuth(UserAuthenticationData userAuthenticationData)
        {
            var sql = "INSERT INTO dbo.AuthData (UserId, Username, Password, Email) VALUES (@UserId, @Username, @Password, @Email)";

            using var connection = new SqlConnection(_connectionSettings.ConnectionString);
            connection.Open();
            var affected = connection.Execute(sql, new
            {
                Username = userAuthenticationData.UserName,
                userAuthenticationData.Password,
                userAuthenticationData.UserId,
                userAuthenticationData.Email
            });
            if (affected <= 0)
            {
                throw new Exception();
            }
            return userAuthenticationData;
        }

        public UserAuthenticationData FindUserAuthByEmail(string email)
        {
            var sql = "SELECT UserId, Username, Password, Email FROM dbo.AuthData WHERE Email = @Email";

            using var connection = new SqlConnection(_connectionSettings.ConnectionString);
            connection.Open();
            var authData = connection.QueryFirstOrDefault<UserAuthenticationData>(sql, new { Email = email });
            return authData;
        }

        public void UpdateUserAuth(UserAuthenticationData userAuthenticationData)
        {
            var sql = "UPDATE dbo.AuthData SET UserId = @UserId, Username = @Username, Password = @Password, Email = @Email WHERE UserId = @UserId";

            try
            {
                using var connection = new SqlConnection(_connectionSettings.ConnectionString);
                connection.Open();
                var affected = connection.Execute(sql, new
                {
                    Username = userAuthenticationData.UserName,
                    userAuthenticationData.Password,
                    userAuthenticationData.UserId,
                    userAuthenticationData.Email
                });
                if (affected <= 0)
                {
                    throw new Exception();
                }
            }
            catch (Exception e)
            {
                throw new InfrastructureException($"{nameof(UpdateUserAuth)} thrown an exception while updating db", e);
            }
        }
    }
}