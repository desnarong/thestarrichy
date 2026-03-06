using BCrypt.Net;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheStarRichyApi.Services
{
    public interface IMemberBinaryTeamService
    {
        Task<MemberBinaryTeamResponseDto> GetDisplayAsync(string? binaryCode = null);
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

        public async Task<MemberBinaryTeamResponseDto> GetDisplayAsync(string? binaryCode = null)
        {
            // Get Passkey from header
            string passkey = _httpContextAccessor.HttpContext.Request.Headers["X-Passkey"];
            if (string.IsNullOrEmpty(passkey))
            {
                return new MemberBinaryTeamResponseDto { Membercode = "" };
            }

            string passwordEncode1 = await GetPasskeyAsync("Passkey1");
            string passwordEncode2 = await GetPasskeyAsync("Passkey2");

            // Verify Passkey
            if (passkey != passwordEncode1 && passkey != passwordEncode2)
            {
                return new MemberBinaryTeamResponseDto { Membercode = "" };
            }

            // Use provided memberCode or get from JWT
            var memberCode = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(memberCode))
            {
                return new MemberBinaryTeamResponseDto { Membercode = "" };
            }

            

            MemberBinaryTeamResponseDto result = null;
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    // ===== STEP 1: SELECT * FROM MemberLevel WHERE MemberCode = @memberCode =====
                    var memberLevelList = new List<dynamic>();

                    string memberLevelQuery = "SELECT * FROM [dbo].[MemberLevel] WHERE MemberCode = @MemberCode";

                    using (var cmd = new SqlCommand(memberLevelQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@MemberCode", memberCode);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                dynamic row = new ExpandoObject();
                                var rowDict = (IDictionary<string, object>)row;

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string columnName = reader.GetName(i);
                                    object columnValue = reader.GetValue(i);
                                    rowDict[columnName] = columnValue == DBNull.Value ? null : columnValue;
                                }

                                memberLevelList.Add(row);
                            }
                        }
                    }

                    // ===== STEP 2: ถ้ามี binaryCode ให้เช็คว่ามีอยู่ใน memberLevelList หรือไม่ =====
                    bool binaryCodeExists = false;

                    if (!string.IsNullOrEmpty(binaryCode) && memberLevelList.Count > 0)
                    {
                        // เอาเฉพาะแถวแรก (ปกติ MemberCode น่าจะมีแค่ record เดียว)
                        var firstRow = memberLevelList.FirstOrDefault();

                        if (firstRow != null)
                        {
                            var dict = (IDictionary<string, object>)firstRow;

                            // Loop ผ่านทุกคอลัมน์ที่ขึ้นต้นด้วย "Member_" เพื่อหา binaryCode
                            foreach (var kvp in dict)
                            {
                                if (kvp.Key.StartsWith("Member_") && kvp.Value?.ToString() == binaryCode)
                                {
                                    binaryCodeExists = true;
                                    break;
                                }
                            }
                        }
                    }

                    // ===== STEP 3: ถ้า binaryCode ไม่มีอยู่ ให้ return ค่าเปล่า =====
                    if (binaryCode == memberCode) binaryCode = null;
                    if (!string.IsNullOrEmpty(binaryCode) && !binaryCodeExists)
                    {
                        return new MemberBinaryTeamResponseDto
                        {
                            Membercode = memberCode,
                            BinaryTree = new List<MemberBinaryNodeDto>()
                        };
                    }
                    // ============================================================
                    
                    // Query ขอมูลหลักจาก [000_Member_Binary_Team]
                    string query = "SELECT TOP 1 Membercode, PositionLevel, ChildCode, Membername, Sponsername, Memberposition, MemberpositionName, MemberpositionRanking";
                    query += ", MemberpositionRankingName, PersonalPV, LeftCountActive, RightCountActive, LeftBal, Rightbal, TotalBalance, CurrentLeftPV, CurrentRightPV";
                    query += ", BWDLeftPV, BWDRightPV, NewLeft, NewRight, Maxto2, TName1, TName2, EName1, EName2, Travelpoint1, travelpoint2";
                    query += ", CurrentMonthQualifyPV, LastMonthQualifyPV, LastMonthQualifyStatus, CurrentMonthQualifyStatus, FirstQdate";
                    query += ", CurrentMonth, NextCMonth, CurrentMonth1, LastCMonth, MemberPositionPicture";
                    query += ", TotalLeftBalance, TotalRightBalance, NextPosition, NextPosaddLeftBalance, NextPosaddRightBalance";
                    query += " FROM [000_Member_Binary_Team] (nolock) ";
                    query += " WHERE Membercode = @Membercode";

                    using (var command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@Membercode", binaryCode ?? memberCode);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                result = MapToResponseDto(reader);
                            }
                        }
                    }

                    if (result != null)
                    {
                        // เรียก Stored Procedure เพื่อดึงข้อมูล团队成员ทั้งหมด
                        string spName = "[dbo].[SP_GetMemberBinaryTree]";

                        using (var downlineCommand = new SqlCommand(spName, con))
                        {
                            downlineCommand.CommandType = CommandType.StoredProcedure;
                            downlineCommand.Parameters.AddWithValue("@Membercode", binaryCode ?? memberCode);

                            var allMembers = new List<dynamic>();

                            using (var downlineReader = await downlineCommand.ExecuteReaderAsync())
                            {
                                int rowCount = 0;
                                while (await downlineReader.ReadAsync())
                                {
                                    dynamic row = new ExpandoObject();
                                    var rowDict = (IDictionary<string, object>)row;

                                    for (int i = 0; i < downlineReader.FieldCount; i++)
                                    {
                                        string columnName = downlineReader.GetName(i);
                                        object columnValue = downlineReader.GetValue(i);
                                        rowDict[columnName] = columnValue == DBNull.Value ? null : columnValue;
                                    }

                                    allMembers.Add(row);
                                    rowCount++;
                                }
                            }

                            // สร้าง Binary Tree ที่สมบูรณ์
                            if (allMembers.Count > 0)
                            {
                                // ค้นหาสมาชิกที่มี Membercode ตรงกับ mainMemberCode
                                var mainMember = allMembers.FirstOrDefault(m =>
                                {
                                    var dict = (IDictionary<string, object>)m;
                                    return dict["Membercode"]?.ToString() == memberCode;
                                });

                                // ถ้าพบ ให้ตั้งค่า ParentCode = null
                                if (mainMember != null)
                                {
                                    var mainMemberDict = (IDictionary<string, object>)mainMember;
                                    mainMemberDict["ParentCode"] = null;
                                }

                                // สร้าง Binary Tree
                                var completeTree = BuildCompleteTree(allMembers, 3);
                                result.BinaryTree = ConvertTreeToDto(completeTree);
                            }
                            else
                            {
                                result.BinaryTree = new List<MemberBinaryNodeDto>();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception
                return new MemberBinaryTeamResponseDto { Membercode = "", Error = ex.Message };
            }

            return result ?? new MemberBinaryTeamResponseDto { Membercode = "" };
        }

        #region DataReader Mapping Methods

        private MemberBinaryTeamResponseDto MapToResponseDto(SqlDataReader reader)
        {
            return new MemberBinaryTeamResponseDto
            {
                Membercode = GetString(reader, "Membercode"),
                PositionLevel = GetString(reader, "PositionLevel"),
                ChildCode = GetString(reader, "ChildCode"),
                Membername = GetString(reader, "Membername"),
                Sponsername = GetString(reader, "Sponsername"),
                Memberposition = GetString(reader, "Memberposition"),
                MemberpositionName = GetString(reader, "MemberpositionName"),
                MemberpositionRanking = GetString(reader, "MemberpositionRanking"),
                MemberpositionRankingName = GetString(reader, "MemberpositionRankingName"),
                PersonalPV = GetDecimal(reader, "PersonalPV"),
                LeftCountActive = GetInt(reader, "LeftCountActive"),
                RightCountActive = GetInt(reader, "RightCountActive"),
                LeftBal = GetDecimal(reader, "LeftBal"),
                Rightbal = GetDecimal(reader, "Rightbal"),
                TotalBalance = GetDecimal(reader, "TotalBalance"),
                CurrentLeftPV = GetDecimal(reader, "CurrentLeftPV"),
                CurrentRightPV = GetDecimal(reader, "CurrentRightPV"),
                BWDLeftPV = GetDecimal(reader, "BWDLeftPV"),
                BWDRightPV = GetDecimal(reader, "BWDRightPV"),
                NewLeft = GetDecimal(reader, "NewLeft"),
                NewRight = GetDecimal(reader, "NewRight"),
                Maxto2 = GetDecimal(reader, "Maxto2"),
                TName1 = GetString(reader, "TName1"),
                TName2 = GetString(reader, "TName2"),
                EName1 = GetString(reader, "EName1"),
                EName2 = GetString(reader, "EName2"),
                Travelpoint1 = GetDecimal(reader, "Travelpoint1"),
                Travelpoint2 = GetDecimal(reader, "Travelpoint2"),
                CurrentMonthQualifyPV = GetDecimal(reader, "CurrentMonthQualifyPV"),
                LastMonthQualifyPV = GetDecimal(reader, "LastMonthQualifyPV"),
                LastMonthQualifyStatus = GetString(reader, "LastMonthQualifyStatus"),
                CurrentMonthQualifyStatus = GetString(reader, "CurrentMonthQualifyStatus"),
                FirstQdate = GetString(reader, "FirstQdate"),
                CurrentMonth = GetString(reader, "CurrentMonth"),
                NextCMonth = GetString(reader, "NextCMonth"),
                CurrentMonth1 = GetString(reader, "CurrentMonth1"),
                LastCMonth = GetString(reader, "LastCMonth"),
                MemberPositionPicture = GetString(reader, "MemberPositionPicture"),
                TotalLeftBalance = GetDecimal(reader, "TotalLeftBalance"),
                TotalRightBalance = GetDecimal(reader, "TotalRightBalance"),
                NextPosition = GetString(reader, "NextPosition"),
                NextPosaddLeftBalance = GetDecimal(reader, "NextPosaddLeftBalance"),
                NextPosaddRightBalance = GetDecimal(reader, "NextPosaddRightBalance")
            };
        }

        private string GetString(SqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
            }
            catch
            {
                return null;
            }
        }

        private decimal? GetDecimal(SqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal))
                    return null;

                object value = reader.GetValue(ordinal);
                return Convert.ToDecimal(value);
            }
            catch
            {
                return null;
            }
        }

        private int? GetInt(SqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal))
                    return null;

                object value = reader.GetValue(ordinal);
                return Convert.ToInt32(value);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Tree Conversion Methods

        private List<MemberBinaryNodeDto> ConvertTreeToDto(List<dynamic> tree)
        {
            var result = new List<MemberBinaryNodeDto>();
            foreach (var node in tree)
            {
                result.Add(ConvertNodeToDto(node));
            }
            return result;
        }

        private MemberBinaryNodeDto ConvertNodeToDto(dynamic node)
        {
            var dict = (IDictionary<string, object>)node;

            var dto = new MemberBinaryNodeDto
            {
                // Properties พื้นฐาน
                Membercode = GetStringValue(dict, "Membercode"),
                PositionLevel = GetStringValue(dict, "PositionLevel"),
                ChildCode = GetStringValue(dict, "ChildCode"),
                Membername = GetStringValue(dict, "Membername"),
                Sponsername = GetStringValue(dict, "Sponsername"),
                Memberposition = GetStringValue(dict, "Memberposition"),
                MemberpositionName = GetStringValue(dict, "MemberpositionName"),
                MemberpositionRanking = GetStringValue(dict, "MemberpositionRanking"),
                MemberpositionRankingName = GetStringValue(dict, "MemberpositionRankingName"),

                // ค่าตัวเลข
                PersonalPV = GetDecimalValue(dict, "PersonalPV"),
                LeftCountActive = GetIntValue(dict, "LeftCountActive"),
                RightCountActive = GetIntValue(dict, "RightCountActive"),
                LeftBal = GetDecimalValue(dict, "LeftBal"),
                Rightbal = GetDecimalValue(dict, "Rightbal"),
                TotalBalance = GetDecimalValue(dict, "TotalBalance"),
                CurrentLeftPV = GetDecimalValue(dict, "CurrentLeftPV"),
                CurrentRightPV = GetDecimalValue(dict, "CurrentRightPV"),
                BWDLeftPV = GetDecimalValue(dict, "BWDLeftPV"),
                BWDRightPV = GetDecimalValue(dict, "BWDRightPV"),
                NewLeft = GetDecimalValue(dict, "NewLeft"),
                NewRight = GetDecimalValue(dict, "NewRight"),
                Maxto2 = GetDecimalValue(dict, "Maxto2"),

                // Text fields
                TName1 = GetStringValue(dict, "TName1"),
                TName2 = GetStringValue(dict, "TName2"),
                EName1 = GetStringValue(dict, "EName1"),
                EName2 = GetStringValue(dict, "EName2"),

                // Travel points
                Travelpoint1 = GetDecimalValue(dict, "Travelpoint1"),
                Travelpoint2 = GetDecimalValue(dict, "Travelpoint2"),

                // Qualify PV
                CurrentMonthQualifyPV = GetDecimalValue(dict, "CurrentMonthQualifyPV"),
                LastMonthQualifyPV = GetDecimalValue(dict, "LastMonthQualifyPV"),
                LastMonthQualifyStatus = GetStringValue(dict, "LastMonthQualifyStatus"),
                CurrentMonthQualifyStatus = GetStringValue(dict, "CurrentMonthQualifyStatus"),
                FirstQdate = GetStringValue(dict, "FirstQdate"),

                // Month fields
                CurrentMonth = GetStringValue(dict, "CurrentMonth"),
                NextCMonth = GetStringValue(dict, "NextCMonth"),
                CurrentMonth1 = GetStringValue(dict, "CurrentMonth1"),
                LastCMonth = GetStringValue(dict, "LastCMonth"),
                MemberPositionPicture = GetStringValue(dict, "MemberPositionPicture"),

                // Balance fields
                TotalLeftBalance = GetDecimalValue(dict, "TotalLeftBalance"),
                TotalRightBalance = GetDecimalValue(dict, "TotalRightBalance"),
                NextPosition = GetStringValue(dict, "NextPosition"),
                NextPosaddLeftBalance = GetDecimalValue(dict, "NextPosaddLeftBalance"),
                NextPosaddRightBalance = GetDecimalValue(dict, "NextPosaddRightBalance"),

                // Properties เฉพาะ Node
                ParentCode = GetStringValue(dict, "ParentCode"),
                IsEmptyNode = dict.ContainsKey("IsEmptyNode") && GetBooleanValue(dict, "IsEmptyNode"),
                Children = new List<MemberBinaryNodeDto>()
            };

            if (dict.ContainsKey("Children") && dict["Children"] != null)
            {
                var children = dict["Children"] as List<dynamic>;
                if (children != null)
                {
                    foreach (var child in children)
                    {
                        dto.Children.Add(ConvertNodeToDto(child));
                    }
                }
            }

            return dto;
        }

        private string GetStringValue(IDictionary<string, object> dict, string key)
        {
            return dict.ContainsKey(key) ? dict[key]?.ToString() : null;
        }

        private decimal? GetDecimalValue(IDictionary<string, object> dict, string key)
        {
            if (dict.ContainsKey(key) && dict[key] != null)
            {
                try
                {
                    return Convert.ToDecimal(dict[key]);
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        private int? GetIntValue(IDictionary<string, object> dict, string key)
        {
            if (dict.ContainsKey(key) && dict[key] != null)
            {
                try
                {
                    return Convert.ToInt32(dict[key]);
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        private bool GetBooleanValue(IDictionary<string, object> dict, string key)
        {
            if (dict.ContainsKey(key) && dict[key] != null)
            {
                try
                {
                    return Convert.ToBoolean(dict[key]);
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        #endregion

        #region Tree Building Methods

        /// <summary>
        /// สร้าง hierarchical tree structure ที่สมบูรณ์
        /// </summary>
        private List<dynamic> BuildCompleteTree(List<dynamic> members, int maxDepth = 3)
        {
            var tree = new List<dynamic>();

            if (members == null || members.Count == 0)
                return tree;

            // สร้าง Dictionary สำหรับค้นหาสมาชิกด้วย Membercode
            var memberDict = new Dictionary<string, dynamic>();
            foreach (var m in members)
            {
                var dict = (IDictionary<string, object>)m;
                string memberCode = dict["Membercode"]?.ToString() ?? "";

                if (!string.IsNullOrEmpty(memberCode))
                {
                    dict["Children"] = new List<dynamic>();
                    memberDict[memberCode] = m;
                }
            }

            // หา Root node (ParentCode = "NULL" หรือไม่มี parent)
            dynamic rootNode = null;
            foreach (var m in members)
            {
                var dict = (IDictionary<string, object>)m;
                string parentCode = dict["ParentCode"]?.ToString() ?? "";
                string memberCode = dict["Membercode"]?.ToString() ?? "";

                if (parentCode == "NULL" || string.IsNullOrEmpty(parentCode) ||
                    parentCode == memberCode || !memberDict.ContainsKey(parentCode))
                {
                    rootNode = m;
                    break;
                }
            }

            if (rootNode == null && members.Count > 0)
            {
                rootNode = members[0];
            }

            if (rootNode == null) return tree;

            // สร้างโครงสร้าง tree โดยใช้ ParentCode
            foreach (var m in members)
            {
                var dict = (IDictionary<string, object>)m;
                string memberCode = dict["Membercode"]?.ToString() ?? "";
                string parentCode = dict["ParentCode"]?.ToString() ?? "";

                if (memberCode == parentCode || parentCode == "NULL" || string.IsNullOrEmpty(parentCode))
                    continue;

                if (memberDict.ContainsKey(parentCode))
                {
                    var parent = memberDict[parentCode];
                    var parentDict = (IDictionary<string, object>)parent;
                    var children = (List<dynamic>)parentDict["Children"];

                    bool exists = children.Any(c => {
                        var cDict = (IDictionary<string, object>)c;
                        return cDict["Membercode"]?.ToString() == memberCode;
                    });

                    if (!exists)
                    {
                        children.Add(m);
                    }
                }
            }

            // ทำให้ tree สมบูรณ์ (เพิ่ม empty nodes) จำกัด depth
            EnsureCompleteBinaryTree(rootNode, 1, maxDepth);

            // คำนวณ PV และ Balance
            CalculatePVAndBalance(rootNode);

            // เรียงลำดับลูกทั้งหมด
            SortTreeRecursively(rootNode);

            tree.Add(rootNode);
            return tree;
        }

        /// <summary>
        /// ทำให้โครงสร้าง tree สมบูรณ์ด้วย empty nodes (จำกัด depth)
        /// </summary>
        private void EnsureCompleteBinaryTree(dynamic node, int currentDepth, int maxDepth)
        {
            if (currentDepth > maxDepth)
                return;

            var dict = (IDictionary<string, object>)node;
            string positionLevel = dict["PositionLevel"]?.ToString() ?? "";

            var children = (List<dynamic>)dict["Children"];

            if (children.Count == 0)
            {
                if (ShouldHaveChildren(positionLevel, currentDepth, maxDepth))
                {
                    AddEmptyChildren(node);
                }
            }
            else
            {
                SortChildrenByPosition(children);

                bool hasLeft = children.Any(c => {
                    var cDict = (IDictionary<string, object>)c;
                    string pos = cDict["PositionLevel"]?.ToString() ?? "";
                    return pos.StartsWith("Leftcode");
                });

                bool hasRight = children.Any(c => {
                    var cDict = (IDictionary<string, object>)c;
                    string pos = cDict["PositionLevel"]?.ToString() ?? "";
                    return pos.StartsWith("Rightcode");
                });

                if (!hasLeft && currentDepth < maxDepth)
                {
                    AddEmptyChild(node, true);
                }
                if (!hasRight && currentDepth < maxDepth)
                {
                    AddEmptyChild(node, false);
                }

                SortChildrenByPosition(children);

                foreach (var child in children)
                {
                    EnsureCompleteBinaryTree(child, currentDepth + 1, maxDepth);
                }
            }
        }

        /// <summary>
        /// ตรวจสอบว่าตำแหน่งนี้ควรมีลูกหรือไม่
        /// </summary>
        private bool ShouldHaveChildren(string positionLevel, int currentDepth, int maxDepth)
        {
            if (currentDepth >= maxDepth)
                return false;

            if (string.IsNullOrEmpty(positionLevel)) return false;
            if (positionLevel == "Root") return true;

            string numPart = positionLevel.Replace("Leftcode", "").Replace("Rightcode", "");
            if (int.TryParse(numPart, out int num))
            {
                return num < 38;
            }

            return false;
        }

        /// <summary>
        /// เพิ่ม empty node ให้กับ parent
        /// </summary>
        private void AddEmptyChild(dynamic parent, bool isLeft)
        {
            var parentDict = (IDictionary<string, object>)parent;
            string parentPos = parentDict["PositionLevel"]?.ToString() ?? "";
            string parentCode = parentDict["Membercode"]?.ToString() ?? "";

            // สร้าง PositionLevel สำหรับลูก
            string childPos = GenerateChildPosition(parentPos, isLeft);

            // สร้าง empty node
            dynamic emptyNode = CreateEmptyNode(childPos, parentCode);

            var children = (List<dynamic>)parentDict["Children"];
            children.Add(emptyNode);
        }

        /// <summary>
        /// เพิ่ม empty node ทั้งสองข้าง
        /// </summary>
        private void AddEmptyChildren(dynamic node)
        {
            AddEmptyChild(node, true);
            AddEmptyChild(node, false);
        }

        /// <summary>
        /// สร้าง empty node
        /// </summary>
        private dynamic CreateEmptyNode(string positionLevel, string parentCode)
        {
            dynamic empty = new ExpandoObject();
            var dict = (IDictionary<string, object>)empty;

            dict["Membercode"] = $"EMPTY_{Guid.NewGuid().ToString().Substring(0, 8)}";
            dict["Membername"] = "(ว่าง)";
            dict["ChildCode"] = "";
            dict["PositionLevel"] = positionLevel;
            dict["ParentCode"] = parentCode;
            dict["Memberposition"] = "";
            dict["MemberpositionName"] = "";
            dict["PersonalPV"] = 0;
            dict["LeftCountActive"] = 0;
            dict["RightCountActive"] = 0;
            dict["CurrentLeftPV"] = 0;
            dict["CurrentRightPV"] = 0;
            dict["TotalBalance"] = 0;
            dict["Children"] = new List<dynamic>();
            dict["IsEmptyNode"] = true;

            return empty;
        }

        /// <summary>
        /// สร้าง PositionLevel สำหรับลูก
        /// </summary>
        private string GenerateChildPosition(string parentPosition, bool isLeft)
        {
            if (string.IsNullOrEmpty(parentPosition) || parentPosition == "Root")
            {
                return isLeft ? "Leftcode1" : "Rightcode1";
            }

            string prefix = parentPosition.StartsWith("Leftcode") ? "Leftcode" : "Rightcode";
            string numPart = parentPosition.Substring(prefix.Length);

            if (int.TryParse(numPart, out int parentNum))
            {
                int parentDepth = numPart.Length;
                int childDepth = parentDepth + 1;

                // คำนวณ offset จาก parent position
                int baseOffset = (int)Math.Pow(2, childDepth - 1) - 1;
                int childNum = childDepth * 10 + baseOffset + (parentNum % 10) * 2 + (isLeft ? 0 : 1);

                return $"{prefix}{childNum}";
            }

            return isLeft ? "Leftcode1" : "Rightcode1";
        }

        /// <summary>
        /// เรียงลำดับลูกตามตำแหน่ง Left/Right
        /// </summary>
        private void SortChildrenByPosition(List<dynamic> children)
        {
            children.Sort((a, b) => {
                var dictA = (IDictionary<string, object>)a;
                var dictB = (IDictionary<string, object>)b;

                string posA = dictA["PositionLevel"]?.ToString() ?? "";
                string posB = dictB["PositionLevel"]?.ToString() ?? "";

                return ComparePositionLevel(posA, posB);
            });
        }

        /// <summary>
        /// เปรียบเทียบตำแหน่ง
        /// </summary>
        private int ComparePositionLevel(string posA, string posB)
        {
            if (posA == posB) return 0;

            if (posA == "Root") return -1;
            if (posB == "Root") return 1;

            bool aIsLeft = posA.StartsWith("Leftcode");
            bool bIsLeft = posB.StartsWith("Leftcode");

            string aNumStr = posA.Replace("Leftcode", "").Replace("Rightcode", "");
            string bNumStr = posB.Replace("Leftcode", "").Replace("Rightcode", "");

            if (!int.TryParse(aNumStr, out int aNum)) aNum = 0;
            if (!int.TryParse(bNumStr, out int bNum)) bNum = 0;

            if (aNum != bNum)
            {
                return aNum.CompareTo(bNum);
            }

            if (aIsLeft && !bIsLeft) return -1;
            if (!aIsLeft && bIsLeft) return 1;

            return 0;
        }

        /// <summary>
        /// เรียงลำดับ tree ทั้งหมด
        /// </summary>
        private void SortTreeRecursively(dynamic node)
        {
            var dict = (IDictionary<string, object>)node;
            var children = (List<dynamic>)dict["Children"];

            SortChildrenByPosition(children);

            foreach (var child in children)
            {
                SortTreeRecursively(child);
            }
        }

        /// <summary>
        /// คำนวณ PV และ Balance
        /// </summary>
        private void CalculatePVAndBalance(dynamic node)
        {
            var dict = (IDictionary<string, object>)node;
            var children = (List<dynamic>)dict["Children"];

            double leftPV = 0;
            double rightPV = 0;
            int leftCount = 0;
            int rightCount = 0;

            foreach (var child in children)
            {
                var childDict = (IDictionary<string, object>)child;
                string childPos = childDict["PositionLevel"]?.ToString() ?? "";
                bool isLeft = childPos.StartsWith("Leftcode");

                CalculatePVAndBalance(child);

                double childPV = Convert.ToDouble(childDict["CurrentLeftPV"] ?? 0) +
                                Convert.ToDouble(childDict["CurrentRightPV"] ?? 0) +
                                Convert.ToDouble(childDict["PersonalPV"] ?? 0);

                bool isActive = Convert.ToInt32(childDict["LeftCountActive"] ?? 0) > 0 ||
                               Convert.ToInt32(childDict["RightCountActive"] ?? 0) > 0;

                if (isLeft)
                {
                    leftPV += childPV;
                    leftCount += isActive ? 1 : 0;
                }
                else
                {
                    rightPV += childPV;
                    rightCount += isActive ? 1 : 0;
                }
            }

            bool isEmptyNode = dict.ContainsKey("IsEmptyNode") && Convert.ToBoolean(dict["IsEmptyNode"] ?? false);

            if (!isEmptyNode)
            {
                dict["CurrentLeftPV"] = leftPV;
                dict["CurrentRightPV"] = rightPV;
                dict["LeftCountActive"] = leftCount;
                dict["RightCountActive"] = rightCount;
                dict["TotalBalance"] = Math.Min(leftPV, rightPV);
            }
        }

        #endregion
    }

    // DTO Classes
    // DTO Classes
    public class MemberBinaryNodeDto
    {
        // Properties จาก MemberBinaryTeamResponseDto ทั้งหมด
        public string Membercode { get; set; }
        public string PositionLevel { get; set; }
        public string ChildCode { get; set; }
        public string Membername { get; set; }
        public string Sponsername { get; set; }
        public string Memberposition { get; set; }
        public string MemberpositionName { get; set; }
        public string MemberpositionRanking { get; set; }
        public string MemberpositionRankingName { get; set; }
        public decimal? PersonalPV { get; set; }
        public int? LeftCountActive { get; set; }
        public int? RightCountActive { get; set; }
        public decimal? LeftBal { get; set; }
        public decimal? Rightbal { get; set; }
        public decimal? TotalBalance { get; set; }
        public decimal? CurrentLeftPV { get; set; }
        public decimal? CurrentRightPV { get; set; }
        public decimal? BWDLeftPV { get; set; }
        public decimal? BWDRightPV { get; set; }
        public decimal? NewLeft { get; set; }
        public decimal? NewRight { get; set; }
        public decimal? Maxto2 { get; set; }
        public string TName1 { get; set; }
        public string TName2 { get; set; }
        public string EName1 { get; set; }
        public string EName2 { get; set; }
        public decimal? Travelpoint1 { get; set; }
        public decimal? Travelpoint2 { get; set; }
        public decimal? CurrentMonthQualifyPV { get; set; }
        public decimal? LastMonthQualifyPV { get; set; }
        public string LastMonthQualifyStatus { get; set; }
        public string CurrentMonthQualifyStatus { get; set; }
        public string FirstQdate { get; set; }
        public string CurrentMonth { get; set; }
        public string NextCMonth { get; set; }
        public string CurrentMonth1 { get; set; }
        public string LastCMonth { get; set; }
        public string MemberPositionPicture { get; set; }
        public decimal? TotalLeftBalance { get; set; }
        public decimal? TotalRightBalance { get; set; }
        public string NextPosition { get; set; }
        public decimal? NextPosaddLeftBalance { get; set; }
        public decimal? NextPosaddRightBalance { get; set; }

        // Properties เฉพาะสำหรับ Node
        public string ParentCode { get; set; }
        public bool IsEmptyNode { get; set; }
        public List<MemberBinaryNodeDto> Children { get; set; }
    }

    public class MemberBinaryTeamResponseDto
    {
        public string Membercode { get; set; }
        public string PositionLevel { get; set; }
        public string ChildCode { get; set; }
        public string Membername { get; set; }
        public string Sponsername { get; set; }
        public string Memberposition { get; set; }
        public string MemberpositionName { get; set; }
        public string MemberpositionRanking { get; set; }
        public string MemberpositionRankingName { get; set; }
        public decimal? PersonalPV { get; set; }
        public int? LeftCountActive { get; set; }
        public int? RightCountActive { get; set; }
        public decimal? LeftBal { get; set; }
        public decimal? Rightbal { get; set; }
        public decimal? TotalBalance { get; set; }
        public decimal? CurrentLeftPV { get; set; }
        public decimal? CurrentRightPV { get; set; }
        public decimal? BWDLeftPV { get; set; }
        public decimal? BWDRightPV { get; set; }
        public decimal? NewLeft { get; set; }
        public decimal? NewRight { get; set; }
        public decimal? Maxto2 { get; set; }
        public string TName1 { get; set; }
        public string TName2 { get; set; }
        public string EName1 { get; set; }
        public string EName2 { get; set; }
        public decimal? Travelpoint1 { get; set; }
        public decimal? Travelpoint2 { get; set; }
        public decimal? CurrentMonthQualifyPV { get; set; }
        public decimal? LastMonthQualifyPV { get; set; }
        public string LastMonthQualifyStatus { get; set; }
        public string CurrentMonthQualifyStatus { get; set; }
        public string FirstQdate { get; set; }
        public string CurrentMonth { get; set; }
        public string NextCMonth { get; set; }
        public string CurrentMonth1 { get; set; }
        public string LastCMonth { get; set; }
        public string MemberPositionPicture { get; set; }
        public decimal? TotalLeftBalance { get; set; }
        public decimal? TotalRightBalance { get; set; }
        public string NextPosition { get; set; }
        public decimal? NextPosaddLeftBalance { get; set; }
        public decimal? NextPosaddRightBalance { get; set; }

        // Property สำหรับ Tree
        public List<MemberBinaryNodeDto> BinaryTree { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Error { get; set; }
    }
}