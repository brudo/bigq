﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigQ
{
    /// <summary>
    /// Manages channels associated with BigQ.
    /// </summary>
    public class ChannelManager
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private ServerConfiguration Config;

        private readonly object ChannelsLock;
        private Dictionary<string, Channel> Channels;         // GUID, Channel
        
        #endregion

        #region Constructors

        public ChannelManager(ServerConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            Config = config;
            ChannelsLock = new object();
            Channels = new Dictionary<string, Channel>();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Retrieves a list of all channels on the server.
        /// </summary>
        /// <returns>A list of Channel objects.</returns>
        public List<Channel> GetChannels()
        {
            lock (ChannelsLock)
            {
                if (Channels == null || Channels.Count < 1)
                {
                    Log("GetChannels no channels found");
                    return null;
                }
                
                List<Channel> ret = new List<Channel>();
                foreach (KeyValuePair<string, Channel> curr in Channels)
                {
                    ret.Add(curr.Value);
                }

                Log("GetChannels returning " + ret.Count + " channel(s)");
                return ret;
            }
        }

        /// <summary>
        /// Retrieves Channel object associated with supplied GUID.
        /// </summary>
        /// <param name="guid">GUID of the channel.</param>
        /// <returns>A populated Channel object or null.</returns>
        public Channel GetChannelByGUID(string guid)
        {
            if (String.IsNullOrEmpty(guid))
            {
                Log("GetChannelByGUID null GUID supplied");
                return null;
            }

            lock (ChannelsLock)
            {
                if (Channels == null || Channels.Count < 1)
                {
                    Log("GetChannelByGUID no channels found");
                    return null;
                }

                foreach (KeyValuePair<string, Channel> curr in Channels)
                {
                    if (String.Compare(curr.Value.ChannelGUID, guid) == 0)
                    {
                        Log("GetChannelByGUID returning channel with GUID " + guid);
                        return curr.Value;
                    }
                }

                Log("GetChannelByGUID no channel found with GUID " + guid);
                return null;
            }
        }

        /// <summary>
        /// Retrieves Channel object associated with supplied name.
        /// </summary>
        /// <param name="name">Name of the channel.</param>
        /// <returns>A populated Channel object or null.</returns>
        public Channel GetChannelByName(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                Log("GetChannelByName null name supplied");
                return null;
            }

            lock (ChannelsLock)
            {
                if (Channels == null || Channels.Count < 1)
                {
                    Log("GetChannelByName no channels found");
                    return null;
                }

                foreach (KeyValuePair<string, Channel> curr in Channels)
                {
                    if (String.Compare(curr.Value.ChannelName, name) == 0)
                    {
                        Log("GetChannelByName returning channel with name " + name);
                        return curr.Value;
                    }
                }

                Log("GetChannelByName no channel found with name " + name);
                return null;
            }
        }

        /// <summary>
        /// Retrieves Client objects that are members of a Channel with supplied GUID.
        /// </summary>
        /// <param name="guid">GUID of the channel.</param>
        /// <returns>A list of Client objects or null.</returns>
        public List<Client> GetChannelMembers(string guid)
        {
            if (String.IsNullOrEmpty(guid))
            {
                Log("GetChannelMembers null GUID supplied");
                return null;
            }

            List<Client> ret = new List<Client>();

            lock (ChannelsLock)
            {
                foreach (KeyValuePair<string, Channel> curr in Channels)
                {
                    if (String.Compare(curr.Key, guid) == 0)
                    {
                        Log("GetChannelMembers found channel GUID " + guid);

                        if (curr.Value.Members != null && curr.Value.Members.Count > 0)
                        {
                            foreach (Client currClient in curr.Value.Members)
                            {
                                ret.Add(currClient);
                            }

                            Log("GetChannelMembers returning " + ret.Count + " member(s) for channel GUID " + guid);
                            return ret;
                        }

                        Log("GetChannelMembers no members found for channel GUID " + guid);
                        return null;
                    }
                }

                Log("GetChannelMembers unable to find channel GUID " + guid);
                return null;
            }
        }

        /// <summary>
        /// Retrieves Client objects that are subscribers of a Channel with supplied GUID.
        /// </summary>
        /// <param name="guid">GUID of the channel.</param>
        /// <returns>A list of Client objects or null.</returns>
        public List<Client> GetChannelSubscribers(string guid)
        {
            if (String.IsNullOrEmpty(guid))
            {
                Log("GetChannelSubscribers null GUID supplied");
                return null;
            }

            List<Client> ret = new List<Client>();

            lock (ChannelsLock)
            {
                foreach (KeyValuePair<string, Channel> curr in Channels)
                {
                    if (String.Compare(curr.Key, guid) == 0)
                    {
                        Log("GetChannelSubscribers found channel GUID " + guid);

                        if (curr.Value.Subscribers != null && curr.Value.Subscribers.Count > 0)
                        {
                            foreach (Client currClient in curr.Value.Subscribers)
                            {
                                ret.Add(currClient);
                            }

                            Log("GetChannelSubscribers returning " + ret.Count + " subscriber(s) for channel GUID " + guid);
                            return ret;
                        }

                        Log("GetChannelSubscribers no subscribers found for channel GUID " + guid);
                        return null;
                    }
                }

                Log("GetChannelSubscribers unable to find channel GUID " + guid);
                return null;
            }
        }

        /// <summary>
        /// Determines if a Client is a member of the specified Channel.
        /// </summary>
        /// <param name="currentClient">The Client.</param>
        /// <param name="currentChannel">The Channel.</param>
        /// <returns>Boolean indicating if the Client is a member of the Channel.</returns>
        public bool IsChannelMember(Client currentClient, Channel currentChannel)
        {
            if (currentClient == null)
            {
                Log("IsChannelMember null client supplied");
                return false;
            }

            if (currentChannel == null)
            {
                Log("IsChannelMember null channel supplied");
                return false;
            }

            lock (ChannelsLock)
            {
                foreach (KeyValuePair<string, Channel> curr in Channels)
                {
                    if (String.Compare(curr.Key, currentChannel.ChannelGUID) == 0)
                    {
                        Log("IsChannelMember found channel GUID " + currentChannel.ChannelGUID);

                        if (curr.Value.Members != null && curr.Value.Members.Count > 0)
                        {
                            foreach (Client currClient in curr.Value.Members)
                            {
                                if (String.Compare(currentClient.ClientGUID, currClient.ClientGUID) == 0)
                                {
                                    Log("IsChannelMember found channel GUID " + currentChannel.ChannelGUID + " member GUID " + currClient.ClientGUID);
                                    return true;
                                }
                            }
                        }

                        Log("IsChannelMember client GUID " + currentClient.ClientGUID + " is not a member of channel GUID " + currentChannel.ChannelGUID);
                        return false;
                    }
                }

                Log("IsChannelMember unable to find channel GUID " + currentChannel.ChannelGUID);
                return false;
            }
        }

        /// <summary>
        /// Determines if a Client is a subscriber of the specified Channel.
        /// </summary>
        /// <param name="currentClient">The Client.</param>
        /// <param name="currentChannel">The Channel.</param>
        /// <returns>Boolean indicating if the Client is a subscriber of the Channel.</returns>
        public bool IsChannelSubscriber(Client currentClient, Channel currentChannel)
        {
            if (currentClient == null)
            {
                Log("IsChannelSubscriber null client supplied");
                return false;
            }

            if (currentChannel == null)
            {
                Log("IsChannelSubscriber null channel supplied");
                return false;
            }

            lock (ChannelsLock)
            {
                foreach (KeyValuePair<string, Channel> curr in Channels)
                {
                    if (String.Compare(curr.Key, currentChannel.ChannelGUID) == 0)
                    {
                        Log("IsChannelSubscriber found channel GUID " + currentChannel.ChannelGUID);

                        if (curr.Value.Subscribers != null && curr.Value.Subscribers.Count > 0)
                        {
                            foreach (Client currClient in curr.Value.Subscribers)
                            {
                                if (String.Compare(currentClient.ClientGUID, currClient.ClientGUID) == 0)
                                {
                                    Log("IsChannelSubscriber found channel GUID " + currentChannel.ChannelGUID + " subscriber GUID " + currClient.ClientGUID);
                                    return true;
                                }
                            }
                        }

                        Log("IsChannelSubscriber client GUID " + currentClient.ClientGUID + " is not a subscriber of channel GUID " + currentChannel.ChannelGUID);
                        return false;
                    }
                }

                Log("IsChannelSubscriber unable to find channel GUID " + currentChannel.ChannelGUID);
                return false;
            }
        }

        /// <summary>
        /// Determines whether or not a channel exists on the server by supplied GUID.
        /// </summary>
        /// <param name="guid">The GUID of the channel.</param>
        /// <returns>Boolean indicating whether or not the channel exists on the server.</returns>
        public bool ChannelExists(string guid)
        {
            if (Channels == null || Channels.Count < 1)
            {
                Log("ChannelExists no channels found");
                return false;
            }
            
            lock (ChannelsLock)
            {
                foreach (KeyValuePair<string, Channel> curr in Channels)
                {
                    if (curr.Value != null)
                    {
                        if (!String.IsNullOrEmpty(curr.Value.ChannelGUID))
                        {
                            if (String.Compare(curr.Value.ChannelGUID, guid) == 0)
                            {
                                Log("ChannelExists found channel GUID " + guid);
                                return true;
                            }
                        }
                    }
                }

                Log("ChannelExists unable to find channel GUID " + guid);
                return false;
            }
        }

        /// <summary>
        /// Adds a Channel object to the list of channels on the server.
        /// </summary>
        /// <param name="currChannel">A populated Channel object or null.</param>
        /// <returns>Boolean indicating success.</returns>
        public bool AddChannel(Channel currChannel)
        {
            if (currChannel == null)
            {
                Log("AddChannel null channel supplied");
                return false;
            }

            lock (ChannelsLock)
            {
                foreach (KeyValuePair<string, Channel> curr in Channels)
                {
                    if (String.Compare(curr.Key, currChannel.ChannelGUID) == 0)
                    {
                        Log("AddChannel channel GUID " + currChannel.ChannelGUID + " already exists");
                        return false;
                    }
                }

                Channels.Add(currChannel.ChannelGUID, currChannel);
                Log("AddChannel added channel " + currChannel.ChannelGUID);
                return true;
            }
        }

        /// <summary>
        /// Adds a Client to a Channel as a member.
        /// </summary>
        /// <param name="currChannel">The Channel.</param>
        /// <param name="currClient">The Client.</param>
        /// <returns>Boolean indicating success or failure.</returns>
        public bool AddChannelMember(Channel currChannel, Client currClient)
        {
            if (currChannel == null)
            {
                Log("AddChannelMember null channel supplied");
                return false;
            }

            if (currClient == null)
            {
                Log("AddChannelMember null client supplied");
                return false;
            }

            bool matchFound = false;

            lock (ChannelsLock)
            {
                foreach (KeyValuePair<string, Channel> curr in Channels)
                {
                    if (String.Compare(curr.Key, currChannel.ChannelGUID) == 0)
                    {
                        Log("AddChannelMember successfully found channel " + currChannel.ChannelGUID);

                        if (curr.Value.Members != null || curr.Value.Members.Count > 0)
                        {
                            foreach (Client c in curr.Value.Members)
                            {
                                if (String.Compare(c.ClientGUID, currClient.ClientGUID) == 0)
                                {
                                    Log("AddChannelMember member GUID " + c.ClientGUID + " already exists in channel GUID " + currChannel.ChannelGUID);
                                    matchFound = true;
                                }
                            }
                        }
                        else
                        {
                            curr.Value.Members = new List<Client>();
                        }

                        if (!matchFound)
                        {
                            Log("AddChannelMember adding member GUID " + currClient.ClientGUID + " to channel GUID " + currChannel.ChannelGUID);
                            curr.Value.Members.Add(currClient);
                            return true;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }

            Log("AddChannelMember unable to find channel GUID " + currChannel.ChannelGUID);
            return false;
        }

        /// <summary>
        /// Adds a Client to a Channel as a subscriber.
        /// </summary>
        /// <param name="currChannel">The Channel.</param>
        /// <param name="currClient">The Client.</param>
        /// <returns>Boolean indicating success or failure.</returns>
        public bool AddChannelSubscriber(Channel currChannel, Client currClient)
        {
            if (currChannel == null)
            {
                Log("AddChannelSubscriber null channel supplied");
                return false;
            }

            if (currClient == null)
            {
                Log("AddChannelSubscriber null client supplied");
                return false;
            }

            bool matchFound = false;

            lock (ChannelsLock)
            {
                foreach (KeyValuePair<string, Channel> curr in Channels)
                {
                    if (String.Compare(curr.Key, currChannel.ChannelGUID) == 0)
                    {
                        Log("AddChannelSubscriber successfully found channel " + currChannel.ChannelGUID);

                        if (curr.Value.Subscribers != null || curr.Value.Subscribers.Count > 0)
                        {
                            foreach (Client c in curr.Value.Subscribers)
                            {
                                if (String.Compare(c.ClientGUID, currClient.ClientGUID) == 0)
                                {
                                    Log("AddChannelSubscriber subscriber GUID " + c.ClientGUID + " already exists in channel GUID " + currChannel.ChannelGUID);
                                    matchFound = true;
                                }
                            }
                        }
                        else
                        {
                            curr.Value.Members = new List<Client>();
                        }

                        if (!matchFound)
                        {
                            Log("AddChannelSubscriber adding subscriber GUID " + currClient.ClientGUID + " to channel GUID " + currChannel.ChannelGUID);
                            curr.Value.Subscribers.Add(currClient);
                            return true;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }

            Log("AddChannelSubscriber unable to find channel GUID " + currChannel.ChannelGUID);
            return false;
        }

        /// <summary>
        /// Removes a Channel object from the server.
        /// </summary>
        /// <param name="guid">The GUID of the channel.</param>
        /// <returns>Boolean indicating success.</returns>
        public bool RemoveChannel(string guid)
        {
            if (String.IsNullOrEmpty(guid))
            {
                Log("RemoveChannel null GUID supplied");
                return false;
            }

            bool found = false;
            Dictionary<string, Channel> updated = new Dictionary<string, Channel>();

            lock (ChannelsLock)
            {
                foreach (KeyValuePair<string, Channel> curr in Channels)
                {
                    if (String.Compare(curr.Key, guid) == 0)
                    {
                        Log("RemoveChannel found channel GUID " + guid + ", skipping to remove");
                        found = true;
                        continue;
                    }

                    updated.Add(curr.Key, curr.Value);
                }

                Channels = updated;
                return found;
            }
        }
        
        /// <summary>
        /// Remove channels associated with the GUID of a client.
        /// </summary>
        /// <param name="ownerGuid">GUID of the client.</param>
        /// <returns>Boolean indicating success.</returns>
        public bool RemoveClientChannels(string ownerGuid, out List<Channel> affectedChannels)
        {
            affectedChannels = new List<Channel>();

            if (String.IsNullOrEmpty(ownerGuid))
            {
                Log("RemoveClientChannels null GUID supplied");
                return false;
            }

            bool found = false;
            Dictionary<string, Channel> updated = new Dictionary<string, Channel>();

            lock (ChannelsLock)
            {
                foreach (KeyValuePair<string, Channel> curr in Channels)
                {
                    if (String.Compare(curr.Value.OwnerGUID, ownerGuid) == 0)
                    {
                        Log("RemoveClientChannels found channel GUID " + curr.Value.ChannelGUID + " owned by GUID " + ownerGuid + ", skipping to remove");
                        affectedChannels.Add(curr.Value);
                        found = true;
                        continue;
                    }

                    updated.Add(curr.Key, curr.Value);
                }

                Channels = updated;
                return found;
            }
        }

        /// <summary>
        /// Remove a Client from a Channel's member list.
        /// </summary>
        /// <param name="currChannel">The Channel from which the Client should be removed.</param>
        /// <param name="currClient">The Client that should be removed from the channel.</param>
        /// <returns>Boolean indicating success or failure.</returns>
        public bool RemoveChannelMember(Channel currChannel, Client currClient)
        {
            if (currChannel == null)
            {
                Log("RemoveChannelMember null channel supplied");
                return false;
            }

            if (currClient == null)
            {
                Log("RemoveChannelMember null client supplied");
                return false;
            }

            bool matchFound = false;

            lock (ChannelsLock)
            {
                Channel updatedChannel = null;

                foreach (KeyValuePair<string, Channel> curr in Channels)
                {
                    if (String.Compare(curr.Key, currChannel.ChannelGUID) == 0)
                    {
                        #region Channel-Found

                        Log("RemoveChannelMember found channel GUID " + currChannel.ChannelGUID + " (" + curr.Value.Members.Count + ") members");
                        updatedChannel = currChannel;
                        List<Client> updatedMembers = new List<Client>();

                        if (curr.Value.Members != null && curr.Value.Members.Count > 0)
                        {
                            foreach (Client c in curr.Value.Members)
                            {
                                if (String.Compare(c.ClientGUID, currClient.ClientGUID) == 0)
                                {
                                    Log("RemoveChannelMember found member GUID " + c.ClientGUID + " in channel GUID " + currChannel.ChannelGUID + ", skipping to remove");
                                    matchFound = true;
                                }
                                else
                                {
                                    updatedMembers.Add(c);
                                }
                            }

                            updatedChannel.Members = updatedMembers;
                        }
                        else
                        {
                            Log("RemoveChannelMember no channel members found");
                        }

                        #endregion
                    }
                }

                if (updatedChannel != null)
                {
                    Dictionary<string, Channel> updatedChannels = new Dictionary<string, Channel>();

                    foreach (KeyValuePair<string, Channel> currKvp in Channels)
                    {
                        if (String.Compare(currKvp.Key, updatedChannel.ChannelGUID) != 0)
                        {
                            updatedChannels.Add(currKvp.Key, currKvp.Value);
                        }
                    }

                    updatedChannels.Add(updatedChannel.ChannelGUID, updatedChannel);
                    Channels = updatedChannels;
                }
            }

            return matchFound;
        }

        /// <summary>
        /// Remove a Client from a Channel's subscriber list.
        /// </summary>
        /// <param name="currChannel">The Channel from which the Client should be removed.</param>
        /// <param name="currClient">The Client that should be removed from the channel.</param>
        /// <returns>Boolean indicating success or failure.</returns>
        public bool RemoveChannelSubscriber(Channel currChannel, Client currClient)
        {
            if (currChannel == null)
            {
                Log("RemoveChannelSubscriber null channel supplied");
                return false;
            }

            if (currClient == null)
            {
                Log("RemoveChannelSubscriber null client supplied");
                return false;
            }

            bool matchFound = false;

            lock (ChannelsLock)
            {
                Channel updatedChannel = null;

                foreach (KeyValuePair<string, Channel> curr in Channels)
                {
                    if (String.Compare(curr.Key, currChannel.ChannelGUID) == 0)
                    {
                        #region Channel-Found

                        Log("RemoveChannelSubscriber found channel GUID " + currChannel.ChannelGUID + " (" + curr.Value.Subscribers.Count + ") subscribers");

                        updatedChannel = currChannel;
                        List<Client> updatedSubscribers = new List<Client>();

                        if (curr.Value.Subscribers != null && curr.Value.Subscribers.Count > 0)
                        {
                            foreach (Client c in curr.Value.Subscribers)
                            {
                                if (String.Compare(c.ClientGUID, currClient.ClientGUID) == 0)
                                {
                                    Log("RemoveChannelSubscriber found subscriber GUID " + c.ClientGUID + " in channel GUID " + currChannel.ChannelGUID + ", skipping to remove");
                                    matchFound = true;
                                }
                                else
                                {
                                    updatedSubscribers.Add(c);
                                }
                            }

                            updatedChannel.Subscribers = updatedSubscribers;
                        }
                        else
                        {
                            Log("RemoveChannelSubscriber no channel subscribers found");
                        }

                        #endregion
                    }
                }

                if (updatedChannel != null)
                {
                    Dictionary<string, Channel> updatedChannels = new Dictionary<string, Channel>();

                    foreach (KeyValuePair<string, Channel> currKvp in Channels)
                    {
                        if (String.Compare(currKvp.Key, updatedChannel.ChannelGUID) != 0)
                        {
                            updatedChannels.Add(currKvp.Key, currKvp.Value);
                        }
                    }

                    updatedChannels.Add(updatedChannel.ChannelGUID, updatedChannel);
                    Channels = updatedChannels;
                }
            }

            return matchFound;
        }

        /// <summary>
        /// Updates an existing Channel object on the server.
        /// </summary>
        /// <param name="currChannel">The Channel object.</param>
        public void UpdateChannel(Channel currChannel)
        {
            if (currChannel == null)
            {
                Log("UpdateChannel null channel supplied");
                return;
            }

            Dictionary<string, Channel> updated = new Dictionary<string, Channel>();

            lock (ChannelsLock)
            {
                foreach (KeyValuePair<string, Channel> curr in Channels)
                {
                    if (String.Compare(currChannel.ChannelGUID, curr.Key) == 0)
                    {
                        Log("UpdateClient found channel GUID " + currChannel.ChannelGUID + ", updating");
                        updated.Add(currChannel.ChannelGUID, currChannel);
                        continue;
                    }
                    updated.Add(curr.Key, curr.Value);
                }

                return;
            }
        }

        /// <summary>
        /// Removes a client from all Channel member and subscriber lists.
        /// </summary>
        /// <param name="ipPort">The IP:port of the Client.</param>
        public void RemoveClient(string ipPort)
        {
            if (String.IsNullOrEmpty(ipPort))
            {
                Log("RemoveClient null IP:port supplied");
                return;
            }

            lock (ChannelsLock)
            {
                if (Channels != null && Channels.Count > 0)
                {
                    Dictionary<string, Channel> updated = new Dictionary<string, Channel>();    // GUID, Channel

                    foreach (KeyValuePair<string, Channel> curr in Channels)
                    {
                        List<Client> updatedMembers = new List<Client>();
                        List<Client> updatedSubscribers = new List<Client>();

                        if (curr.Value.Members != null && curr.Value.Members.Count > 0)
                        {
                            foreach (Client currMember in curr.Value.Members)
                            {
                                if (String.Compare(currMember.IpPort(), ipPort) == 0)
                                {
                                    Log("RemoveClient removing member GUID " + currMember.ClientGUID + " from channel GUID " + curr.Value.ChannelGUID);
                                    continue;
                                }
                                else
                                {
                                    updatedMembers.Add(currMember);
                                }
                            }
                        }

                        if (curr.Value.Subscribers != null && curr.Value.Subscribers.Count > 0)
                        {
                            foreach (Client currSubscriber in curr.Value.Subscribers)
                            {
                                if (String.Compare(currSubscriber.IpPort(), ipPort) == 0)
                                {
                                    Log("RemoveClient removing subscriber GUID " + currSubscriber.ClientGUID + " from channel GUID " + curr.Value.ChannelGUID);
                                    continue;
                                }
                                else
                                {
                                    updatedSubscribers.Add(currSubscriber);
                                }
                            }
                        }

                        curr.Value.Members = updatedMembers;
                        curr.Value.Subscribers = updatedSubscribers;
                        updated.Add(curr.Key, curr.Value);
                    }

                    Channels = updated;
                }
            }
        }

        #endregion

        #region Private-Methods

        #endregion

        #region Private-Logging-Methods

        private void Log(string message)
        {
            if (Config.Debug.Enable && Config.Debug.ConsoleLogging && Config.Debug.ChannelMgmt)
            {
                Console.WriteLine(message);
            }
        }

        private void LogException(string method, Exception e)
        {
            Log("================================================================================");
            Log(" = Method: " + method);
            Log(" = Exception Type: " + e.GetType().ToString());
            Log(" = Exception Data: " + e.Data);
            Log(" = Inner Exception: " + e.InnerException);
            Log(" = Exception Message: " + e.Message);
            Log(" = Exception Source: " + e.Source);
            Log(" = Exception StackTrace: " + e.StackTrace);
            Log("================================================================================");
        }

        private void PrintException(string method, Exception e)
        {
            Console.WriteLine("================================================================================");
            Console.WriteLine(" = Method: " + method);
            Console.WriteLine(" = Exception Type: " + e.GetType().ToString());
            Console.WriteLine(" = Exception Data: " + e.Data);
            Console.WriteLine(" = Inner Exception: " + e.InnerException);
            Console.WriteLine(" = Exception Message: " + e.Message);
            Console.WriteLine(" = Exception Source: " + e.Source);
            Console.WriteLine(" = Exception StackTrace: " + e.StackTrace);
            Console.WriteLine("================================================================================");
        }

        #endregion
    }
}
