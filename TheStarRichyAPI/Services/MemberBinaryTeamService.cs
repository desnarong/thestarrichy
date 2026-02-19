using BCrypt.Net;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;

namespace TheStarRichyApi.Services
{
    public interface IMemberBinaryTeamService
    {
        Task<List<dynamic>> GetDisplayAsync(string? memberCode = null);
    }
    public class MemberBinaryTeamService : IMemberBinaryTeamService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MemberBinaryTeamService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<string> GetPermissionAsync(string column, string memberCode)
        {
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");
            string MemberPermission = "";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();
                    string query = $"SELECT {column}  from M06_permission where M06_PX1=@Membercode";

                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@Membercode", memberCode);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    if (!reader.IsDBNull(0))
                                    {
                                        MemberPermission = reader.GetString(0);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Log exception in production
            }

            return MemberPermission;
        }
        public async Task<string> GetPasskeyAsync(string column)
        {
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");
            string password = "";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();
                    string query = $"SELECT {column} FROM S02";
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
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
                    }
                }
            }
            catch (Exception)
            {
                // Log exception in production
            }

            return password;
        }
        public async Task<List<dynamic>> GetDisplayAsync(string? memberCode = null)
        {
            // Get Passkey from header
            string passkey = _httpContextAccessor.HttpContext.Request.Headers["X-Passkey"];
            if (string.IsNullOrEmpty(passkey))
            {
                return new List<dynamic> { new { Membercode = "" } };
            }

            string passwordEncode1 = await GetPasskeyAsync("Passkey1");
            string passwordEncode2 = await GetPasskeyAsync("Passkey2");

            // Verify Passkey
            if (passkey != passwordEncode1 && passkey != passwordEncode2)
            {
                return new List<dynamic> { new { Membercode = "" } };
            }

            // Use provided memberCode or get from JWT
            if (string.IsNullOrEmpty(memberCode))
            {
                memberCode = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            
            if (string.IsNullOrEmpty(memberCode))
            {
                return new List<dynamic> { new { Membercode = "" } };
            }

            var result = new List<dynamic>();
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    //string Memberpermission = await GetPermissionAsync("M16", memberCode);


                    string query = "SELECT Membercode, PositionLevel, ChildCode, Membername, Sponsername, Memberposition, MemberpositionName, MemberpositionRanking";
                    query += ", MemberpositionRankingName, PersonalPV, LeftCountActive, RightCountActive, LeftBal, Rightbal, TotalBalance, CurrentLeftPV, CurrentRightPV";
                    query += ", BWDLeftPV, BWDRightPV, NewLeft, NewRight, Maxto2, TName1, TName2, EName1, EName2, Travelpoint1, travelpoint2";
                    query += ", CurrentMonthQualifyPV, LastMonthQualifyPV, LastMonthQualifyStatus, CurrentMonthQualifyStatus, FirstQdate";
                    query += ", CurrentMonth, NextCMonth, CurrentMonth1, LastCMonth, MemberPostionPicture";

                    // เพิ่ม Field ใหม่ที่ต้องการ
                    query += ", TotalLeftBalance, TotalRightBalance, NextPosition, NextPosaddLeftBalance, NextPosaddRightBalance";

                    query += " FROM [000_Member_Binary_Team] (nolock) ";
                    query += " WHERE Membercode = @Membercode";

                    using (var command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@Membercode", memberCode);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                // Create a dynamic object (ExpandoObject) to store row data
                                dynamic row = new System.Dynamic.ExpandoObject();
                                var rowDict = (IDictionary<string, object>)row;

                                // Read each column dynamically
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string columnName = reader.GetName(i);
                                    object columnValue = reader.GetValue(i);
                                    rowDict[columnName] = columnValue;
                                }

                                result.Add(row);
                            }
                        }
                    }

                    // ลบ Query ยาวๆ ที่มี UNION ALL ทิ้งไปเลยครับ แล้วใช้แค่นี้
                    string spName = "[dbo].[sp_GetMemberBinaryTree]";

                    using (var downlineCommand = new SqlCommand(spName, con))
                    {
                        // บอก C# ว่าเรากำลังเรียกใช้ Procedure นะ
                        downlineCommand.CommandType = CommandType.StoredProcedure;

                        // โยนรหัส @Membercode ส่งไปให้ Procedure ใน Database
                        downlineCommand.Parameters.AddWithValue("@Membercode", memberCode);

                        var allMembers = new List<dynamic>();

                        // พอสั่ง ExecuteReader ปุ๊บ... Procedure จะไปทำ UNION ALL ให้ข้างใน และคายผลลัพธ์ทั้งหมดกลับมาให้ทันที
                        using (var downlineReader = await downlineCommand.ExecuteReaderAsync())
                        {
                            while (await downlineReader.ReadAsync())
                            {
                                dynamic row = new System.Dynamic.ExpandoObject();
                                var rowDict = (IDictionary<string, object>)row;

                                for (int i = 0; i < downlineReader.FieldCount; i++)
                                {
                                    string columnName = downlineReader.GetName(i);
                                    object columnValue = downlineReader.GetValue(i);
                                    rowDict[columnName] = columnValue == DBNull.Value ? null : columnValue;
                                }

                                allMembers.Add(row);
                            }
                        }

                        // เอาข้อมูลไปเรียงเป็น Tree ต่อได้เลย
                        if (allMembers.Count > 0)
                        {
                            var tree = BuildTreeFromList(allMembers);

                            if (result != null && result.Count > 0)
                            {
                                dynamic mainResult = result[0];
                                var mainDict = (IDictionary<string, object>)mainResult;
                                mainDict["BinaryTree"] = tree;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception
                return new List<dynamic> { new { Membercode = "", Error = ex.Message, StackTrace = ex.StackTrace } };
            }

            return result.Count > 0 ? result : new List<dynamic> { new { Membercode = "" } };
        }

        /// <summary>
        /// สร้าง hierarchical tree structure จาก list ของ members
        /// โดยใช้ PositionLevel ในการหาความสัมพันธ์
        /// รองรับ Leftcode1, Leftcode21, Leftcode31 และ Rightcode ทุกระดับ
        /// </summary>
        private List<dynamic> BuildTreeFromList(List<dynamic> members)
        {
            var tree = new List<dynamic>();

            if (members == null || members.Count == 0)
                return tree;

            // 1. หา Root node ก่อน (PositionLevel = "Root" หรือ "Membercode")
            var rootNode = members.FirstOrDefault(m => {
                var dict = (IDictionary<string, object>)m;
                string pl = dict["PositionLevel"]?.ToString() ?? "";
                return pl == "Root" || pl == "Membercode";
            });

            if (rootNode == null && members.Count > 0)
            {
                rootNode = members[0];
            }

            if (rootNode == null) return tree;

            // เพิ่ม Children ให้ root
            var rootDict = (IDictionary<string, object>)rootNode;
            rootDict["Children"] = new List<dynamic>();

            // 2. สร้าง Dictionary สำหรับค้นหา position ทั้งหมด
            var positionDict = new Dictionary<string, dynamic>();
            
            // เพิ่ม root เข้า dict
            string rootPosition = rootDict["PositionLevel"]?.ToString() ?? "Root";
            positionDict[rootPosition] = rootNode;

            // 3. จัดกลุ่ม members ตาม PositionLevel
            foreach (var m in members)
            {
                var dict = (IDictionary<string, object>)m;
                string positionLevel = dict["PositionLevel"]?.ToString() ?? "";
                string memberCode = dict["Membercode"]?.ToString() ?? "";
                string childCode = dict["ChildCode"]?.ToString() ?? "";
                
                // ข้าม Root เพราามได้เพิ่มไปแล้ว
                if (positionLevel == "Root" || positionLevel == "Membercode")
                    continue;
                
                // ข้ามถ้าไม่มี positionLevel หรือไม่มี memberCode/childCode
                if (string.IsNullOrEmpty(positionLevel))
                    continue;
                
                // ถ้ามี memberCode ที่ไม่ว่าง ให้เพิ่มเข้า dict
                if (!string.IsNullOrEmpty(memberCode) && memberCode != "ว่าง/Empty")
                {
                    dict["Children"] = new List<dynamic>();
                    positionDict[positionLevel] = m;
                }
            }

            // 4. ผูก parent-child จาก PositionLevel
            // รูปแบบ: Leftcode1 -> Root, Leftcode21 -> Leftcode1, Leftcode31 -> Leftcode21
            foreach (var m in members)
            {
                var dict = (IDictionary<string, object>)m;
                string positionLevel = dict["PositionLevel"]?.ToString() ?? "";
                string memberCode = dict["Membercode"]?.ToString() ?? "";
                
                if (string.IsNullOrEmpty(positionLevel) || positionLevel == "Root" || positionLevel == "Membercode")
                    continue;
                
                // หา parent จาก PositionLevel
                string parentPosition = GetParentPosition(positionLevel);
                
                if (parentPosition != null && positionDict.ContainsKey(parentPosition))
                {
                    var parent = positionDict[parentPosition];
                    var parentDict = (IDictionary<string, object>)parent;
                    var children = (List<dynamic>)parentDict["Children"];
                    
                    // ตรวจสอบว่ายังไม่มีใน children
                    bool exists = children.Any(c => {
                        var cDict = (IDictionary<string, object>)c;
                        return cDict["PositionLevel"]?.ToString() == positionLevel;
                    });
                    
                    if (!exists)
                    {
                        children.Add(m);
                    }
                }
            }

            // 5. สร้าง empty nodes สำหรับตำแหน่งที่ว่าง
            AddEmptyNodes(rootNode, positionDict);

            tree.Add(rootNode);
            return tree;
        }

        /// <summary>
        /// หา parent PositionLevel จาก PositionLevel ปัจจุบัน
        /// Leftcode31 -> Leftcode21, Rightcode32 -> Leftcode21
        /// Leftcode21 -> Leftcode1, Rightcode22 -> Leftcode1
        /// Leftcode1 -> Root, Rightcode1 -> Root
        /// </summary>
        private string? GetParentPosition(string positionLevel)
        {
            if (string.IsNullOrEmpty(positionLevel))
                return null;

            // Leftcode31 -> Leftcode21 (เอาเลขท้าย 1 ตัวออก)
            // Leftcode21 -> Leftcode1
            // Leftcode1 -> Root
            
            if (positionLevel.StartsWith("Leftcode") || positionLevel.StartsWith("Rightcode"))
            {
                string numPart = positionLevel.Replace("Leftcode", "").Replace("Rightcode", "");
                
                if (numPart.Length > 1)
                {
                    // ตัดเลขตัวสุดท้ายออก
                    string parentNum = numPart.Substring(0, numPart.Length - 1);
                    
                    // เก็บ prefix (Left หรือ Right)
                    bool isLeft = positionLevel.StartsWith("Leftcode");
                    
                    if (parentNum.Length == 1)
                    {
                        // Level 2 -> Level 1: Leftcode21 -> Leftcode1
                        return (isLeft ? "Leftcode" : "Rightcode") + parentNum;
                    }
                    else if (parentNum.Length >= 2)
                    {
                        // Level 3 -> Level 2: Leftcode31 -> Leftcode21
                        // แปลง 31 -> 21, 32 -> 22
                        // Leftcode31 = left ของ 21, Rightcode32 = right ของ 21
                        int currentNum = int.Parse(numPart);
                        int parentNum2 = currentNum - 10; // 31 -> 21, 32 -> 22
                        
                        return (isLeft ? "Leftcode" : "Rightcode") + parentNum2;
                    }
                }
                else if (numPart == "1")
                {
                    // Level 1 -> Root
                    return "Root";
                }
            }
            
            return "Root";
        }

        /// <summary>
        /// เพิ่ม empty nodes สำหรับตำแหน่งที่ไม่มีข้อมูล
        /// เพิ่มทุกตำแหน่งที่เป็นไปได้ ตามโครงสร้างไบนารี่
        /// </summary>
        private void AddEmptyNodes(dynamic rootNode, Dictionary<string, dynamic> positionDict)
        {
            // สร้าง empty node template
            Func<string, string, dynamic> createEmptyNode = (pos, parentPos) => {
                dynamic empty = new System.Dynamic.ExpandoObject();
                var dict = (IDictionary<string, object>)empty;
                dict["Membercode"] = "";
                dict["Membername"] = "";
                dict["ChildCode"] = "ว่าง/Empty";
                dict["PositionLevel"] = pos;
                dict["ParentCode"] = parentPos;
                dict["Memberposition"] = "";
                dict["MemberpositionName"] = "";
                dict["PersonalPV"] = 0;
                dict["LeftCountActive"] = 0;
                dict["RightCountActive"] = 0;
                dict["Children"] = new List<dynamic>();
                return empty;
            };

            // Level 1 - เพิ่มเสมอถ้าไม่มี
            if (!positionDict.ContainsKey("Leftcode1"))
            {
                var rootDict = (IDictionary<string, object>)rootNode;
                var children = (List<dynamic>)rootDict["Children"];
                // สร้าง placeholder node สำหรับ Leftcode1
                dynamic left1Node = createEmptyNode("Leftcode1", "Root");
                positionDict["Leftcode1"] = left1Node;
                children.Add(left1Node);
            }

            if (!positionDict.ContainsKey("Rightcode1"))
            {
                var rootDict = (IDictionary<string, object>)rootNode;
                var children = (List<dynamic>)rootDict["Children"];
                // สร้าง placeholder node สำหรับ Rightcode1
                dynamic right1Node = createEmptyNode("Rightcode1", "Root");
                positionDict["Rightcode1"] = right1Node;
                children.Add(right1Node);
            }

            // Level 2 - เพิ่มให้ครบทุกตำแหน่ง
            var level2Positions = new[] { 
                ("Leftcode21", "Leftcode1"), 
                ("Rightcode22", "Leftcode1"), 
                ("Leftcode23", "Rightcode1"), 
                ("Rightcode24", "Rightcode1") 
            };
            
            foreach (var (pos, parentPos) in level2Positions)
            {
                if (!positionDict.ContainsKey(pos))
                {
                    // หา parent - ถ้าไม่มีให้สร้าง parent placeholder ก่อน
                    if (!positionDict.ContainsKey(parentPos))
                    {
                        dynamic parentNode = createEmptyNode(parentPos, "Root");
                        var rootDict = (IDictionary<string, object>)rootNode;
                        var rootChildren = (List<dynamic>)rootDict["Children"];
                        rootChildren.Add(parentNode);
                        positionDict[parentPos] = parentNode;
                    }
                    
                    var parent = positionDict[parentPos];
                    var parentDict = (IDictionary<string, object>)parent;
                    var children = (List<dynamic>)parentDict["Children"];
                    
                    dynamic newNode = createEmptyNode(pos, parentPos);
                    positionDict[pos] = newNode;
                    children.Add(newNode);
                }
            }

            // Level 3 - เพิ่มให้ครบทุกตำแหน่ง
            var level3Positions = new[] { 
                ("Leftcode31", "Leftcode21"), ("Rightcode32", "Leftcode21"),
                ("Leftcode33", "Rightcode22"), ("Rightcode34", "Rightcode22"),
                ("Leftcode35", "Leftcode23"), ("Rightcode36", "Leftcode23"),
                ("Leftcode37", "Rightcode24"), ("Rightcode38", "Rightcode24")
            };
            
            foreach (var (pos, parentPos) in level3Positions)
            {
                if (!positionDict.ContainsKey(pos))
                {
                    // หา parent - ถ้าไม่มีให้สร้าง parent placeholder ก่อน
                    if (!positionDict.ContainsKey(parentPos))
                    {
                        dynamic parentNode = createEmptyNode(parentPos, GetParentPosition(parentPos) ?? "Root");
                        var rootDict = (IDictionary<string, object>)rootNode;
                        
                        // หา parent ของ parent แล้วเพิ่ม
                        string grandparentPos = GetParentPosition(parentPos) ?? "Root";
                        if (positionDict.ContainsKey(grandparentPos))
                        {
                            var gp = positionDict[grandparentPos];
                            var gpDict = (IDictionary<string, object>)gp;
                            var gpChildren = (List<dynamic>)gpDict["Children"];
                            gpChildren.Add(parentNode);
                        }
                        positionDict[parentPos] = parentNode;
                    }
                    
                    var parent = positionDict[parentPos];
                    var parentDict = (IDictionary<string, object>)parent;
                    var children = (List<dynamic>)parentDict["Children"];
                    
                    dynamic newNode = createEmptyNode(pos, parentPos);
                    positionDict[pos] = newNode;
                    children.Add(newNode);
                }
            }
        }
    }
}
