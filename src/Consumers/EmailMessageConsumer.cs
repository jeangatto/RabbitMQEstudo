using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitWebApp.AppSettings;
using RabbitWebApp.Extensions;
using RabbitWebApp.Services;
using RabbitWebApp.ViewModels;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitWebApp.Consumers
{
    public class EmailMessageConsumer : BackgroundService
    {
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMQConfig _config;

        public EmailMessageConsumer(IOptions<RabbitMQConfig> options, IServiceProvider serviceProvider)
        {
            _config = options.Value;
            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory { HostName = _config.Host };
            var connection = factory.CreateConnection();

            _channel = connection.CreateModel();

            // Método QueueDeclare: cria uma fila, e é indepotente.
            // Isto é, ele só vai criar a fila em caso de sua inexistência,
            // não realizando nada caso já exista. 
            _channel.QueueDeclare(
                queue: _config.Queue,
                // Durável: se sim, metadados dela são armazenados no disco e poderão ser recuperados
                // após o reinício do nó do RabbitMQ. Além disso, em caso de mensagens persistentes,
                // estas são restauradas após o reinício do nó junto a fila durável.
                // Em caso de uma mensagem persistente em uma fila não-durável,
                // ela ainda será perdida após reiniciar o RabbitMQ.
                durable: false,
                // Exclusiva: se sim, apenas uma conexão será permitida a ela,
                // e após esta encerrar, a fila é apagada.
                exclusive: false,
                // Auto-Deletar: se sim, a fila vai ser apagada caso, após um consumer ter se conectado,
                // todos se desconectaram e ela ficar sem conexões ativas.
                autoDelete: false);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // EventingBasicConsumer: responsável pela configuração dos eventos relacionados ao consumo,
            // e pelo início efetivo deles.
            var consumer = new EventingBasicConsumer(_channel);

            // Received: onde se tem acesso à mensagem recebida na fila a ser especificada,
            // através da propriedade eventArgs.Body
            consumer.Received += (sender, eventArgs) =>
            {
                var contentArray = eventArgs.Body.ToArray();
                var contentString = Encoding.UTF8.GetString(contentArray);
                var message = contentString.FromJson<EmailViewModel>();

                Notify(message);

                // Realização do Ack, que reconhece a mensagem como entregue.
                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };

            // Início do consumo, utilizando como parâmetros a fila especificada na configuração,
            // o reconhecimento automático de entrega (autoAck) como falso,
            // e o objeto consumer de tipo EventingBasicConsumer
            _channel.BasicConsume(_config.Queue, false, consumer);
            return Task.CompletedTask;
        }

        private void Notify(EmailViewModel model)
        {
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();
            service.NewEmailAsync(model.From, model.To, model.Title);
        }
    }
}
