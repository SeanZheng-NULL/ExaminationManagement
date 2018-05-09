﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ExaminationManagement.Models
{
    /// <summary>
    /// 数据库操作
    /// </summary>
    public class SQLManager
    {
        private SqlConnection _connection;

        /// <summary>
        /// 连接数据库
        /// </summary>
        public SQLManager()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ExamDb"].ConnectionString;
            this._connection = new SqlConnection(connectionString);
        }
        #region  select
        /// <summary>
        /// 登录验证
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>验证成功返回用户类型，失败返回NotFound</returns>
        public DataBaseModels.RoleType CheckUser(string username, string password)
        {
            using (SqlCommand command = _connection.CreateCommand())
            {
                password = Encryption(password);
                command.CommandText = "select role_id from tb_users where id=@username and passwd=@password";
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();
                SqlParameter[] parameters = { new SqlParameter("@username", username), new SqlParameter("@password", password) };
                command.Parameters.AddRange(parameters);
                object reslut = command.ExecuteScalar();
                _connection.Close();
                if (reslut == null)
                    return DataBaseModels.RoleType.NotFound;
                switch (int.Parse(reslut.ToString()))
                {
                    case 0:
                        return DataBaseModels.RoleType.Admin;
                    case 1:
                        return DataBaseModels.RoleType.Teacher;
                    case 2:
                        return DataBaseModels.RoleType.Student;
                    default:
                        return DataBaseModels.RoleType.NotFound;
                }
            }
        }
        /// <summary>
        /// 获取专业列表
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DataBaseModels.Major> GetMajors()
        {
            using (SqlCommand command = _connection.CreateCommand())
            {
                command.CommandText = "select * from tb_major";
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    DataBaseModels.Major major = new DataBaseModels.Major
                    {
                        Major_id = reader.GetInt32(0),
                        MajorName = reader.GetString(1)
                    };
                    yield return major;
                }
                reader.Close();
                _connection.Close();
            }
        }

        /// <summary>
        /// 获取课程信息
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DataBaseModels.Course> GetCourses()
        {
            //string selectCommandText= @"select tb_course.course_id,tb_course.name,tb_course.grade,tb_course.credit,tb_teachinfo.name
            //        from tb_course left join tb_teachinfo 
            //        on tb_course.tea_id = tb_teachinfo.tea_id";
            SqlDataAdapter adapter = new SqlDataAdapter("select * from tb_course", _connection);
            DataTable table = new DataTable();
            adapter.Fill(table);

            foreach (DataRow item in table.Rows)
            {
                DataBaseModels.Course course = new DataBaseModels.Course
                {
                    CourseId = item.Field<int>(0),
                    CourseName = item.Field<string>(1),
                    Grade = item.Field<int?>(2),
                    Credit = item.Field<double>(3),
                    Teacher = item.Field<int?>(4)
                };
                yield return course;
            }
        }
        //
        public Dictionary<int,string> GetTeachers()
        {
            Dictionary<int, string> keyValuePairs = new Dictionary<int, string>();
            using (SqlCommand command = _connection.CreateCommand())
            {
                command.CommandText = "select distinct tea_id,name from tb_teachinfo";
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int value = reader.GetInt32(0);
                    string text = reader.GetString(1);
                    keyValuePairs.Add(value, text);
                }
                reader.Close();
                _connection.Close();
            }
            return keyValuePairs;
        }
        //未验证
        public DataBaseModels.StuInfo GetStuInfo(string studentId)
        {
            DataBaseModels.StuInfo info = null;
            using (SqlCommand command = _connection.CreateCommand())
            {
                command.CommandText = "select * from tb_stuinfo where stu_id=@studentId";
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();
                SqlParameter parameter = new SqlParameter("@studentId", studentId);
                command.Parameters.Add(parameter);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    info = new DataBaseModels.StuInfo
                    {
                        Stu_id = reader[0].ToString(),
                        Name = reader[1].ToString(),
                        Birth = reader[2].ToString(),
                        Photo = reader[3].ToString(),
                        Tel = reader[4].ToString(),
                        Email = reader[5].ToString(),
                        Major_id = reader.GetInt32(6),
                        Enroll_year = reader.GetInt32(7),
                        Credit_got = reader.GetDouble(8),
                        Credit_need = reader.GetDouble(9)
                    };
                }
                reader.Close();
                _connection.Close();
                return info;
            }
        }
        #endregion

        #region
        /// <summary>
        /// 添加专业
        /// </summary>
        /// <param name="majors"></param>
        /// <returns></returns>
        public bool AddMajors(string[] majors)
        {
            SqlDataAdapter adapter = new SqlDataAdapter("select * from tb_major", _connection);
            new SqlCommandBuilder(adapter);
            DataTable table = new DataTable();
            adapter.Fill(table);

            foreach (var item in majors)
            {
                DataRow row = table.NewRow();
                row[1] = item;
                table.Rows.Add(row);
            }
            return adapter.Update(table) > 0 ? true : false;
        }
        public bool UpdateMajors(int majorId,string majorName)
        {
            using (SqlCommand command = _connection.CreateCommand())
            {
                command.CommandText = "update tb_major set name=@majorName where major_id=@majorId";
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();
                command.Parameters.AddWithValue("@majorName", majorName);
                command.Parameters.AddWithValue("@majorId", majorId);
                int changeNumber = command.ExecuteNonQuery();
                _connection.Close();
                if (changeNumber > 0)
                    return true;
                return false;
            }
        }
        public bool DeleteMajors(int majorId)
        {
            using (SqlCommand command = _connection.CreateCommand())
            {
                command.CommandText = "delete from tb_major where major_id=@Id";
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();
                SqlParameter parameter = new SqlParameter("@Id", majorId);
                command.Parameters.Add(parameter);
                int changeNumber = command.ExecuteNonQuery();
                _connection.Close();
                if (changeNumber > 0)
                    return true;
                return false;
            }
        }
        /// <summary>
        /// 添加课程
        /// </summary>
        /// <param name="courses">课程:课程名称和学分</param>
        /// <returns></returns>
        public bool AddCourse(WebModels.Course[] courses)
        {
            SqlDataAdapter adapter = new SqlDataAdapter("select * from tb_course", _connection);
            SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
            DataTable table = new DataTable();
            adapter.Fill(table);

            foreach (var item in courses)
            {
                DataRow row = table.NewRow();
                row[1] = item.CourseName;
                row[3] = item.Credit;
                table.Rows.Add(row);
            }

            return adapter.Update(table) > 0 ? true : false;
        }
        public bool DeleteCourse(int courseId)
        {
            using (SqlCommand command = _connection.CreateCommand())
            {
                command.CommandText = "delete tb_course where course_id=@Id";
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();
                SqlParameter parameter = new SqlParameter("@Id", courseId);
                command.Parameters.Add(parameter);
                int changeNumber = command.ExecuteNonQuery();
                _connection.Close();
                if (changeNumber > 0)
                    return true;
                return false;
            }
        }

        public bool AddTeachers(WebModels.Teacher[] teachers)
        {
            SqlDataAdapter teacher_adapter = new SqlDataAdapter("select * from tb_teachinfo", _connection);
            SqlDataAdapter user_adapter = new SqlDataAdapter("select * from tb_users", _connection);
            SqlCommandBuilder teacher_builder = new SqlCommandBuilder(teacher_adapter);
            SqlCommandBuilder user_builder = new SqlCommandBuilder(user_adapter);
            DataTable tb_teachers = new DataTable();
            DataTable tb_users = new DataTable();
            teacher_adapter.Fill(tb_teachers);
            user_adapter.Fill(tb_users);

            foreach (var item in teachers)
            {
                DataRow tea = tb_teachers.NewRow();
                DataRow user = tb_users.NewRow();
                tea[0] = item.Tea_id;
                tea[1] = item.TeaName;
                tea[6] = item.MajorId;

                user[0] = item.Tea_id;
                user[1] = Encryption(item.Passwd);
                user[2] = 1;

                tb_teachers.Rows.Add(tea);
                tb_users.Rows.Add(user);
            }

            int t = teacher_adapter.Update(tb_teachers);
            int u = user_adapter.Update(tb_users);
            if (t > 0 && u > 0)
                return true;
            return false;
        }
        #endregion




        #region others
        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="text">明文</param>
        /// <returns>密文</returns>
        private string Encryption(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] encryptdata = md5.ComputeHash(Encoding.Default.GetBytes(text));
            return Convert.ToBase64String(encryptdata);
        }
        #endregion
    }
}