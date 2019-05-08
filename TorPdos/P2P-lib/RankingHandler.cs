using System;

namespace P2P_lib
{
    [Serializable]
    public class RankingHandler {
        //Deliberately Amazing Peer Practicality Estimation Ranker
        public int GetRank(Peer peer) {
            int
                scoreDiskSpace = ScoreDiskSpace(peer.diskSpace),
                scoreLatency = ScoreLatency(peer.GetAverageLatency()),
                scoreUptime = UpdateUptime(peer, false),
                scoreTotal = scoreLatency + scoreDiskSpace + scoreUptime;

            Console.WriteLine(peer.GetIp() + " | disk:" + (peer.diskSpace / 1e+9) + " = " + scoreDiskSpace + " | ping:" + peer.GetAverageLatency() + " = " + scoreLatency + " | upt:" + scoreUptime); 
            peer.Rating = scoreTotal;
            return scoreTotal;
        }

        //Calc score from disk space
        private int ScoreDiskSpace(long diskSpaceBytes) {
            double diskSpace = diskSpaceBytes / 1e+9; //Convert to GB

            int score =
                diskSpace < 0 ? 0:
                diskSpace < 5 ? 10000 : 
                diskSpace < 10 ? 20000 : 
                30000;
            return score;
        }

        //Calc score from average latency
        private int ScoreLatency(long ping) {
            int score = 
                ping < 0 ? 0 : 
                ping < 50 ? 50000 : 
                ping < 100 ? 25000 : 
                0;
            return score;
        }

        //Update uptime score
        public int UpdateUptime(Peer peer, bool changedSinceLast = true) {

            //Calculate time since last state shift (online/offline) or uptime update
            long timespan = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() - peer.timestamp;
            int max = 100000, mid = max / 2;
            bool online = peer.IsOnline();

            //Update the timestamp on peer
            peer.timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            //Flips online, if the state has changed. This is done, because the rating is calculated from the last timestamp until now.
            if (changedSinceLast){
                online = !online;
            }

            //The rating is then calculated based on the peer state doing the timespan and the length of the timespan
            if (online) {
                for (long i = 0; i < timespan; i++) {
                    if (peer.uptimeScore >= max) {
                        peer.uptimeScore = max;
                        break;
                    }
                    else if (peer.uptimeScore < mid) {
                        peer.uptimeScore++;
                    }
                    else if (peer.uptimeScore < max) {
                        peer.uptimeScore = peer.uptimeScore + mid / peer.uptimeScore;
                    }
                }
            } else {
                for (long i = 0; i < timespan; i++) {
                    if (peer.uptimeScore <= 0) {
                        peer.uptimeScore = 0;
                        break;
                    }
                    else if (peer.uptimeScore > mid) {
                        peer.uptimeScore--;
                    }
                    else if (peer.uptimeScore > 0) {
                        peer.uptimeScore = peer.uptimeScore - peer.uptimeScore / mid;
                    }
                }
            }
            //The score is then returned
            return Convert.ToInt32(Math.Round(peer.uptimeScore));
        }
    }
}
