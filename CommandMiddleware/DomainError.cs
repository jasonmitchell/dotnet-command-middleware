namespace CommandMiddleware
{
    public readonly struct DomainError
    {
        public string Key { get; }
        public string Message { get; }

        public DomainError(string key, string message)
        {
            Key = key;
            Message = message;
        }
    }
}