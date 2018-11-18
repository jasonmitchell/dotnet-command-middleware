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
            
            var processor = new CommandProcessorBuilder()
                .Use((_, next) => next())
                .Use((_, next) => next())
                .Handle<TestCommand>(c =>
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
            var processor = new CommandProcessorBuilder().Build();
            Func<Task> action = () => processor(new TestCommand());

            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public async Task ExecutesMiddleware()
        {
            var middlewareExecuted = false;
            var processor = new CommandProcessorBuilder()
                .Use(async (_, next) =>
                {
                    middlewareExecuted = true;
                    await next();
                })
                .Handle<TestCommand>(_ => Task.CompletedTask)
                .Build();

            await processor(new TestCommand());
            middlewareExecuted.Should().BeTrue();
        }

        [Fact]
        public async Task DoesNotExecuteHandlerIfMiddlewareDoesNotExecuteNext()
        {
            var handlerExecuted = false;
            var processor = new CommandProcessorBuilder()
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

        private class TestCommand { }
    }
}