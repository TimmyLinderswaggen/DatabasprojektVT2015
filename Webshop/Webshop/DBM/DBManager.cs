﻿using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Webshop.App_Start;

namespace Webshop.DBM
{
    public class DBManager
    {
        private bool ConnectionOpen { get; set; }

        private MySqlConnection Connection { get; set; }

        public DBManager()
        {
            //GetConnection();
            CreateTablesIfNotExists();
        }

        private void CreateTablesIfNotExists()
        {

            if (File.Exists(HostingEnvironment.MapPath(@"~/Content/tables.sql")))
            {
                var cmd = CreateCmd();
                String tables = File.ReadAllText(HostingEnvironment.MapPath(@"~/Content/tables.sql"));
                cmd.CommandText = tables;
                ExecuteQueryAndClose(cmd);
            }
        }

        public void ExecuteQueryAndClose(MySqlCommand cmd)
        {
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                string MessageString = "The following error occurred loading the Column details: "
                    + e.ErrorCode + " - " + e.Message;
            }
            finally
            {
                SafeClose();
            }
        }

        public T Read<T>(MySqlCommand cmd)
        {
            MySqlDataReader reader = cmd.ExecuteReader();

            var list = new List<Dictionary<string, object>>();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.GetName(i), reader.IsDBNull(i) ? null : reader.GetValue(i));
                }

                list.Add(row);
            }

            reader.Close();

            if (list.Count == 1)
            {
                return list[0].GetObject<T>();
            }

            var def = default(T);
            if (def is ValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
            {
                throw new InvalidOperationException(
                    string.Format("Cannot instantiate with non-nullable type: {0}",
                        typeof(T)));
            }
            return def;
        }

        public List<T> ReadList<T>(MySqlCommand cmd)
        {
             MySqlDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();

                var list = new List<Dictionary<string, object>>();
                while (reader.Read())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row.Add(reader.GetName(i), reader.IsDBNull(i) ? null : reader.GetValue(i));
                    }

                    list.Add(row);
                }

                List<T> results = new List<T>();
                foreach (var item in list)
                {
                    results.Add(item.GetObject<T>());
                }

                return results;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }

        }

        protected MySqlCommand CreateCmd()
        {
            // TODO ERROR
            return GetConnection().CreateCommand();
        }

        protected MySqlCommand CreateCmdTransaction()
        {
            MySqlCommand cmd = CreateCmd();
            cmd.Transaction = Connection.BeginTransaction();
            return cmd;
        }

        protected void SafeClose()
        {
            try
            {
                Connection.Close();
                ConnectionOpen = false;
                Console.WriteLine("closed connection");
            }
            catch (Exception e)
            {
            }
        }

        private MySqlConnection GetConnection()
        {
            if (ConnectionOpen)
            {
                Console.WriteLine("BEWARE: connection was requsted and already opened");
                return Connection;
            }

            ConnectionOpen = false;

            Connection = new MySqlConnection();
            Connection.ConnectionString = ConfigurationManager.ConnectionStrings["ShopConnection"].ConnectionString;

            if (OpenLocalConnection())
            {
                ConnectionOpen = true;
                Console.WriteLine("opened connection");
                return Connection;
            }
            else
            {
                return null;
                throw new Exception("Please Open a connection first");
            }
        }

        private bool OpenLocalConnection()
        {
            try
            {
                Connection.Open();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }



       /* ~DBManager()
        {
            Console.WriteLine("Closed SQL Connection");
            Connection.Close();
        }*/

    }
}
