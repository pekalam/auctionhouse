using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestConfigurationAccessor;

namespace FunctionalTests
{
    public class CommandDbHelper
    {
        public static void ClearUsersAuthenticationData()
        {
            var settings = TestConfig.Instance.GetAuctionhouseRepositorySettings();
            using var connection = new SqlConnection(settings.ConnectionString);
            connection.Open();

            connection.Execute("TRUNCATE TABLE [AuctionhouseDatabase].[dbo].[AuthData]");
        }
    }
}
