using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CommandMiddleware.Tests
{
    public class PipelineTests
    {
        [Fact]
        public async Task ExecutesMiddleware()
        {
            var middlewareExecuted = false;
            var pipeline = new Pipeline()
                .Use(async (_, ctx, next) =>
                {
                    await next();
                })
                .Use(async (_, next) =>
                {
                    middlewareExecuted = true;
                    await next();
                })
                .Build();

            await pipeline(new TestInput());
            middlewareExecuted.Should().BeTrue();
        }
        
        [Fact]
        public async Task ExecutesMiddlewareInOrderOfRegistration()
        {
            var middlewareExecuted = new List<int>();
            var pipeline = new Pipeline()
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
                .Build();

            await pipeline(new TestInput());

            middlewareExecuted.Should().BeEquivalentTo(new[] {1, 2, 3});
        }
        
        [Fact]
        public async Task DoesNotContinueExecutingIfMiddlewareDoesNotExecuteNext()
        {
            var middlewareExecuted = new List<int>();
            var pipeline = new Pipeline()
                .Use(async (_, next) =>
                {
                    middlewareExecuted.Add(1);
                    await next();
                })
                .Use((_, next) =>
                {
                    middlewareExecuted.Add(2);
                    return Task.CompletedTask;
                })
                .Use(async (_, next) =>
                {
                    middlewareExecuted.Add(3);
                    await next();
                })
                .Build();

            await pipeline(new TestInput());

            middlewareExecuted.Should().BeEquivalentTo(new[] {1, 2});
        }
        
        [Fact]
        public void ExceptionInMiddlewareBubblesUp()
        {
            var pipeline = new Pipeline()
                .Use((_, __) => throw new Exception())
                .Build();

            Func<Task> action = () => pipeline(new TestInput());
            action.Should().Throw<Exception>();
        }
        
        private class TestInput{}
    }
}