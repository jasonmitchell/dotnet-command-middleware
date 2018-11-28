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
            var pipeline = new PipelineBuilder()
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
            var pipeline = new PipelineBuilder()
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

            middlewareExecuted[0].Should().Be(1);
            middlewareExecuted[1].Should().Be(2);
            middlewareExecuted[2].Should().Be(3);
        }
        
        [Fact]
        public void ExceptionInMiddlewareBubblesUp()
        {
            var pipeline = new PipelineBuilder()
                .Use((_, __) => throw new Exception())
                .Build();

            Func<Task> action = () => pipeline(new TestInput());
            action.Should().Throw<Exception>();
        }
        
        private class TestInput{}
    }
}