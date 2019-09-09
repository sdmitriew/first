using MyFirstProject.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyFiratProject.Dal
{
    public class Repository
    {
        private readonly AppSettings _appSettings;
        public Repository(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public void Get()
        {
            var connection = new  Npgsql.NpgsqlConnection(_appSettings.DbConnection);
            var cmd = new Npgsql.NpgsqlCommand("select * from tokens", connection);
            connection.Open();
            var reader =  cmd.ExecuteReader();
            connection.Close();

        }
    }
}
