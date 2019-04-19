namespace MyIoC
{
    public interface ICustomerDAL
    {
    }

    [Export(typeof(ICustomerDAL))]
    public class CustomerDAL : ICustomerDAL
    {
        public CustomerDAL(CustomerDatCtorInjection injection) { }
    }

    [Export]
    public class CustomerDatCtorInjection
    {

    }
}