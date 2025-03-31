namespace Demo.Samples
{
    public class User
    {
        public string Name { get; set; } = String.Empty;
        public int Age { get; set; }
        public string[] Hobbies { get; set; } = Array.Empty<string>();
        public Address Address { get; set; } = new Address();
    }
}


