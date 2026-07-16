namespace Agent
{
    public enum RequestType
    {
        Animation,
        Voice,
        Balloon,
        Move,
        Gesture,
        Show,
        Hide
    }

    public class Request
    {
        public required RequestType Type { get; set; }
        public object? Data { get; set; }
    }
}
