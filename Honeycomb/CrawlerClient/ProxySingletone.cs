﻿using System;
using System.ServiceModel;
using System.Windows;
using Honeycomb;

namespace CrawlerClient
{

    #region Public enums/event args

    public enum CallBackType
    {
        Receive,
        ReceiveWhisper,
        UserEnter,
        UserLeave
    };

    public class ProxyEventArgs : EventArgs
    {
        public ClientCrawlerInfo[] list;
    }

    /// <summary>
    ///     Proxy callback event args
    /// </summary>
    public class ProxyCallBackEventArgs : EventArgs
    {
        /// <summary>
        ///     Callback type <see cref="CallBackType">CallBackType</see>
        /// </summary>
        public CallBackType callbackType;

        /// <summary>
        ///     The incoming message
        /// </summary>
        public string message = "";

        public ClientCrawlerInfo person = null;
    }

    #endregion

    #region ConnectionSingleton class

    /// <summary>
    ///     Provides a thread safe singleton which deals with
    ///     This class implements <see cref="IChatCallback">IChatCallback</see> as such
    ///     implementation details are found here for the expected <see cref="IChatCallback">IChatCallback</see>
    ///     interface methods
    ///     This class also provides 2 events for subscribers to hook to, namely
    ///     OnProxyCallBackEvent / OnProxyEvent.
    /// </summary>
    public sealed class ConnectionSingleton : IRemoteCrawlerCallback
    {
        #region Instance Fields

        public delegate void ProxyCallBackEventHandler(object sender, ProxyCallBackEventArgs e);

        public delegate void ProxyEventHandler(object sender, ProxyEventArgs e);

        private static readonly Lazy<ConnectionSingleton> lazySingleton =
            new Lazy<ConnectionSingleton>(() => new ConnectionSingleton());

        private RemoteCrawlerClient proxy;

        //main proxy event

        public event ProxyEventHandler ProxyEvent;
        //callback proxy event

        public event ProxyCallBackEventHandler ProxyCallBackEvent;

        #endregion

        #region Ctor

        /// <summary>
        ///     Blank constructor
        /// </summary>
        private ConnectionSingleton()
        {
        }

        #endregion

        #region Public Methods

        #region IChatCallback implementation

        /// <summary>
        ///     Recieve a message from a fellow chatter, so call the
        ///     internal Receive() method passing it these input parameters
        ///     and the <see cref="CallBackType">CallBackType.Receive</see>
        ///     type
        /// </summary>
        public void Receive(ClientCrawlerInfo sender, string message)
        {
            Receive(sender, message, CallBackType.Receive);
        }

        /// <summary>
        ///     Recieve a message from a fellow chatter, so call the
        ///     internal Receive() method passing it these input parameters
        ///     and the <see cref="CallBackType">CallBackType.ReceiveWhisper</see>
        ///     type
        /// </summary>
        /// <param name="sender">The <see cref="Common.Person">current chatter</see></param>
        /// <param name="message">The message</param>
        public void StartCrawling(ClientCrawlerInfo sender, string message)
        {
            //Here we crawl. Crawl and crawl.


           //And then we want to return results
            Receive(sender, message, CallBackType.ReceiveWhisper);
        }

        /// <summary>
        ///     New chatter entered chat room, so call the
        ///     internal UserEnterLeave() method passing it the input parameters
        ///     and the <see cref="CallBackType">CallBackType.UserEnter</see>
        ///     type
        /// </summary>
        /// <param name="sender">The <see cref="Common.Person">current chatter</see></param>
        /// <param name="message">The message</param>
        public void UserEnter(ClientCrawlerInfo person)
        {
            UserEnterLeave(person, CallBackType.UserEnter);
        }

        /// <summary>
        ///     An existing chatter left chat room, so call the
        ///     internal UserEnterLeave() method passing it the input parameters
        ///     and the <see cref="CallBackType">CallBackType.UserLeave</see>
        ///     type
        /// </summary>
        /// <param name="sender">The <see cref="Common.Person">current chatter</see></param>
        /// <param name="message">The message</param>
        public void UserLeave(ClientCrawlerInfo person)
        {
            UserEnterLeave(person, CallBackType.UserLeave);
        }

        /// <summary>
        ///     Calls by either the Receive() or ReceiveWhisper() <see cref="IChatCallback">IChatCallback</see>
        ///     method implementations, and simply raises the OnProxyCallBackEvent() event
        ///     to any subscribers
        /// </summary>
        /// <param name="sender">The <see cref="Common.Person">current chatter</see></param>
        /// <param name="message">The message</param>
        /// <param name="callbackType">
        ///     Could be <see cref="CallBackType">CallBackType.Receive</see> or
        ///     <see cref="CallBackType">CallBackType.ReceiveWhisper</see>
        /// </param>
        private void Receive(ClientCrawlerInfo sender, string message, CallBackType callbackType)
        {
            var e = new ProxyCallBackEventArgs();
            e.message = message;
            e.callbackType = callbackType;
            e.person = sender;
            OnProxyCallBackEvent(e);
        }

