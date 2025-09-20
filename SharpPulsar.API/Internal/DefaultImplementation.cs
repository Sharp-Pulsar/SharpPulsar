

namespace SharpPulsar.API.Internal
{
    /// <summary>
	/// This class loads the implementation for <seealso cref="PulsarClientImplementationBinding"/>
	/// and allows you to decouple the API from the actual implementation.
	/// <b>This class is internal to the Pulsar API implementation, and it is not part of the public API
	/// it is not meant to be used by client applications.</b>
	/// </summary>
	public class DefaultImplementation
    {
        private static readonly IPulsarClientImplementationBinding _defaultImplementation;
        static DefaultImplementation()
        {
            IPulsarClientImplementationBinding impl;
            try
            {
                impl = (IPulsarClientImplementationBinding)ReflectionUtils.NewClassInstance<IPulsarClientImplementationBinding>("SharpPulsar.PulsarClientImplementationBindingImpl");//.GetConstructor().newInstance();
            }
            catch (Exception error)
            {
                throw new Exception("Cannot load Pulsar Client Implementation: " + error, error);
            }
            _defaultImplementation = impl;
        }

        /// <summary>
        /// Access the actual implementation of the Pulsar Client API. </summary>
        /// <returns> the loaded implementation. </returns>
        public static IPulsarClientImplementationBinding GetDefaultImplementation
        {
            get
            {
                return _defaultImplementation;
            }
        }

    }

}
