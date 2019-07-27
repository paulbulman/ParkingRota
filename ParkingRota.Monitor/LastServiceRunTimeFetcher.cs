namespace ParkingRota.Monitor
{
    using System;
    using System.Data.SqlClient;
    using Data;
    using NodaTime;

    public class LastServiceRunTimeFetcher
    {
        private readonly string connectionString;

        public LastServiceRunTimeFetcher(string connectionString) => this.connectionString = connectionString;

        public Instant Fetch()
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT TOP (1) LastServiceRunTime FROM SystemParameterLists";
                    command.CommandTimeout = (int)Duration.FromSeconds(10).TotalSeconds;

                    connection.Open();
                    var result = (DateTime)command.ExecuteScalar();
                    connection.Close();

                    return DbConvert.Instant.FromDb(result);
                }
            }
        }
    }
}