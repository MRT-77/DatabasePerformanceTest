namespace DatabasePerformanceTest.DB.MS_SQL
{
    public class PersonData
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public double Value { get; set; }

        public virtual Person Person { get; set; }
    }
}
