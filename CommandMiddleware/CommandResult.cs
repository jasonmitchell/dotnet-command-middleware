namespace CommandMiddleware
{
    public static class Command
    {
        public static CommandResult Rejected() => new CommandResult(false);
        public static CommandResult Rejected<TState>(TState state) => new CommandResult(false, state);
        public static CommandResult Handled() => new CommandResult(true);
        public static CommandResult Handled<TState>(TState state) => new CommandResult(true, state);
    }
    
    public class CommandResult
    {
        public bool Success { get; }
        public object State { get; }

        internal CommandResult(bool success)
        {
            Success = success;
        }

        internal CommandResult(bool success, object state)
        {
            Success = success;
            State = state;
        }
    }
}