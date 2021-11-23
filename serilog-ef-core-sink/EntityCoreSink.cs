 public class EntityCoreSink : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;
        private readonly IApplicationBuilder _applicationBuilder;
        private readonly JsonFormatter _jsonFormmatter;
        private readonly object _lock = new object();

        public EntityCoreSink(IApplicationBuilder applicationBuilder, IFormatProvider formatProvider)
        {
            this._formatProvider = formatProvider;
            this._applicationBuilder = applicationBuilder ?? throw new ArgumentNullException(nameof(applicationBuilder));
            this._jsonFormmatter = new JsonFormatter(formatProvider: formatProvider);
        }

        public void Emit(LogEvent logEvent)
        {
            lock (_lock)
            {
                if (logEvent == null)
                    return;

                try
                {
                    using (var serviceScope = this._applicationBuilder.ApplicationServices.CreateScope())
                    {
                        var context = serviceScope.ServiceProvider.GetService<IStGateIdentityDbContext>();
                        context.IdentityServerLog.Add(this.CreateIdentityLog(logEvent));
                        context.SaveChanges();
                    }
                }
                catch (Exception error)
                {
                    Console.WriteLine("Erro ao criar log: " + error.Message);
                }
            }
        }

        private IdentityServerLog CreateIdentityLog(LogEvent logEvent)
        {
            string jsonSource = this.ConvertLogEventToJson(logEvent);

            JObject jObject = JObject.Parse(jsonSource);
            JToken properties = jObject["Properties"];

            return new IdentityServerLog()
            {
                Exception = logEvent.Exception?.ToString(),
                Level = logEvent.Level.ToString(),
                LogEvent = jsonSource,
                Message = this._formatProvider == null ? null : logEvent.RenderMessage(this._formatProvider),
                MessageTemplate = logEvent.MessageTemplate?.ToString(),
                TimeStamp = DateTime.Now,
                Properties = properties.ToString()
            };
        }

        private string ConvertLogEventToJson(LogEvent logEvent)
        {
            if (logEvent == null)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            using (StringWriter writer = new StringWriter(sb))
            {
                this._jsonFormmatter.Format(logEvent, writer);
            }

            return sb.ToString();
        }
    }
