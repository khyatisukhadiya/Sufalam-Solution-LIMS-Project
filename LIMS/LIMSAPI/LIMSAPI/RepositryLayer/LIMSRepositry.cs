using System;
using System.Collections.Generic;
using System.Data;
using System.Formats.Tar;
using System.IO.Pipelines;
using System.Linq;
using System.Reflection;
using System.Transactions;
using Azure;
using Azure.Core;
using LIMSAPI.Helpers;
using LIMSAPI.Models;
using LIMSAPI.Models.FinanceModal;
using LIMSAPI.Models.Master;
using LIMSAPI.Models.Transaction;
using LIMSAPI.Models.TransactionModal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Azure.Core.HttpHeader;

namespace LIMSAPI.RepositryLayer
{
    public class LIMSRepositry : LIMSRepositryInterface
    {

        public readonly IConfiguration _configuration;
        public readonly SqlConnection _sqlConnection;
        public readonly DuplicateChecker _duplicateChecker;
        public readonly AddFilter _addFilter;

        public LIMSRepositry(IConfiguration configuration)
        {
            _duplicateChecker = new DuplicateChecker(configuration);
            _configuration = configuration;
            _sqlConnection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
            _addFilter = new AddFilter(_configuration);
        }


        // DUPLICATE CHECKER
        public bool IsDuplicate(string table, string nameCol, string codeCol, string nameVal, string codeVal, int? excludeId = null, string idCol = "Id", Dictionary<string, object> additionalConditions = null)

        {
            return _duplicateChecker.IsDuplicate(table, nameCol, codeCol, nameVal, codeVal, excludeId, idCol, additionalConditions);
        }



