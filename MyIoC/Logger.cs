namespace MyIoC
{
    [Export]
    public class Logger
    {
          [Import]
          public LoggerInjection Injection { get; set; }
    }

    [Export]
    public class LoggerInjection
    {
    }  
}
