using System;
using System.Threading.Tasks;
using MassTransit;

namespace ChannelUsage
{
    class Program
    {
        static IBusControl _serverBus;

        public static async Task Main()
        {
            _serverBus = Bus.Factory.CreateUsingRabbitMq(c =>
            {
                var host = c.Host("localhost", "/");

                c.ReceiveEndpoint(host, "channel_test", ep => { ep.Handler<Message>(cx => cx.RespondAsync(new Response())); });
            });

            await _serverBus.StartAsync();
            try
            {
                bool keepGoing = true;

                Task.Run(() =>
                {
                    Console.Write("Press enter to exist");
                    Console.ReadLine();
                    keepGoing = false;
                });

                int index = 0;
                while (keepGoing)
                {
                    await Task.Delay(100);

                    Console.WriteLine("Request {0}", ++index);
                    var client = Bus.Factory.CreateUsingRabbitMq(c =>
                    {
                        var host = c.Host("localhost", "/");
                    });

                    await client.StartAsync();
                    try
                    {
                        var request = client.CreatePublishRequestClient<Message, Response>(TimeSpan.FromSeconds(30));
                        await request.Request(new Message());
                    }
                    finally
                    {
                        await client.StopAsync();
                    }
                }
            }
            finally
            {
                await _serverBus.StopAsync();
            }
        }
    }

    class Message
    {
    }

    class Response
    {
    }
}