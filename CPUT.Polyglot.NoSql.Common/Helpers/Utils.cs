namespace CPUT.Polyglot.NoSql.Common.Helpers
{
    public static class Utils
    {

        public enum Command
        {
            NONE,
            FETCH,
            MODIFY,
            ADD
        }

        public enum Database
        {
            NONE,
            REDIS,
            CASSANDRA,
            NEO4J,
            MONGODB
        }

        public static string ReadTemplate(string path)
        {
            string data = string.Empty;

            //Open the json file using a stream reader.
            using (var sr = new StreamReader(path))
            {
                // Read the stream as a string, and write the string to the console.
                data = sr.ReadToEnd();
            }

            return data;
        }
    }
}
