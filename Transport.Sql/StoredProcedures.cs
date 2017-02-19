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
            [SqlFacet(MaxSize = 255, IsNullable = false)] SqlString fullFileName)
        {
            if (fullFileName.IsNull) throw new ArgumentNullException("fullFileName");

        }

        [SqlProcedure]
        public static void WriteTransportFile(
            [SqlFacet(MaxSize = 255, IsNullable = false)] SqlString fullFileName,
            [SqlFacet(MaxSize = 255, IsNullable = false)] SqlString tableName)
        {
            if (fullFileName.IsNull) throw new ArgumentNullException("fullFileName");
            if (tableName.IsNull) throw new ArgumentNullException("tableName");

            using (var connection = new SqlConnection(ContextConnectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(string.Format("SELECT * FROM {0}", tableName.Value), connection))
                using (var reader = command.ExecuteReader())
                using (var output = new FileStream(fullFileName.Value, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    DataSerializer.Serialize(output, reader);
                }
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
