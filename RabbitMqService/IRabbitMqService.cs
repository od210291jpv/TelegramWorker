namespace RabbitMqService
{
    public interface IRabbitMqService
    {
        Task SendMessage(string message, string queue);

        Task SendMessage(object obj, string queue);
    }
}
