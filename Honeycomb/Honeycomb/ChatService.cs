using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using Honeycomb.Shared;

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
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
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
        static ConcurrentDictionary<ClientCrawlerInfo, CrawlerClientEventHandler> chatters = new ConcurrentDictionary<ClientCrawlerInfo, CrawlerClientEventHandler>();
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

        public bool StartCrawling()
        {
            if (callback != null)
            {
                callback.StartCrawling("http://webometrics.krc.karelia.ru/");
                return true;
            }
            else
            {
                return false;
            }
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

        #endregion
        #region IRemoteCrawler implementation

        public ClientCrawlerInfo[] Join(ClientCrawlerInfo clientCrawlerNewInfo)
        {
            bool userAdded = false;
                this.clientCrawlerInfo = clientCrawlerNewInfo;
                userAdded = chatters.TryAdd(clientCrawlerInfo, MyEventHandler);
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

        public ClientCrawlerInfo[] WorkaroundMethod(Shared.BadLinkDTO bl, Shared.InternalLinkDTO il, Shared.ExternalLinkDTO el, Shared.SeedDTO sd)
        {
            throw new NotImplementedException();
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

        public void ReturnCrawlingResults(CrawlerResultsDTO resultsDto)
        {
            SaveClientResultsToDatabase(resultsDto);
            var dbModel = new QuickModelDataContext();
        }

        private void SaveClientResultsToDatabase(CrawlerResultsDTO resultsDto)
        {

            var dbModel = new QuickModelDataContext();
            foreach (var internalLink in resultsDto.InternalLinksList)
            {
                InternalLink
            }
        }
      
        public void Leave()
        {
            if (this.clientCrawlerInfo == null)
                return;

            //get the chatters CrawlerClientEventHandler delegate
            CrawlerClientEventHandler chatterToRemove = getPersonHandler(this.clientCrawlerInfo.ClientName);

            //carry out a critical section, that removes the chatter from the
            //internal list of chatters
            //lock (syncObj)
            //{
            //    chatters.TryRemove(this.clientCrawlerInfo, chatterToRemove);
            //}
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
                        callback.StartCrawling( e.message);
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

 