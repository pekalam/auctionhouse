using System;
using Dapper;
using Microsoft.Data.SqlClient;
using Users.Domain.Auth;
using Users.Domain.Repositories;

namespace Adapter.Dapper.AuctionhouseDatabase
{
    internal class ResetPasswordCodeRepository : IResetPasswordCodeRepository
    {
        private MsSqlConnectionSettings _settings;

        public ResetPasswordCodeRepository(MsSqlConnectionSettings settings)
        {
            _settings = settings;
        }

        public ResetCodeRepresentation CreateResetPasswordCode(ResetCodeRepresentation resetCodeRepresentation)
        {
            var sql = "INSERT INTO dbo.ResetPasswordCode (Checked, DateCreated, Email) " +
                      "OUTPUT Inserted.Id, Inserted.ResetCode, Inserted.DateCreated, Inserted.Checked, Inserted.Email " +
                      "VALUES (@Checked, @DateCreated, @Email)";

            using (var connection = new SqlConnection(_settings.ConnectionString))
            {
                connection.Open();
                var resetCode = connection.QueryFirst<ResetCodeRepresentation>(sql,
                    new
                    {
                        resetCodeRepresentation.Checked,
                        resetCodeRepresentation.DateCreated,
                        resetCodeRepresentation.Email
                    });
                return resetCode;
            }
        }

        public void UpdateResetPasswordCode(ResetCodeRepresentation resetCodeRepresentation)
        {
            var sql = "UPDATE dbo.ResetPasswordCode " +
                      "SET Checked = @Checked, DateCreated = @DateCreated, Email = @Email " +
                      "WHERE Id = @Id";

            using (var connection = new SqlConnection(_settings.ConnectionString))
            {
                connection.Open();
                var affected = connection.Execute(sql,
                    new
                    {
                        resetCodeRepresentation.Id,
                        ResetCode = resetCodeRepresentation.ResetCode.Value,
                        resetCodeRepresentation.Checked,
                        resetCodeRepresentation.DateCreated,
                        resetCodeRepresentation.Email
                    });
                if (affected != 1)
                {
                    throw new Exception();
                }
            }
        }

        public void RemoveResetPasswordCode(ResetCode resetCode, string email)
        {
            var sql = "DELETE FROM dbo.ResetPasswordCode WHERE ResetCode = @ResetCode AND Email = @Email";
            using (var connection = new SqlConnection(_settings.ConnectionString))
            {
                connection.Open();
                connection.Execute(sql, new { ResetCode = resetCode.Value, Email = email });
            }
        }

        public void RemoveResetCodesByEmail(string email)
        {
            var sql = "DELETE FROM dbo.ResetPasswordCode WHERE Email = @Email";
            using (var connection = new SqlConnection(_settings.ConnectionString))
            {
                connection.Open();
                connection.Execute(sql, new { Email = email });
            }
        }

        public ResetCodeRepresentation FindResetPasswordCode(ResetCode resetCode, string email)
        {
            var sql = "SELECT TOP 1 Id, ResetCode, DateCreated, Checked, Email FROM dbo.ResetPasswordCode " +
                      "WHERE ResetCode = @ResetCode AND Email = @Email " +
                      "ORDER BY DateCreated DESC";

            using (var connection = new SqlConnection(_settings.ConnectionString))
            {
                connection.Open();
                var found = connection.QueryFirstOrDefault<ResetCodeRepresentation>(sql,
                    new
                    {
                        ResetCode = resetCode.Value,
                        Email = email
                    });
                return found;
            }
        }

        public int CountResetCodesForEmail(string email)
        {
            var sql = "SELECT COUNT(Id) FROM dbo.ResetPasswordCode " +
                      "WHERE Email = @Email";

            using (var connection = new SqlConnection(_settings.ConnectionString))
            {
                connection.Open();
                var count = connection.ExecuteScalar<int>(sql, new { Email = email });
                return count;
            }
        }
    }
}