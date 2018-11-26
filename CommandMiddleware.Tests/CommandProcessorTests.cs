using System;
using System.Collections.Generic;
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
            var processor = new CommandProcessorBuilder()
                .Use((_, next) => next())
                .Build();
            
            Func<Task> action = () => processor(new TestCommand());

            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ThrowsExceptionWhenRegisteringDuplicateCommandHandlers()
        {
            var builder = new CommandProcessorBuilder()
                .Handle<TestCommand>(_ => Task.CompletedTask);

            Action action = () => builder.Handle<TestCommand>(_ => Task.CompletedTask);
            action.Should().Throw<Exception>();
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
        public async Task ExecutesMiddlewareInOrderOfRegistration()
        {
            var middlewareExecuted = new List<int>();
            var processor = new CommandProcessorBuilder()
                .Use(async (_, next) =>
                {
                    middlewareExecuted.Add(1);
                    await next();
                })
                .Use(async (_, next) =>
                {
                    middlewareExecuted.Add(2);
                    await next();
                })
                .Use(async (_, next) =>
                {
                    middlewareExecuted.Add(3);
                    await next();
                })
                .Handle<TestCommand>(_ => Task.CompletedTask)
                .Build();

            await processor(new TestCommand());

            middlewareExecuted[0].Should().Be(1);
            middlewareExecuted[1].Should().Be(2);
            middlewareExecuted[2].Should().Be(3);
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

        [Fact]
        public void ExceptionInMiddlewareBubblesUp()
        {
            var processor = new CommandProcessorBuilder()
                .Use((_, __) => throw new Exception())
                .Handle<TestCommand>(_ => Task.CompletedTask)
                .Build();

            Func<Task> action = () => processor(new TestCommand());
            action.Should().Throw<Exception>();
        }
        
        [Fact]
        public void ExceptionInHandlerBubblesUp()
        {
            var processor = new CommandProcessorBuilder()
                .Handle<TestCommand>(_ => throw new Exception())
                .Build();

            Func<Task> action = () => processor(new TestCommand());
            action.Should().Throw<Exception>();
        }

        private class TestCommand { }
    }
}