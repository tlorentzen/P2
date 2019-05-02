using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2P_lib
{
    public class DAPPER {
        //Deliberately Amazing Peer Practicality Estimation Ranker
        public int GetRank(Peer peer) {
            double
                scoreUptime = UpdateUptime(peer.IsOnline(), peer.uptimeScore, peer.timespan);
            int
                scoreDiskSpace = ScoreDiskSpace(peer.diskSpace),
                scoreLatency = ScoreLatency(peer.GetAverageLatency()),
                scoreTotal = scoreLatency + scoreDiskSpace + Convert.ToInt32(Math.Round(scoreUptime));

            return scoreTotal;
        }

        //Calc score from disk space
        private int ScoreDiskSpace(long diskSpaceBytes) {
            double diskSpace = (double)diskSpaceBytes / 1e+9; //Convert to GB

            int score = diskSpace < 5 ? 10000 : diskSpace < 10 ? 20000 : 30000;
            return score;
        }

        //Calc score from average latency
        private int ScoreLatency(int ping) {
            int score = ping < 50 ? 50000 : ping < 100 ? 25000 : 0;
            return score;
        }

        //Update uptime score
        public double UpdateUptime(bool online, double uptimeScore, long timespan) {
            int
                max = 100000,
                mid = max / 2;

            if (online) {
                for (long i = 0; i < timespan; i++) {
                    if (uptimeScore >= max) {
                        uptimeScore = max;
                        break;
                    }
                    else if (uptimeScore < mid) {
                        uptimeScore++;
                    }
                    else if (uptimeScore < max) {
                        uptimeScore = uptimeScore + mid / uptimeScore;
                    }
                }
            } else {
                for (long i = 0; i < timespan; i++) {
                    if (uptimeScore <= 0) {
                        uptimeScore = 0;
                        break;
                    }
                    else if (uptimeScore > mid) {
                        uptimeScore--;
                    }
                    else if (uptimeScore > 0) {
                        uptimeScore = uptimeScore - uptimeScore / mid;
                    }
                }
            }
            return uptimeScore;
        }
    }
}
