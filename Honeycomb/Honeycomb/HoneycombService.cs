// Reference added to make WCF happen
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Honeycomb
{

    /// <summary>
    /// GPH Quick Message Service Operations
    /// </summary>
    [ServiceContract(
    Name = "HoneycombService",
    Namespace = "Honeycomb",
SessionMode = SessionMode.Required,
    CallbackContract = typeof(IMessageServiceCallback))]

    public interface IMessageServiceInbound
    {
        [OperationContract]
        int JoinTheConversation(string userName);
        [OperationContract(IsOneWay = true)]
        void ReceiveMessage(string userName, List<string> addressList, string userMessage);
        [OperationContract]
        int LeaveTheConversation(string userName);

    }

    public interface IMessageServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void NotifyUserJoinedTheConversation(string userName);

        [OperationContract(IsOneWay = true)]
        void NotifyUserOfMessage(string userName, String userMessage);

        [OperationContract(IsOneWay = true)]
        void NotifyUserLeftTheConversation(string userName);
    }

    /// <summary>
    /// GPH Quick Message Service behaviour
    /// </summary>
    /// <remarks>
    /// Single is the default concurrency mode, but it never hurts to be explicit.
    /// Note the single threading model still functions properly with use of one way methods for callbacks 
    /// since they are marked as one way on the contract.
    /// </remarks>
    [ServiceBehavior(
            ConcurrencyMode = ConcurrencyMode.Single,
            InstanceContextMode = InstanceContextMode.PerCall)]

    public class HoneycombService : IMessageServiceInbound
    {
        private static List<IMessageServiceCallback> _callbackList = new List<IMessageServiceCallback>();
        //  number of current users - 0 to begin with
        private static int _registeredUsers = 0;

        public int JoinTheConversation(string userName)
        {
            // Subscribe the user to the conversation
            IMessageServiceCallback registeredUser = OperationContext.Current.GetCallbackChannel<IMessageServiceCallback>();

            if (!_callbackList.Contains(registeredUser))
            {
                _callbackList.Add(registeredUser);
            }

            _callbackList.ForEach(
                delegate(IMessageServiceCallback callback)
                {
                    callback.NotifyUserJoinedTheConversation(userName);
                    _registeredUsers++;
                });

            return _registeredUsers;
        }

        public void ReceiveMessage(string userName, List<string> addressList, string userMessage)
        {

            // Notify the users of a message.
            // Use an anonymous delegate and generics to do our dirty work.
            _callbackList.ForEach(
                delegate(IMessageServiceCallback callback)
                { callback.NotifyUserOfMessage(userName, userMessage); });
        }

        public int LeaveTheConversation(string userName)
        {
            // Unsubscribe the user from the conversation.      
            IMessageServiceCallback registeredUser = OperationContext.Current.GetCallbackChannel<IMessageServiceCallback>();

            if (_callbackList.Contains(registeredUser))
            {
                _callbackList.Remove(registeredUser);
                _registeredUsers--;
            }

            // Notify everyone that user has arrived.
            // Use an anonymous delegate and generics to do our dirty work.
            _callbackList.ForEach(
                delegate(IMessageServiceCallback callback)
                { callback.NotifyUserLeftTheConversation(userName); });

            return _registeredUsers;
        }

    }
}
