using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace TheStarRichyApi.Services
{
    public interface ILoginService
    {
        Task<string> GetPasskeyAsync(string column);
        Task<(List<GetInfoMember3>, string)> GetMemberSignInAsync(string username, string password, string passkey, string ipaddress = "127.0.0.1");
    }
    public class LoginService : ILoginService
    {
        private readonly IConfiguration _configuration;

        public LoginService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string Encode(string password)
        {
            if (string.IsNullOrEmpty(password))
                return "";

            string fieldCont = password.Trim();
            string newField = "";
            int position = 0;

            while (position < fieldCont.Length)
            {
                char mChar = fieldCont[position];
                newField += (char)(mChar + 3);
                position++;
            }

            return $"123{(char)50}{newField}{(char)50}!!!!";
        }

        public async Task<string> GetPasskeyAsync(string column)
        {
            string? connectionString = _configuration.GetConnectionString("MLMConnectionString");
            string password = "";

            try
            {
                using SqlConnection con = new(connectionString);
                await con.OpenAsync();
                string query = $"SELECT {column} FROM S02";
                using SqlCommand command = new(query, con);
                using SqlDataReader reader = await command.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            password = reader.GetString(0);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Log exception in production
            }

            return password;
        }

        public async Task<(List<GetInfoMember3>, string)> GetMemberSignInAsync(string username, string password, string passkey, string ipaddress = "127.0.0.1")
        {
            string passwordEncode1 = await GetPasskeyAsync("Passkey1");
            string passwordEncode2 = await GetPasskeyAsync("Passkey2");

            var result = new List<GetInfoMember3>();
            var memberInfo = new GetInfoMember3();

            // Verify Passkey using original Encode
            if (passkey != passwordEncode1 && passkey != passwordEncode2)
            {
                memberInfo.Membercode = "";
                result.Add(memberInfo);
                return (result, null);
            }

            string? connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();
                    string query = "SELECT * FROM [000_Member_info_for_mobile] WHERE Membercode = @Username AND Password = @Password";

                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", Encode(password));

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    if (!reader.IsDBNull(reader.GetOrdinal("MemberTypeCode")))
                                    {
                                        memberInfo.MemberTypeCode = reader.GetString(reader.GetOrdinal("MemberTypeCode"));
                                        memberInfo.Membercode = reader.GetString(reader.GetOrdinal("Membercode"));
                                        memberInfo.Idcardname = reader.GetString(reader.GetOrdinal("Idcardname"));
                                        memberInfo.PresentAddress = reader.GetString(reader.GetOrdinal("PresentAddress"));
                                        memberInfo.PresentDistrict = reader.GetString(reader.GetOrdinal("PresentDistrict"));
                                        memberInfo.PresentDistrictCode = reader.GetString(reader.GetOrdinal("PresentDistrictCode"));
                                        memberInfo.PresentPostcode = reader.GetString(reader.GetOrdinal("PresentPostcode"));
                                        memberInfo.MobilePhone = reader.GetString(reader.GetOrdinal("MobilePhone"));
                                        memberInfo.Email = reader.GetString(reader.GetOrdinal("Email"));
                                        memberInfo.CountryCode = reader.GetString(reader.GetOrdinal("CountryCode"));
                                        memberInfo.Countryname = reader.GetString(reader.GetOrdinal("Countryname"));
                                        memberInfo.MemberFlag = "Y";
                                    }
                                }
                            }
                            else
                            {
                                memberInfo = new GetInfoMember3 { MemberFlag = "N" };
                            }
                        }
                    }
                }

                string token = null;
                if (memberInfo.MemberFlag == "Y")
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Name, username),
                        new Claim(ClaimTypes.NameIdentifier, memberInfo.Membercode),
                        new Claim(ClaimTypes.Role, memberInfo.MemberTypeCode)
                    };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var tokenDescriptor = new JwtSecurityToken(
                        issuer: _configuration["Jwt:Issuer"],
                        audience: _configuration["Jwt:Audience"],
                        claims: claims,
                        expires: DateTime.Now.AddDays(7),
                        signingCredentials: creds
                    );

                    token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        await con.OpenAsync();
                        string query = "Update M06 set M06_X113=M06_X113+1  where   M06_PX1=@Membercode";
                        using (SqlCommand command = new SqlCommand(query, con))
                        {
                            command.Parameters.AddWithValue("@Membercode", memberInfo.Membercode);
                            await command.ExecuteNonQueryAsync();
                        }

                        query = "insert into [LogDirectSale] ([ComName],[Message],[Username],[Computername])  Values(@username,'Login เข้าใช้งาน',@username,@ipaddress)";
                        using (SqlCommand command = new SqlCommand(query, con))
                        {
                            command.Parameters.AddWithValue("@username", username);
                            command.Parameters.AddWithValue("@ipaddress", ipaddress);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }

                result.Add(memberInfo);
                return (result, token);
            }
            catch (Exception)
            {
                // Log exception in production
                throw;
            }
        }
    }
    public class GetInfoMember3
    {
        public string Membercode { get; set; } = "";
        public string Idcardname { get; set; } = "";
        public string MemberTypeCode { get; set; } = "";
        public string PresentAddress { get; set; } = "";
        public string PresentDistrict { get; set; } = "";
        public string PresentDistrictCode { get; set; } = "";
        public string PresentPostcode { get; set; } = "";
        public string CountryCode { get; set; } = "";
        public string Countryname { get; set; } = "";
        public string MobilePhone { get; set; } = "";
        public string Email { get; set; } = "";
        public string MemberFlag { get; set; } = "";
    }
}
