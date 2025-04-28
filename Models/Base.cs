namespace std.Models
{
    public class Base
    {
        public class Users
        {
            public int UserId { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public bool Status { get; set; }
        }

        public class Class
        {
            public int ClassId { get; set; }
            public string Name { get; set; }
            public string Grade { get; set; }
            public int Capacity { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
