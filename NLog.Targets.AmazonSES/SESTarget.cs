using System;
using System.Collections;
using System.Linq;
using System.Threading;
using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using NLog.Common;
using NLog.Layouts;

namespace NLog.Targets.AmazonSES
{
    [Target("SESTarget")]
    public sealed class SESTarget : TargetWithLayout
    {
        private static AmazonSimpleEmailService _ses;
        private static GetSendQuotaResponse _quota;
        private static readonly Queue _queue = Queue.Synchronized(new Queue());
        private static Thread _mta;

        public string AwsAccessKey { get; set; }
        public string AwsSecretKey { get; set; }
        public Layout Subject { get; set; }
        public string From { get; set; }
        public string To { get; set; }

        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            var logMessage = logEvents.Aggregate(string.Empty,
                                (current, logEvent) => current + string.Format("{0}{1}", this.Layout.Render(logEvent.LogEvent), Environment.NewLine));

            // Queue up the message
            var message = new Message();
            message.WithBody(new Body
            {
                Text = new Content { Charset = "UTF-8", Data = logMessage }
                //,Html = new Content { Charset = "UTF-8", Data = logMessage }
            });
            message.WithSubject(new Content { Charset = "UTF-8", Data = Subject.Render(logEvents[logEvents.Length - 1].LogEvent) });
            _queue.Enqueue(message);

            StartMta();
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var logMessage = this.Layout.Render(logEvent);

            // Queue up the message
            var message = new Message();
            message.WithBody(new Body
            {
                Text = new Content { Charset = "UTF-8", Data = logMessage }
                //,Html = new Content { Charset = "UTF-8", Data = logMessage }
            });
            message.WithSubject(new Content { Charset = "UTF-8", Data = Subject.Render(logEvent) });
            _queue.Enqueue(message);

            StartMta();
        }

        private void StartMta()
        {
            // Check if there is already a thread running, if not start one to start proccessing the queue
            if (_mta != null && _mta.IsAlive)
                return;

            _mta = new Thread(() =>
            {
                // Create an amazon ses client
                // Retreive send rate
                if (_ses == null)
                {
                    _ses = AWSClientFactory.CreateAmazonSimpleEmailServiceClient(AwsAccessKey, AwsSecretKey);

                    // Try and get the sending quota from amazon
                    // This will fail if invalid credentials are provided
                    try
                    {
                        _quota = _ses.GetSendQuota(new GetSendQuotaRequest());
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Fatal("Error retreiving send quota from amazone: {0}", ex.Message);
                        return;
                    }

                    InternalLogger.Info("Amazon SES quota loaded, max send rate: {0} per second, 24 hour quota: {1}, sent in the last 24 hours: {2}", _quota.GetSendQuotaResult.MaxSendRate, _quota.GetSendQuotaResult.Max24HourSend, _quota.GetSendQuotaResult.SentLast24Hours);
                }

                while (true)
                {
                    // Try and get a message from the queue
                    // will fail if there are no messages to send
                    Message message = null;
                    try
                    {
                        message = (Message)_queue.Dequeue();
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Error("Error dequeuing message: {0}", ex.Message);
                        return;
                    }

                    var request = new SendEmailRequest();
                    request.WithDestination(new Destination(To.Split(',').ToList()));
                    request.WithMessage(message);
                    request.WithSource(From);

                    try
                    {
                        var response = _ses.SendEmail(request);

                        var result = response.SendEmailResult;

                        InternalLogger.Debug("Amazon SES message sent, id: {0}", result.MessageId);
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Error("Error sending message: {0}", ex.Message);
                    }

                    // sleep for the required send rate quota
                    Thread.Sleep((int)(1000 / _quota.GetSendQuotaResult.MaxSendRate));
                }
            });
            _mta.Start();
        }
    }
}
