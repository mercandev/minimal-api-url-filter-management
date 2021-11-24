namespace MinimalApiUrlFilter.Const
{
    public static class UrlFilterConst
    {
        public const string HTTPS = "https://";
        public const string HTTP = "http://";
        public const string URL_CACHE_KEY = "urlfilter-{0}-{1}";
        public const string SEPARETE_URL_ADDRESS_REGEX_PATTERN = @"[:/.]";
        public const string CHECK_URL_IS_VALID_REGEX_PATTERN = @"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?$";
        public const string WWW = "www";


    }
}
