using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitWebApp.AppSettings;
using RabbitWebApp.Extensions;
using RabbitWebApp.ViewModels;
using System.Net.Mime;
using System.Text;

namespace RabbitWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly RabbitMQConfig _config;
        private readonly ConnectionFactory _connectionFactory;

        public MessagesController(IOptions<RabbitMQConfig> options)
        {
            _config = options.Value;
            _connectionFactory = new ConnectionFactory { HostName = _config.Host };
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Post([FromBody] EmailViewModel email)
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // Método QueueDeclare: cria uma fila, e é indepotente.
                // Isto é, ele só vai criar a fila em caso de sua inexistência,
                // não realizando nada caso já exista. 
                channel.QueueDeclare(
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

                var messageBytes = Encoding.UTF8.GetBytes(email.ToJson());

                // Método BasicPublish: realiza a publicação da mensagem (que está em formato Array de Bytes),
                // passando o exchange padrão, e como Routing Key o nome da fila.
                channel.BasicPublish(
                    // Exchange: são agentes responsáveis por rotear as mensagens para filas,
                    // utilizando atributos de cabeçalho, routing keys, ou bindings.
                    exchange: "",
                    // Routing Key: funciona como um relacionamento entre um Exchange e uma fila,
                    // descrevendo para qual fila a mensagem deve ser direcionada.
                    routingKey: _config.Queue,
                    basicProperties: null,
                    body: messageBytes);
            }

            return Accepted();
        }
    }
}