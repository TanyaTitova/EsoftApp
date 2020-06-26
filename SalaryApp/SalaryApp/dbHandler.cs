﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Windows;

namespace SalaryApp
{
    class dbHandler
    {
        List<int> dataId;
        Dictionary<int, string> executors;
        List<double> coeffList;


        public string GetUser(int id)
        {

            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();
            try
            {
                MySqlCommand command = new MySqlCommand("SELECT FullName FROM `users` WHERE id = '" + id + "'", conn);
                command.ExecuteNonQuery();
                using (DbDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetString(0);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            return null;

        }

        public int GetManager(int id)
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();
            try
            {
                MySqlCommand command = new MySqlCommand("SELECT Manager FROM `relationship` WHERE Performer = '" + id + "'", conn);
                command.ExecuteNonQuery();
                using (DbDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetInt32(0);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            return 0;
        }

        public Dictionary<int, string> GetExecutors(MySqlConnection conn, int id)
        {
            executors = new Dictionary<int, string>();

            MySqlCommand command = new MySqlCommand("SELECT * FROM `relationship` WHERE Manager = '" + id + "'", conn);
            command.ExecuteNonQuery();

            using (DbDataReader reader = command.ExecuteReader())
            {
                dataId = new List<int>();

                while (reader.Read())
                {
                    dataId.Add(reader.GetInt32(2));
                }
            }

            foreach (int p in dataId)
            {
                command = new MySqlCommand("SELECT * FROM `users` WHERE id = '" + p + "'", conn);
                command.ExecuteNonQuery();

                using (DbDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        executors.Add(p, reader.GetString(3));
                    }
                }
            }

            return executors;
        }

        public List<double> GetCoefficients(MySqlConnection conn, int id)
        {
            coeffList = new List<double>();
            MySqlCommand command = new MySqlCommand("SELECT * FROM `coefficients` WHERE Manager = '" + id + "'", conn);
            using (DbDataReader reader = command.ExecuteReader())
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                if (reader.Read())
                {
                    for (int i = 2; i <= 10; i++)
                    {
                        coeffList.Add(Convert.ToDouble(reader.GetString(i)));
                    }
                }
            }

            return coeffList;
        }

        public string GetGrade(MySqlConnection conn, int id)
        {
            string grade = null;
            MySqlCommand command = new MySqlCommand("SELECT * FROM `users` WHERE id = '" + id + "'", conn);
            command.ExecuteNonQuery();

            using (DbDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    grade = reader.GetString(5);
                }
            }

            return grade;

        }
    }
}
