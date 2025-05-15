using System.Data;
using System.Formats.Tar;
using Azure;
using Azure.Core;
using LIMSAPI.Helpers;
using LIMSAPI.Models.FinanceModal;
using LIMSAPI.Models.Master;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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
                if(_sqlConnection.State  != ConnectionState.Open)
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
            var response  = new StateModal();

            try
            {
                if(_sqlConnection.State != ConnectionState.Open)
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
                if(reader.Read())
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
            catch(Exception ex)
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

                if(_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                if(cityModal.CityId > 0)
                {
                    string query = $@"UPDATE city SET CityCode = @CityCode, CityName = @CityName, stateId = @StateId WHERE CityId = @CityId

                                     SELECT c.CityId, c.CityCode, c.CityName, c.StateId, c.IsActive, c.StateName 
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
            catch(Exception ex)
            {
                throw new Exception("Error fetching saving data",ex);
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
                    isActiveColumn : "city.IsActive"
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
                if(_sqlConnection.State != ConnectionState.Open)
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
;            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching city", ex);
            }
            return response;
        }

        public CityModal GetCityById( int CityId)
        {
            var response = new CityModal();
            try
            {
                if(_sqlConnection.State != ConnectionState.Open)
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
                if(_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                if(areaModal.AreaId > 0)
                {
                    string query = $@" UPDATE area SET AreaCode = @AreaCode, AreaName = @AreaName, CityId = @CityId WHERE AreaId = @AreaId

                                         SELECT a.AreaId, a.AreaCode, a.AreaName, a.CityId, a.IsActive , a.CityName
                                         FROM area a
                                         INNER JOIN city c ON a.CityId = c.CityId
                                         WHERE a.AreaId = @AreaId";
                    
                    using var common = new SqlCommand(query, _sqlConnection);
                    common.Parameters.AddWithValue("@AreaId", areaModal.AreaId);
                    common.Parameters.AddWithValue("@AreaCode",areaModal.AreaCode);
                    common.Parameters.AddWithValue("@AreaName", areaModal.AreaName);
                    common.Parameters.AddWithValue("@CityId", areaModal.CityId);

                    using var reader = common.ExecuteReader();
                    if (reader.Read())
                    {
                        response.AreaId = Convert.ToInt32(reader["AreaId"]);
                        response.AreaCode = reader["AreaCode"].ToString();
                        response.AreaName = reader["AreaName"].ToString();
                        response.CityId = Convert.ToInt32(reader["CityId"]);
                        response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                        response.CityName = reader["CItyName"].ToString();
                    }
                    else
                    {
                        throw new Exception("Update Successed but not found data");
                    }
                }
                else
                {
                    string query = $@"INSERT INTO area (AreaCode, AreaName, CityId) OUTPUT INSERTED.AreaId VALUES (@AreaCode, @AreaName, @CityId)";

                    var common = new SqlCommand(query, _sqlConnection);
                    common.Parameters.AddWithValue("@AreaCode", areaModal.AreaCode);
                    common.Parameters.AddWithValue("@AreaName", areaModal.AreaName);
                    common.Parameters.AddWithValue("@CityId", areaModal.CityId);

                    var insertedId = (int) common.ExecuteScalar();

                    string cityQuery = "SELECT CityName FROM city WHERE CityId = @CityId";
                    var cityCommon = new SqlCommand(cityQuery, _sqlConnection);
                    cityCommon.Parameters.AddWithValue("CityId", areaModal.CityId);

                    string CityName = (string?)cityCommon.ExecuteScalar() ?? "";
                    
                    response.AreaId = insertedId;
                    response.AreaCode = areaModal.AreaCode;
                    response.AreaName = areaModal.AreaName;
                    response.CityId = areaModal.CityId;
                    response.IsActive = true;
                    response.CityName = CityName;

                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetxhing saving data", ex);
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
                    codeColumn: "AreaCode",
                    idColumn: "AreaId",
                    filter: filterModel,
                    mapFunc: reader => new AreaModal
                    {
                        AreaId = Convert.ToInt32(reader["AreaId"]),
                        AreaCode = reader["AreaCode"].ToString(),
                        AreaName = reader["AreaName"].ToString(),
                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                        CityId = Convert.ToInt32(reader["CityId"]),
                        CityName = reader["CityName"].ToString()
                    },
                    selectColumns : "a.AreaId, a.AreaCode, a.AreaName, a.IsActive, a.CityId, c.CityName",
                    isActiveColumn : "a.IsActive"
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
                if(_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = $@"UPDATE area SET IsActive = CASE WHEN IsActive = 1 THEN 0 Else 1 END WHERE AreaId = @AreaId
                                SELECT a.AreaId, a.AreaCode, a.AreaName, a.CityId, a.IsActive , c.CityName
                                         FROM area a
                                         INNER JOIN city c ON a.CityId = c.CityId
                                         WHERE a.AreaId = @AreaId";

                var common = new SqlCommand(query, _sqlConnection);
                common.Parameters.AddWithValue("@AreaId", AreaId);

                var reader = common.ExecuteReader();
                if (reader.Read())
                {
                    response.AreaId = Convert.ToInt32(reader["AreaId"]);
                    response.AreaCode = reader["AreaCode"].ToString();
                    response.AreaName = reader["AreaName"].ToString();
                    response.CityId = Convert.ToInt32(reader["CityId"]);
                    response.IsActive = Convert.ToBoolean(reader["IsActive"]);
                    response.CityName = reader["CityName"].ToString();
                }
            }
            catch ( Exception ex )
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
                if(_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = $@" SELECT a.AreaId, a.AreaCode, a.AreaName, a.CityId, a.IsActive , c.CityName
                                         FROM area a
                                         INNER JOIN city c ON a.CityId = c.CityId
                                         WHERE a.IsActive = 1";

                using(SqlCommand command = new SqlCommand(query, _sqlConnection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.Add(new AreaModal
                            {
                                AreaId = Convert.ToInt32(reader["AreaId"]),
                                AreaCode = reader["AreaCode"].ToString(),
                                AreaName = reader["AreaName"].ToString(),
                                IsActive = Convert.ToBoolean(reader["IsActive"]),
                                CityId = Convert.ToInt32(reader["CityId"]),
                                CityName = reader["CityName"].ToString()
                            });
                        }
                    }
                }
            }
            catch ( Exception ex )
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
                if(_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = $@"SELECT a.AreaId, a.AreaCode, a.AreaName, a.CityId, a.IsActive , c.CityName
                                         FROM area a
                                         INNER JOIN city c ON a.CityId = c.CityId
                                         WHERE a.AreaId = @AreaId";

                var common = new SqlCommand(query, _sqlConnection);
                common.Parameters.AddWithValue("@AreaId", AreaId);

                var reader = common.ExecuteReader();
                if (reader.Read())
                {
                    response.AreaId = Convert.ToInt32(reader["AreaId"]);
                    response.AreaCode = reader["AreaCode"].ToString();
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
                    _sqlConnection.Close();
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
            var response = new List<ServiceModal>();

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
                                response.Add(new ServiceModal
                                {
                                    ServiceId = Convert.ToInt32(reader["ServiceId"]),
                                    ServiceName = reader["ServiceName"].ToString(),
                                    ServiceCode = reader["ServiceCode"].ToString(),
                                    B2BAmount = Convert.ToInt32(reader["B2BAmount"]),
                                    B2CAmount = Convert.ToInt32(reader["B2CAmount"]),
                                    IsActive = Convert.ToBoolean(reader["IsActive"]),
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw new Exception("Error fetching all service", ex);
            }

            return response;
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
                
                if(_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }
                SqlCommand command;

                if (paymentModal.PaymentId > 0)
                {
                    string query = @"UPDATE payment
                                    SET PaymentName = @PaymentName
                                    WHERE PaymentId = @PaymentId
                              
                                    SELECT PaymentId, PaymentName, IsActive FROM payment WHERE PaymentId = @PaymentId";

                     command = new SqlCommand(query, _sqlConnection);


                    command.Parameters.AddWithValue("@PaymentId", paymentModal.PaymentId);
                }
                else
                {
                    string query = @"INSERT INTO payment(PaymentName) OUTPUT INSERTED.PaymentId VALUES (@PaymentName)";

                    command = new SqlCommand(query, _sqlConnection);
                }

                command.Parameters.AddWithValue("@PaymentName", paymentModal.PaymentName);

                if(paymentModal.PaymentId > 0)
                {
                    using var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        response.PaymentId = Convert.ToInt32(reader["PaymentId"]);
                        response.PaymentName = reader["PaymentName"].ToString();
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
                    response.IsActive = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching data in to the payment",ex);
            }
            finally
            {
                _sqlConnection.Close();
                _sqlConnection.Dispose();
            }
            return response;
        }
    }
}
