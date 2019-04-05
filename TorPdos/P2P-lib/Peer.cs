﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using P2P_lib.Messages;

namespace P2P_lib{
    [Serializable]
    public class Peer{
        private IPAddress _ip;
        private int _rating;
        private string _UUID;
        private DateTime _lastSeen;
        private int _pings_without_response = 0;
        private long _nextPing = 0;
        private bool _online = false;

        public Peer() : this(null, null){ }

        public Peer(string uuid, string ip){
            this._UUID = uuid;

            if (ip == null || ip.Equals("")){
                this.SetIP(NetworkHelper.getLocalIPAddress());
            } else{
                this.SetIP(ip);
            }

            Rating = 100;
        }

        public bool isOnline(){
            return this._online;
        }

        public void setOnline(bool online){

            if(online != this._online){
                if(online){
                    Console.WriteLine(this._ip+" - is now online!");
                }else{
                    Console.WriteLine(this._ip+" - is now offline!");
                }
            }

            this._online = online;
            this._pings_without_response = 0;
        }

        public void Ping(){
            this.Ping(0);
        }

        public void Ping(long millis){
            long time = (millis > 0) ? millis : DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (this._nextPing < time){
                PingMessage ping = new PingMessage(this);
                ping.from = NetworkHelper.getLocalIPAddress();
                ping.type = Messages.TypeCode.REQUEST;
                ping.statuscode = StatusCode.OK;
                ping.Send();

                // Ping every 60 seconds and if a peer didnt respond add extra time before retrying.
                this._nextPing = DateTimeOffset.Now.ToUnixTimeMilliseconds() + 10000 +
                                 (this._pings_without_response * 10000);
                this._pings_without_response++;

                if (this._pings_without_response >= 2){
                    this.setOnline(false);
                }
            }
        }

        [JsonConstructor]
        private Peer(string uuid, string stringIp, int rating, DateTime lastSeen){
            if (string.IsNullOrEmpty(uuid)) throw new NullReferenceException();
            _UUID = uuid;
            this.SetIP(stringIp);
            Rating = rating;
            _lastSeen = lastSeen;
        }

        public void SetIP(string ip){
            this._ip = IPAddress.Parse(ip);
        }

        public string GetIP(){
            return this._ip.ToString();
        }

        public DateTime lastSeen => _lastSeen;
        
        public string stringIP => _ip.ToString();
        public string UUID => _UUID;

        public void UpdateLastSeen(){
            _lastSeen = DateTime.Now;
        }

        public string getUUID(){
            return this._UUID;
        }

        public int Rating{
            get => _rating;
            set{
                if ((_rating + value) > 100){
                    _rating = 100;
                } else if ((_rating + value) < 0){
                    _rating = 0;
                } else{
                    _rating += value;
                }
            }
        }

        public int CompareTo(object obj){
            return String.Compare(_UUID, ((Peer) obj)._UUID, StringComparison.Ordinal);
        }

        public bool Equals(Peer peer){
            if (peer == null){
                return false;
            }

            return this._UUID.Equals(peer._UUID);
        }

        public override bool Equals(object obj){
            if (obj == null){
                return false;
            }

            if (!(obj is Peer p)){
                return false;
            }

            return this._UUID.Equals(p._UUID);
        }

        public override int GetHashCode(){
            return (_UUID != null ? _UUID.GetHashCode() : 0);
        }
    }
}