using LIMSAPI.Models;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Data.SqlClient;

namespace LIMSAPI.Helpers
{
    public class AddFilter
    {
        public readonly IConfiguration _configuration;

        public AddFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        internal List<T> GetFilteredList<T>(
        string tableName, string nameColumn, string idColumn, string codeColumn,
        FilterModel filter, Func<SqlDataReader, T> mapFunc, string? selectColumns = null, string? isActiveColumn = "IsActive")
        {
            var resultList = new List<T>();

            try
            {
                using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
                connection.Open();

                string columnsToSelect = selectColumns ?? $"{idColumn}, {nameColumn}, {codeColumn}";
                string query = $"SELECT {columnsToSelect} FROM {tableName} WHERE 1=1";

                var command = new SqlCommand();
                command.Connection = connection;

                // Add conditions dynamically
                if (!string.IsNullOrWhiteSpace(filter.Name))
                {
                    query += $" AND {nameColumn} LIKE @Name";
                    command.Parameters.AddWithValue("@Name", $"%{filter.Name}%");
                }

                if (!string.IsNullOrWhiteSpace(filter.Code))
                {
                    query += $" AND {codeColumn} LIKE @Code";
                    command.Parameters.AddWithValue("@Code", $"%{filter.Code}%");
                }

                if (filter.Id.HasValue)
                {
                    query += $" AND {idColumn} = @Id";
                    command.Parameters.AddWithValue("@Id", filter.Id.Value);
                }

                
                if (filter.IsActive.HasValue)
                {
                    query += $" AND {isActiveColumn} = @IsActive";
                    command.Parameters.AddWithValue("@IsActive", filter.IsActive.Value);
                }

                command.CommandText = query;

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    resultList.Add(mapFunc(reader));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while fetching data from {tableName}.", ex);
            }

            return resultList;
        }



    }
}
