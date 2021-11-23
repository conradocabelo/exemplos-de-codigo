public static class EntityFrameworkCoreSinkExtensions
{
    public static LoggerConfiguration EntityFrameworkSink(
             this LoggerSinkConfiguration loggerConfiguration,
             IApplicationBuilder applicationBuilder,
             IFormatProvider formatProvider = null)
    {
        return loggerConfiguration.Sink(new EntityCoreSink(applicationBuilder, formatProvider));
    }
}
