using System;
using Newtonsoft.Json;
using System.Net;
using P2P_lib.Messages;
using System.Collections.Generic;
using P2P_lib.Handlers;
using P2P_lib.Helpers;

namespace P2P_lib{
    [Serializable]
    public class Peer{
        private IPAddress _ip;
        private int _rating;
        private readonly string _uuid;
        private DateTime _lastSeen;
        private int _pingsWithoutResponse;
        private long _nextPing;
        private bool _online;
        private long[] _pingList = new long[]{-1, -1, -1, -1, -1};
        public long diskSpace = 0, timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        public double uptimeScore = 50000;
        private RankingHandler rankHandler = new RankingHandler();

        public delegate void PeerWentOnline();

        public static event PeerWentOnline PeerSwitchedOnline;

        public Peer() : this(null, null){ }

        public Peer(string uuid, string ip){
            this._uuid = uuid;

            if (ip == null || ip.Equals("")){
                this.SetIp(NetworkHelper.GetLocalIpAddress());
            } else{
                this.SetIp(ip);
            }

            Rating = 0;
        }

        public bool IsOnline(){
            return this._online;
        }
        /// <summary>
        /// Toggles whether a peer is online.
        /// </summary>
        /// <param name="online">Boolean of whether the peer got online, or offline.</param>

        public void SetOnline(bool online){
            if (online != this._online){
                if (online){
                    DiskHelper.ConsoleWrite(this._ip + " - is now online!");
                    rankHandler.UpdateUptime(this);
                    PeerSwitchedOnline?.Invoke();
                } else{
                    DiskHelper.ConsoleWrite(this._ip + " - is now offline!");
                    rankHandler.UpdateUptime(this);
                }
            }

            this._online = online;
            this._pingsWithoutResponse = 0;
        }

        public void Ping(string pathToFolder){
            this.Ping(0, pathToFolder);
        }

        public void Ping(long millis, string pathToFolder){
            long time = (millis > 0) ? millis : DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (this._nextPing < time){
                PingMessage ping = new PingMessage(this);
                ping.from = NetworkHelper.GetLocalIpAddress();
                ping.type = Messages.TypeCode.REQUEST;
                ping.statusCode = StatusCode.OK;
                ping.diskSpace = DiskHelper.GetTotalAvailableSpace(pathToFolder);
                ping.Send();

                // Ping every 60 seconds and if a peer didnt respond add extra time before retrying.
                this._nextPing = DateTimeOffset.Now.ToUnixTimeMilliseconds() + 10000 +
                                 (this._pingsWithoutResponse * 10000);
                this._pingsWithoutResponse++;

                if (this._pingsWithoutResponse >= 2){
                    this.SetOnline(false);
                }
            }
        }

        [JsonConstructor]
        private Peer(string uuid, string stringIp, int rating, DateTime lastSeen){
            if (string.IsNullOrEmpty(uuid)) throw new NullReferenceException();
            _uuid = uuid;
            this.SetIp(stringIp);
            Rating = rating;
            _lastSeen = lastSeen;
        }

        public void SetIp(string ip){
            this._ip = IPAddress.Parse(ip);
        }

        public string GetIp(){
            return this._ip.ToString();
        }

        public DateTime lastSeen => _lastSeen;

        public string StringIp => _ip.ToString();
        
        public string UUID => _uuid;
        
        public void UpdateLastSeen(){
            _lastSeen = DateTime.Now;
        }

        /// <summary>
        /// Gets the UUID of the current peer.
        /// </summary>
        /// <returns>Returns the UUID of the current peer.</returns>
        public string GetUuid(){
            return this._uuid;
        }

        /// <summary>
        /// Returns rating of the current peer.
        /// </summary>
        public int Rating{
            get => _rating;
            set{ _rating = value; }
        }
        
        /// <summary>
        /// Checks whether the two peers are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Returns whether the two peers UUID are equal</returns>
        public override bool Equals(object obj){
            if (obj == null){
                return false;
            }

            if (!(obj is Peer p)){
                return false;
            }

            return this._uuid.Equals(p._uuid);
        }

        /// <summary>
        /// Returns hashcode of the UUID.
        /// </summary>
        /// <returns>Returns a hashed version of UUID</returns>
        public override int GetHashCode(){
            return (_uuid != null ? _uuid.GetHashCode() : 0);
        }

        /// <summary>
        /// Adds ping to the most recent pings.
        /// </summary>
        /// <param name="newPing"> This is the new ping, which should be added to the list.</param>
        public void AddPingToList(long newPing){
            // Added cap to ping.
            if (newPing > 500){
                newPing = 500;
            }

            if (this._pingList[0] == -1){
                for (int i = 0; i < _pingList.Length; i++){
                    this._pingList[i] = newPing;
                }
            } else{
                for (int i = _pingList.Length - 1; i > 0; i--){
                    this._pingList[i] = this._pingList[i - 1];
                }

                this._pingList[0] = newPing;
            }
        }

        /// <summary>
        /// Gets average latency from _pingList
        /// </summary>
        /// <returns>Returns the average of the last 10 pings, in format of a long</returns>
        public long GetAverageLatency(){
            long sum = 0;
            for (int i = 0; i < _pingList.Length; i++){
                sum += this._pingList[i];
            }

            return (sum / _pingList.Length);
        }
    }

    /// <summary>
    /// Compares peers by rating, helper function for sorting top peers.
    /// </summary>
    public class ComparePeersByRating : IComparer<Peer>{
        public int Compare(Peer x, Peer y){
            return (x.Rating < y.Rating ? 1 : (x.Rating > y.Rating ? -1 : 0));
        }
    }
}