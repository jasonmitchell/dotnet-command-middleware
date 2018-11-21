namespace CommandMiddleware
{
    public class CommandContext
    {
        public bool RanToCompletion { get; private set; }
        public object Result { get; private set; }

        public void Complete(object result = null)
        {
            if (!RanToCompletion)
            {
                RanToCompletion = true;
                Result = result;
            }
        }
    }
}