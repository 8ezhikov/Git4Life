using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Validation;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Windows;
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
        //callback interface for clients
        ICrawlerClientCallback callback = null;
        private ObservableCollection<ICrawlerClientCallback> _connectedCrawlersCallbacks = new ObservableCollection<ICrawlerClientCallback>();
        private ConcurrentStack<Seed> globalSeedStack = new ConcurrentStack<Seed>();  
        //delegate used for BroadcastEvent
        public delegate void CrawlerClientEventHandler(object sender, CrawlEventArgs e);
        public static event CrawlerClientEventHandler ChatEvent;
        private CrawlerClientEventHandler myEventHandler = null;
        //holds a list of chatters, and a delegate to allow the BroadcastEvent to work
        //out which chatter delegate to invoke
        static ConcurrentDictionary<ClientCrawlerInfo, CrawlerClientEventHandler> chatters = new ConcurrentDictionary<ClientCrawlerInfo, CrawlerClientEventHandler>();

        public ObservableCollection<ClientCrawlerInfo> ConnectedClientCrawlers
        {
            get { return _connectedClientCrawlers; }
            set { _connectedClientCrawlers = value; }
        }

        public ObservableCollection<ICrawlerClientCallback> ConnectedCrawlersCallbacks
        {
            get { return _connectedCrawlersCallbacks; }
            set { _connectedCrawlersCallbacks = value; }
        }

        private ObservableCollection<ClientCrawlerInfo> _connectedClientCrawlers = new ObservableCollection<ClientCrawlerInfo>();

        #endregion
        #region Helpers

        public void SaveSeed(Seed emp)
        {
            var dbContext = new CrawlerEntities();

            dbContext.Seeds.Add(emp);
            dbContext.SaveChanges();
        }

        public bool StartCrawling()
        {
            var dbContext = new CrawlerEntities();

            globalSeedStack.PushRange(dbContext.Seeds.ToArray());
            foreach (var crawlerCallback in ConnectedCrawlersCallbacks)
            {

                Seed nextSeed;
                if (globalSeedStack.TryPop(out nextSeed))
                {
                    crawlerCallback.StartCrawling(nextSeed.SeedDomainName);
                }
                else
                {
                    throw new Exception("Couldn't pop from stack");
                }

            }
            return true;

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
            ConnectedClientCrawlers.Add(clientCrawlerNewInfo);
            bool userAdded = false;
            //this.clientCrawlerInfo = clientCrawlerNewInfo;
            
               var  tempCallback = OperationContext.Current.GetCallbackChannel<ICrawlerClientCallback>();
            ConnectedCrawlersCallbacks.Add(tempCallback);
               // CrawlEventArgs e = new CrawlEventArgs();
               // e.msgType = MessageType.UserEnter;
               // e.clientCrawlerInfo = this.clientCrawlerInfo;
               //// BroadcastMessage(e);
               // //add this newly joined chatters CrawlerClientEventHandler delegate, to the global
               // //multicast delegate for invocation
               // ChatEvent += myEventHandler;
               // var list = new ClientCrawlerInfo[chatters.Count];
               // //carry out a critical section that copy all chatters to a new list
               // lock (syncObj)
               // {
               //     chatters.Keys.CopyTo(list, 0);
               // }
                return ConnectedClientCrawlers.ToArray();
         
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
            //CrawlEventArgs e = new CrawlEventArgs();
            //e.msgType = MessageType.Receive;
            //e.clientCrawlerInfo = this.clientCrawlerInfo;
            //e.message = msg;
            //BroadcastMessage(e);



        }

        public void ReturnCrawlingResults(CrawlerResultsDTO resultsDto)
        {
            SaveClientResultsToDatabase(resultsDto);
        }

        private void SaveClientResultsToDatabase(CrawlerResultsDTO resultsDto)
        {

            var dbContext = new CrawlerEntities();
            foreach (var internalLink in resultsDto.InternalLinksList)
            {
                var dbLink = ConvertInternalLinkDTOtoDB(internalLink);
                dbContext.InternalLinks.Add(dbLink);
            }
            foreach (var externalLink in resultsDto.ExternalLinksList)
            {
                var dbLink = ConvertExternalLinkDTOtoDB(externalLink);
                dbContext.ExternalLinks.Add(dbLink);
            }
            foreach (var badLink in resultsDto.BadLinksList)
            {
                var dbLink = ConvertBadLinkDTOtoDB(badLink);
                dbContext.BadLinks.Add(dbLink);
            }
            try
            {
                dbContext.SaveChanges();

            }

            catch (DbEntityValidationException e)
            {
                var errorString = "";
                foreach (var eve in e.EntityValidationErrors)
                {
                    errorString += String.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errorString += String.Format("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }

                MessageBox.Show(errorString);
                throw;
            }

        }

        private InternalLink ConvertInternalLinkDTOtoDB(InternalLinkDTO internalLinkDto)
        {
            var internalLinkDB = new InternalLink();
            internalLinkDB.PageSeedLink = internalLinkDto.PageSeedLink;
            internalLinkDB.PageIdSeedSpecific = internalLinkDto.PageIdSeedSpecific;
            internalLinkDB.IsProcessed = internalLinkDto.IsProcessed;
            internalLinkDB.PageLevel = internalLinkDto.PageLevel;
            internalLinkDB.PageLink = internalLinkDto.PageLink;
            internalLinkDB.IsHtml = internalLinkDto.IsHtml;
            internalLinkDB.OriginalPageLink = internalLinkDto.OriginalPageLink;
            internalLinkDB.LinkCount = internalLinkDto.LinkCount;
            return internalLinkDB;
        }


        private ExternalLink ConvertExternalLinkDTOtoDB(ExternalLinkDTO linkDto)
        {
            var linkDB = new ExternalLink();

          
            linkDB.PageSeedLink = linkDto.PageSeedLink;
            linkDB.LinkAnchor = linkDto.LinkAnchor;
            linkDB.LinkPath = linkDto.LinkPath;
            linkDB.LinkWeight = linkDto.LinkWeight;
            linkDB.OriginalPageLevel = linkDto.OriginalPageLevel;
            linkDB.OriginalPageLink = linkDto.OriginalPageLink;
            linkDB.LinkCount = linkDto.LinkCount;
            return linkDB;
        }
        private BadLink ConvertBadLinkDTOtoDB(BadLinkDTO linkDto)
        {
            var linkDB = new BadLink();


            linkDB.LinkPath = linkDto.LinkPath;
            linkDB.OriginalPageLink = linkDto.OriginalPageLink;
            return linkDB;
        }

        public void Leave()
        {
            //if (this.clientCrawlerInfo == null)
            //    return;

            ////get the chatters CrawlerClientEventHandler delegate
            //CrawlerClientEventHandler chatterToRemove = getPersonHandler(this.clientCrawlerInfo.ClientName);

            ////carry out a critical section, that removes the chatter from the
            ////internal list of chatters
            ////lock (syncObj)
            ////{
            ////    chatters.TryRemove(this.clientCrawlerInfo, chatterToRemove);
            ////}
            ////unwire the chatters delegate from the multicast delegate, so that 
            ////it no longer gets invokes by globally broadcasted methods
            //ChatEvent -= chatterToRemove;
            //CrawlEventArgs e = new CrawlEventArgs();
            //e.msgType = MessageType.UserLeave;
            //e.clientCrawlerInfo = this.clientCrawlerInfo;
            //this.clientCrawlerInfo = null;
            ////broadcast this leave message to all other remaining connected
            ////chatters
            //BroadcastMessage(e);
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
                        callback.StartCrawling(e.message);
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

