using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using Microsoft.SqlServer.Server;
using ProtoBuf.Data;

namespace ProtoBuf.Transport.Sql
{
    public class StoredProcedures
    {
        private const string ContextConnectionString = "context connection=true";

        [SqlProcedure]
        public static void ReadTransportFile(
            [SqlFacet(MaxSize = 255, IsNullable = false)] SqlString fullFileName,
            [SqlFacet(MaxSize = 255, IsNullable = false)] SqlString serverName,
            [SqlFacet(MaxSize = 255, IsNullable = false)] SqlString databaseName,
            [SqlFacet(MaxSize = 255, IsNullable = false)] SqlString tableName,
            [SqlFacet(MaxSize = 255, IsNullable = false)] SqlString userName,
            [SqlFacet(MaxSize = 255, IsNullable = false)] SqlString password)
        {
            if (fullFileName.IsNull) throw new ArgumentNullException("fullFileName");
            if (tableName.IsNull) throw new ArgumentNullException("serverName");
            if (tableName.IsNull) throw new ArgumentNullException("databaseName");
            if (tableName.IsNull) throw new ArgumentNullException("tableName");
            if (userName.IsNull) throw new ArgumentNullException("userName");
            if (password.IsNull) throw new ArgumentNullException("password");

            using (var input = new FileStream(fullFileName.Value, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var reader = new OfflineDataPackReader();
                DataPack dataPack = reader.Read(input, "RRTFv6");

                var dataPart = dataPack.DataParts[0];

                var connectionString = string.Format(@"Server={0};Database={1};User Id={2};Password={3};", serverName, databaseName, userName, password);

                using (var partStream = dataPart.GetStream())
                using (var dataReader = DataSerializer.Deserialize(partStream))
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, null))
                    {
                        bulkCopy.DestinationTableName = tableName.Value;
                        bulkCopy.BatchSize = 500;

                        bulkCopy.WriteToServer(dataReader);
                    }
                }
            }
        }

        [SqlProcedure]
        public static void WriteTransportFile(
            [SqlFacet(MaxSize = 255, IsNullable = false)] SqlString fullFileName,
            [SqlFacet(MaxSize = 255, IsNullable = false)] SqlString tableName,
            [SqlFacet(MaxSize = 255, IsNullable = true)] SqlString fields)
        {
            if (fullFileName.IsNull) throw new ArgumentNullException("fullFileName");
            if (tableName.IsNull) throw new ArgumentNullException("tableName");

            string fieldFilter = "*";
            if (!fields.IsNull)
                fieldFilter = fields.Value;

            var commandText = string.Format("SELECT {0} FROM {1}", fieldFilter, tableName.Value);
            var streamContainer = new DataStreamContainer(ContextConnectionString, commandText);
            var dataPart = new DataPart(streamContainer);

            var dataPack = new DataPack("RRTFv6");
            dataPack.Description = "Экспериментальная выгрузка данных";

            dataPack.DataParts.Add(dataPart);

            using (var output = new FileStream(fullFileName.Value, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                TransportSerializer.Serialize(dataPack, output);
            }
        }


        ////[SqlProcedure]
        ////public static void WriteTransportFile(
        ////    [SqlFacet(MaxSize = 255, IsNullable = false)] SqlString fullFileName,
        ////    [SqlFacet(MaxSize = 255, IsNullable = false)] SqlString tableName)
        ////{
        ////    if (fullFileName.IsNull) throw new ArgumentNullException("fullFileName");
        ////    if (tableName.IsNull) throw new ArgumentNullException("fullFileName");

        ////    using (var connection = new SqlConnection(ContextConnectionString))
        ////    {
        ////        connection.Open();

        ////        using (var command = new SqlCommand(string.Format("SELECT * FROM {0}", tableName.Value), connection))
        ////        using (var reader = command.ExecuteReader())
        ////        using (var output = new FileStream(fullFileName.Value, FileMode.Create, FileAccess.Write, FileShare.None))
        ////        {
        ////            DataSerializer.Serialize(output, reader);
        ////        }
        ////    }
        ////}
    }
}
