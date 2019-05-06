using System;

namespace P2P_lib
{
    public class Dapper {
        //Deliberately Amazing Peer Practicality Estimation Ranker
        public int GetRank(Peer peer) {

            int
                scoreDiskSpace = ScoreDiskSpace(peer.diskSpace),
                scoreLatency = ScoreLatency(peer.GetAverageLatency()),
                scoreUptime = UpdateUptime(peer, false),
                scoreTotal = scoreLatency + scoreDiskSpace + scoreUptime;

            return scoreTotal;
        }

        //Calc score from disk space
        private int ScoreDiskSpace(long diskSpaceBytes) {
            double diskSpace = diskSpaceBytes / 1e+9; //Convert to GB

            int score = diskSpace < 5 ? 10000 : diskSpace < 10 ? 20000 : 30000;
            return score;
        }

        //Calc score from average latency
        private int ScoreLatency(long ping) {
            int score = ping < 50 ? 50000 : ping < 100 ? 25000 : 0;
            return score;
        }

        //Update uptime score
        public int UpdateUptime(Peer peer, bool changedSinceLast = true) {
            int
                max = 100000,
                mid = max / 2;
            bool
                online = peer.IsOnline();

            if (changedSinceLast){
                online = !online;
            }

            if (online) {
                for (long i = 0; i < peer.timespan; i++) {
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
                for (long i = 0; i < peer.timespan; i++) {
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
            peer.timespan = 0;
            return Convert.ToInt32(Math.Round(peer.uptimeScore));
        }
    }
}
