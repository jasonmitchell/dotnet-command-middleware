namespace CommandMiddleware
{
    public interface ICommandProcessorBuilder
    {
        ICommandProcessorBuilder Use(CommandMiddleware middleware);
        ICommandProcessorBuilder Use(ContextualCommandMiddleware middleware);
        ICommandProcessorBuilder Handle<TCommand>(CommandHandler<TCommand> handler);
        ICommandProcessorBuilder Handle<TCommand>(ContextualCommandHandler<TCommand> handler);
        CommandDelegate Build();
    }
}