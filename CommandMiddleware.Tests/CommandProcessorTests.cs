using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CommandMiddleware.Tests
{
    public class CommandProcessorTests
    {
        [Fact]
        public async Task HandlesCommand()
        {
            object handled = null;
            
            var processor = new CommandProcessor()
                .Use((_, next) => next())
                .Use((_, next) => next())
                .Handle<TestCommand>(c =>
                {
                    handled = c;
                    return Task.CompletedTask;
                })
                .Handle<OtherTestCommand>((c, ctx) =>
                {
                    handled = c;
                    return Task.CompletedTask;
                })
                .Build();
            
            var command = new TestCommand();
            await processor(command);
            
            handled.Should().Be(command);
        }

        [Fact]
        public void ThrowsExceptionIfNoHandlerForCommand()
        {
            var processor = new CommandProcessor()
                .Use((_, next) => next())
                .Build();
            
            Func<Task> action = () => processor(new TestCommand());

            action.Should().Throw<CommandHandlerNotFoundException>();
        }

        [Fact]
        public void ThrowsExceptionWhenRegisteringDuplicateCommandHandlers()
        {
            var builder = new CommandProcessor()
                .Handle<TestCommand>(_ => Task.CompletedTask);

            Action action = () => builder.Handle<TestCommand>(_ => Task.CompletedTask);
            action.Should().Throw<CommandHandlerAlreadyRegisteredException>();
        }

        [Fact]
        public async Task DoesNotExecuteHandlerIfMiddlewareDoesNotExecuteNext()
        {
            var handlerExecuted = false;
            var processor = new CommandProcessor()
                .Use((_, __) => Task.CompletedTask)
                .Handle<TestCommand>(_ =>
                {
                    handlerExecuted = true;
                    return Task.CompletedTask;
                })
                .Build();

            await processor(new TestCommand());
            handlerExecuted.Should().BeFalse();
        }
        
        [Fact]
        public void ExceptionInHandlerBubblesUp()
        {
            var processor = new CommandProcessor()
                .Handle<TestCommand>(_ => throw new Exception())
                .Build();

            Func<Task> action = () => processor(new TestCommand());
            action.Should().Throw<Exception>();
        }

        private class TestCommand { }
        private class OtherTestCommand { }
    }
}