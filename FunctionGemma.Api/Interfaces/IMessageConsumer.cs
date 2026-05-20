using System.Threading.Tasks;

namespace FunctionGemma.Api.Interfaces
{
    public interface IMessageConsumer
    {
        Task ConsumeMessages();
    }
}