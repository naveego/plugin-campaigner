using System;
using System.Threading.Tasks;
using Grpc.Core;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginCampaigner.API.Factory;
using PluginCampaigner.Helper;

namespace PluginCampaigner.API.Read
{
    public static partial class Read
    {
        public static async Task<long> ReadRecordsRealTimeAsync(IApiClient apiClient, ReadRequest request,
            IServerStreamWriter<Record> responseStream,
            ServerCallContext context)
        {
            var schema = request.Schema;
            var jobVersion = request.DataVersions.JobDataVersion;
            var recordsCount = 0;

            try
            {
                var realTimeState = JsonConvert.DeserializeObject<RealTimeState>(request.RealTimeStateJson);
                var realTimeSettings = JsonConvert.DeserializeObject<RealTimeSettings>(request.RealTimeSettingsJson);

                if (jobVersion > realTimeState.JobVersion)
                {
                    realTimeState.LastReadTime = DateTime.MinValue;
                }

                while (!context.CancellationToken.IsCancellationRequested)
                {
                    var records = ReadRecordsAsync(apiClient, schema, realTimeState.LastReadTime);

                    await foreach (var record in records)
                    {

                        // publish record
                        await responseStream.WriteAsync(record);
                        recordsCount++;
                    }

                    realTimeState.LastReadTime = DateTime.Now;
                    realTimeState.JobVersion = jobVersion;

                    var realTimeStateCommit = new Record
                    {
                        Action = Record.Types.Action.RealTimeStateCommit,
                        RealTimeStateJson = JsonConvert.SerializeObject(realTimeState)
                    };
                    await responseStream.WriteAsync(realTimeStateCommit);

                    await Task.Delay(realTimeSettings.PollingInterval * 1000, context.CancellationToken);
                }
            }
            catch (TaskCanceledException e)
            {
                return recordsCount;
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message, context);
                throw;
            }
            
            return recordsCount;
        }
    }
}