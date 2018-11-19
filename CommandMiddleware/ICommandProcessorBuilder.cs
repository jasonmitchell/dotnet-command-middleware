namespace CommandMiddleware
{
    public interface ICommandProcessorBuilder
    {
        ICommandProcessorBuilder Use(CommandMiddleware middleware);
        ICommandProcessorBuilder Handle<TCommand>(CommandHandler<TCommand> handler);
        ICommandProcessorBuilder Handle<TCommand>(ContextualCommandHandler<TCommand> handler);
        //ICommandProcessorBuilder Handle<TCommand>(CommandHandler<CommandContext<TCommand>> handler);
        CommandDelegate Build();
    }
}