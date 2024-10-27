using GeekShopping.PaymentProcessor.Interface;

namespace GeekShopping.PaymentProcessor
{
    public class ProcessPayment : IProcessPayment
    {
        public bool PaymentProcessor()
        {
            return true;
        }
    }
}
