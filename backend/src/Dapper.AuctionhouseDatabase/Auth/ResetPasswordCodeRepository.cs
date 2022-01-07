using Core.DomainFramework;
using Dapper;
using Microsoft.Data.SqlClient;
using Users.Domain.Auth;
using Users.Domain.Repositories;

namespace Adapter.Dapper.AuctionhouseDatabase
{
    internal class ResetPasswordCodeRepository : IResetPasswordCodeRepository
    {
        private readonly MsSqlConnectionSettings _settings;

        public ResetPasswordCodeRepository(MsSqlConnectionSettings settings)
        {
            _settings = settings;
        }

        public ResetCodeRepresentation CreateResetPasswordCode(ResetCodeRepresentation resetCodeRepresentation)
        {
            var sql = "INSERT INTO dbo.ResetPasswordCode (Checked, DateCreated, Email) " +
                      "OUTPUT Inserted.Id, Inserted.ResetCode, Inserted.DateCreated, Inserted.Checked, Inserted.Email " +
                      "VALUES (@Checked, @DateCreated, @Email)";

            try
            {
                using var connection = new SqlConnection(_settings.ConnectionString);
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
            catch (Exception e)
            {
                throw new InfrastructureException($"{nameof(CreateResetPasswordCode)} thrown an exception while querying db", e);
            }
        }

        public void UpdateResetPasswordCode(ResetCodeRepresentation resetCodeRepresentation)
        {
            var sql = "UPDATE dbo.ResetPasswordCode " +
                      "SET Checked = @Checked, DateCreated = @DateCreated, Email = @Email " +
                      "WHERE Id = @Id";

            try
            {

                using var connection = new SqlConnection(_settings.ConnectionString);
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
                    throw new ArgumentException("Could not update ResetPasswordCode");
                }
            }
            catch (Exception e)
            {
                throw new InfrastructureException($"{nameof(UpdateResetPasswordCode)} thrown an exception while updatng dbo.ResetPasswordCode", e);
            }
        }

        public void RemoveResetPasswordCode(ResetCode resetCode, string email)
        {
            var sql = "DELETE FROM dbo.ResetPasswordCode WHERE ResetCode = @ResetCode AND Email = @Email";

            try
            {
                using var connection = new SqlConnection(_settings.ConnectionString);
                connection.Open();
                connection.Execute(sql, new { ResetCode = resetCode.Value, Email = email });
            }
            catch (Exception e)
            {
                throw new InfrastructureException($"{nameof(RemoveResetPasswordCode)} thrown an exception while removing item {resetCode.Value} from dbo.ResetPasswordCode", e);
            }
        }

        public void RemoveResetCodesByEmail(string email)
        {
            var sql = "DELETE FROM dbo.ResetPasswordCode WHERE Email = @Email";

            try
            {
                using var connection = new SqlConnection(_settings.ConnectionString);
                connection.Open();
                connection.Execute(sql, new { Email = email });
            }
            catch (Exception e)
            {
                throw new InfrastructureException($"{nameof(RemoveResetCodesByEmail)} thrown an exception while deleting from dbo.ResetPasswordCode", e);
            }
        }

        public ResetCodeRepresentation FindResetPasswordCode(ResetCode resetCode, string email)
        {
            var sql = "SELECT TOP 1 Id, ResetCode, DateCreated, Checked, Email FROM dbo.ResetPasswordCode " +
                      "WHERE ResetCode = @ResetCode AND Email = @Email " +
                      "ORDER BY DateCreated DESC";

            ResetCodeRepresentation? found;

            try
            {
                using var connection = new SqlConnection(_settings.ConnectionString);
                connection.Open();
                found = connection.QueryFirstOrDefault<ResetCodeRepresentation>(sql,
                    new
                    {
                        ResetCode = resetCode.Value,
                        Email = email
                    });
            }
            catch (Exception e)
            {
                throw new InfrastructureException($"{nameof(FindResetPasswordCode)} thrown an exception while selecting from dbo.ResetPasswordCode", e);
            }
            return found;
        }

        public int CountResetCodesForEmail(string email)
        {
            var sql = "SELECT COUNT(Id) FROM dbo.ResetPasswordCode " +
                      "WHERE Email = @Email";

            try
            {
                using var connection = new SqlConnection(_settings.ConnectionString);
                connection.Open();
                var count = connection.ExecuteScalar<int>(sql, new { Email = email });
                return count;
            }
            catch (Exception e)
            {
                throw new InfrastructureException($"{nameof(CountResetCodesForEmail)} thrown an exception while counting rows from dbo.ResetPasswordCode", e);
            }
        }
    }
}