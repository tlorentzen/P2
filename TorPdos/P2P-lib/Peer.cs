using System;
using Newtonsoft.Json;
using System.Net;
using P2P_lib.Messages;
using System.Collections.Generic;

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
        private long[] _pingList = new long[] {-1, -1, -1, -1, -1};
        public long diskSpace, timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
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

            Rating = 100;
        }

        public bool IsOnline(){
            return this._online;
        }

        public void SetOnline(bool online){
            if (online != this._online){
                if (online){
                    Console.WriteLine(this._ip + " - is now online!");
                    rankHandler.UpdateUptime(this);
                    PeerSwitchedOnline?.Invoke();
                } else{
                    Console.WriteLine(this._ip + " - is now offline!");
                    rankHandler.UpdateUptime(this);
                }
            }

            this._online = online;
            this._pingsWithoutResponse = 0;
        }

        public void Ping(string pathToFolder) {
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

        public string GetUuid(){
            return this._uuid;
        }

        public int Rating{
            get => _rating;
            set{
                _rating = value;
            }
        }

        public int CompareTo(object obj){
            return string.Compare(_uuid, ((Peer) obj)._uuid, StringComparison.Ordinal);
        }

        public bool Equals(Peer peer){
            if (peer == null){
                return false;
            }

            return this._uuid.Equals(peer._uuid);
        }

        public override bool Equals(object obj){
            if (obj == null){
                return false;
            }

            if (!(obj is Peer p)){
                return false;
            }

            return this._uuid.Equals(p._uuid);
        }

        public override int GetHashCode(){
            return (_uuid != null ? _uuid.GetHashCode() : 0);
        }

        //Adds newPing to _pingList by replacing oldest ping
        public int AddPingToList(long newPing){
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
            return 0;
        }

        //Gets average latency from _pingList
        public long GetAverageLatency(){
            long sum = 0;
            for (int i = 0; i < _pingList.Length; i++){
                sum += this._pingList[i];
            }
            return (sum / _pingList.Length);
        }

    }

    public class ComparePeersByRating : IComparer<Peer> {
        public int Compare(Peer x, Peer y) {
            return (x.Rating < y.Rating ? 1 : (x.Rating > y.Rating ? -1 : 0));
        }
    }
}