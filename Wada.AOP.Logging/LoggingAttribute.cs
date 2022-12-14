using MethodDecorator.Fody.Interfaces;
using NLog;
using System.Reflection;

namespace Wada.AOP.Logging
{
    // Any attribute which provides OnEntry/OnExit/OnException with proper args
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Assembly | AttributeTargets.Module)]
    public class LoggingAttribute : Attribute, IMethodDecorator
    {
        private object? _instance;
        private MethodBase? _method;
        private ILogger _logger = LogManager.GetCurrentClassLogger();

        // instance, method and args can be captured here and stored in attribute instance fields
        // for future usage in OnEntry/OnExit/OnException

        public void Init(object instance, MethodBase method, object[] args)
        {
            _instance = instance;
            _method = method;
        }

        public void OnEntry()
        {
            _logger.Trace($"Class {_instance}, Method {_method?.Name} is executing");
        }

        public void OnExit()
        {
            _logger.Trace($"Class {_instance}, Method {_method?.Name} is executed");
        }

        public void OnException(Exception exception)
        {
            _logger.Error(exception, $"Class {_instance}, Method {_method?.Name} threw exception:{exception.Message}");
        }
    }
}
