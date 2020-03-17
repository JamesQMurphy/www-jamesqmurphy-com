namespace JamesQMurphy.Auth
{
    public class ApplicationRole
    {
        public const string ADMINISTRATOR = "Administrator";
        public const string REGISTERED_USER = "RegisterUser";
        public string Name { get; set; }
        public string NormalizedName => Name.ToUpperInvariant();

        public static ApplicationRole Administrator = new ApplicationRole { Name = ADMINISTRATOR };
        public static ApplicationRole RegisteredUser = new ApplicationRole { Name = REGISTERED_USER };
    }
}
