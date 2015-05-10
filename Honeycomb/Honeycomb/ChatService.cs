using System;
using System.Collections.Generic;
using System.ServiceModel;
using Honeycomb.Interfaces;

namespace Honeycomb
{
   
   
    #region Public enums/event args
    /// <summary>
    /// A simple enumeration for dealing with the chat message types
    /// </summary>
    public enum MessageType { Receive, UserEnter, UserLeave, ReceiveWhisper };

    /// <summary>
    /// This class is used when carrying out any of the 4 chat callback actions
    /// such as Receive, StartCrawling, UserEnter, UserLeave <see cref="ICrawlerClientCallback">
    /// ICrawlerClientCallback</see> for more details
    /// </summary>
    public class CrawlEventArgs : EventArgs
    {
        public MessageType msgType;
        public ClientCrawlerInfo clientCrawlerInfo;
        public string message;
    }
    #endregion
    #region RemoteCrawlerService
    /// <summary>
    /// This class provides the service that is used by all clients. This class
    /// uses the bindings as specified in the App.Config, to allow a true peer-2-peer
    /// chat to be perfomed.
    /// 
    /// This class also implements the <see cref="IRemoteCrawler">IRemoteCrawler</see> interface in order
    /// to facilitate a common chat interface for all chat clients
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class RemoteCrawlerService : IRemoteCrawler
    {
        #region Instance fields
        //thread sync lock object
        private static Object syncObj = new Object();
        //callback interface for clients
        ICrawlerClientCallback callback = null;
        //delegate used for BroadcastEvent
        public delegate void CrawlerClientEventHandler(object sender, CrawlEventArgs e);
        public static event CrawlerClientEventHandler ChatEvent;
        private CrawlerClientEventHandler myEventHandler = null;
        //holds a list of chatters, and a delegate to allow the BroadcastEvent to work
        //out which chatter delegate to invoke
        static Dictionary<ClientCrawlerInfo, CrawlerClientEventHandler> chatters = new Dictionary<ClientCrawlerInfo, CrawlerClientEventHandler>();
        //current ClientCrawlerInfo 
        private ClientCrawlerInfo clientCrawlerInfo;
        #endregion
        #region Helpers
        /// <summary>
        /// Searches the intenal list of chatters for a particular ClientCrawlerInfo, and returns
        /// true if the ClientCrawlerInfo could be found
        /// </summary>
        /// <param ClientName="ClientCrawlerInfo">the ClientName of the <see cref="Common.Person">ClientCrawlerInfo</see> to find</param>
        /// <returns>True if the <see cref="Common.Person">ClientCrawlerInfo</see> was found in the
        /// internal list of chatters</returns>
        private bool checkIfPersonExists(string name)
        {
            foreach (ClientCrawlerInfo p in chatters.Keys)
            {
                if (p.ClientName.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Searches the intenal list of chatters for a particular ClientCrawlerInfo, and returns
        /// the individual chatters CrawlerClientEventHandler delegate in order that it can be
        /// invoked
        /// </summary>
        /// <param ClientName="ClientCrawlerInfo">the ClientName of the <see cref="Common.Person">ClientCrawlerInfo</see> to find</param>
        /// <returns>The True CrawlerClientEventHandler delegate for the <see cref="Common.Person">ClientCrawlerInfo</see> who matched
        /// the ClientName input parameter</returns>
        private CrawlerClientEventHandler getPersonHandler(string name)
        {
            foreach (ClientCrawlerInfo p in chatters.Keys)
            {
                if (p.ClientName.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    CrawlerClientEventHandler crawlerClientTo = null;
                    chatters.TryGetValue(p, out crawlerClientTo);
                    return crawlerClientTo;
                }
            }
            return null;
        }

        ///// <summary>
        ///// Searches the intenal list of chatters for a particular ClientCrawlerInfo, and returns
        ///// the actual <see cref="Common.Person">ClientCrawlerInfo</see> whos ClientName matches
        ///// the ClientName input parameter
        ///// </summary>
        ///// <param ClientName="ClientCrawlerInfo">the ClientName of the <see cref="Common.Person">ClientCrawlerInfo</see> to find</param>
        ///// <returns>The actual <see cref="Common.Person">ClientCrawlerInfo</see> whos ClientName matches
        ///// the ClientName input parameter</returns>
        //private ClientCrawlerInfo getPerson(string name)
        //{
        //    foreach (ClientCrawlerInfo p in chatters.Keys)
        //    {
        //        if (p.ClientName.Equals(name, StringComparison.OrdinalIgnoreCase))
        //        {
        //            return p;
        //        }
        //    }
        //    return null;
        //}
        #endregion
        #region IRemoteCrawler implementation

        /// <summary>
        /// Takes a <see cref="Common.Person">ClientCrawlerInfo</see> and allows them
        /// to join the chat room, if there is not already a chatter with
        /// the same ClientName
        /// </summary>
        /// <param ClientName="ClientCrawlerInfo"><see cref="Common.Person">ClientCrawlerInfo</see> joining</param>
        /// <returns>An array of <see cref="Common.Person">ClientCrawlerInfo</see> objects</returns>
        public ClientCrawlerInfo[] Join(ClientCrawlerInfo clientCrawlerInfo)
        {
            bool userAdded = false;
            //create a new CrawlerClientEventHandler delegate, pointing to the MyEventHandler() method
            myEventHandler = new CrawlerClientEventHandler(MyEventHandler);

            //carry out a critical section that checks to see if the new chatter
            //ClientName is already in use, if its not allow the new chatter to be
            //added to the list of chatters, using the ClientCrawlerInfo as the key, and the
            //CrawlerClientEventHandler delegate as the value, for later invocation
            lock (syncObj)
            {
                if (!checkIfPersonExists(clientCrawlerInfo.ClientName) && clientCrawlerInfo != null)
                { 
                    this.clientCrawlerInfo = clientCrawlerInfo;
                    chatters.Add(clientCrawlerInfo, MyEventHandler);
                    userAdded = true;
                }
            }

            //if the new chatter could be successfully added, get a callback instance
            //create a new message, and broadcast it to all other chatters, and then 
            //return the list of al chatters such that connected clients may show a
            //list of all the chatters
            if (userAdded)
            {
                callback = OperationContext.Current.GetCallbackChannel<ICrawlerClientCallback>();
                CrawlEventArgs e = new CrawlEventArgs();
                e.msgType = MessageType.UserEnter;
                e.clientCrawlerInfo = this.clientCrawlerInfo;
                BroadcastMessage(e);
                //add this newly joined chatters CrawlerClientEventHandler delegate, to the global
                //multicast delegate for invocation
                ChatEvent += myEventHandler;
                var list = new ClientCrawlerInfo[chatters.Count];
                //carry out a critical section that copy all chatters to a new list
                lock (syncObj)
                {
                    chatters.Keys.CopyTo(list, 0);
                }
                return list;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Broadcasts the input msg parameter to all connected chatters
        /// by using the BroadcastMessage() method
        /// </summary>
        /// <param ClientName="msg">The message to broadcast to all chatters</param>
        public void ReturnIntermediateResults(string msg)
        {
            CrawlEventArgs e = new CrawlEventArgs();
            e.msgType = MessageType.Receive;
            e.clientCrawlerInfo = this.clientCrawlerInfo;
            e.message = msg;
            BroadcastMessage(e);
        }

        /// <summary>
        /// Broadcasts the input msg parameter to all the <see cref="Common.Person">
        /// ClientCrawlerInfo</see> whos ClientName matches the to input parameter
        /// by looking up the ClientCrawlerInfo from the internal list of chatters
        /// and invoking their CrawlerClientEventHandler delegate asynchronously.
        /// Where the MyEventHandler() method is called at the start of the
        /// asynch call, and the EndAsync() method at the end of the asynch call
        /// </summary>
        /// <param ClientName="to">The persons ClientName to send the message to</param>
        /// <param ClientName="msg">The message to broadcast to all chatters</param>
        public void ReturnCrawlingResults(string to, string msg)
        {
            CrawlEventArgs e = new CrawlEventArgs();
            e.msgType = MessageType.ReceiveWhisper;
            e.clientCrawlerInfo = this.clientCrawlerInfo;
            e.message = msg;
            try
            {
                CrawlerClientEventHandler chatterTo;
                //carry out a critical section, that attempts to find the 
                //correct ClientCrawlerInfo in the list of chatters.
                //if a ClientCrawlerInfo match is found, the matched chatters 
                //CrawlerClientEventHandler delegate is invoked asynchronously
                lock (syncObj)
                {
                    chatterTo = getPersonHandler(to);
                    if (chatterTo == null)
                    {
                        throw new KeyNotFoundException("The ClientCrawlerInfo whos ClientName is " + to +
                                                        " could not be found");
                    }
                }
                //do a async invoke on the chatter (call the MyEventHandler() method, and the
                //EndAsync() method at the end of the asynch call
                chatterTo.BeginInvoke(this, e, new AsyncCallback(EndAsync), null);
            }
            catch (KeyNotFoundException)
            {
            }
        }

        /// <summary>
        /// A request has been made by a client to leave the chat room,
        /// so remove the <see cref="Common.Person">ClientCrawlerInfo </see>from
        /// the internal list of chatters, and unwire the chatters
        /// delegate from the multicast delegate, so that it no longer
        /// gets invokes by globally broadcasted methods
        /// </summary>
        public void Leave()
        {
            if (this.clientCrawlerInfo == null)
                return;

            //get the chatters CrawlerClientEventHandler delegate
            CrawlerClientEventHandler chatterToRemove = getPersonHandler(this.clientCrawlerInfo.ClientName);

            //carry out a critical section, that removes the chatter from the
            //internal list of chatters
            lock (syncObj)
            {
                chatters.Remove(this.clientCrawlerInfo);
            }
            //unwire the chatters delegate from the multicast delegate, so that 
            //it no longer gets invokes by globally broadcasted methods
            ChatEvent -= chatterToRemove;
            CrawlEventArgs e = new CrawlEventArgs();
            e.msgType = MessageType.UserLeave;
            e.clientCrawlerInfo = this.clientCrawlerInfo;
            this.clientCrawlerInfo = null;
            //broadcast this leave message to all other remaining connected
            //chatters
            BroadcastMessage(e);
        }
        #endregion
        #region private methods

        /// <summary>
        /// This method is called when ever one of the chatters
        /// CrawlerClientEventHandler delegates is invoked. When this method
        /// is called it will examine the events CrawlEventArgs to see
        /// what type of message is being broadcast, and will then
        /// call the correspoding method on the clients callback interface
        /// </summary>
        /// <param ClientName="sender">the sender, which is not used</param>
        /// <param ClientName="e">The CrawlEventArgs</param>
        private void MyEventHandler(object sender, CrawlEventArgs e)
        {
            try
            {
                switch (e.msgType)
                {
                    case MessageType.Receive:
                        callback.Receive(e.clientCrawlerInfo, e.message);
                        break;
                    case MessageType.ReceiveWhisper:
                        callback.StartCrawling(e.clientCrawlerInfo, e.message);
                        break;
                    //case MessageType.UserEnter:
                    //    callback.UserEnter(e.clientCrawlerInfo);
                    //    break;
                    //case MessageType.UserLeave:
                    //    callback.UserLeave(e.clientCrawlerInfo);
                    //    break;
                }
            }
            catch
            {
                Leave();
            }
        }

        /// <summary>
        ///loop through all connected chatters and invoke their 
        ///CrawlerClientEventHandler delegate asynchronously, which will firstly call
        ///the MyEventHandler() method and will allow a asynch callback to call
        ///the EndAsync() method on completion of the initial call
        /// </summary>
        /// <param ClientName="e">The CrawlEventArgs to use to send to all connected chatters</param>
        private void BroadcastMessage(CrawlEventArgs e)
        {

            CrawlerClientEventHandler temp = ChatEvent;

            //loop through all connected chatters and invoke their 
            //CrawlerClientEventHandler delegate asynchronously, which will firstly call
            //the MyEventHandler() method and will allow a asynch callback to call
            //the EndAsync() method on completion of the initial call
            if (temp != null)
            {
                foreach (CrawlerClientEventHandler handler in temp.GetInvocationList())
                {
                    handler.BeginInvoke(this, e, new AsyncCallback(EndAsync), null);
                }
            }
        }


        /// <summary>
        /// Is called as a callback from the asynchronous call, so simply get the
        /// delegate and do an EndInvoke on it, to signal the asynchronous call is
        /// now completed
        /// </summary>
        /// <param ClientName="ar">The asnch result</param>
        private void EndAsync(IAsyncResult ar)
        {
            CrawlerClientEventHandler d = null;

            try
            {
                //get the standard System.Runtime.Remoting.Messaging.AsyncResult,and then
                //cast it to the correct delegate type, and do an end invoke
                var asres = (System.Runtime.Remoting.Messaging.AsyncResult)ar;
                d = ((CrawlerClientEventHandler)asres.AsyncDelegate);
                d.EndInvoke(ar);
            }
            catch
            {
                ChatEvent -= d;
            }
        }
        #endregion
    }
    #endregion
}

 