        /// <summary>
        ///     Calls by either the UserEnter() or UserLeave() <see cref="IChatCallback">IChatCallback</see>
        ///     method implementations, and simply raises the OnProxyCallBackEvent() event
        ///     to any subscribers
        /// </summary>
        /// <param name="sender">The <see cref="Common.Person">current chatter</see></param>
        /// <param name="message">The message</param>
        /// <param name="callbackType">
        ///     Could be <see cref="CallBackType">CallBackType.UserEnter</see> or
        ///     <see cref="CallBackType">CallBackType.UserLeave</see>
        /// </param>
        private void UserEnterLeave(ClientCrawlerInfo person, CallBackType callbackType)
        {
            var e = new ProxyCallBackEventArgs();
            e.person = person;
            e.callbackType = callbackType;
            OnProxyCallBackEvent(e);
        }

        #endregion

        /// <summary>
        ///     Returns a singleton <see cref="ConnectionSingleton">ConnectionSingleton</see>
        /// </summary>
        /// <returns>a singleton <see cref="ConnectionSingleton">ConnectionSingleton</see></returns>
        public static ConnectionSingleton Instance
        {
            get { return lazySingleton.Value; }
        }

        /// <summary>
        ///     Begins an asynchronous join operation on the underlying <see cref="ChatProxy">ChatProxy</see>
        ///     which will call the OnEndJoin() method on completion
        /// </summary>
        /// <param name="p">The <see cref="Common.Person">chatter</see> to try and join with</param>
        public void Connect(ClientCrawlerInfo p)
        {
            var site = new InstanceContext(this);
            proxy = new RemoteCrawlerClient(site);

            ClientCrawlerInfo[] list = proxy.Join(p);
            HandleEndJoin(list);
            //IAsyncResult ee = proxy.JoinAsync(p);
            //ee.AsyncWaitHandle = await 
            //IAsyncResult iar = proxy.BeginJoin(p, new AsyncCallback(OnEndJoin), null);
        }

        //private void OnEndJoin(IAsyncResult iar)
        //{
        //    try
        //    {
        //        Person[] list = proxy.EndJoin(iar);
        //        HandleEndJoin(list);
        //    }
        //    catch (Exception e)
        //    {
        //        MessageBox.Show(e.Message, "Error",
        //                           MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}
        /// <summary>
        ///     Is called as a callback from the asynchronous call, so simply get the
        ///     list of <see cref="Common.Person">Chatters</see> that will
        ///     be yielded as part of the Asynch Join call
        /// </summary>
        /// <param name="iar">The asnch result</param>
        /// <summary>
        ///     If the input list is not empty, then call the
        ///     OnProxyEvent() event, to raise the event for subscribers
        /// </summary>
        /// <param name="list">The list of <see cref="Common.Person">Chatters</see> </param>
        private void HandleEndJoin(ClientCrawlerInfo[] list)
        {
            if (list == null)
            {
                MessageBox.Show("Error: List is empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ExitChatSession();
            }
            else
            {
                var e = new ProxyEventArgs();
                e.list = list;
                OnProxyEvent(e);
            }
        }

        /// <summary>
        ///     Raises the event for connected subscribers
        /// </summary>
        /// <param name="e"><see cref="ProxyCallBackEventArgs">ProxyCallBackEventArgs</see> event args</param>
        protected void OnProxyCallBackEvent(ProxyCallBackEventArgs e)
        {
            if (ProxyCallBackEvent != null)
            {
                // Invokes the delegates. 
                ProxyCallBackEvent(this, e);
            }
        }

        /// <summary>
        ///     Raises the event for connected subscribers
        /// </summary>
        /// <param name="e"><see cref="ProxyEventArgs">ProxyEventArgs</see> event args</param>
        protected void OnProxyEvent(ProxyEventArgs e)
        {
            if (ProxyEvent != null)
            {
                // Invokes the delegates. 
                ProxyEvent(this, e);
            }
        }

        /// <summary>
        ///     Will either call the Whisper/Say ChatProxy methods
        ///     passing in the required parameters.
        /// </summary>
        /// <param name="to">
        ///     The chatters name who the message
        ///     is for
        /// </param>
        /// <param name="msg">The message</param>
        /// <param name="pvt">
        ///     If true will call the ChatProxy.Whisper()
        ///     method, otherwise will call the ChatProxy.Say() method
        /// </param>
        public void SayAndClear(string to, string msg, bool pvt)
        {
            if (!pvt)
                proxy.Say(msg);
            else
                proxy.Whisper(to, msg);
        }

        /// <summary>
        ///     First calls the ChatProxy.Leave (ClientBase&ltIChat&gt.Leave())
        ///     method, and finally calls the AbortProxy() method
        /// </summary>
        public void ExitChatSession()
        {
            try
            {
                proxy.Leave();
            }
            catch
            {
            }
            finally
            {
                AbortProxy();
            }
        }

        public void AbortProxy()
        {
            if (proxy != null)
            {
                proxy.Abort();
                proxy.Close();
                proxy = null;
            }
        }

        #endregion
    }

    #endregion
}