        // COUNTRY
        public CountryModal AddUpdatedCountry(CountryModal countryModal)
        {
            var response = new CountryModal();

            try
            {

                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query;
                SqlCommand command;

                if (countryModal.CountryId > 0)
                {
                    // UPDATE + SELECT updated row
                    query = @"
                UPDATE country 
                SET CountryCode = @CountryCode, CountryName = @CountryName 
                WHERE CountryId = @CountryId;

                SELECT CountryId, CountryCode, CountryName, IsActive 
                FROM country 
                WHERE CountryId = @CountryId";

                    command = new SqlCommand(query, _sqlConnection);
                    command.Parameters.AddWithValue("@CountryId", countryModal.CountryId);
                }
                else
                {
                    // INSERT and return inserted ID
                    query = @"
                INSERT INTO country (CountryCode , CountryName) 
                OUTPUT INSERTED.CountryId 
                VALUES (@CountryCode, @CountryName)";

                    command = new SqlCommand(query, _sqlConnection);
                }

                command.Parameters.AddWithValue("@CountryCode", countryModal.CountryCode);
                command.Parameters.AddWithValue("@CountryName", countryModal.CountryName);

                if (countryModal.CountryId > 0)
                {
                    using var reader = command.ExecuteReader();

                    // Move to the SELECT result set
                    if (reader.Read())
                    {
                        response.CountryId = reader.GetInt32(0);
                        response.CountryCode = reader.GetString(1);
                        response.CountryName = reader.GetString(2);
                        response.IsActive = !reader.IsDBNull(3) && reader.GetBoolean(3);
                    }
                    else
                    {
                        throw new Exception("Update succeeded but no data returned.");
                    }
                }

                else
                {
                    // Return inserted row info
                    int insertedId = (int)command.ExecuteScalar();
                    response.CountryId = insertedId;
                    response.CountryCode = countryModal.CountryCode;
                    response.CountryName = countryModal.CountryName;
                    response.IsActive = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving country: " + ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }

            return response;
        }

        public List<CountryModal> GetAllCountries(FilterModel filter)
        {
            try
            {
                return _addFilter.GetFilteredList<CountryModal>(
                    tableName: "country",
                    nameColumn: "CountryName",
                    idColumn: "CountryId",
                    codeColumn: "CountryCode",
                    filter: filter,
                    mapFunc: reader => new CountryModal
                    {
                        CountryId = reader.GetInt32(0),
                        CountryName = reader.GetString(1),
                        CountryCode = reader.GetString(2),
                        IsActive = reader.GetBoolean(3)
                    },
                    selectColumns: "CountryId, CountryName, CountryCode, IsActive"
                );
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching countries.", ex);
            }
        }

        public CountryModal DeleteCountryById(int CountryId)
        {
            var response = new CountryModal();
            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                    _sqlConnection.Open();

                string query = @"UPDATE country SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END WHERE  CountryId= @CountryId;

                                SELECT CountryId, CountryName, CountryCode, IsActive FROM country WHERE CountryId = @CountryId;";

                var commond = new SqlCommand(query, _sqlConnection);
                commond.Parameters.AddWithValue("@CountryId", CountryId);

                var reader = commond.ExecuteReader();
                if (reader.Read())
                {
                    response.CountryId = reader.GetInt32(0);
                    response.CountryName = reader.GetString(1);
                    response.CountryCode = reader.GetString(2);
                    response.IsActive = reader.GetBoolean(3);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }

        public List<CountryModal> GetCountry()
        {
            var response = new List<CountryModal>();

            try
            {

                using (SqlConnection sqlConnection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
                {

                    sqlConnection.Open();


                    string query = "SELECT CountryId, CountryName, CountryCode, IsActive FROM country WHERE IsActive = 1";

                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {


                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                response.Add(new CountryModal
                                {
                                    CountryId = reader.GetInt32(0),
                                    CountryName = reader.GetString(1),
                                    CountryCode = reader.GetString(2),
                                    IsActive = reader.GetBoolean(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw new Exception("Error fetching all countries", ex);
            }

            return response;
        }

        public CountryModal GetCountryById(int CountryId)
        {
            var response = new CountryModal();

            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = "SELECT * FROM country WHERE CountryId = @CountryId";

                var commond = new SqlCommand(query, _sqlConnection);
                commond.Parameters.AddWithValue("@CountryId", CountryId);

                var reader = commond.ExecuteReader();
                if (reader.Read())
                {
                    response.CountryId = reader.GetInt32(0);
                    response.CountryCode = reader.GetString(1);
                    response.CountryName = reader.GetString(2);
                    response.IsActive = reader.GetBoolean(3);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }



        // STATE
        public StateModal AddUpdatedState(StateModal stateModal)
        {
            var response = new StateModal();

            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                if (stateModal.StateId > 0)
                {
                    // UPDATE + SELECT to return updated row
                    string query = @"
                UPDATE state
                SET StateCode = @StateCode, StateName = @StateName, CountryId = @CountryId
                WHERE StateId = @StateId;

                SELECT s.StateId, s.StateName, s.StateCode, s.CountryId, s.IsActive, c.CountryName
                FROM state s
                INNER JOIN country c ON s.CountryId = c.CountryId
                WHERE s.StateId = @StateId";

                    using var command = new SqlCommand(query, _sqlConnection);
                    command.Parameters.AddWithValue("@StateId", stateModal.StateId);
                    command.Parameters.AddWithValue("@StateCode", stateModal.StateCode);
                    command.Parameters.AddWithValue("@StateName", stateModal.StateName);
                    command.Parameters.AddWithValue("@CountryId", stateModal.CountryId);

                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        response.StateId = reader.GetInt32(0);
                        response.StateName = reader.GetString(1);
                        response.StateCode = reader.GetString(2);
                        response.CountryId = reader.GetInt32(3);
                        response.IsActive = !reader.IsDBNull(4) && reader.GetBoolean(4);
                        response.CountryName = reader.GetString(5);
                    }
                    else
                    {
                        throw new Exception("Update succeeded but no data returned.");
                    }
                }
                else
                {
                    // INSERT and return new ID
                    string query = @"
                INSERT INTO state (StateName, StateCode, CountryId)
                OUTPUT INSERTED.StateId 
                VALUES (@StateName, @StateCode, @CountryId)";

                    using var command = new SqlCommand(query, _sqlConnection);
                    command.Parameters.AddWithValue("@StateName", stateModal.StateName);
                    command.Parameters.AddWithValue("@StateCode", stateModal.StateCode);
                    command.Parameters.AddWithValue("@CountryId", stateModal.CountryId);

                    int insertedId = (int)command.ExecuteScalar();

                    // Fetch country name separately
                    string countryQuery = "SELECT CountryName FROM country WHERE CountryId = @CountryId";
                    using var countryCommand = new SqlCommand(countryQuery, _sqlConnection);
                    countryCommand.Parameters.AddWithValue("@CountryId", stateModal.CountryId);

                    string countryName = (string?)countryCommand.ExecuteScalar() ?? "";

                    response.StateId = insertedId;
                    response.StateName = stateModal.StateName;
                    response.StateCode = stateModal.StateCode;
                    response.CountryId = stateModal.CountryId;
                    response.CountryName = countryName;
                    response.IsActive = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving state: " + ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }

            return response;
        }

        public List<StateModal> GetAllStates(FilterModel filter)
        {
            try
            {
                string query = $@"state 
                               JOIN country ON state.CountryId = country.CountryId";


                return _addFilter.GetFilteredList<StateModal>(
                    tableName: query,
                    nameColumn: "StateName",
                    idColumn: "StateId",
                    codeColumn: "StateCode",
                    filter: filter,
                    mapFunc: reader => new StateModal
                    {
                        StateId = reader.GetInt32(0),
                        StateName = reader.GetString(1),
                        StateCode = reader.GetString(2),
                        CountryId = reader.GetInt32(3),
                        IsActive = reader.GetBoolean(4),
                        CountryName = reader.IsDBNull(5) ? null : reader.GetString(5)
                    },
                    selectColumns: "state.StateId, state.StateName, state.StateCode, state.CountryId, state.IsActive, country.CountryName",
                    isActiveColumn: "state.IsActive"

                );
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching State.", ex);
            }
        }

        public StateModal DeleteStateById(int StateId)
        {
            var response = new StateModal();

            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = @"UPDATE state SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END WHERE  StateId= @StateId;

                                SELECT state.StateId, state.StateName, state.StateCode, state.CountryId, state.IsActive, country.CountryName 
                                     FROM state 
                                     JOIN country ON state.CountryId = country.CountryId 
                                      WHERE StateId= @StateId";

                var commond = new SqlCommand(query, _sqlConnection);
                commond.Parameters.AddWithValue("@StateId", StateId);

                var reader = commond.ExecuteReader();
                if (reader.Read())
                {
                    response.StateId = reader.GetInt32(0);
                    response.StateCode = reader.GetString(1);
                    response.StateName = reader.GetString(2);
                    response.CountryId = reader.GetInt32(3);
                    response.IsActive = reader.GetBoolean(4);
                    response.CountryName = reader.GetString(5);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }

            return response;
        }

        public List<StateModal> GetState()
        {
            var response = new List<StateModal>();

            try
            {

                using (SqlConnection _sqlConnection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
                {
                    _sqlConnection.Open();

                    string query = @"SELECT state.StateId, state.StateName, state.StateCode, state.CountryId, state.IsActive, country.CountryName 
                                     FROM state 
                                     JOIN country ON state.CountryId = country.CountryId 
                                      WHERE state.IsActive = 1";

                    using (SqlCommand command = new SqlCommand(query, _sqlConnection))

                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                response.Add(new StateModal
                                {
                                    StateId = reader.GetInt32(0),
                                    StateName = reader.GetString(1),
                                    StateCode = reader.GetString(2),
                                    CountryId = reader.GetInt32(3),
                                    IsActive = reader.GetBoolean(4),
                                    CountryName = reader.GetString(5),
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error Fetching State", ex);
            }
            return response;
        }

        public StateModal GetStateById(int StateId)
        {
            var response = new StateModal();

            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = @"SELECT state.StateId, state.StateName, state.StateCode, state.CountryId, state.IsActive, country.CountryName 
                                     FROM state 
                                     JOIN country ON state.CountryId = country.CountryId 
                                      WHERE StateId= @StateId";

                var commond = new SqlCommand(query, _sqlConnection);
                commond.Parameters.AddWithValue("@StateId", StateId);

                var reader = commond.ExecuteReader();
                if (reader.Read())
                {
                    response.StateId = reader.GetInt32(0);
                    response.StateName = reader.GetString(1);
                    response.StateCode = reader.GetString(2);
                    response.CountryId = reader.GetInt32(3);
                    response.IsActive = reader.GetBoolean(4);
                    response.CountryName = reader.GetString(5);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }




        // CITY
        public CityModal AddUpdatedCity(CityModal cityModal)
        {
            var response = new CityModal();

            try
            {

                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                if (cityModal.CityId > 0)
                {
                    string query = $@"UPDATE city SET CityCode = @CityCode, CityName = @CityName, stateId = @StateId WHERE CityId = @CityId

                                     SELECT c.CityId, c.CityCode, c.CityName, s.StateId, c.IsActive, s.StateName 
                                     WHERE city c
                                     INNER JOIN state s ON c.StateId = s.StateId
                                     WHERE c.CityId = @CityId";

                    using var common = new SqlCommand(query, _sqlConnection);
                    common.Parameters.AddWithValue("@CityId", cityModal.CityId);
                    common.Parameters.AddWithValue("@CityCode", cityModal.CityCode);
                    common.Parameters.AddWithValue("@CityName", cityModal.CityName);
                    common.Parameters.AddWithValue("@StateId", cityModal.StateId);

                    using var reader = common.ExecuteReader();
                    if (reader.Read())
                    {
                        response.CityId = Convert.ToInt32(reader["CityId"]);
                        response.CityCode = reader["CityCode"].ToString();
                        response.CityName = reader["CityName"].ToString();
                        response.StateId = Convert.ToInt32("StateId");
                        response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                        response.StateName = reader["StateName"].ToString();
                    }
                    else
                    {
                        throw new Exception("Update succcessed but not data returned.");
                    }
                }
                else
                {
                    string query = $@"INSERT INTO city(CityCode, CityName, StateId ) OUTPUT INSERTED.CityId VALUES (@CityCode, @CityName, @StateId) ";

                    using var common = new SqlCommand(query, _sqlConnection);
                    common.Parameters.AddWithValue("@CityCode", cityModal.CityCode);
                    common.Parameters.AddWithValue("@CityName", cityModal.CityName);
                    common.Parameters.AddWithValue("@StateId", cityModal.StateId);

                    int insertedId = (int)common.ExecuteScalar();

                    string stateQuery = "SELECT StateName FROM state WHERE StateId = @StateId";
                    using var stateCommond = new SqlCommand(stateQuery, _sqlConnection);
                    stateCommond.Parameters.AddWithValue("@StateId", cityModal.StateId);

                    string stateName = (string?)stateCommond.ExecuteScalar() ?? "";

                    response.CityId = insertedId;
                    response.CityCode = cityModal.CityCode;
                    response.CityName = cityModal.CityName;
                    response.StateId = cityModal.StateId;
                    response.StateName = stateName;
                    response.IsActive = true;
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching saving data", ex);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }

        public List<CityModal> GetAllCities(FilterModel filter)
        {
            try
            {
                string query = $@"city
                                  JOIN state ON city.StateId = state.StateId";

                return _addFilter.GetFilteredList<CityModal>(
                    tableName: query,
                    nameColumn: "CityName",
                    codeColumn: "CityCode",
                    idColumn: "CityId",
                    filter: filter,
                    mapFunc: reader => new CityModal
                    {
                        CityId = Convert.ToInt32(reader["CityId"]),
                        CityCode = reader["CityCode"].ToString(),
                        CityName = reader["CityName"].ToString(),
                        StateId = Convert.ToInt32(reader["StateId"]),
                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                        StateName = reader["StateName"].ToString()
                    },
                    selectColumns: "city.CityId, city.CityCode, city.CityName, city.StateId, city.IsActive, state.StateName",
                    isActiveColumn: "city.IsActive"
                );
            }
            catch (Exception ex)
            {
                throw new Exception("An error ocuured while fatching city", ex);
            }
        }

        public CityModal DeleteCityById(int CityId)
        {
            var response = new CityModal();

            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = $@"UPDATE city SET IsActive = CASE WHEN  Isactive = 1 THEN 0 ELSE 1 END WHERE CityId = @CityId

                            SELECT c.CityId, c.CityCode, c.CityName, c.StateId, c.IsActive, s.StateName
                            FROM city c
                            JOIN state s ON c.StateId = s.StateId
                            WHERE CityId = @CityId";

                var commond = new SqlCommand(query, _sqlConnection);
                commond.Parameters.AddWithValue("@CityId", CityId);

                var reader = commond.ExecuteReader();
                if (reader.Read())
                {
                    response.CityId = Convert.ToInt32(reader["CityId"]);
                    response.CityCode = reader["CityCode"].ToString();
                    response.CityName = reader["CityName"].ToString();
                    response.StateId = Convert.ToInt32(reader["StateId"]);
                    response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                    response.StateName = reader["StateName"].ToString();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }

        public List<CityModal> GetCityIsActive()
        {
            var response = new List<CityModal>();

            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = $@" SELECT c.CityId, c.CityCode, c.CityName, c.StateId, c.IsActive, s.StateName
                            FROM city c
                            JOIN state s ON c.StateId = s.StateId
                            WHERE c.IsActive = 1";

                using (SqlCommand command = new SqlCommand(query, _sqlConnection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.Add(new CityModal
                            {
                                CityId = Convert.ToInt32(reader["CityId"]),
                                CityCode = reader["CityCode"].ToString(),
                                CityName = reader["CityName"].ToString(),
                                StateId = Convert.ToInt32(reader["StateId"]),
                                IsActive = Convert.ToBoolean(reader["IsActive"]),
                                StateName = reader["StateName"].ToString()
                            });
                        }
                    }
                }
;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching city", ex);
            }
            return response;
        }

        public CityModal GetCityById(int CityId)
        {
            var response = new CityModal();
            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = $@" SELECT c.CityId, c.CityCode, c.CityName, c.StateId, c.IsActive, s.StateName
                            FROM city c
                            JOIN state s ON c.StateId = s.StateId
                            WHERE CityId = @CityId";

                var common = new SqlCommand(query, _sqlConnection);
                common.Parameters.AddWithValue("@CityId", CityId);

                var reader = common.ExecuteReader();
                if (reader.Read())
                {
                    response.CityId = Convert.ToInt32(reader["CityId"]);
                    response.CityCode = reader["CityCode"].ToString();
                    response.CityName = reader["CityName"].ToString();
                    response.StateId = Convert.ToInt32(reader["StateId"]);
                    response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                    response.StateName = reader["StateName"].ToString();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }




        // AREA
        public AreaModal AddUpdatedArea(AreaModal areaModal)
        {
            var response = new AreaModal();

            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                if (areaModal.AreaId > 0)
                {
                    string query = $@" UPDATE area SET PinCode = @PinCode, AreaName = @AreaName, CityId = @CityId WHERE AreaId = @AreaId

                                         SELECT a.AreaId, a.PinCode, a.AreaName, c.CityId, a.IsActive , c.CityName
                                         FROM area a
                                         INNER JOIN city c ON a.CityId = c.CityId
                                         WHERE a.AreaId = @AreaId";

                    using var common = new SqlCommand(query, _sqlConnection);
                    common.Parameters.AddWithValue("@AreaId", areaModal.AreaId);
                    common.Parameters.AddWithValue("@PinCode", areaModal.PinCode);
                    common.Parameters.AddWithValue("@AreaName", areaModal.AreaName);
                    common.Parameters.AddWithValue("@CityId", areaModal.CityId);

                    using var reader = common.ExecuteReader();
                    if (reader.Read())
                    {
                        response.AreaId = Convert.ToInt32(reader["AreaId"]);
                        response.PinCode = reader["PinCode"].ToString();
                        response.AreaName = reader["AreaName"].ToString();
                        response.CityId = Convert.ToInt32(reader["CityId"]);
                        response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                        response.CityName = reader["CityName"].ToString();
                    }
                    else
                    {
                        throw new Exception("Update Successed but not found data");
                    }
                }
                else
                {
                    string query = $@"INSERT INTO area (PinCode, AreaName, CityId) OUTPUT INSERTED.AreaId VALUES (@PinCode, @AreaName, @CityId)";

                    var common = new SqlCommand(query, _sqlConnection);
                    common.Parameters.AddWithValue("@PinCode", areaModal.PinCode);
                    common.Parameters.AddWithValue("@AreaName", areaModal.AreaName);
                    common.Parameters.AddWithValue("@CityId", areaModal.CityId);

                    var insertedId = (int)common.ExecuteScalar();

                    string cityQuery = "SELECT CityName FROM city WHERE CityId = @CityId";
                    var cityCommon = new SqlCommand(cityQuery, _sqlConnection);
                    cityCommon.Parameters.AddWithValue("CityId", areaModal.CityId);

                    string CityName = (string?)cityCommon.ExecuteScalar() ?? "";

                    response.AreaId = insertedId;
                    response.PinCode = areaModal.PinCode;
                    response.AreaName = areaModal.AreaName;
                    response.CityId = areaModal.CityId;
                    response.IsActive = true;
                    response.CityName = CityName;

                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching saving data" + ex.Message, ex);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }

        public List<AreaModal> GetAllArea(FilterModel filterModel)
        {

            try
            {

                string query = @$"area a 
                                  JOIN city c ON a.CityId = c.CityId";


                return _addFilter.GetFilteredList<AreaModal>(
                    tableName: query,
                    nameColumn: "AreaName",
                    codeColumn: "PinCode",
                    idColumn: "AreaId",
                    filter: filterModel,
                    mapFunc: reader => new AreaModal
                    {
                        AreaId = Convert.ToInt32(reader["AreaId"]),
                        PinCode = reader["PinCode"].ToString(),
                        AreaName = reader["AreaName"].ToString(),
                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                        CityId = Convert.ToInt32(reader["CityId"]),
                        CityName = reader["CityName"].ToString()
                    },
                    selectColumns: "a.AreaId, a.PinCode, a.AreaName, a.IsActive, a.CityId, c.CityName",
                    isActiveColumn: "a.IsActive"
               );
            }
            catch (Exception ex)
            {
                throw new Exception("an erroe occure in getching data", ex);
            }

        }

        public AreaModal DeleteAreaById(int AreaId)
        {
            var response = new AreaModal();
            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = $@"UPDATE area SET IsActive = CASE WHEN IsActive = 1 THEN 0 Else 1 END WHERE AreaId = @AreaId
                                SELECT a.AreaId, a.PinCode, a.AreaName, a.CityId, a.IsActive , c.CityName
                                         FROM area a
                                         INNER JOIN city c ON a.CityId = c.CityId
                                         WHERE a.AreaId = @AreaId";

                var common = new SqlCommand(query, _sqlConnection);
                common.Parameters.AddWithValue("@AreaId", AreaId);

                var reader = common.ExecuteReader();
                if (reader.Read())
                {
                    response.AreaId = Convert.ToInt32(reader["AreaId"]);
                    response.PinCode = reader["PinCode"].ToString();
                    response.AreaName = reader["AreaName"].ToString();
                    response.CityId = Convert.ToInt32(reader["CityId"]);
                    response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                    response.CityName = reader["CityName"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }

        public List<AreaModal> GetAreaIsActive()
        {
            var response = new List<AreaModal>();

            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = $@" SELECT a.AreaId, a.PinCode, a.AreaName, a.CityId, a.IsActive , c.CityName
                                         FROM area a
                                         INNER JOIN city c ON a.CityId = c.CityId
                                         WHERE a.IsActive = 1";

                using (SqlCommand command = new SqlCommand(query, _sqlConnection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.Add(new AreaModal
                            {
                                AreaId = Convert.ToInt32(reader["AreaId"]),
                                PinCode = reader["PinCode"].ToString(),
                                AreaName = reader["AreaName"].ToString(),
                                IsActive = Convert.ToBoolean(reader["IsActive"]),
                                CityId = Convert.ToInt32(reader["CityId"]),
                                CityName = reader["CityName"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("error fetching data", ex);
            }
            return response;
        }

        public AreaModal GetAreaById(int AreaId)
        {
            var response = new AreaModal();

            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = $@"SELECT a.AreaId, a.PinCode, a.AreaName, a.CityId, a.IsActive , c.CityName
                                         FROM area a
                                         INNER JOIN city c ON a.CityId = c.CityId
                                         WHERE a.AreaId = @AreaId";

                var common = new SqlCommand(query, _sqlConnection);
                common.Parameters.AddWithValue("@AreaId", AreaId);

                var reader = common.ExecuteReader();
                if (reader.Read())
                {
                    response.AreaId = Convert.ToInt32(reader["AreaId"]);
                    response.PinCode = reader["PinCode"].ToString();
                    response.AreaName = reader["AreaName"].ToString();
                    response.CityId = Convert.ToInt32(reader["CityId"]);
                    response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                    response.CityName = reader["CityName"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return response;
        }



        // BRANCH

        public BranchModal AddUpdatedBranch(BranchModal branchModal)
        {
            var response = new BranchModal();

            try
            {

                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query;
                SqlCommand command;

                if (branchModal.BranchId > 0)
                {
                    // UPDATE + SELECT updated row
                    query = @"
                UPDATE branch 
                SET BranchCode = @BranchCode, BranchName = @BranchName 
                WHERE BranchId = @BranchId;

                SELECT BranchId, BranchCode, BranchName, IsActive 
                FROM branch 
                WHERE BranchId = @BranchId";

                    command = new SqlCommand(query, _sqlConnection);
                    command.Parameters.AddWithValue("@BranchId", branchModal.BranchId);
                }
                else
                {
                    // INSERT and return inserted ID
                    query = @"
                INSERT INTO branch (BranchCode , BranchName) 
                OUTPUT INSERTED.BranchId 
                VALUES (@BranchCode, @BranchName)";

                    command = new SqlCommand(query, _sqlConnection);
                }

                command.Parameters.AddWithValue("@BranchCode", branchModal.BranchCode);
                command.Parameters.AddWithValue("@BranchName", branchModal.BranchName);

                if (branchModal.BranchId > 0)
                {
                    using var reader = command.ExecuteReader();

                    // Move to the SELECT result set
                    if (reader.Read())
                    {
                        response.BranchId = Convert.ToInt32(reader["BranchId"]);
                        response.BranchCode = reader["BranchCode"].ToString();
                        response.BranchName = reader["BranchName"].ToString();
                        response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                    }
                    else
                    {
                        throw new Exception("Update succeeded but no data returned.");
                    }
                }

                else
                {
                    // Return inserted row info
                    int insertedId = (int)command.ExecuteScalar();
                    response.BranchId = insertedId;
                    response.BranchCode = branchModal.BranchCode;
                    response.BranchName = branchModal.BranchName;
                    response.IsActive = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving branch: " + ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }

            return response;
        }

        public List<BranchModal> GetBranches(FilterModel filterModel)
        {
            try
            {
                return _addFilter.GetFilteredList<BranchModal>(
                    tableName: "branch",
                    nameColumn: "BranchName",
                    idColumn: "BranchId",
                    codeColumn: "BranchCode",
                    filter: filterModel,
                    mapFunc: reader => new BranchModal
                    {
                        BranchId = Convert.ToInt32(reader["BranchId"]),
                        BranchCode = reader["BranchCode"].ToString(),
                        BranchName = reader["BranchName"].ToString(),
                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                    },
                    selectColumns: "BranchId, BranchCode, BranchName, IsActive"
                );
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching brances.", ex);
            }
        }

        public BranchModal DeleteBranchById(int BranchId)
        {
            var response = new BranchModal();
            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                    _sqlConnection.Open();

                string query = @"UPDATE branch SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END WHERE  BranchId= @BranchId;

                                SELECT BranchId, BranchName, BranchCode, IsActive FROM branch WHERE BranchId = @BranchId;";

                var commond = new SqlCommand(query, _sqlConnection);
                commond.Parameters.AddWithValue("@BranchId", BranchId);

                var reader = commond.ExecuteReader();
                if (reader.Read())
                {
                    response.BranchId = Convert.ToInt32(reader["BranchId"]);
                    response.BranchCode = reader["BranchCode"].ToString();
                    response.BranchName = reader["BranchName"].ToString();
                    response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }

        public List<BranchModal> GetBranchIsActive()
        {
            var response = new List<BranchModal>();

            try
            {

                using (SqlConnection sqlConnection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
                {

                    sqlConnection.Open();


                    string query = "SELECT BranchId, BranchName, BranchCode, IsActive FROM branch WHERE IsActive = 1";

                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {


                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                response.Add(new BranchModal
                                {
                                    BranchId = Convert.ToInt32(reader["BranchId"]),
                                    BranchCode = reader["BranchCode"].ToString(),
                                    BranchName = reader["BranchName"].ToString(),
                                    IsActive = Convert.ToBoolean(reader["IsActive"]),
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw new Exception("Error fetching all branches", ex);
            }

            return response;
        }

        public BranchModal GetBranchById(int BranchId)
        {
            var response = new BranchModal();

            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = "SELECT * FROM branch WHERE BranchId = @BranchId";

                var commond = new SqlCommand(query, _sqlConnection);
                commond.Parameters.AddWithValue("@BranchId", BranchId);

                var reader = commond.ExecuteReader();
                if (reader.Read())
                {
                    response.BranchId = Convert.ToInt32(reader["BranchId"]);
                    response.BranchCode = reader["BranchCode"].ToString();
                    response.BranchName = reader["BranchName"].ToString();
                    response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }



        // B2B

        public B2BModal AddUpdatedB2B(B2BModal b2BModal)
        {
            var response = new B2BModal();

            try
            {

                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query;
                SqlCommand command;

                if (b2BModal.B2BId > 0)
                {
                    // UPDATE + SELECT updated row
                    query = @"
                UPDATE b2b 
                SET B2BCode = @B2BCode, B2BName = @B2BName 
                WHERE B2BId = @B2BId;

                SELECT B2BId, B2BCode, B2BName, IsActive 
                FROM b2b 
                WHERE B2BId = @B2BId";

                    command = new SqlCommand(query, _sqlConnection);
                    command.Parameters.AddWithValue("@B2BId", b2BModal.B2BId);
                }
                else
                {
                    // INSERT and return inserted ID
                    query = @"
                INSERT INTO b2b (B2BCode , B2BName) 
                OUTPUT INSERTED.B2BId 
                VALUES (@B2BCode, @B2BName)";

                    command = new SqlCommand(query, _sqlConnection);
                }

                command.Parameters.AddWithValue("@B2BCode", b2BModal.B2BCode);
                command.Parameters.AddWithValue("@B2BName", b2BModal.B2BName);

                if (b2BModal.B2BId > 0)
                {
                    using var reader = command.ExecuteReader();

                    // Move to the SELECT result set
                    if (reader.Read())
                    {
                        response.B2BId = Convert.ToInt32(reader["B2BId"]);
                        response.B2BCode = reader["B2BCode"].ToString();
                        response.B2BName = reader["B2BName"].ToString();
                        response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                    }
                    else
                    {
                        throw new Exception("Update succeeded but no data returned.");
                    }
                }

                else
                {
                    // Return inserted row info
                    int insertedId = (int)command.ExecuteScalar();
                    response.B2BId = insertedId;
                    response.B2BCode = b2BModal.B2BCode;
                    response.B2BName = b2BModal.B2BName;
                    response.IsActive = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving b2b: " + ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }

            return response;
        }

        public List<B2BModal> GetB2Bs(FilterModel filterModel)
        {
            try
            {
                return _addFilter.GetFilteredList<B2BModal>(
                    tableName: "b2b",
                    nameColumn: "B2BName",
                    idColumn: "B2BId",
                    codeColumn: "B2BCode",
                    filter: filterModel,
                    mapFunc: reader => new B2BModal
                    {
                        B2BId = Convert.ToInt32(reader["B2BId"]),
                        B2BCode = reader["B2BCode"].ToString(),
                        B2BName = reader["B2BName"].ToString(),
                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                    },
                    selectColumns: "B2BId, B2BCode, B2BName, IsActive"
                );
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching brances.", ex);
            }
        }

        public B2BModal DeleteB2BById(int B2BId)
        {
            var response = new B2BModal();
            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                    _sqlConnection.Open();

                string query = @"UPDATE b2b SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END WHERE  B2BId= @B2BId;

                                SELECT B2BId, B2BName, B2BCode, IsActive FROM b2b WHERE B2BId = @B2BId;";

                var commond = new SqlCommand(query, _sqlConnection);
                commond.Parameters.AddWithValue("@B2BId", B2BId);

                var reader = commond.ExecuteReader();
                if (reader.Read())
                {
                    response.B2BId = Convert.ToInt32(reader["B2BId"]);
                    response.B2BCode = reader["B2BCode"].ToString();
                    response.B2BName = reader["B2BName"].ToString();
                    response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }

        public List<B2BModal> GetB2BIsActive()
        {
            var response = new List<B2BModal>();

            try
            {

                using (SqlConnection sqlConnection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
                {

                    sqlConnection.Open();


                    string query = "SELECT B2BId, B2BName, B2BCode, IsActive FROM b2b WHERE IsActive = 1";

                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                response.Add(new B2BModal
                                {
                                    B2BId = Convert.ToInt32(reader["B2BId"]),
                                    B2BCode = reader["B2BCode"].ToString(),
                                    B2BName = reader["B2BName"].ToString(),
                                    IsActive = Convert.ToBoolean(reader["IsActive"]),
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw new Exception("Error fetching all b2bs", ex);
            }

            return response;
        }

        public B2BModal GetB2BById(int B2BId)
        {
            var response = new B2BModal();

            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = "SELECT * FROM b2b WHERE B2BId = @B2BId";

                var commond = new SqlCommand(query, _sqlConnection);
                commond.Parameters.AddWithValue("@B2BId", B2BId);

                var reader = commond.ExecuteReader();
                if (reader.Read())
                {
                    response.B2BId = Convert.ToInt32(reader["B2BId"]);
                    response.B2BCode = reader["B2BCode"].ToString();
                    response.B2BName = reader["B2BName"].ToString();
                    response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }



        // DOCTOR 
        public DoctorModal AddUpdatedDoctor(DoctorModal doctorModal)
        {
            var response = new DoctorModal();

            try
            {

                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query;
                SqlCommand command;

                if (doctorModal.DoctorId > 0)
                {
                    // UPDATE + SELECT updated row
                    query = @"
                UPDATE doctor 
                SET DoctorCode = @DoctorCode, DoctorName = @DoctorName, Email=@Email, PhoneNumber=@PhoneNumber 
                WHERE DoctorId = @DoctorId;

                SELECT DoctorId, DoctorCode, DoctorName, Email, PhoneNumber, IsActive 
                FROM doctor 
                WHERE DoctorId = @DoctorId";

                    command = new SqlCommand(query, _sqlConnection);
                    command.Parameters.AddWithValue("@DoctorId", doctorModal.DoctorId);
                }
                else
                {
                    // INSERT and return inserted ID
                    query = @"
                INSERT INTO doctor (DoctorCode , DoctorName, Email, PhoneNumber) 
                OUTPUT INSERTED.DoctorId 
                VALUES (@DoctorCode, @DoctorName, @Email, @PhoneNumber)";

                    command = new SqlCommand(query, _sqlConnection);
                }

                command.Parameters.AddWithValue("@DoctorCode", doctorModal.DoctorCode);
                command.Parameters.AddWithValue("@DoctorName", doctorModal.DoctorName);
                command.Parameters.AddWithValue("@Email", doctorModal.Email);
                command.Parameters.AddWithValue("@PhoneNumber", doctorModal.PhoneNumber);

                if (doctorModal.DoctorId > 0)
                {
                    using var reader = command.ExecuteReader();

                    // Move to the SELECT result set
                    if (reader.Read())
                    {
                        response.DoctorId = Convert.ToInt32(reader["DoctorId"]);
                        response.DoctorCode = reader["DoctorCode"].ToString();
                        response.DoctorName = reader["DoctorName"].ToString();
                        response.Email = reader["Email"].ToString();
                        response.PhoneNumber = reader["PhoneNumber"].ToString();
                        response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                    }
                    else
                    {
                        throw new Exception("Update succeeded but no data returned.");
                    }
                }

                else
                {
                    // Return inserted row info
                    int insertedId = (int)command.ExecuteScalar();
                    response.DoctorId = insertedId;
                    response.DoctorCode = doctorModal.DoctorCode;
                    response.DoctorName = doctorModal.DoctorName;
                    response.Email = doctorModal.Email;
                    response.PhoneNumber = doctorModal.PhoneNumber;
                    response.IsActive = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving b2b: " + ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }

            return response;
        }

        public List<DoctorModal> GetDoctorByFilter(FilterModel filterModel)
        {
            try
            {
                return _addFilter.GetFilteredList<DoctorModal>(
                    tableName: "doctor",
                    nameColumn: "DoctorName",
                    idColumn: "DoctorId",
                    codeColumn: "DoctorCode",
                    filter: filterModel,
                    mapFunc: reader => new DoctorModal
                    {
                        DoctorId = Convert.ToInt32(reader["DoctorId"]),
                        DoctorCode = reader["DoctorCode"].ToString(),
                        DoctorName = reader["DoctorName"].ToString(),
                        Email = reader["Email"].ToString(),
                        PhoneNumber = reader["PhoneNumber"].ToString(),
                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                    },
                    selectColumns: "DoctorId, DoctorCode, DoctorName, Email, PhoneNumber, IsActive"
                );
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching doctor.", ex);
            }
        }

        public DoctorModal GetDoctorById(int DoctorId)
        {
            var response = new DoctorModal();

            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = "SELECT * FROM doctor WHERE DoctorId = @DoctorId";

                var commond = new SqlCommand(query, _sqlConnection);
                commond.Parameters.AddWithValue("@DoctorId", DoctorId);

                var reader = commond.ExecuteReader();
                if (reader.Read())
                {
                    response.DoctorId = Convert.ToInt32(reader["DoctorId"]);
                    response.DoctorCode = reader["DoctorCode"].ToString();
                    response.DoctorName = reader["DoctorName"].ToString();
                    response.Email = reader["Email"].ToString();
                    response.PhoneNumber = reader["PhoneNumber"].ToString();
                    response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }

        public DoctorModal DeleteDoctorById(int DoctorId)
        {
            var response = new DoctorModal();
            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                    _sqlConnection.Open();

                string query = @"UPDATE doctor SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END WHERE  DoctorId= @DoctorId;

                                SELECT DoctorId, DoctorName, DoctorCode, Email, PhoneNumber, IsActive FROM doctor WHERE DoctorId = @DoctorId;";

                var commond = new SqlCommand(query, _sqlConnection);
                commond.Parameters.AddWithValue("@DoctorId", DoctorId);

                var reader = commond.ExecuteReader();
                if (reader.Read())
                {
                    response.DoctorId = Convert.ToInt32(reader["DoctorId"]);
                    response.DoctorCode = reader["DoctorCode"].ToString();
                    response.DoctorName = reader["DoctorName"].ToString();
                    response.Email = reader["Email"].ToString();
                    response.PhoneNumber = reader["PhoneNumber"].ToString();
                    response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }

        public List<DoctorModal> GetDoctorIsActive()
        {
            var response = new List<DoctorModal>();

            try
            {

                using (SqlConnection sqlConnection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
                {

                    sqlConnection.Open();


                    string query = "SELECT DoctorId, DoctorName, DoctorCode, Email, PhoneNumber, IsActive FROM doctor WHERE IsActive = 1";

                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {


                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                response.Add(new DoctorModal
                                {
                                    DoctorId = Convert.ToInt32(reader["DoctorId"]),
                                    DoctorCode = reader["DoctorCode"].ToString(),
                                    DoctorName = reader["DoctorName"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    PhoneNumber = reader["PhoneNumber"].ToString(),
                                    IsActive = Convert.ToBoolean(reader["IsActive"]),
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw new Exception("Error fetching all branches", ex);
            }

            return response;
        }



        // TEST

        public TestModal AddUpdatedTest(TestModal testModal)
        {
            var response = new TestModal();

            try
            {

                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query;
                SqlCommand command;

                if (testModal.TestId > 0)
                {
                    // UPDATE + SELECT updated row
                    query = @"
                UPDATE test 
                SET TestCode = @TestCode, TestName = @TestName 
                WHERE TestId = @TestId;

                SELECT TestId, TestCode, TestName, IsActive 
                FROM test 
                WHERE TestId = @TestId";

                    command = new SqlCommand(query, _sqlConnection);
                    command.Parameters.AddWithValue("@TestId", testModal.TestId);
                }
                else
                {
                    // INSERT and return inserted ID
                    query = @"
                INSERT INTO test (TestCode , TestName) 
                OUTPUT INSERTED.TestId 
                VALUES (@TestCode, @TestName)";

                    command = new SqlCommand(query, _sqlConnection);
                }

                command.Parameters.AddWithValue("@TestCode", testModal.TestCode);
                command.Parameters.AddWithValue("@TestName", testModal.TestName);

                if (testModal.TestId > 0)
                {
                    using var reader = command.ExecuteReader();

                    // Move to the SELECT result set
                    if (reader.Read())
                    {
                        response.TestId = Convert.ToInt32(reader["TestId"]);
                        response.TestCode = reader["TestCode"].ToString();
                        response.TestName = reader["TestName"].ToString();
                        response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                    }
                    else
                    {
                        throw new Exception("Update succeeded but no data returned.");
                    }
                }

                else
                {
                    // Return inserted row info
                    int insertedId = (int)command.ExecuteScalar();
                    response.TestId = insertedId;
                    response.TestCode = testModal.TestCode;
                    response.TestName = testModal.TestName;
                    response.IsActive = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving test: " + ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }

            return response;
        }

        public List<TestModal> GetTestByFilter(FilterModel filterModel)
        {
            try
            {
                return _addFilter.GetFilteredList<TestModal>(
                    tableName: "test",
                    nameColumn: "TestName",
                    idColumn: "TestId",
                    codeColumn: "TestCode",
                    filter: filterModel,
                    mapFunc: reader => new TestModal
                    {
                        TestId = Convert.ToInt32(reader["TestId"]),
                        TestName = reader["TestName"].ToString(),
                        TestCode = reader["TestCode"].ToString(),
                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                    },
                    selectColumns: "TestId, TestName, TestCode, IsActive"
                );
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching test.", ex);
            }
        }

        public TestModal GetTestById(int TestId)
        {
            var response = new TestModal();

            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = "SELECT * FROM test WHERE TestId = @TestId";

                var commond = new SqlCommand(query, _sqlConnection);
                commond.Parameters.AddWithValue("@TestId", TestId);

                var reader = commond.ExecuteReader();
                if (reader.Read())
                {
                    response.TestId = Convert.ToInt32(reader["TestId"]);
                    response.TestCode = reader["TestCode"].ToString();
                    response.TestName = reader["TestName"].ToString();
                    response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }

        public TestModal DeleteTestById(int TestId)
        {
            var response = new TestModal();
            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                    _sqlConnection.Open();

                string query = @"UPDATE test SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END WHERE  TestId= @TestId;

                                SELECT TestId, TestName, TestCode, IsActive FROM test WHERE TestId = @TestId;";

                var commond = new SqlCommand(query, _sqlConnection);
                commond.Parameters.AddWithValue("@TestId", TestId);

                var reader = commond.ExecuteReader();
                if (reader.Read())
                {
                    response.TestId = Convert.ToInt32(reader["TestId"]);
                    response.TestCode = reader["TestCode"].ToString();
                    response.TestName = reader["TestName"].ToString();
                    response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }

        public List<TestModal> GetTestIsActive()
        {
            var response = new List<TestModal>();

            try
            {

                using (SqlConnection sqlConnection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
                {

                    sqlConnection.Open();


                    string query = "SELECT TestId, TestName, TestCode, IsActive FROM test WHERE IsActive = 1";

                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {


                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                response.Add(new TestModal
                                {
                                    TestId = Convert.ToInt32(reader["TestId"]),
                                    TestName = reader["TestName"].ToString(),
                                    TestCode = reader["TestCode"].ToString(),
                                    IsActive = Convert.ToBoolean(reader["IsActive"]),
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw new Exception("Error fetching all test", ex);
            }

            return response;
        }



        // SERVICE
        public ServiceModal AddUpdatedServiceModal(ServiceModal serviceModal, List<TestModal> testModals)
        {
            var response = new ServiceModal();

            try
            {

                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query;
                SqlCommand command;

                if (serviceModal.ServiceId > 0)
                {
                    // UPDATE + SELECT updated row
                    query = @"
                      UPDATE service 
                      SET ServiceCode = @ServiceCode, ServiceName = @ServiceName, B2BAmount=@B2BAmount, B2CAmount=@B2CAmount 
                      WHERE ServiceId = @ServiceId;

                     SELECT ServiceId, ServiceCode, ServiceName, B2BAmount,B2CAmount,IsActive 
                     FROM service 
                     WHERE ServiceId = @ServiceId";

                    command = new SqlCommand(query, _sqlConnection);
                    command.Parameters.AddWithValue("@ServiceId", serviceModal.ServiceId);
                }
                else
                {
                    // INSERT and return inserted ID
                    query = @"
                     INSERT INTO service (ServiceCode , ServiceName, B2BAmount, B2CAmount) 
                     OUTPUT INSERTED.ServiceId 
                     VALUES (@ServiceCode, @ServiceName, @B2BAmount, @B2CAmount)";

                    command = new SqlCommand(query, _sqlConnection);
                }

                command.Parameters.AddWithValue("@ServiceCode", serviceModal.ServiceCode);
                command.Parameters.AddWithValue("@ServiceName", serviceModal.ServiceName);
                command.Parameters.AddWithValue("@B2BAmount", serviceModal.B2BAmount);
                command.Parameters.AddWithValue("@B2CAmount", serviceModal.B2CAmount);

                if (serviceModal.ServiceId > 0)
                {
                    using var reader = command.ExecuteReader();

                    // Move to the SELECT result set
                    if (reader.Read())
                    {
                        response.ServiceId = Convert.ToInt32(reader["ServiceId"]);
                        response.ServiceCode = reader["ServiceCode"].ToString();
                        response.ServiceName = reader["ServiceName"].ToString();
                        response.B2BAmount = Convert.ToInt32(reader["B2BAmount"]);
                        response.B2CAmount = Convert.ToInt32(reader["B2CAmount"]);
                        response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                    }
                    else
                    {
                        throw new Exception("Update succeeded but no data returned.");
                    }
                }

                else
                {
                    // Return inserted row info
                    int insertedId = (int)command.ExecuteScalar();
                    response.ServiceId = insertedId;
                    response.ServiceCode = serviceModal.ServiceCode;
                    response.ServiceName = serviceModal.ServiceName;
                    response.B2BAmount = serviceModal.B2BAmount;
                    response.B2CAmount = serviceModal.B2CAmount;
                    response.IsActive = true;

                    serviceModal.ServiceId = insertedId;
                }

                if (serviceModal.ServiceId > 0 && testModals != null && testModals.Count > 0)
                {
                    foreach (var testModal in testModals)
                    {
                        if (testModal.TestId > 0)
                        {
                            var checkQuery = @"SELECT ServiceTestId FROM serviceTestMap 
                               WHERE ServiceId = @ServiceId AND TestId = @TestId";

                            using (var checkCommand = new SqlCommand(checkQuery, _sqlConnection))
                            {
                                checkCommand.Parameters.AddWithValue("@ServiceId", serviceModal.ServiceId);
                                checkCommand.Parameters.AddWithValue("@TestId", testModal.TestId);

                                object existingId = checkCommand.ExecuteScalar();

                                if (existingId == null)
                                {
                                    var insertTest = @"INSERT INTO serviceTestMap (ServiceId, TestId) 
                                       VALUES (@ServiceId, @TestId)";
                                    using (var insertCommand = new SqlCommand(insertTest, _sqlConnection))
                                    {
                                        insertCommand.Parameters.AddWithValue("@ServiceId", serviceModal.ServiceId);
                                        insertCommand.Parameters.AddWithValue("@TestId", testModal.TestId);
                                        insertCommand.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    var updateTest = @"UPDATE serviceTestMap 
                                       SET IsActive = 1 
                                       WHERE ServiceTestId = @ServiceTestId";
                                    using (var updateCommand = new SqlCommand(updateTest, _sqlConnection))
                                    {
                                        updateCommand.Parameters.AddWithValue("@ServiceTestId", (int)existingId);
                                        updateCommand.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving service: " + ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }

            return response;

        }

        public List<ServiceModal> GetServiceByFilter(FilterModel filterModel)
        {
            var services = new List<ServiceModal>();

            try
            {

                services = _addFilter.GetFilteredList<ServiceModal>(
                    tableName: "service",
                    nameColumn: "ServiceName",
                    idColumn: "ServiceId",
                    codeColumn: "ServiceCode",
                    filter: filterModel,
                    mapFunc: reader => new ServiceModal
                    {
                        ServiceId = Convert.ToInt32(reader["ServiceId"]),
                        ServiceCode = reader["ServiceCode"].ToString(),
                        ServiceName = reader["ServiceName"].ToString(),
                        B2BAmount = Convert.ToInt32(reader["B2BAmount"]),
                        B2CAmount = Convert.ToInt32(reader["B2CAmount"]),
                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                        Test = new List<TestModal>()
                    },
                    selectColumns: "ServiceId, ServiceCode, ServiceName, B2BAmount, B2CAmount, IsActive"
                );


                if (_sqlConnection.State != ConnectionState.Open)
                    _sqlConnection.Open();


                foreach (var service in services)
                {
                    string testQuery = @"
                SELECT t.TestId, t.TestCode, t.TestName
                FROM serviceTestMap stm
                INNER JOIN test t ON stm.TestId = t.TestId
                WHERE stm.ServiceId = @ServiceId AND stm.IsActive = 1";

                    using (var testCmd = new SqlCommand(testQuery, _sqlConnection))
                    {
                        testCmd.Parameters.AddWithValue("@ServiceId", service.ServiceId);

                        using (var reader = testCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                service.Test.Add(new TestModal
                                {
                                    TestId = Convert.ToInt32(reader["TestId"]),
                                    TestCode = reader["TestCode"].ToString(),
                                    TestName = reader["TestName"].ToString()
                                });
                            }
                        }
                    }
                }

                return services;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching services with tests.", ex);
            }
            finally
            {
                if (_sqlConnection.State == ConnectionState.Open)
                {
                    _sqlConnection.Close();
                }
            }
        }

        public ServiceModal GetServiceById(int serviceId)
        {
            var response = new ServiceModal();

            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }


                string query = "SELECT * FROM service WHERE ServiceId = @ServiceId";
                using (var command = new SqlCommand(query, _sqlConnection))
                {
                    command.Parameters.AddWithValue("@ServiceId", serviceId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            response.ServiceId = Convert.ToInt32(reader["ServiceId"]);
                            response.ServiceCode = reader["ServiceCode"].ToString();
                            response.ServiceName = reader["ServiceName"].ToString();
                            response.B2BAmount = Convert.ToInt32(reader["B2BAmount"]);
                            response.B2CAmount = Convert.ToInt32(reader["B2CAmount"]);
                            response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }


                response.Test = new List<TestModal>();

                string testQuery = @"
                     SELECT t.TestId, t.TestCode, t.TestName, stm.ServiceTestId, stm.IsActive, stm.ServiceId
                      FROM serviceTestMap stm
                       INNER JOIN test t ON stm.TestId = t.TestId
                          WHERE stm.ServiceId = @ServiceId AND stm.IsActive = 1";

                using (var testCmd = new SqlCommand(testQuery, _sqlConnection))
                {
                    testCmd.Parameters.AddWithValue("@ServiceId", serviceId);

                    using (var testReader = testCmd.ExecuteReader())
                    {
                        while (testReader.Read())
                        {
                            response.Test.Add(new TestModal
                            {
                                TestId = Convert.ToInt32(testReader["TestId"]),
                                TestCode = testReader["TestCode"].ToString(),
                                TestName = testReader["TestName"].ToString(),
                                IsActive = Convert.ToBoolean(testReader["IsActive"]),
                                ServiceTestId = Convert.ToInt32(testReader["ServiceTestId"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching service by ID: " + ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }

            return response;
        }

        public ServiceModal DeleteServiceById(int ServiceId)
        {
            var response = new ServiceModal();
            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                    _sqlConnection.Open();

                string query = @"UPDATE service SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END WHERE  ServiceId= @ServiceId;

                                SELECT ServiceId, ServiceName, ServiceCode, B2BAmount, B2CAmount, IsActive FROM service WHERE ServiceId = @ServiceId;";

                var commond = new SqlCommand(query, _sqlConnection);
                commond.Parameters.AddWithValue("@ServiceId", ServiceId);

                var reader = commond.ExecuteReader();
                if (reader.Read())
                {
                    response.ServiceId = Convert.ToInt32(reader["ServiceId"]);
                    response.ServiceName = reader["ServiceName"].ToString();
                    response.ServiceCode = reader["ServiceCode"].ToString();
                    response.B2BAmount = Convert.ToInt32(reader["B2BAmount"]);
                    response.B2CAmount = Convert.ToInt32(reader["B2CAmount"]);
                    response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }

        public List<ServiceModal> GetServiceIsActive()
        {
            var services = new List<ServiceModal>();

            try
            {

                using (SqlConnection sqlConnection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
                {

                    sqlConnection.Open();


                    string query = "SELECT ServiceId, ServiceName, ServiceCode,B2BAmount,B2CAmount, IsActive FROM service WHERE IsActive = 1";

                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {


                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                services.Add(new ServiceModal
                                {
                                    ServiceId = Convert.ToInt32(reader["ServiceId"]),
                                    ServiceName = reader["ServiceName"].ToString(),
                                    ServiceCode = reader["ServiceCode"].ToString(),
                                    B2BAmount = Convert.ToInt32(reader["B2BAmount"]),
                                    B2CAmount = Convert.ToInt32(reader["B2CAmount"]),
                                    IsActive = Convert.ToBoolean(reader["IsActive"]),
                                    Test = new List<TestModal>(),
                                });
                            }
                        }
                    }

                    if (_sqlConnection.State != ConnectionState.Open)
                    {
                        _sqlConnection.Open();
                    }

                    foreach (var service in services)
                    {
                        string testQuery = @"
                       SELECT t.TestId, t.TestCode, t.TestName
                        FROM serviceTestMap stm
                        INNER JOIN test t ON stm.TestId = t.TestId
                        WHERE stm.ServiceId = @ServiceId AND stm.IsActive = 1";

                        using (var testCmd = new SqlCommand(testQuery, _sqlConnection))
                        {
                            testCmd.Parameters.AddWithValue("@ServiceId", service.ServiceId);

                            using (var reader = testCmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    service.Test.Add(new TestModal
                                    {
                                        TestId = Convert.ToInt32(reader["TestId"]),
                                        TestCode = reader["TestCode"].ToString(),
                                        TestName = reader["TestName"].ToString()
                                    });
                                }
                            }
                        }
                    }
                    return services;
                }
            }
            catch (Exception ex)
            {

                throw new Exception("Error fetching all service", ex);
            }

            return services;
        }




        // SERVICETESTMAP
        public ServiceTestMap DeleteServiceMapTestById(int ServiceTestId)
        {
            var response = new ServiceTestMap();
            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                    _sqlConnection.Open();


                //select ServiceTestId,serviceTestMap.ServiceId, serviceTestMap.TestId, serviceTestMap.IsActive
                //      from serviceTestMap 
                //      inner join service on service.ServiceId = serviceTestMap.ServiceId
                //      inner join test on test.TestId = serviceTestMap.TestId
                //delete from serviceTestMap where ServiceTestId = @ServiceTestId;

                string query = @"select ServiceTestId,serviceTestMap.ServiceId, serviceTestMap.TestId, serviceTestMap.IsActive
                                 from serviceTestMap 
                                 inner join service on service.ServiceId = serviceTestMap.ServiceId
                                 inner join test on test.TestId = serviceTestMap.TestId
                                 UPDATE serviceTestMap 
                                 SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END 
                                 WHERE ServiceTestId = @ServiceTestId;";


                var commond = new SqlCommand(query, _sqlConnection);
                commond.Parameters.AddWithValue("@ServiceTestId", ServiceTestId);

                var reader = commond.ExecuteReader();
                if (reader.Read())
                {

                    response.ServiceTestId = Convert.ToInt32(reader["ServiceTestId"]);
                    response.ServiceId = Convert.ToInt32(reader["ServiceId"]);
                    response.TestId = Convert.ToInt32(reader["TestId"]);
                    response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }


        // PAYMENT
        public PaymentModal AddUpdatedPayment(PaymentModal paymentModal)
        {
            var response = new PaymentModal();

            try
            {

                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }
                SqlCommand command;

                if (paymentModal.PaymentId > 0)
                {
                    string query = @"UPDATE payment
                                    SET PaymentName = @PaymentName, IsCash = @IsCash, IsCheque = @IsCheque, IsOnline = @IsOnline 
                                    WHERE PaymentId = @PaymentId
                              
                                    SELECT PaymentId, PaymentName, IsCash, IsCheque, IsOnline, IsActive FROM payment WHERE PaymentId = @PaymentId";

                    command = new SqlCommand(query, _sqlConnection);


                    command.Parameters.AddWithValue("@PaymentId", paymentModal.PaymentId);
                }
                else
                {
                    string query = @"INSERT INTO payment(PaymentName, IsCash, IsCheque, IsOnline) OUTPUT INSERTED.PaymentId VALUES (@PaymentName, @IsCash, @IsCheque, @IsOnline)";

                    command = new SqlCommand(query, _sqlConnection);
                }

                command.Parameters.AddWithValue("@PaymentName", paymentModal.PaymentName);
                command.Parameters.AddWithValue("@IsCash", paymentModal.IsCash);
                command.Parameters.AddWithValue("@IsCheque", paymentModal.IsCheque);
                command.Parameters.AddWithValue("IsOnline", paymentModal.IsOnline);

                if (paymentModal.PaymentId > 0)
                {
                    using var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        response.PaymentId = Convert.ToInt32(reader["PaymentId"]);
                        response.PaymentName = reader["PaymentName"].ToString();
                        response.IsCash = Convert.ToBoolean(reader["IsCash"]);
                        response.IsCheque = Convert.ToBoolean(reader["IsCheque"]);
                        response.IsOnline = Convert.ToBoolean(reader["IsOnline"]);
                        response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                    }
                    else
                    {
                        throw new Exception("Update successed but not data retrived.");
                    }
                }
                else
                {
                    int insertedId = (int)command.ExecuteScalar();
                    response.PaymentId = insertedId;
                    response.PaymentName = paymentModal.PaymentName;
                    response.IsCash = paymentModal.IsCash;
                    response.IsCheque = paymentModal.IsCheque;
                    response.IsOnline = paymentModal.IsOnline;
                    response.IsActive = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching data in to the payment", ex);
            }
            finally
            {
                _sqlConnection.Close();
                _sqlConnection.Dispose();
            }
            return response;
        }

        public List<PaymentModal> GetPaymentByFilter(FilterModel filterModel)
        {
            try
            {
                return _addFilter.GetFilteredList<PaymentModal>(
                    tableName: "payment",
                    codeColumn: "PaymentName",
                    nameColumn: "PaymentName",
                    idColumn: "PaymentId",
                    filter: filterModel,
                    mapFunc: reader => new PaymentModal
                    {
                        PaymentId = Convert.ToInt32(reader["PaymentId"]),
                        PaymentName = reader["PaymentName"].ToString(),
                        IsCash = Convert.ToBoolean(reader["IsCash"]),
                        IsCheque = Convert.ToBoolean(reader["IsCash"]),
                        IsOnline = Convert.ToBoolean(reader["IsOnline"]),
                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                    },
                    selectColumns: "PaymentId, PaymentName, IsCash, IsCheque, IsOnline, IsActive"
                );
            }
            catch (Exception ex)
            {
                throw new Exception("An error are occure to fetching payment", ex);
            }
        }

        public PaymentModal GetPaymentById(int PaymentId)
        {
            var response = new PaymentModal();

            try
            {

                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = "SELECT * FROM payment WHERE PaymentId = @PaymentId";

                SqlCommand command = new SqlCommand(query, _sqlConnection);

                command.Parameters.AddWithValue("@PaymentId", PaymentId);

                var readre = command.ExecuteReader();

                if (readre.Read())
                {
                    response.PaymentId = Convert.ToInt32(readre["PaymentId"]);
                    response.PaymentName = readre["PaymentName"].ToString();
                    response.IsCash = Convert.ToBoolean(readre["IsCash"]);
                    response.IsCheque = Convert.ToBoolean(readre["IsCheque"]);
                    response.IsOnline = Convert.ToBoolean(readre["IsOnline"]);
                    response.IsActive = Convert.ToBoolean(readre["IsActive"]);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("error accour to fetch payment by id", ex);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }

        public PaymentModal DeletePaymentById(int PaymentId)
        {
            var response = new PaymentModal();
            try
            {

                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = @"UPDATE payment SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END WHERE PaymentId = @PaymentId

                                 SELECT PaymentId, PaymentName, IsActive FROM payment WHERE PaymentId = @PaymentId";

                SqlCommand command = new SqlCommand(query, _sqlConnection);

                command.Parameters.AddWithValue("@PaymentId", PaymentId);

                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    response.PaymentId = Convert.ToInt32(reader["PaymentId"]);
                    response.PaymentName = reader["PaymentName"].ToString();
                    response.IsCash = Convert.ToBoolean(reader["IsCash"]);
                    response.IsCheque = Convert.ToBoolean(reader["IsCheque"]);
                    response.IsOnline = Convert.ToBoolean(reader["IsOnline"]);
                    response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error accure to fetch delete payment by id", ex);
            }
            return response;
        }

        public List<PaymentModal> GetPaymentIsActive()
        {
            var response = new List<PaymentModal>();

            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }


                string query = "SELECT PaymentId, PaymentName,IsCash, IsCheque, IsOnline, IsActive FROM payment WHERE IsActive = 1";

                SqlCommand command = new SqlCommand(query, _sqlConnection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        response.Add(new PaymentModal()
                        {
                            PaymentId = Convert.ToInt32(reader["PaymentId"]),
                            PaymentName = reader["PaymentName"].ToString(),
                            IsCash = Convert.ToBoolean(reader["IsCash"]),
                            IsCheque = Convert.ToBoolean(reader["IsCash"]),
                            IsOnline = Convert.ToBoolean(reader["IsOnline"]),
                            IsActive = Convert.ToBoolean(reader["IsActive"]),
                        });
                    }
                }


            }
            catch (Exception ex)
            {
                throw new Exception("Fetching an error in payment isActive", ex);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }





        // SAMPLEREGISTER

        //public SampleRegister AddUpdateSampleRegister(SampleRegister sampleRegister, List<ServiceModal> serviceModals)
        public SampleRegister AddUpdateSampleRegister(SampleRegister sampleRegister)
        {
            var response = new SampleRegister();

            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                SqlCommand command;

                if (sampleRegister.SampleRegisterId > 0)
                {
                    string query = @"
                                     UPDATE sampleregister
                                     SET Date = @Date, BranchId = @BranchId, TotalAmount = @TotalAmount,IsB2B = @IsB2B, B2BId = @B2BId, PhoneNumber = @PhoneNumber, Title = @Title, FirstName = @FirstName, MiddleName = @MiddleName, LastName = @LastName, DOB = @DOB, Age = @Age,
                                     Gender = @Gender, Email = @Email, CityId = @CityId, AreaId = @AreaId, Address = @Address, Amount = @Amount, ChequeNo = @ChequeNo, ChequeDate = @ChequeDate, TransactionId = @TransactionId, DoctorId = @DoctorId, CreatedBy = @CreatedBy, PaymentId = @PaymentId
                                     WHERE SampleRegisterId = @SampleRegisterId;

                                      SELECT s.SampleRegisterId, s.Date, b.BranchId, b.BranchName, s.TotalAmount, s.IsB2B, k.B2BId, k.B2BName, s.PhoneNumber, s.Title, s.FirstName, s.MiddleName, s.LastName, s.DOB, s.Age, 
                                      s.Gender, s.Email, c.CityId, c.CityName, a.AreaId, a.AreaName, s.Address, s.Amount,s.ChequeNo, s.ChequeDate, s.transactionId, d.DoctorId, d.DoctorName, s.CreatedBy, p.PaymentId, p.PaymentName, s.IsActive
                                      FROM sampleregister s
                                      INNER JOIN branch b ON s.BranchId = b.BranchId
                                      LEFT JOIN b2b k ON s.B2BId = k.B2BId
                                      LEFT JOIN city c ON s.CityId = c.CityId
                                      LEFT JOIN area a ON s.AreaId = a.AreaId
                                      LEFT JOIN doctor d ON s.DoctorId = d.DoctorId
                                      INNER JOIN payment p ON s.PaymentId = p.PaymentId
                                      WHERE s.SampleRegisterId = @SampleRegisterId;";



                    using var common = new SqlCommand(query, _sqlConnection);
                    common.Parameters.AddWithValue("@SampleRegisterId", sampleRegister.SampleRegisterId);
                    common.Parameters.AddWithValue("@Date", sampleRegister.Date);
                    common.Parameters.AddWithValue("@BranchId", sampleRegister.BranchId);
                    common.Parameters.AddWithValue("@TotalAmount", sampleRegister.TotalAmount);
                    common.Parameters.AddWithValue("@IsB2B", sampleRegister.IsB2B);
                    common.Parameters.AddWithValue("@B2BId", sampleRegister.B2BId ?? (object)DBNull.Value);
                    common.Parameters.AddWithValue("@PhoneNumber", sampleRegister.PhoneNumber);
                    common.Parameters.AddWithValue("@Title", sampleRegister.Title);
                    common.Parameters.AddWithValue("@FirstName", sampleRegister.FirstName);
                    common.Parameters.AddWithValue("@MiddleName", sampleRegister.MiddleName);
                    common.Parameters.AddWithValue("@LastName", sampleRegister.LastName);
                    common.Parameters.AddWithValue("@DOB", sampleRegister.DOB);
                    common.Parameters.AddWithValue("@Age", sampleRegister.Age);
                    common.Parameters.AddWithValue("@Gender", sampleRegister.Gender);
                    common.Parameters.AddWithValue("@Email", sampleRegister.Email);
                    common.Parameters.AddWithValue("@CityId", sampleRegister.CityId ?? (object)DBNull.Value);
                    common.Parameters.AddWithValue("@AreaId", sampleRegister.AreaId ?? (object)DBNull.Value);
                    common.Parameters.AddWithValue("@Address", sampleRegister.Address ?? (object)DBNull.Value);
                    common.Parameters.AddWithValue("@Amount", sampleRegister.Amount);
                    common.Parameters.AddWithValue("@ChequeNo", sampleRegister.ChequeNo ?? (object)DBNull.Value);
                    common.Parameters.AddWithValue("@ChequeDate", sampleRegister.ChequeDate ?? (object)DBNull.Value);
                    common.Parameters.AddWithValue("@TransactionId", sampleRegister.TransactionId ?? (object)DBNull.Value);
                    common.Parameters.AddWithValue("@DoctorId", sampleRegister.DoctorId ?? (object)DBNull.Value);
                    common.Parameters.AddWithValue("@CreatedBy", sampleRegister.CreatedBy ?? (object)DBNull.Value);
                    common.Parameters.AddWithValue("@PaymentId", sampleRegister.PaymentId);
                    //common.Parameters.AddWithValue("@CreatedDate", sampleRegister.CreatedDate);



                    using var reader = common.ExecuteReader();
                    if (reader.Read())
                    {
                        response.SampleRegisterId = Convert.ToInt32(reader["SampleRegisterId"]);
                        response.Date = Convert.ToDateTime(reader["Date"]);
                        response.BranchId = Convert.ToInt32(reader["BranchId"]);
                        response.BranchName = reader["BranchName"].ToString();
                        response.TotalAmount = Convert.ToInt32(reader["TotalAmount"]);
                        response.IsB2B = Convert.ToBoolean(reader["IsB2B"]);
                        response.B2BId = reader["B2BId"] != DBNull.Value ? Convert.ToInt32(reader["B2BId"]) : null;
                        response.B2BName = reader["B2BName"].ToString();
                        response.PhoneNumber = reader["PhoneNumber"].ToString();
                        response.Title = reader["Title"].ToString();
                        response.FirstName = reader["FirstName"].ToString();
                        response.MiddleName = reader["MiddleName"].ToString();
                        response.LastName = reader["LastName"].ToString();
                        response.DOB = Convert.ToDateTime(reader["DOB"]);
                        response.Age = Convert.ToInt32(reader["Age"]);
                        response.Gender = reader["Gender"].ToString();
                        response.Email = reader["Email"].ToString();
                        response.CityId = reader["CityId"] != DBNull.Value ? Convert.ToInt32(reader["CityId"]) : null;
                        response.CityName = reader["CityName"].ToString();
                        response.AreaId = reader["AreaId"] != DBNull.Value ? Convert.ToInt32(reader["AreaId"]) : null;
                        response.AreaName = reader["AreaName"].ToString();
                        response.Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : null;
                        response.Amount = Convert.ToInt32(reader["Amount"]);
                        response.ChequeNo = reader["ChequeNo"] != DBNull.Value ? reader["ChequeNo"].ToString() : null;
                        response.ChequeDate = reader["ChequeDate"] != DBNull.Value ? Convert.ToDateTime(reader["ChequeDate"]) : null;
                        response.TransactionId = reader["TransactionId"] != DBNull.Value ? reader["TransactionId"].ToString() : null;
                        response.DoctorId = reader["DoctorId"] != DBNull.Value ? Convert.ToInt32(reader["DoctorId"]) : null;
                        response.DoctorName = reader["DoctorName"].ToString();
                        response.CreatedBy = reader["CreatedBy"] != DBNull.Value ? reader["CreatedBy"].ToString() : null;
                        response.PaymentId = Convert.ToInt32(reader["PaymentId"]);
                        response.PaymentName = reader["PaymentName"].ToString();
                        //response.CreatedDate = reader["ChequeDate"] != DBNull.Value ? Convert.ToDateTime(reader["ChequeDate"]) : null;
                        response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                        response.ServiceMapping = new List<ServiceMapping>();
                        //response.PaymentMapping = new List<PaymentMapping>();
                    }
                    else
                    {
                        throw new Exception("Update succcessed but not data returned.");
                    }
                }
                else
                {
                    string query = $@"INSERT INTO sampleregister(Date, BranchId, TotalAmount,IsB2B, B2BId, PhoneNumber, Title, FirstName, MiddleName, LastName, DOB, Age, Gender, Email, CityId, AreaId, Address, Amount, ChequeNo, ChequeDate, TransactionId, DoctorId, CreatedBy, PaymentId) OUTPUT INSERTED.SampleRegisterId 
                                                        VALUES (@Date, @BranchId, @TotalAmount, @IsB2B, @B2BId, @PhoneNumber, @Title, @FirstName, @MiddleName, @LastName, @DOB, @Age, @Gender, @Email, @CityId, @AreaId, @Address, @Amount, @ChequeNo, @ChequeDate, @TransactionId, @DoctorId, @CreatedBy, @PaymentId) ";

                    using var common = new SqlCommand(query, _sqlConnection);
                    common.Parameters.AddWithValue("@Date", sampleRegister.Date);
                    common.Parameters.AddWithValue("@BranchId", sampleRegister.BranchId);
                    common.Parameters.AddWithValue("@TotalAmount", sampleRegister.TotalAmount);
                    common.Parameters.AddWithValue("@IsB2B", sampleRegister.IsB2B);
                    common.Parameters.AddWithValue("@B2BId", sampleRegister.B2BId ?? (object)DBNull.Value);
                    common.Parameters.AddWithValue("@PhoneNumber", sampleRegister.PhoneNumber);
                    common.Parameters.AddWithValue("@Title", sampleRegister.Title);
                    common.Parameters.AddWithValue("@FirstName", sampleRegister.FirstName);
                    common.Parameters.AddWithValue("@MiddleName", sampleRegister.MiddleName);
                    common.Parameters.AddWithValue("@LastName", sampleRegister.LastName);
                    common.Parameters.AddWithValue("@DOB", sampleRegister.DOB);
                    common.Parameters.AddWithValue("@Age", sampleRegister.Age);
                    common.Parameters.AddWithValue("@Gender", sampleRegister.Gender);
                    common.Parameters.AddWithValue("@Email", sampleRegister.Email);
                    common.Parameters.AddWithValue("@CityId", sampleRegister.CityId ?? (object)DBNull.Value);
                    common.Parameters.AddWithValue("@AreaId", sampleRegister.AreaId ?? (object)DBNull.Value);
                    common.Parameters.AddWithValue("@Address", sampleRegister.Address ?? (object)DBNull.Value);
                    common.Parameters.AddWithValue("@Amount", sampleRegister.Amount);
                    common.Parameters.AddWithValue("@ChequeNo", sampleRegister.ChequeNo ?? (object)DBNull.Value);
                    common.Parameters.AddWithValue("@ChequeDate", sampleRegister.ChequeDate ?? (object)DBNull.Value);
                    common.Parameters.AddWithValue("@TransactionId", sampleRegister.TransactionId ?? (object)DBNull.Value);
                    common.Parameters.AddWithValue("@DoctorId", sampleRegister.DoctorId ?? (object)DBNull.Value);
                    common.Parameters.AddWithValue("@CreatedBy", sampleRegister.CreatedBy ?? (object)DBNull.Value);
                    common.Parameters.AddWithValue("@PaymentId", sampleRegister.PaymentId);
                    //common.Parameters.AddWithValue("@CreatdeDate", sampleRegister.CreatedDate);
                    common.Parameters.AddWithValue("@IsActive", sampleRegister.IsActive);

                    int insertedId = (int)common.ExecuteScalar();



                    // branch
                    string branchQuery = "SELECT BranchName FROM branch WHERE BranchId = @BranchId";
                    using var branchCommond = new SqlCommand(branchQuery, _sqlConnection);
                    branchCommond.Parameters.AddWithValue("@BranchId", sampleRegister.BranchId);
                    string branchName = (string?)branchCommond.ExecuteScalar() ?? "";


                    // b2b
                    string b2bQuery = "SELECT B2BName FROM b2b WHERE B2BId = @B2BId";
                    using var b2bCommond = new SqlCommand(b2bQuery, _sqlConnection);
                    b2bCommond.Parameters.AddWithValue("@B2BId", sampleRegister.B2BId == null ? DBNull.Value : (object)sampleRegister.B2BId);
                    string b2bName = (string?)b2bCommond.ExecuteScalar() ?? "";


                    // city
                    string cityQuery = "SELECT CityName FROM city WHERE CityId = @CityId";
                    using var cityCommond = new SqlCommand(cityQuery, _sqlConnection);
                    cityCommond.Parameters.AddWithValue("@CityId", sampleRegister.CityId == null ? DBNull.Value : (object)sampleRegister.CityId);
                    string cityName = (string?)cityCommond.ExecuteScalar() ?? "";


                    // area
                    string areaQuery = "SELECT AreaName FROM area WHERE AreaId = @AreaId";
                    using var areaCommond = new SqlCommand(areaQuery, _sqlConnection);
                    areaCommond.Parameters.AddWithValue("@AreaId", sampleRegister.AreaId == null ? DBNull.Value : (object)sampleRegister.AreaId);
                    string areaName = (string?)areaCommond.ExecuteScalar() ?? "";


                    // doctor
                    string doctorQuery = "SELECT DoctorName FROM doctor WHERE DoctorId = @DoctorId";
                    using var doctorCommond = new SqlCommand(doctorQuery, _sqlConnection);
                    doctorCommond.Parameters.AddWithValue("@DoctorId", sampleRegister.DoctorId == null ? DBNull.Value : (object)sampleRegister.DoctorId);
                    string doctorName = (string?)doctorCommond.ExecuteScalar() ?? "";


                    // payment
                    string paymentQuery = "SELECT PaymentName FROM payment WHERE PaymentId = @PaymentId";
                    using var paymentCommond = new SqlCommand(paymentQuery, _sqlConnection);
                    paymentCommond.Parameters.AddWithValue("@PaymentId", sampleRegister.PaymentId);
                    string paymentName = (string?)paymentCommond.ExecuteScalar() ?? "";


                    response.SampleRegisterId = insertedId;
                    response.Date = sampleRegister.Date;
                    response.BranchId = sampleRegister.BranchId;
                    response.BranchName = branchName;
                    response.TotalAmount = sampleRegister.TotalAmount;
                    response.IsB2B = sampleRegister.IsB2B;
                    response.B2BId = sampleRegister.B2BId;
                    response.B2BName = b2bName;
                    response.PhoneNumber = sampleRegister.PhoneNumber;
                    response.Title = sampleRegister.Title;
                    response.FirstName = sampleRegister.FirstName;
                    response.MiddleName = sampleRegister.MiddleName;
                    response.LastName = sampleRegister.LastName;
                    response.DOB = sampleRegister.DOB;
                    response.Age = sampleRegister.Age;
                    response.Gender = sampleRegister.Gender;
                    response.Email = sampleRegister.Email;
                    response.CityId = sampleRegister.CityId;
                    response.CityName = cityName;
                    response.AreaId = sampleRegister.AreaId;
                    response.AreaName = areaName;
                    response.Address = sampleRegister.Address;
                    response.Amount = sampleRegister.Amount;
                    response.ChequeNo = sampleRegister.ChequeNo;
                    response.ChequeDate = sampleRegister.ChequeDate;
                    response.TransactionId = sampleRegister.TransactionId;
                    response.DoctorId = sampleRegister.DoctorId;
                    response.DoctorName = doctorName;
                    response.CreatedBy = sampleRegister.CreatedBy;
                    response.PaymentId = sampleRegister.PaymentId;
                    response.IsActive = true;
                    response.ServiceMapping = new List<ServiceMapping>();
                    //response.PaymentMapping = new List<PaymentMapping>();


                    sampleRegister.SampleRegisterId = insertedId;
                }

                if (sampleRegister.SampleRegisterId > 0 && sampleRegister.ServiceMapping != null && sampleRegister.ServiceMapping.Count > 0)
                {
                    foreach (var serviceModal in sampleRegister.ServiceMapping)
                    {
                        if (serviceModal.ServiceId > 0)
                        {
                            var checkQuery = @"SELECT SampleServiceMapId FROM sampleServiceMap 
                               WHERE SampleRegisterId = @SampleRegisterId AND ServiceId = @ServiceId";

                            using (var checkCommand = new SqlCommand(checkQuery, _sqlConnection))
                            {
                                checkCommand.Parameters.AddWithValue("@SampleRegisterId", sampleRegister.SampleRegisterId);
                                checkCommand.Parameters.AddWithValue("@ServiceId", serviceModal.ServiceId);

                                object existingId = checkCommand.ExecuteScalar();

                                if (existingId == null)
                                {
                                    var insertTest = @"INSERT INTO sampleServiceMap (SampleRegisterId, ServiceId) 
                                       VALUES (@SampleRegisterId, @ServiceId)";
                                    using (var insertCommand = new SqlCommand(insertTest, _sqlConnection))
                                    {
                                        insertCommand.Parameters.AddWithValue("@SampleRegisterId", sampleRegister.SampleRegisterId);
                                        insertCommand.Parameters.AddWithValue("@ServiceId", serviceModal.ServiceId);
                                        insertCommand.ExecuteNonQuery();
                                    }
                                }
                                //else
                                //{
                                //    var updateTest = @"UPDATE sampleServiceMap 
                                //       SET IsActive = 1 
                                //       WHERE SampleServiceMapId = @SampleServiceMapId";
                                //    using (var updateCommand = new SqlCommand(updateTest, _sqlConnection))
                                //    {
                                //        updateCommand.Parameters.AddWithValue("@SampleServiceMapId", (int)existingId);
                                //        updateCommand.ExecuteNonQuery();
                                //    }
                                //}
                            }
                        }
                    }

                }

                //if (sampleRegister.SampleRegisterId > 0 && sampleRegister.PaymentMapping != null)
                //{
                //    foreach (var paymentModal in sampleRegister.PaymentMapping)
                //    {
                //        if (paymentModal.PaymentId > 0)
                //        {
                //            var checkQuery = @"SELECT SamplePaymentMapId FROM samplepaymentmap 
                //               WHERE SampleRegisterId = @SampleRegisterId AND PaymentId = @PaymentId";

                //            using (var checkCommand = new SqlCommand(checkQuery, _sqlConnection))
                //            {
                //                checkCommand.Parameters.AddWithValue("@SampleRegisterId", sampleRegister.SampleRegisterId);
                //                checkCommand.Parameters.AddWithValue("@PaymentId", paymentModal.PaymentId);

                //                object existingId = checkCommand.ExecuteScalar();

                //                if (existingId == null)
                //                {
                //                    var insertTest = @"INSERT INTO samplepaymentmap (SampleRegisterId, PaymentId) 
                //                       VALUES (@SampleRegisterId, @PaymentId)";
                //                    using (var insertCommand = new SqlCommand(insertTest, _sqlConnection))
                //                    {
                //                        insertCommand.Parameters.AddWithValue("@SampleRegisterId", sampleRegister.SampleRegisterId);
                //                        insertCommand.Parameters.AddWithValue("@PaymentId", paymentModal.PaymentId);
                //                        insertCommand.ExecuteNonQuery();

                //                    }
                //                }
                //                //else
                //                //{
                //                //    var updateTest = @"UPDATE samplepaymentmap 
                //                //       SET IsActive = 1 
                //                //       WHERE SamplePaymentMapId = @SamplePaymentMapId";
                //                //    using (var updateCommand = new SqlCommand(updateTest, _sqlConnection))
                //                //    {
                //                //        updateCommand.Parameters.AddWithValue("@SamplePaymentMapId", (int)existingId);
                //                //        updateCommand.ExecuteNonQuery();
                //                //    }
                //                //}                               
                //            }
                //        }
                //    }
                //}

            }
            catch (Exception ex)
            {
                throw new Exception("erroe fetching into sampleregister" + ex, ex);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }

        public List<SampleRegister> GetSampleByFilter(FilterModel filterModel)
        {
            try
            {
                string query = @"sampleregister 
                            INNER JOIN branch ON sampleregister.BranchId = branch.BranchId
                            LEFT JOIN  b2b ON sampleregister.B2BId = b2b.B2BId
                            LEFT JOIN city ON sampleregister.CityId = city.CityId
                            LEFT JOIN area ON sampleregister.AreaId = area.AreaId
                            LEFT JOIN doctor ON sampleregister.DoctorId = doctor.DoctorId
                            INNER JOIN payment ON sampleregister.PaymentId = payment.PaymentId";

                var sampleRegisters = _addFilter.GetFilteredList<SampleRegister>(
                    tableName: query,
                    nameColumn: "MiddleName",
                    idColumn: "SampleRegisterId",
                    codeColumn: "sampleregister.PhoneNumber",
                    filter: filterModel,

                    mapFunc: reader => new SampleRegister
                    {
                        SampleRegisterId = Convert.ToInt32(reader["SampleRegisterId"]),
                        Date = Convert.ToDateTime(reader["Date"]),
                        BranchId = Convert.ToInt32(reader["BranchId"]),
                        BranchName = reader["BranchName"].ToString(),
                        TotalAmount = Convert.ToInt32(reader["TotalAmount"]),
                        IsB2B = reader["IsB2B"] != DBNull.Value ? Convert.ToBoolean(reader["IsB2B"]) : false,
                        B2BId = reader["B2BId"] != DBNull.Value ? Convert.ToInt32(reader["B2BId"]) : null,
                        B2BName = reader["B2BName"].ToString(),
                        PhoneNumber = reader["PhoneNumber"].ToString(),
                        Title = reader["Title"].ToString(),
                        FirstName = reader["FirstName"].ToString(),
                        MiddleName = reader["MiddleName"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        DOB = Convert.ToDateTime(reader["DOB"]),
                        Age = Convert.ToInt32(reader["Age"]),
                        Gender = reader["Gender"].ToString(),
                        Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : null,
                        CityId = reader["CityId"] != DBNull.Value ? Convert.ToInt32(reader["CityId"]) : null,
                        CityName = reader["CityName"].ToString(),
                        AreaId = reader["AreaId"] != DBNull.Value ? Convert.ToInt32(reader["AreaId"]) : null,
                        AreaName = reader["AreaName"].ToString(),
                        Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : null,
                        Amount = Convert.ToInt32(reader["Amount"]),
                        ChequeNo = reader["ChequeNo"] != DBNull.Value ? reader["ChequeNo"].ToString() : null,
                        ChequeDate = reader["ChequeDate"] != DBNull.Value ? Convert.ToDateTime(reader["ChequeDate"]) : null,
                        TransactionId = reader["TransactionId"] != DBNull.Value ? reader["TransactionId"].ToString() : null,
                        DoctorId = reader["DoctorId"] != DBNull.Value ? Convert.ToInt32(reader["DoctorId"]) : null,
                        DoctorName = reader["DoctorName"].ToString(),
                        CreatedBy = reader["CreatedBy"] != DBNull.Value ? reader["CreatedBy"].ToString() : null,
                        PaymentId = Convert.ToInt32(reader["PaymentId"]),
                        PaymentName = reader["PaymentName"].ToString(),
                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                        ServiceMapping = new List<ServiceMapping>(),
                        //PaymentMapping = new List<PaymentMapping>(),
                    },
                    selectColumns: "sampleregister.SampleRegisterId, sampleregister.Date, branch.BranchId, branch.BranchName, sampleregister.TotalAmount, sampleregister.IsB2B, b2b.B2BId, b2b.B2BName, sampleregister.PhoneNumber, sampleregister.Title, sampleregister.FirstName, sampleregister.MiddleName, sampleregister.LastName, sampleregister.DOB, sampleregister.Age, sampleregister.Gender, sampleregister.Email, city.CityId, city.CityName, area.AreaId, area.AreaName, sampleregister.Address,sampleregister.Amount, sampleregister.ChequeNo, sampleregister.ChequeDate, sampleregister.TransactionId, doctor.DoctorId, doctor.DoctorName, sampleregister.CreatedBy, payment.PaymentId, payment.PaymentName, sampleregister.IsActive",
                isActiveColumn: "sampleregister.IsActive"
                );

                if (sampleRegisters?.Count > 0)
                {
                    if (_sqlConnection.State != ConnectionState.Open)
                    {
                        _sqlConnection.Open();
                    }

                    foreach (var service in sampleRegisters)
                    {
                        string testQuery = @"
                           SELECT s.ServiceId, s.ServiceCode, s.ServiceName, s.B2BAmount, s.B2CAmount, 
                           stm.SampleServiceMapId, stm.SampleRegisterId
                           FROM sampleServiceMap stm
                           INNER JOIN service s ON stm.ServiceId = s.ServiceId
                           WHERE stm.SampleRegisterId = @SampleRegisterId ";
                        //AND stm.IsActive = 1";, stm.IsActive

                        using (var testCmd = new SqlCommand(testQuery, _sqlConnection))
                        {
                            testCmd.Parameters.AddWithValue("@SampleRegisterId", service.SampleRegisterId);

                            using (var reader = testCmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    service.ServiceMapping.Add(new ServiceMapping
                                    {
                                        ServiceId = Convert.ToInt32(reader["ServiceId"]),
                                        ServiceCode = reader["ServiceCode"].ToString(),
                                        ServiceName = reader["ServiceName"].ToString(),
                                        B2BAmount = Convert.ToInt32(reader["B2BAmount"]),
                                        B2CAmount = Convert.ToInt32(reader["B2CAmount"]),
                                        //IsActive = Convert.ToBoolean(reader["IsActive"]),
                                        SampleServiceMapId = Convert.ToInt32(reader["SampleServiceMapId"]),
                                    });
                                }
                            }
                        }
                    }
                }

                //if (sampleRegisters?.Count > 0)
                //{
                //    if (_sqlConnection.State != ConnectionState.Open)
                //    {
                //        _sqlConnection.Open();
                //    }

                //    foreach (var payment in sampleRegisters)
                //    {
                //        string paymentQuery = @"
                //    SELECT p.PaymentId, p.PaymentName, p.IsCash, p.IsCheque, p.IsOnline,
                //           stm.SamplePaymentMapId, stm.SampleRegisterId
                //    FROM samplepaymentmap stm
                //    INNER JOIN payment p ON stm.PaymentId = p.PaymentId
                //    WHERE stm.SampleRegisterId = @SampleRegisterId";
                //        //AND stm.IsActive = 1";, stm.IsActive

                //        using (var testCmd = new SqlCommand(paymentQuery, _sqlConnection))
                //        {
                //            testCmd.Parameters.AddWithValue("@SampleRegisterId", payment.SampleRegisterId);

                //            using (var reader = testCmd.ExecuteReader())
                //            {
                //                while (reader.Read())
                //                {
                //                    payment.PaymentMapping.Add(new PaymentMapping
                //                    {
                //                        PaymentId = Convert.ToInt32(reader["PaymentId"]),
                //                        PaymentName = reader["PaymentName"].ToString(),
                //                        IsCash = Convert.ToBoolean(reader["IsCash"]),
                //                        IsCheque = Convert.ToBoolean(reader["IsCheque"]),
                //                        IsOnline = Convert.ToBoolean(reader["IsOnline"]),
                //                        //IsActive = Convert.ToBoolean(reader["IsActive"]),
                //                    });
                //                }
                //            }
                //        }
                //    }
                //}
                return sampleRegisters;
            }
            catch (Exception ex)
            {
                throw new Exception("Error ocuur fecthing  SampleRegister" + ex.Message);
            }
        }

        public SampleRegister GetSampleRegisterById(int SampleRegisterId)
        {
            var response = new SampleRegister();

            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }


                string query = @"SELECT * FROM sampleregister
                                 INNER JOIN branch ON sampleregister.BranchId = branch.BranchId
                                 LEFT JOIN b2b ON sampleregister.B2BId = b2b.B2BId
                                 LEFT JOIN city ON sampleregister.CityId = city.CityId
                                 LEFT JOIN area ON sampleregister.AreaId = area.AreaId
                                 LEFT JOIN doctor ON sampleregister.DoctorId = doctor.DoctorId 
                                 INNER JOIN payment ON sampleregister.PaymentId = payment.PaymentId
                                 WHERE SampleRegisterId = @SampleRegisterId";

                using (var command = new SqlCommand(query, _sqlConnection))
                {
                    command.Parameters.AddWithValue("@SampleRegisterId", SampleRegisterId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            response.SampleRegisterId = Convert.ToInt32(reader["SampleRegisterId"]);
                            response.Date = Convert.ToDateTime(reader["Date"]);
                            response.BranchId = Convert.ToInt32(reader["BranchId"]);
                            response.BranchName = reader["BranchName"].ToString();
                            response.TotalAmount = Convert.ToInt32(reader["TotalAmount"]);
                            response.IsB2B = reader["IsB2B"] != DBNull.Value ? Convert.ToBoolean(reader["IsB2B"]) : false;
                            response.B2BId = reader["B2BId"] != DBNull.Value ? Convert.ToInt32(reader["B2BId"]) : null;
                            response.B2BName = reader["B2BName"].ToString();
                            response.PhoneNumber = reader["PhoneNumber"].ToString();
                            response.Title = reader["Title"].ToString();
                            response.FirstName = reader["FirstName"].ToString();
                            response.MiddleName = reader["MiddleName"].ToString();
                            response.LastName = reader["LastName"].ToString();
                            response.DOB = Convert.ToDateTime(reader["DOB"]);
                            response.Age = Convert.ToInt32(reader["Age"]);
                            response.Gender = reader["Gender"].ToString();
                            response.Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : null;
                            response.CityId = reader["CityId"] != DBNull.Value ? Convert.ToInt32(reader["CityId"]) : null;
                            response.CityName = reader["CityName"].ToString();
                            response.AreaId = reader["AreaId"] != DBNull.Value ? Convert.ToInt32(reader["AreaId"]) : null;
                            response.AreaName = reader["AreaName"].ToString();
                            response.Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : null;
                            response.Amount = Convert.ToInt32(reader["Amount"]); ;
                            response.ChequeNo = reader["ChequeNo"] != DBNull.Value ? reader["ChequeNo"].ToString() : null;
                            response.ChequeDate = reader["ChequeDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["ChequeDate"]);
                            response.TransactionId = reader["TransactionId"] != DBNull.Value ? reader["TransactionId"].ToString() : null;
                            response.DoctorId = reader["DoctorId"] != DBNull.Value ? Convert.ToInt32(reader["DoctorId"]) : null;
                            response.DoctorName = reader["DoctorName"].ToString();
                            response.CreatedBy = reader["CreatedBy"] != DBNull.Value ? reader["CreatedBy"].ToString() : null;
                            response.PaymentId = Convert.ToInt32(reader["PaymentId"]);
                            response.PaymentName = reader["PaymentName"].ToString();
                            response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                            response.ServiceMapping = new List<ServiceMapping>();
                            //response.PaymentMapping = new List<PaymentMapping>(); 
                        }
                        else
                        {
                            return null;
                        }
                    }
                }


                response.ServiceMapping = new List<ServiceMapping>();

                string testQuery = @"
                    SELECT s.ServiceId, s.ServiceCode, s.ServiceName, s.B2BAmount, s.B2CAmount, 
                           stm.SampleServiceMapId, stm.SampleRegisterId
                    FROM sampleServiceMap stm
                    INNER JOIN service s ON stm.ServiceId = s.ServiceId
                    WHERE stm.SampleRegisterId = @SampleRegisterId ";
                //AND stm.IsActive = 1";,  stm.IsActive

                using (var testCmd = new SqlCommand(testQuery, _sqlConnection))
                {
                    testCmd.Parameters.AddWithValue("@SampleRegisterId", SampleRegisterId);

                    using (var reader = testCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.ServiceMapping.Add(new ServiceMapping
                            {
                                ServiceId = Convert.ToInt32(reader["ServiceId"]),
                                ServiceCode = reader["ServiceCode"].ToString(),
                                ServiceName = reader["ServiceName"].ToString(),
                                B2BAmount = Convert.ToInt32(reader["B2BAmount"]),
                                B2CAmount = Convert.ToInt32(reader["B2CAmount"]),
                                //IsActive = Convert.ToBoolean(reader["IsActive"]),
                                SampleServiceMapId = Convert.ToInt32(reader["SampleServiceMapId"]),
                                //CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                            });
                        }
                    }
                }


                //response.PaymentMapping = new List<PaymentMapping>();

                //string paymentQuery = @"
                //    SELECT p.PaymentId, p.PaymentName, p.IsCash, p.IsCheque, p.IsOnline,
                //           stm.SamplePaymentMapId,  stm.SampleRegisterId
                //    FROM samplepaymentmap stm
                //    INNER JOIN payment p ON stm.PaymentId = p.PaymentId
                //    WHERE stm.SampleRegisterId = @SampleRegisterId"; 
                //    //AND stm.IsActive = 1";,  stm.IsActive

                //using (var testCmd = new SqlCommand(paymentQuery, _sqlConnection))
                //{
                //    testCmd.Parameters.AddWithValue("@SampleRegisterId", SampleRegisterId);

                //    using (var reader = testCmd.ExecuteReader())
                //    {
                //        while (reader.Read())
                //        {
                //            response.PaymentMapping.Add(new PaymentMapping 
                //            {
                //                PaymentId = Convert.ToInt32(reader["PaymentId"]),
                //                PaymentName = reader["PaymentName"].ToString(),
                //                IsCash = Convert.ToBoolean(reader["IsCash"]),
                //                IsCheque = Convert.ToBoolean(reader["IsCheque"]),
                //                IsOnline = Convert.ToBoolean(reader["IsOnline"]),
                //                //IsActive = Convert.ToBoolean(reader["IsActive"]),
                //            });
                //        }
                //    }
                //}

            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching Sampleregister by ID: " + ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }

            return response;
        }


        //public SampleRegister DeleteSampleRegisterById(int SampleRegisterId)
        //{
        //    var response = new SampleRegister();
        //    try
        //    {
        //        if (_sqlConnection.State != ConnectionState.Open)
        //            _sqlConnection.Open();

        //        string query = @"
        //                          UPDATE sampleregister 
        //                          SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END 
        //                          WHERE SampleRegisterId = @SampleRegisterId;

        //                           SELECT s.SampleRegisterId, s.Date, b.BranchId, b.BranchName, s.TotalAmount, s.IsB2B,
        //                           k.B2BId, k.B2BName, s.PhoneNumber, s.Title, s.FirstName, s.MiddleName, 
        //                           s.LastName, s.DOB, s.Age, s.Gender, s.Email, c.CityId, c.CityName, 
        //                           a.AreaId, a.AreaName, s.Address, s.Amount, s.ChequeNo, s.ChequeDate, s.TransactionId, s.IsActive
        //                           FROM sampleregister s
        //                           INNER JOIN branch b ON s.BranchId = b.BranchId
        //                           LEFT JOIN b2b k ON s.B2BId = k.B2BId
        //                           LEFT JOIN city c ON s.CityId = c.CityId
        //                           LEFT JOIN area a ON s.AreaId = a.AreaId
        //                           WHERE s.SampleRegisterId = @SampleRegisterId;";
        //                          //INNER JOIN doctor d ON s.DoctorId = d.DoctorId


        //        var commond = new SqlCommand(query, _sqlConnection);
        //        commond.Parameters.AddWithValue("@SampleRegisterId", SampleRegisterId);

        //        var reader = commond.ExecuteReader();
        //        if (reader.Read())
        //        {
        //            response.SampleRegisterId = Convert.ToInt32(reader["SampleRegisterId"]);
        //            response.Date = Convert.ToDateTime(reader["Date"]);
        //            response.BranchId = Convert.ToInt32(reader["BranchId"]);
        //            response.BranchName = reader["BranchName"].ToString();
        //            response.TotalAmount = Convert.ToInt32(reader["TotalAmount"]);
        //            response.IsB2B = Convert.ToBoolean(reader["IsB2B"]);
        //            response.B2BId = reader["B2BId"] != DBNull.Value ? Convert.ToInt32(reader["B2BId"]) : null;
        //            response.B2BName = reader["B2BName"].ToString();
        //            response.PhoneNumber = reader["PhoneNumber"].ToString();
        //            response.Title = reader["Title"].ToString();
        //            response.FirstName = reader["FirstName"].ToString();
        //            response.MiddleName = reader["MiddleName"].ToString();
        //            response.LastName = reader["LastName"].ToString();
        //            response.DOB = Convert.ToDateTime(reader["DOB"]);
        //            response.Age = Convert.ToInt32(reader["Age"]);
        //            response.Gender = reader["Gender"].ToString();
        //            response.Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : null;
        //            response.CityId = reader["CityId"] != DBNull.Value ? Convert.ToInt32(reader["CityId"]) : null;
        //            response.CityName = reader["CityName"].ToString();
        //            response.AreaId = reader["AreaId"] != DBNull.Value ? Convert.ToInt32(reader["AreaId"]) : null;
        //            response.AreaName = reader["AreaName"].ToString();
        //            response.Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : null;
        //            response.Amount = Convert.ToInt32(reader["Amount"]); ;
        //            response.ChequeNo = reader["ChequeNo"] != DBNull.Value ? reader["ChequeNo"].ToString() : null;
        //            response.ChequeDate = reader["ChequeDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["ChequeDate"]);
        //            response.TransactionId = reader["TransactionId"] != DBNull.Value ? reader["TransactionId"].ToString() : null;
        //            //response.DoctorId = Convert.ToInt32(reader["DoctorId"]);
        //            //response.DoctorName = reader["DoctorName"].ToString();
        //            response.IsActive = Convert.ToBoolean(reader["IsActive"]);
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //    finally
        //    {
        //        _sqlConnection.Close();
        //    }
        //    return response;
        //}


        // SAMPLE SERVICE MAP

        //public List<SampleRegister> GetSampleByIsActive()
        //{

        //    var response = new List<SampleRegister>();

        //    try
        //    {
        //        if (_sqlConnection.State != ConnectionState.Open)
        //        {
        //            _sqlConnection.Open();
        //        }

        //        string query = @"SELECT * FROM sampleregister
        //                         INNER JOIN branch ON sampleregister.BranchId = branch.BranchId
        //                         LEFT JOIN b2b ON sampleregister.B2BId = b2b.B2BId
        //                         LEFT JOIN city ON sampleregister.CityId = city.CityId
        //                         LEFT JOIN area ON sampleregister.AreaId = area.AreaId
        //                         LEFT JOIN doctor ON sampleregister.DoctorId = doctor.DoctorId
        //                         INNER JOIN payment ON sampleregister.PaymentId = payment.PaymentId";

        //        SqlCommand command = new SqlCommand(query, _sqlConnection);

        //        using (SqlDataReader reader = command.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                response.Add(new SampleRegister()
        //                {
        //                    SampleRegisterId = Convert.ToInt32(reader["SampleRegisterId"]),
        //                    Date = Convert.ToDateTime(reader["Date"]),
        //                    BranchId = Convert.ToInt32(reader["BranchId"]),
        //                    BranchName = reader["BranchName"].ToString(),
        //                    TotalAmount = Convert.ToInt32(reader["TotalAmount"]),
        //                    IsB2B = reader["IsB2B"] != DBNull.Value ? Convert.ToBoolean(reader["IsB2B"]) : false,
        //                    B2BId = reader["B2BId"] != DBNull.Value ? Convert.ToInt32(reader["B2BId"]) : null,
        //                    B2BName = reader["B2BName"].ToString(),
        //                    PhoneNumber = reader["PhoneNumber"].ToString(),
        //                    Title = reader["Title"].ToString(),
        //                    FirstName = reader["FirstName"].ToString(),
        //                    MiddleName = reader["MiddleName"].ToString(),
        //                    LastName = reader["LastName"].ToString(),
        //                    DOB = Convert.ToDateTime(reader["DOB"]),
        //                    Age = Convert.ToInt32(reader["Age"]),
        //                    Gender = reader["Gender"].ToString(),
        //                    Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : null,
        //                    CityId = reader["CityId"] != DBNull.Value ? Convert.ToInt32(reader["CityId"]) : null,
        //                    CityName = reader["CityName"].ToString(),
        //                    AreaId = reader["AreaId"] != DBNull.Value ? Convert.ToInt32(reader["AreaId"]) : null,
        //                    AreaName = reader["AreaName"].ToString(),
        //                    Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : null,
        //                    Amount = Convert.ToInt32(reader["Amount"]),
        //                    ChequeNo = reader["ChequeNo"] != DBNull.Value ? reader["ChequeNo"].ToString() : null,
        //                    ChequeDate = reader["ChequeDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["ChequeDate"]),
        //                    TransactionId = reader["TransactionId"] != DBNull.Value ? reader["TransactionId"].ToString() : null,
        //                    DoctorId = reader["DoctorId"] != DBNull.Value ? Convert.ToInt32(reader["DoctorId"]) : null,
        //                    DoctorName = reader["DoctorName"].ToString(),
        //                    CreatedBy = reader["CreatedBy"] != DBNull.Value ? reader["CreatedBy"].ToString() : null,
        //                    PaymentId = Convert.ToInt32(reader["PaymentId"]),
        //                    PaymentName = reader["PaymentName"].ToString(),
        //                    IsActive = Convert.ToBoolean(reader["IsActive"]),
        //                    ServiceMapping = new List<ServiceMapping>(),
        //                    //PaymentMapping = new List<PaymentMapping>(),
        //                });
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error fetching Sampleregister by: " + ex.Message);
        //    }
        //    finally
        //    {
        //        _sqlConnection.Close();
        //    }
        //    return response;
        //}

        // SAMPLE SERVICE MAP ID
        public SampleServiceMap DeleteSampleServiceMapId(int SampleServiceMapId)
        {
            var response = new SampleServiceMap();
            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }


                string query = @"select SampleServiceMapId, sampleServiceMap.ServiceId, sampleServiceMap.SampleRegisterId
                                 from sampleServiceMap 
                                 inner join service on service.ServiceId = sampleServiceMap.ServiceId
                                 inner join sampleregister on sampleregister.SampleRegisterId = sampleServiceMap.SampleRegisterId
                                 DELETE FROM sampleServiceMap
                                 WHERE SampleServiceMapId = @SampleServiceMapId;";


                var commond = new SqlCommand(query, _sqlConnection);
                commond.Parameters.AddWithValue("@SampleServiceMapId", SampleServiceMapId);

                var reader = commond.ExecuteReader();
                if (reader.Read())
                {

                    response.SampleServiceMapId = Convert.ToInt32(reader["SampleServiceMapId"]);
                    response.SampleRegisterId = Convert.ToInt32(reader["SampleRegisterId"]);
                    response.ServiceId = Convert.ToInt32(reader["ServiceId"]);
                    //response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }




        // TEST RESULT
        public TestResultModal AddUpdateTestResult(TestResultModal resultModal)
        {
            var response = new TestResultModal();

            try
            {

                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string chechQuery = @"SELECT COUNT(1) FROM testResultDetails 
                               WHERE SampleRegisterId = @SampleRegisterId AND ServiceId = @ServiceId AND TestId = @TestId";


                using (var checkCmd = new SqlCommand(chechQuery, _sqlConnection))
                {
                    checkCmd.Parameters.AddWithValue("@SampleRegisterId", resultModal.SampleRegisterId);
                    checkCmd.Parameters.AddWithValue("@ServiceId", resultModal.ServiceId);
                    checkCmd.Parameters.AddWithValue("@TestId", resultModal.TestId);

                    int exists = (int)checkCmd.ExecuteScalar();

                    if (exists > 0)
                    {

                        if (_sqlConnection.State != ConnectionState.Open)
                        {
                            _sqlConnection.Open();
                        }

                        string query = @"UPDATE testResultDetails 
                                         SET ResultValue = @ResultValue, ValidationStatus = @ValidationStatus, ValidateBy = @ValidateBy, CreatedBy = @CreatedBy
                                         WHERE SampleRegisterId = @SampleRegisterId AND ServiceId = @ServiceId AND TestId = @TestId;

                                         SELECT t.TestResultId, s.SampleRegisterId, sh.ServiceId, sh.ServiceName, ts.TestId, ts.TestName, t.ResultValue, t.ValidationStatus, t.ValidateBy, t.CreatedBy, t.IsActive
                                         FROM testResultDetails t
                                         INNER JOIN sampleregister s ON t.SampleRegisterId = s.SampleRegisterId
                                         INNER JOIN service sh ON t.ServiceId = sh.ServiceId 
                                         INNER JOIN test ts ON t.TestId = ts.TestId
                                         WHERE t.SampleRegisterId =  @SampleRegisterId AND t.ServiceId = @ServiceId AND t.TestId = @TestId";



                        using var common = new SqlCommand(query, _sqlConnection);
                        //common.Parameters.AddWithValue("@TestResultId", resultModal.TestResultId);
                        common.Parameters.AddWithValue("@SampleRegisterId", resultModal.SampleRegisterId);
                        common.Parameters.AddWithValue("@ServiceId", resultModal.ServiceId);
                        common.Parameters.AddWithValue("@TestId", resultModal.TestId);
                        common.Parameters.AddWithValue("@ResultValue", resultModal.ResultValue);
                        common.Parameters.AddWithValue("@ValidationStatus", resultModal.ValidationStatus);
                        common.Parameters.AddWithValue("@ValidateBy", resultModal.ValidateBy ?? (object)DBNull.Value);
                        common.Parameters.AddWithValue("@CreatedBy", resultModal.CreatedBy ?? (object)DBNull.Value);


                        using var reader = common.ExecuteReader();
                        if (reader.Read())
                        {
                            response.TestResultId = Convert.ToInt32(reader["TestResultId"]);
                            response.SampleRegisterId = Convert.ToInt32(reader["SampleRegisterId"]);
                            response.ServiceId = Convert.ToInt32(reader["ServiceId"]);
                            response.ServiceName = reader["ServiceName"].ToString();
                            response.TestId = Convert.ToInt32(reader["TestId"]);
                            response.TestName = reader["TestName"].ToString();
                            response.ResultValue = reader["ResultValue"].ToString();
                            response.ValidationStatus = Convert.ToBoolean(reader["ValidationStatus"]);
                            response.CreatedBy = reader["CreatedBy"].ToString();
                            response.ValidateBy = reader["ValidateBy"].ToString();
                            response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                        }
                        else
                        {
                            throw new Exception("Update Successfully but no fetch data properly");
                        }
                    }
                    else
                    {

                        if (_sqlConnection.State != ConnectionState.Open)
                        {
                            _sqlConnection.Open();
                        }

                        string query = @"INSERT INTO testResultDetails(SampleRegisterId, ServiceId, TestId, ResultValue, ValidationStatus, CreatedBy, ValidateBy) OUTPUT INSERTED.TestResultId
                                         VALUES (@SampleRegisterId, @ServiceId, @TestId, @ResultValue, @ValidationStatus, @CreatedBy, @ValidateBy)";

                        using var common = new SqlCommand(query, _sqlConnection);
                        //common.Parameters.AddWithValue("@TestResultId", resultModal.TestResultId);
                        common.Parameters.AddWithValue("@SampleRegisterId", resultModal.SampleRegisterId);
                        common.Parameters.AddWithValue("@ServiceId", resultModal.ServiceId);
                        common.Parameters.AddWithValue("@TestId", resultModal.TestId);
                        common.Parameters.AddWithValue("@ResultValue", resultModal.ResultValue);
                        common.Parameters.AddWithValue("@ValidationStatus", resultModal.ValidationStatus);
                        common.Parameters.AddWithValue("@CreatedBy", resultModal.CreatedBy ?? (object)DBNull.Value);
                        common.Parameters.AddWithValue("@ValidateBy", resultModal.ValidateBy ?? (object)DBNull.Value);
                        common.Parameters.AddWithValue("@IsActive", resultModal.IsActive);


                        int insertedId = (int)common.ExecuteScalar();


                        // service
                        string serviceQuery = "SELECT ServiceName FROM service WHERE ServiceId = @ServiceId";
                        using var serviceCommond = new SqlCommand(serviceQuery, _sqlConnection);
                        serviceCommond.Parameters.AddWithValue("@ServiceId", resultModal.ServiceId);
                        string serviceName = (string?)serviceCommond.ExecuteScalar() ?? "";


                        // Test
                        string TestQuery = "SELECT TestName FROM test WHERE TestId = @TestId";
                        using var testCommond = new SqlCommand(TestQuery, _sqlConnection);
                        testCommond.Parameters.AddWithValue("@TestId", resultModal.TestId);
                        string testName = (string?)testCommond.ExecuteScalar() ?? "";


                        response.TestResultId = insertedId;
                        response.SampleRegisterId = resultModal.SampleRegisterId;
                        response.ServiceId = resultModal.ServiceId;
                        response.ServiceName = resultModal.ServiceName;
                        response.TestId = resultModal.TestId;
                        response.TestName = resultModal.TestName;
                        response.ResultValue = resultModal.ResultValue;
                        response.ValidationStatus = resultModal.ValidationStatus;
                        response.ValidateBy = resultModal.ValidateBy;
                        response.CreatedBy = resultModal.CreatedBy;
                        response.IsActive = true;
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(" fetch data in error accure", ex);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }

        public TestResultDto AddUpdateTestResults(TestResultDto testResults)
        {
            var response = new TestResultDto();

            try
            {
                foreach (var sample in testResults.SampleRegister)
                {
                    foreach (var service in sample.Services)
                    {
                        foreach (var test in service.Tests)
                        {
                            var testResultModal = new TestResultModal
                            {
                                SampleRegisterId = sample.SampleRegisterId,
                                ServiceId = service.ServiceId,
                                TestId = test.TestId,
                                ResultValue = test.ResultValue,
                                ValidationStatus = test.ValidationStatus,
                                CreatedBy = test.CreatedBy,
                                ValidateBy = test.ValidateBy,
                                IsActive = test.IsActive,
                            };
                            AddUpdateTestResult(testResultModal);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("not proper work", ex);
            }
            response.SampleRegister = testResults.SampleRegister;
            return response;
        }

        public TestResultDto GetTestResultById(int SampleRegisterId)
        {
            var results = new TestResultDto();

            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = @"SELECT t.TestResultId, t.SampleRegisterId, t.ServiceId, t.TestId, t.ResultValue, t.ValidationStatus, t.CreatedBy, t.ValidateBy, t.IsActive,
                                s.ServiceName, ts.TestName
                                 FROM testResultDetails t
                                 INNER JOIN service s ON t.ServiceId = s.ServiceId
                                 INNER JOIN test ts ON t.TestId = ts.TestId
                                 WHERE t.SampleRegisterId = @SampleRegisterId";

                using (var command = new SqlCommand(query, _sqlConnection))
                {
                    command.Parameters.AddWithValue("@SampleRegisterId", SampleRegisterId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            if (results.SampleRegister == null)
                            {
                                results.SampleRegister = new List<SampleRegisterDto>();
                            }

                            if (results.SampleRegister.Count == 0 || results.SampleRegister.Last().SampleRegisterId != SampleRegisterId)
                            {
                                results.SampleRegister.Add(new SampleRegisterDto
                                {
                                    SampleRegisterId = SampleRegisterId,
                                    Services = new List<ServiceDto>()
                                });
                            }

                            var lastSample = results.SampleRegister.Last();

                            if (lastSample.Services == null)
                            {
                                lastSample.Services = new List<ServiceDto>();
                            }

                            if (lastSample.Services.Count == 0 || lastSample.Services.Last().ServiceId != Convert.ToInt32(reader["ServiceId"]))
                            {
                                lastSample.Services.Add(new ServiceDto
                                {
                                    ServiceId = Convert.ToInt32(reader["ServiceId"]),
                                    ServiceName = reader["ServiceName"].ToString(),
                                    Tests = new List<TestDto>()
                                });
                            }

                            var lastService = lastSample.Services.Last();
                            lastService.Tests.Add(new TestDto
                            {
                                TestId = Convert.ToInt32(reader["TestId"]),
                                TestName = reader["TestName"].ToString(),
                                ResultValue = reader["ResultValue"].ToString(),
                                ValidationStatus = Convert.ToBoolean(reader["ValidationStatus"]),
                                CreatedBy = reader["CreatedBy"].ToString(),
                                ValidateBy = reader["ValidateBy"].ToString(),
                                IsActive = Convert.ToBoolean(reader["IsActive"])
                            });


                            //results.Add(new TestResultModal
                            //{
                            //    TestResultId = Convert.ToInt32(reader["TestResultId"]),
                            //    SampleRegisterId = Convert.ToInt32(reader["SampleRegisterId"]),
                            //    ServiceId = Convert.ToInt32(reader["ServiceId"]),
                            //    ServiceName = reader["ServiceName"].ToString(),
                            //    TestId = Convert.ToInt32(reader["TestId"]),
                            //    TestName = reader["TestName"].ToString(),
                            //    ResultValue = reader["ResultValue"].ToString(),
                            //    ValidationStatus = reader["ValidationStatus"].ToString(),
                            //    CreatedBy = reader["CreatedBy"].ToString(),
                            //    ValidateBy = reader["ValidateBy"].ToString(),
                            //    IsActive = Convert.ToBoolean(reader["IsActive"])
                            //});
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching test results by SampleRegisterId: " + ex.Message, ex);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return results;
        }

        public List<TestResultModal> GetTestResultsById(int sampleRegisterId)
        {
            var response = new List<TestResultModal>();

            try
            {
                if (_sqlConnection.State != ConnectionState.Open)
                    _sqlConnection.Open();


                string query = @"SELECT t.TestResultId, t.SampleRegisterId, t.ServiceId, t.TestId, t.ResultValue, t.ValidationStatus, t.CreatedBy, t.ValidateBy, t.IsActive,
                                s.ServiceName, ts.TestName
                                 FROM testResultDetails t
                                 INNER JOIN service s ON t.ServiceId = s.ServiceId
                                 INNER JOIN test ts ON t.TestId = ts.TestId
                                 WHERE t.SampleRegisterId = @SampleRegisterId";

                using (var command = new SqlCommand(query, _sqlConnection))
                {
                    command.Parameters.AddWithValue("@SampleRegisterId", sampleRegisterId);

                    var reader = command.ExecuteReader();
                    {
                        while (reader.Read())
                        {
                               response.Add(new TestResultModal
                               {
                                  TestResultId = Convert.ToInt32(reader["TestResultId"]),
                                  SampleRegisterId = Convert.ToInt32(reader["SampleRegisterId"]),
                                  ServiceId = Convert.ToInt32(reader["ServiceId"]),
                                  ServiceName = reader["ServiceName"].ToString(),
                                  TestId = Convert.ToInt32(reader["TestId"]),
                                  TestName = reader["TestName"].ToString(),
                                  ResultValue = reader["ResultValue"].ToString(),
                                  ValidationStatus = Convert.ToBoolean(reader["ValidationStatus"]),
                                  CreatedBy = reader["CreatedBy"].ToString(),
                                  ValidateBy = reader["ValidateBy"].ToString(),
                                  IsActive = Convert.ToBoolean(reader["IsActive"])
                              });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching test results by SampleRegisterId: " + ex.Message, ex);
            }
            finally
            {
                _sqlConnection.Close();
            }

            return response;
        }


        // TEST APPROVAL
        public TestApprovalResultModal GetApprovalResultBySampleRegisterId(int sampleRegisterId)
        {
            var testApprovalResultModal = new TestApprovalResultModal();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                string query = @"
                                 SELECT 
                                     sr.SampleRegisterId, sr.Title, sr.FirstName, sr.MiddleName, sr.Gender, sr.Age, sr.DOB, sr.Date, sr.PhoneNumber,
                                     sr.BranchId, b.BranchName,
                                     sr.B2BId, bb.B2BName
                                 FROM SampleRegister sr
                                 LEFT JOIN Branch b ON sr.BranchId = b.BranchId
                                 LEFT JOIN B2B bb ON sr.B2BId = bb.B2BId
                                 WHERE sr.SampleRegisterId = @SampleRegisterId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SampleRegisterId", sampleRegisterId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            testApprovalResultModal.SampleRegisterId = Convert.ToInt32(reader["SampleRegisterId"]);
                            //testApprovalResultModal.SampleRegisterModals = new SampleRegisterModal
                            testApprovalResultModal = new TestApprovalResultModal
                            {
                                SampleRegisterId = Convert.ToInt32(reader["SampleRegisterId"]),
                                Title = reader["Title"]?.ToString(),
                                FirstName = reader["FirstName"]?.ToString(),
                                MiddleName = reader["MiddleName"]?.ToString(),
                                Gender = reader["Gender"]?.ToString(),
                                Age = Convert.ToInt32(reader["Age"]),
                                DOB = (DateTime)(reader["DOB"] as DateTime?),
                                Date = (DateTime)(reader["Date"] as DateTime?),
                                PhoneNumber = reader["PhoneNumber"]?.ToString(),
                                BranchId = reader["BranchId"] as int?,
                                BranchName = reader["BranchName"]?.ToString(),
                                B2BId = reader["B2BId"] as int?,
                                B2BName = reader["B2BName"]?.ToString()
                            };
                        }
                        else
                        {
                            return null; 
                        }
                    }
                }

                string serviceQuery = @"
                                         SELECT ssm.SampleServiceMapId, ssm.SampleRegisterId, ssm.ServiceId, ssm.CreatedOn,
                                                s.ServiceName, s.ServiceCode, s.B2BAmount, s.B2CAmount, s.IsActive
                                         FROM sampleServiceMap ssm
                                         INNER JOIN Service s ON ssm.ServiceId = s.ServiceId
                                         WHERE ssm.SampleRegisterId = @SampleRegisterId";

                using (var serviceCommand = new SqlCommand(serviceQuery, connection))
                {
                    serviceCommand.Parameters.AddWithValue("@SampleRegisterId", sampleRegisterId);

                    using (var mapReader = serviceCommand.ExecuteReader())
                    {
                        var mappedServices = new List<serviceMapping>();
                        while (mapReader.Read())
                        {
                            var serviceMapping = new serviceMapping
                            {
                                ServiceId = Convert.ToInt32(mapReader["ServiceId"]),
                                ServiceName = mapReader["ServiceName"]?.ToString(),
                            };
                            mappedServices.Add(serviceMapping);
                        }
                        testApprovalResultModal.serviceMappings = mappedServices;
                    }
                }

           
                string testQuery = @"
                                     SELECT 
                                         tr.TestId, t.TestName, tr.ResultValue, tr.ValidationStatus, tr.TestResultId, tr.IsActive, s.ServiceId
                                     FROM testResultDetails tr
                                     INNER JOIN test t ON tr.TestId = t.TestId
                                     INNER JOIN Service s on tr.ServiceId = s.ServiceId
                                     WHERE tr.SampleRegisterId = @SampleRegisterId";

                using (var testCmd = new SqlCommand(testQuery, connection))
                {
                    testCmd.Parameters.AddWithValue("@SampleRegisterId", sampleRegisterId);

                    using (var testReader = testCmd.ExecuteReader())
                    {
                        while (testReader.Read())
                        {
                            var test = new Test
                            {
                                TestId = Convert.ToInt32(testReader["TestId"]),
                                TestName = testReader["TestName"]?.ToString(),
                                ResultValue = testReader["ResultValue"]?.ToString(),
                                ValidationStatus = Convert.ToBoolean(testReader["ValidationStatus"]),
                                IsActive = testReader["IsActive"] != DBNull.Value && Convert.ToBoolean(testReader["IsActive"]),
                                ServiceId = Convert.ToInt32(testReader["ServiceId"]),
                            };

                            testApprovalResultModal.TestresultId = Convert.ToInt32(testReader["TestResultId"]);
                            testApprovalResultModal.Tests.Add(test);
                        }
                    }
                }

                return testApprovalResultModal;
            }
        }


    }
}
