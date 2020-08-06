using System.Collections.Generic;

namespace DatabasePerformanceTest.DB.MS_SQL
{
    public class Person
    {
        public Person()
        {
            PersonData = new HashSet<PersonData>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<PersonData> PersonData { get; set; }
    }
}
