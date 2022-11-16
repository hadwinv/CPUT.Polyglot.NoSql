namespace CPUT.Polyglot.NoSql.Models._data.prep
{
    public class UDataset
    {
        public List<UFaculty> Faculties { get; set; }
        public List<UStudent> Students { get; set; }

        public UDataset()
        {
            Faculties = new List<UFaculty>();
            Students = new List<UStudent>();
        }
    }
}
