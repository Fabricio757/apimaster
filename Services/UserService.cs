using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;
using System.Data.SqlClient;
using System.Data;


namespace WebApi.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        IEnumerable<User> GetAll();
        User GetById(int id);
        void Add(string name);
    }

    public class UserService : IUserService
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private List<User> _users = new List<User>
        {
            new User { Id = 1, FirstName = "Fabricio", LastName = "Antonini", Username = "Fabri", Password = "test" },
            new User { Id = 2, FirstName = "L", LastName = "Antonini", Username = "LLL", Password = "test" }
        };

        private readonly AppSettings _appSettings;
        private readonly ConnectionString _connectionString;

        public UserService(IOptions<AppSettings> appSettings, IOptions<ConnectionString> connectionString)
        {
            _appSettings = appSettings.Value;
            _connectionString = connectionString.Value;
            
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            User user = null;
            using (SqlConnection sqlConnection = new SqlConnection(_connectionString.UsersDB))
            {
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand("select * from Users where Username = @username and Password = @password", sqlConnection);
                sqlCommand.Parameters.Add("@username",  SqlDbType.VarChar);
                sqlCommand.Parameters["@username"].Value = model.Username;
                sqlCommand.Parameters.Add("@password",  SqlDbType.VarChar);
                sqlCommand.Parameters["@password"].Value = model.Password;

                SqlDataReader dr = sqlCommand.ExecuteReader();
                dr.Read();
                
                if(dr.HasRows)
                {
                    user = new User { Id = (int)dr["Id"], FirstName = dr["FirstName"].ToString(), LastName = dr["LastName"].ToString(), Username = dr["Username"].ToString(), Password = dr["Password"].ToString() };
                }
                sqlConnection.Close();
            }

            //var user = _users.SingleOrDefault(x => x.Username == model.Username && x.Password == model.Password);

            // return null if user not found
            if (user == null) {
                var userEmpty = new User { Id = 0, FirstName = "", LastName = "", Username = "Sin Usuario", Password = "" };
                return new AuthenticateResponse(userEmpty, "");
            }

            // authentication successful so generate jwt token
            var token = generateJwtToken(user);

            return new AuthenticateResponse(user, token);
        }

        public IEnumerable<User> GetAll()
        {
            return _users;
        }

        public User GetById(int id)
        {
            return _users.FirstOrDefault(x => x.Id == id);
        }

        public void Add(string name)
        {
            int n = _users.Count;
            _users.Add(new User(){LastName = name, Id = n});
        }
        // helper methods

        private string generateJwtToken(User user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                //Expires = DateTime.UtcNow.AddDays(7),
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}