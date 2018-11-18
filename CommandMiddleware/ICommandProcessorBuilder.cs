namespace CommandMiddleware
{
    public interface ICommandProcessorBuilder
    {
        ICommandProcessorBuilder Use(CommandMiddleware middleware);
        ICommandProcessorBuilder Handle<TCommand>(CommandHandler<TCommand> handler);
        CommandDelegate Build();
    }
}