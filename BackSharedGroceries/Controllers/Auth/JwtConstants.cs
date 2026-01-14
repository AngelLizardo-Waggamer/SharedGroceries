namespace BackSharedGroceries.Controllers.Auth
{
    /// <summary>
    /// Static class that contains entities for the JWT configuration retrieved from environment variables.
    /// </summary>
    public static class JwtConstants
    {

        /*
            The three main constants required for JWT generation are stored in environment variables, but to avoid constant calls to look for their values they are cached
            in private static fields the first time they are requested. Hence, only the first call to each property will access the environment variables, subsequent calls will return the cached value.
        */

        private static string? _secretKey;
        private static string? _issuer;
        private static string? _audience;

        public static string SecretKey
        {
            get
            {
                if (_secretKey == null)
                {
                    _secretKey = Environment.GetEnvironmentVariable("JWT_KEY") ??
                        throw new InvalidOperationException("JWT_KEY environment variable is not set.");
                }
                return _secretKey;
            }
        }

        public static string Issuer
        {
            get
            {
                if (_issuer == null)
                {
                    _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ??
                        throw new InvalidOperationException("JWT_ISSUER environment variable is not set.");
                }
                return _issuer;
            }
        }

        public static string Audience
        {
            get
            {
                if (_audience == null)
                {
                    _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ??
                        throw new InvalidOperationException("JWT_AUDIENCE environment variable is not set.");
                }
                return _audience;
            }
        }
    }
}