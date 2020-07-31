//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using AngleSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Infrastructure
{
    public class SqlServerVaultService : ISecurityTokensService
    {
        public SqlServerVaultService()
        {
        }

        #region ISecurityTokensService implementation

        public async Task AddOrUpdateAsync(string key, IDictionary<string, string> values)
        {
            // Prepare the SQL Connection
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlVault"].ConnectionString);

            // Prepare the SQL Command
            SqlCommand cmd = new SqlCommand("WriteSecurityTokens", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@KeyId", SqlDbType.NVarChar, 100).Value = key;
            cmd.Parameters.Add("@Tokens", SqlDbType.NVarChar, -1).Value = JsonConvert.SerializeObject(new { values });

            // Execute the command
            using (connection)
            {
                connection.Open();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<IDictionary<string, string>> GetAsync(string key, string version = null)
        {
            IDictionary<string, string> result = null;

            // Prepare the SQL Connection
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlVault"].ConnectionString);

            // Prepare the SQL Command
            SqlCommand cmd = new SqlCommand("ReadSecurityTokens", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@KeyId", SqlDbType.NVarChar, 100).Value = key;
            cmd.Parameters.Add("@Tokens", SqlDbType.NVarChar, -1).Direction = ParameterDirection.Output;

            // Execute the command
            using (connection)
            {
                connection.Open();
                await cmd.ExecuteNonQueryAsync();

                var values = (string)cmd.Parameters["@Tokens"].Value;
                if (!string.IsNullOrEmpty(values))
                {
                    result = JsonConvert.DeserializeAnonymousType(values, new { values = new Dictionary<string, string>() })?.values;
                }
            }

            if (result == null)
            {
                result = new Dictionary<string, string>();
            }

            return result;
        }

        public async Task<List<string>> ListKeysAsync()
        {
            var result = new List<string>();

            // Prepare the SQL Connection
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlVault"].ConnectionString);

            // Prepare the SQL Command
            SqlCommand cmd = new SqlCommand("ListSecurityTokensKeys", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            // Execute the command
            using (connection)
            {
                connection.Open();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(reader.GetString(0));
                    }
                }
            }

            return result;
        }

        public async Task RemoveKeyAsync(string key)
        {
            // Prepare the SQL Connection
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlVault"].ConnectionString);

            // Prepare the SQL Command
            SqlCommand cmd = new SqlCommand("RemoveSecurityTokens", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@KeyId", SqlDbType.NVarChar, 100).Value = key;

            // Execute the command
            using (connection)
            {
                connection.Open();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task CleanupTokensAsync()
        {
            // Prepare the SQL Connection
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlVault"].ConnectionString);

            // Prepare the SQL Command
            SqlCommand cmd = new SqlCommand("CleanupSecurityTokens", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            // Execute the command
            using (connection)
            {
                connection.Open();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        #endregion
    }
}
