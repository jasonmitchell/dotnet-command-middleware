namespace CommandMiddleware
{
    public class PipelineContext
    {
        public object Input { get; }

        public PipelineContext(object input)
        {
            Input = input;
        }
    }
}