using API_Ekzamen.Model;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Data.SqlClient;

namespace API_Ekzamen.Controllers
{
    public class KinoController : Controller
    {
        const string conStr = @"Data Source = DESKTOP-S23LER7; Initial Catalog = WPF_Ekz; Integrated Security=True; TrustServerCertificate=Yes;";

        [HttpGet("/GetFilm")]
        public async Task<List<Film>> GetFilm(string data)
        {
            try
            {
                using (SqlConnection db = new SqlConnection(conStr))
                {
                    db.Open();
                    var result = db.Query<Film>("pFilm", new { data }, commandType: CommandType.StoredProcedure).ToList();
                    db.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPost("/AddOrEditFilm")]
        public async Task<bool> AddOrEditFilm(int id, string name, string description, string genre, string duration, string poster)
        {
            try
            {
                using (SqlConnection db = new SqlConnection(conStr))
                {
                    await db.OpenAsync();

                    var parameters = new DynamicParameters();
                    parameters.Add("@id", id);
                    parameters.Add("@name", name);
                    parameters.Add("@description", description);
                    parameters.Add("@genre", genre);
                    parameters.Add("@duration", duration);
                    parameters.Add("@poster", poster);

                    await db.ExecuteAsync("pFilm;2", parameters, commandType: CommandType.StoredProcedure);

                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpPost("/DeleteFilm")]
        public async Task<bool> DeleteFilm(int id)
        {
            try
            {
                using (SqlConnection db = new SqlConnection(conStr))
                {
                    await db.OpenAsync();

                    var parameters = new DynamicParameters();
                    parameters.Add("@id", id);

                    await db.ExecuteAsync("pFilm;3", parameters, commandType: CommandType.StoredProcedure);

                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpPost("/AddOrEditSession")]
        public async Task<bool> AddOrEditSession(int id, int hall_id, string time, int film_id, decimal priceAdult, decimal priceStudent, decimal priceChild)
        {
            try
            {
                using (SqlConnection db = new SqlConnection(conStr))
                {
                    await db.OpenAsync();

                    var parameters = new DynamicParameters();
                    parameters.Add("@id", id);
                    parameters.Add("@hall_id", hall_id);
                    parameters.Add("@time", time);
                    parameters.Add("@film_id", film_id);
                    parameters.Add("@priceAdult", priceAdult);
                    parameters.Add("@priceStudent", priceStudent);
                    parameters.Add("@priceChild", priceChild);

                    await db.ExecuteAsync("pSession;2", parameters, commandType: CommandType.StoredProcedure);

                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpPost("/DeleteSession")]
        public async Task<bool> DeleteSession(int id)
        {
            try
            {
                using (SqlConnection db = new SqlConnection(conStr))
                {
                    await db.OpenAsync();

                    var parameters = new DynamicParameters();
                    parameters.Add("@id", id);

                    await db.ExecuteAsync("pSession;3", parameters, commandType: CommandType.StoredProcedure);

                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpGet("/GetSessionsByFilmOrGenre")]
        public async Task<List<Hall>> GetSessionsByFilmOrGenre(string data)
        {
            try
            {
                List<Hall> halls = new List<Hall>();

                using (SqlConnection db = new SqlConnection(conStr))
                {
                    db.Open();

                    var results = await db.QueryAsync<Hall, SessionWithFilm, Hall>(
                        @"
                           SELECT 
                               h.id AS id,
                               h.name AS name,
                               s.time AS SessionTime,
                               f.name AS FilmTitle,
                               f.duration AS FilmDuration,
                               f.genre AS Genre,
                               s.priceAdult AS PriceAdult,
                               s.priceStudent AS PriceStudent,
                               s.priceChild AS PriceChild,
                               s.id AS sessionId
                           FROM Session s
                           JOIN Hall h ON h.id = s.hall_id
                           JOIN Film f ON f.id = s.film_id
                           WHERE (@data IS NULL OR f.name LIKE '%' + @data + '%' OR f.genre LIKE '%' + @data + '%')
                         ",
                        (hall, session) =>
                        {
                            Hall existingHall = halls.FirstOrDefault(h => h.id == hall.id);
                            if (existingHall == null)
                            {
                                existingHall = hall;
                                existingHall.session = new List<SessionWithFilm>();
                                halls.Add(existingHall);
                            }
                            existingHall.session.Add(session);
                            return existingHall;
                        },
                        new { data },
                        splitOn: "SessionTime",
                        commandType: CommandType.Text
                    );

                    db.Close();
                    return halls.Distinct().ToList();
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet("/GetPlace")]
        public async Task<List<Place>> GetPlace(int hall_id, int session_id)
        {
            try
            {
                using (SqlConnection db = new SqlConnection(conStr))
                {
                    db.Open();
                    var result = db.Query<Place>("pGetPlaces", new { hall_id, session_id }, commandType: CommandType.StoredProcedure).ToList();
                    db.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        [HttpGet("/GetTicket")]
        public async Task<List<Ticket2>> GetTicket(int user_id)
        {
            try
            {
                using (SqlConnection db = new SqlConnection(conStr))
                {
                    db.Open();
                    var result = db.Query<Ticket2>("pTicket", new { user_id }, commandType: CommandType.StoredProcedure).ToList();
                    db.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet("/GetTicketAdmin")]
        public async Task<List<Ticket2>> GetTicketAdmin()
        {
            try
            {
                using (SqlConnection db = new SqlConnection(conStr))
                {
                    db.Open();
                    var result = db.Query<Ticket2>("pGetTicketAdmin", commandType: CommandType.StoredProcedure).ToList();
                    db.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet("/BuyTicket")]
        public async Task<List<Ticket>> BuyTicket(int place_id, int session_id, int user_id, decimal amount)
        {
            try
            {
                using (SqlConnection db = new SqlConnection(conStr))
                {
                    db.Open();
                    var result = db.Query<Ticket>("pBuyTicket", new { place_id, session_id, user_id, amount }, commandType: CommandType.StoredProcedure).ToList();
                    db.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPost("/ReturnTicket")]
        public async Task<bool> ReturnTicket(int ticket_id)
        {
            try
            {
                using (SqlConnection db = new SqlConnection(conStr))
                {
                    await db.OpenAsync();
                    await db.ExecuteAsync("pReturnTicket", new { ticket_id }, commandType: CommandType.StoredProcedure);
                    db.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        [HttpPost("/AddUsers")]
        public async Task<bool> AddUsers(string Login, string Password, string Phone)
        {
            try
            {
                using (SqlConnection db = new SqlConnection(conStr))
                {
                    await db.OpenAsync();

                    var parameters = new DynamicParameters();
                    parameters.Add("@login", Login);
                    parameters.Add("@pwd", Password);
                    parameters.Add("@phone", Phone);

                    await db.ExecuteAsync("pUser", parameters, commandType: CommandType.StoredProcedure);

                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpGet("/LogIn")]
        public async Task<List<User>> LogIn(string login, string pwd)
        {
            try
            {
                using (SqlConnection db = new SqlConnection(conStr))
                {
                    db.Open();
                    var result = db.Query<User>("pUser;2", new { login, pwd }, commandType: CommandType.StoredProcedure).ToList();
                    db.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet("/GetHallCmb")]
        public async Task<List<Hall>> GetHallCmb()
        {
            try
            {
                using (SqlConnection db = new SqlConnection(conStr))
                {
                    db.Open();
                    var result = db.Query<Hall>("pGetHallCmd", commandType: CommandType.StoredProcedure).ToList();
                    db.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet("/GetFilmCmb")]
        public async Task<List<Film>> GetFilmCmb()
        {
            try
            {
                using (SqlConnection db = new SqlConnection(conStr))
                {
                    db.Open();
                    var result = db.Query<Film>("pGetFilmCmd", commandType: CommandType.StoredProcedure).ToList();
                    db.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}