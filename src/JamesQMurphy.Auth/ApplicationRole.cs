namespace JamesQMurphy.Auth
{
    public class ApplicationRole
    {
        public string Name { get; set; }
        public string NormalizedName => Name.ToUpperInvariant();

        public static ApplicationRole Administrator = new ApplicationRole { Name = "Administrator" };
        public static ApplicationRole RegisteredUser = new ApplicationRole { Name = "RegisteredUser" };
    }
}
