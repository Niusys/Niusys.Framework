using System;
using System.Text.RegularExpressions;
using RabbitMQ.Client;

namespace Niusys.Extensions.MessageQueue.RabbitMq
{
    public abstract class ConnectionSetting
    {
        #region Properties
        /// <summary>
        /// 	Gets or sets the virtual host to publish to.
        /// </summary>
        public virtual string VHost { get; set; } = "/";

        /// <summary>
        /// 	Gets or sets the username to use for
        /// 	authentication with the message broker. The default
        /// 	is 'guest'
        /// </summary>
        public virtual string UserName { get; set; } = "guest";

        /// <summary>
        /// 	Gets or sets the password to use for
        /// 	authentication with the message broker.
        /// 	The default is 'guest'
        /// </summary>
        public virtual string Password { get; set; } = "guest";

        /// <summary>
        /// 	Gets or sets the port to use
        /// 	for connections to the message broker (this is the broker's
        /// 	listening port).
        /// 	The default is '5672'.
        /// </summary>
        public virtual ushort Port { get; set; } = 5672;

        /// <summary>
        /// 	Gets or sets the AMQP protocol (version) to use
        /// 	for communications with the RabbitMQ broker. The default
        /// 	is the RabbitMQ.Client-library's default protocol.
        /// </summary>
        public IProtocol Protocol { get; set; } = Protocols.DefaultProtocol;

        /// <summary>
        /// 	Gets or sets the host name of the broker to log to.
        /// </summary>
        /// <remarks>
        /// 	Default is 'localhost'
        /// </remarks>
        public virtual string HostName { get; set; } = "localhost";

        public string ConnectionKeyName { get; set; }

        /// <summary>
        /// 	Gets or sets the exchange to bind the logger output to.
        /// </summary>
        /// <remarks>
        /// 	Default is 'log4net-logging'
        /// </remarks>
        public virtual string Exchange { get; set; } = "app-logging";

        /// <summary>
        ///   Gets or sets the exchange type to bind the logger output to.
        /// </summary>
        /// <remarks>
        ///   Default is 'topic'
        /// </remarks>
        public string ExchangeType { get; set; } = RabbitMQ.Client.ExchangeType.Topic;

        /// <summary>
        /// 	Gets or sets the setting specifying whether the exchange
        ///		is durable (persisted across restarts)
        /// </summary>
        /// <remarks>
        /// 	Default is true
        /// </remarks>
        public bool Durable { get; set; } = true;

        /// <summary>
        /// 	Gets or sets the setting specifying whether the exchange
        ///     should be declared or used passively.
        /// </summary>
        /// <remarks>
        /// 	Default is false
        /// </remarks>
        public bool Passive { get; set; }

        /// <summary>
        /// 	Gets or sets the application id to specify when sending. Defaults to null,
        /// 	and then IBasicProperties.AppId will be the name of the logger instead.
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of messages to save in the case
        /// that the RabbitMQ instance goes down. Must be >= 1. Defaults to 10240.
        /// </summary>
        public int MaxBuffer { get; set; } = 102400 * 5;

        /// <summary>
        /// Gets or sets the number of heartbeat seconds to have for the RabbitMQ connection.
        /// If the heartbeat times out, then the connection is closed (logically) and then
        /// re-opened the next time a log message comes along.
        /// </summary>
        public ushort HeartBeatSeconds { get; set; } = 30;

        public void LoadFromConnectionString(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var connString = value;
                if (ConnectionStringBuilder.IsValidConnectionString(connString))
                {
                    ConnectionStringBuilder csb = new ConnectionStringBuilder(connString);
                    this.HostName = csb.HostName;
                    this.Port = csb.Port;
                    this.UserName = csb.UserName;
                    this.Password = csb.Password;
                    if (!string.IsNullOrWhiteSpace(csb.VirtualHost))
                        this.VHost = csb.VirtualHost;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to format the data in the body as a JSON structure.
        /// Having it as a JSON structure means that you can more easily interpret the data
        /// at its final resting place, than if it were a simple string - i.e. you don't
        /// have to mess with advanced parsers if you have this options for all of your
        /// applications. A product that you can use for viewing logs
        /// generated is logstash (http://logstash.net), elasticsearch (https://github.com/elasticsearch/elasticsearch)
        /// and kibana (http://rashidkpc.github.com/Kibana/)
        /// </summary>
        public bool UseJSON { get; set; } = false;

        /// <summary>
        /// Enables SSL support to connect to the Message Queue. If this is enabled,
        /// SslCertPath and SslCertPassphrase are required! For more information please
        /// visit http://www.rabbitmq.com/ssl.html
        /// </summary>
        public bool UseSsl { get; set; } = false;

        /// <summary>
        /// Location of client SSL certificate
        /// </summary>
        public string SslCertPath { get; set; }

        /// <summary>
        /// Passphrase for generated SSL certificate defined in SslCertPath
        /// </summary>
        public string SslCertPassphrase { get; set; }

        /// <summary>
        /// The amount of milliseconds to wait when starting a connection
        /// before moving on to next task
        /// </summary>
        public int Timeout { get; set; } = 5;

        /// <summary>
        /// Gets or sets compression type.
        /// Available compression methods: None, GZip
        /// </summary>
        public CompressionTypes Compression { get; set; } = CompressionTypes.GZip;

        /// <summary>
        /// Routing Key
        /// </summary>
        public string RoutingKey { get; set; }
        #endregion

        internal class ConnectionStringBuilder
        {
            private const string ConnectionStringRegex = @"^(.+)://(.+):(.+)@(.+):(.+)/(.*)$";
            public string Portocal { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public string HostName { get; set; }
            public ushort Port { get; set; }
            public string VirtualHost { get; set; }

            public ConnectionStringBuilder(string connectionString)
            {
                InitConnectionString(connectionString);
            }

            private void InitConnectionString(string connectionString)
            {
                var match = Regex.Match(connectionString, ConnectionStringRegex);
                if (!match.Success)
                {
                    throw new InvalidCastException(string.Format("{0} is not a valid RabbitMQ connection string!", connectionString));
                }

                this.Portocal = match.Groups[1].Value;
                this.UserName = match.Groups[2].Value;
                this.Password = match.Groups[3].Value;
                this.HostName = match.Groups[4].Value;
                this.Port = ushort.Parse(match.Groups[5].Value);
                this.VirtualHost = match.Groups[6].Value;
            }

            public static bool IsValidConnectionString(string connectionString)
            {
                var match = Regex.Match(connectionString, ConnectionStringRegex);
                return match.Success;
            }

            public static ConnectionStringBuilder BuildConnectionString(string connectionString)
            {
                return new ConnectionStringBuilder(connectionString);
            }
        }
    }
}
