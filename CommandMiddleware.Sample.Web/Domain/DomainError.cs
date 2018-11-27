namespace CommandMiddleware.Sample.Web.Domain
{
    public struct DomainError
    {
        public string Key { get; }
        public string Message { get; }

        internal DomainError(string key, string message)
        {
            Key = key;
            Message = message;
        }
    }
}