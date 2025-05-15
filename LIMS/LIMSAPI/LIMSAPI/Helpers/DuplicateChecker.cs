using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace LIMSAPI.Helpers
{
    public class DuplicateChecker
    {
        private readonly IConfiguration _configuration;

        public DuplicateChecker(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool IsDuplicate(string table, string nameCol, string codeCol, string nameVal, string codeVal, int? excludeId = null, string idCol = "Id", Dictionary<string, object> additionalConditions = null)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();

            var query = $@"
                   SELECT COUNT(*) 
                       FROM [{table}] 
                       WHERE ({nameCol} = @nameVal OR {codeCol} = @codeVal)";

            // Add additional conditions to the query if they exist
            if (additionalConditions != null && additionalConditions.Any())
            {
                foreach (var condition in additionalConditions)
                {
                    query += $" AND [{condition.Key}] = @{condition.Key}";
                }
            }

            // Exclude the current record if needed
            if (excludeId.HasValue)
            {
                query += $" AND [{idCol}] != @excludeId";
            }

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@nameVal", nameVal);
            command.Parameters.AddWithValue("@codeVal", codeVal);

            // Add additional conditions parameters
            if (additionalConditions != null && additionalConditions.Any())
            {
                foreach (var condition in additionalConditions)
                {
                    command.Parameters.AddWithValue($"@{condition.Key}", condition.Value);
                }
            }

            // Exclude the current record if needed
            if (excludeId.HasValue)
            {
                command.Parameters.AddWithValue("@excludeId", excludeId.Value);
            }

            int count = (int)command.ExecuteScalar();
            return count > 0;
        }

    }
}