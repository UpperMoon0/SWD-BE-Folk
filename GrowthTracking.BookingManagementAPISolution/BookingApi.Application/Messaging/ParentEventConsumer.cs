﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GrowthTracking.ShareLibrary.Logs;

namespace BookingApi.Application.Messaging
{
    public class ParentEventConsumer : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;

        public ParentEventConsumer(IConfiguration configuration)
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = configuration["RabbitMQ:UserName"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "parent.events", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!_connection.IsOpen)
                    {
                        _connection.Dispose();
                        var factory = new ConnectionFactory
                        {
                            HostName = "localhost",
                            Port = 5672,
                            UserName = "guest",
                            Password = "guest"
                        };
                        _connection = factory.CreateConnection();
                        _channel = _connection.CreateModel();
                        _channel.QueueDeclare(queue: "parent.events", durable: false, exclusive: false, autoDelete: false, arguments: null);
                    }

                    var consumer = new EventingBasicConsumer(_channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var eventData = JsonSerializer.Deserialize<ParentEvent>(message);
                        if (eventData != null && eventData.EventType == "ParentCreated")
                        {
                            LogHandler.LogToConsole($"Received ParentCreated event: ParentId={eventData.ParentId}");
                        }
                    };
                    _channel.BasicConsume(queue: "parent.events", autoAck: true, consumer: consumer);
                    break;
                }
                catch (Exception ex)
                {
                    LogHandler.LogExceptions(ex);
                    Thread.Sleep(5000); // Retry after 5 seconds
                }
            }
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }

    public class ParentEvent
    {
        public Guid ParentId { get; set; }
        public string FullName { get; set; }
        public string EventType { get; set; }
    }
}