
namespace SharpPulsar.Messages.Consumer
{
    public sealed class Unsubscribe
    {
        /// <summary>
        /// this message does not return anything
        /// but when the operation fails, the exception should be added to the BlockCollection<ClientException>
        /// so that the front end can consume and be aware - in case of no exception add null
        /// the front checks to see if it is null to know it was successfully
        /// </summary>
        /// 
        public static Unsubscribe Instance = new Unsubscribe();
    }
    
}
