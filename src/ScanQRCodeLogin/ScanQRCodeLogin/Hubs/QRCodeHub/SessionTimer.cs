using System;
using System.Collections.Concurrent;
using System.Timers;

namespace ScanQRCodeLogin.Hubs
{
    public class SessionTimer : IDisposable
    {
        /// <summary>
        /// 存储客户端对应的Timer
        /// </summary>
        public static readonly ConcurrentDictionary<string, SessionTimer> Timers;

        private readonly Timer _timer;

        static SessionTimer()
        {
            Timers = new ConcurrentDictionary<string, SessionTimer>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionId"></param>
        private SessionTimer(string connectionId, QRCodeTypes qrCodeType, string targetUrl)
        {
            ConnectionId = connectionId;
            _timer = new Timer
            {
                Interval = Utility.ActivityTimerInterval()
            };
            _timer.Elapsed += (s, e) => MonitorElapsedTime(qrCodeType, targetUrl);
            _timer.Start();
        }

        public int TimeCount { get; set; }

        /// <summary>
        /// 客户端连接Id
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// 启动Timer
        /// </summary>
        /// <param name="connectionId"></param>
        public static void StartTimer(string connectionId, QRCodeTypes qrCodeType, string targetUrl)
        {
            var newTimer = new SessionTimer(connectionId, qrCodeType, targetUrl);
            if (!Timers.TryAdd(connectionId, newTimer))
            {
                newTimer.Dispose();
            }
        }

        /// <summary>
        /// 停止Timer
        /// </summary>
        /// <param name="connectionId"></param>
        public static void StopTimer(string connectionId)
        {
            SessionTimer oldTimer;
            if (Timers.TryRemove(connectionId, out oldTimer))
            {
                oldTimer.Dispose();
            }
        }

        /// <summary>
        /// 重置Timer
        /// </summary>
        public void ResetTimer()
        {
            TimeCount = 0;
            _timer.Stop();
            _timer.Start();
        }

        public void Dispose()
        {
            // Stop might not be necessary since we call Dispose
            _timer.Stop();
            _timer.Dispose();
        }

        /// <summary>
        /// 给客户端发送消息
        /// </summary>
        private void MonitorElapsedTime(QRCodeTypes qrCodeType, string targetUrl)
        {
            Utility.ClearExpiredUUID();
            var uuid = Utility.GetUUID(ConnectionId, qrCodeType, targetUrl);
            //if (TimeCount >= Utility.TimerValue())
            //{
            //    StopTimer(ConnectionId);
            //    Notifier.SendQRCodeUUID(ConnectionId, uuid);
            //    Notifier.SessionTimeOut(ConnectionId, TimeCount);
            //}
            //else
            //{
            Notifier.SendQRCodeUUID(ConnectionId, uuid);
            Notifier.SendElapsedTime(ConnectionId, TimeCount);
            //}
            TimeCount++;
            if (TimeCount > 1000)
            {
                TimeCount = 0;
            }
        }
    }
}