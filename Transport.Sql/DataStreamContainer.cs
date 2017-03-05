using System.Data.SqlClient;
using System.IO;
using ProtoBuf.Data;
using ProtoBuf.Transport.Abstract;

namespace ProtoBuf.Transport.Sql
{
    public class DataStreamContainer
        : IDataContainer
    {
        private readonly string _connectionString;
        private readonly string _commandText;

        public DataStreamContainer(string connectionString, string commandText)
        {
            _connectionString = connectionString;
            _commandText = commandText;
        }

        public Stream GetStream()
        {
            var memoryStream = new MemoryStream();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(_commandText, connection))
                using (var reader = command.ExecuteReader())
                {
                    DataSerializer.Serialize(memoryStream, reader);
                }
            }

            memoryStream.Position = 0;

            return memoryStream;
        }

        public void CopyToStream(Stream stream)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(_commandText, connection))
                using (var reader = command.ExecuteReader())
                {
                    DataSerializer.Serialize(stream, reader);
                }
            }
        }
    }
}