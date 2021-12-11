using IOClasses;
using Microsoft.AspNetCore.SignalR;
using RFIDReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrackingClient.Services;

namespace TrackingClient.Hubs
{
    public class PageHub : Hub
    {
        public async Task UpdateRFIDTag(RFID_Tag tag)
        {
            if (Global.HubContext != null)
                await Global.HubContext.Clients.All.SendAsync("ReceiveRFIDTag", tag);
        }

        public async Task UpdateCurrentProcessingState(Processing.State state)
        {
            //if (Clients != null)
            //{
            //    await Clients.All.SendAsync("ReceiveProcessingState", state);
            //    return;
            //}
            if (Global.HubContext != null)
            {
                await Global.HubContext.Clients.All.SendAsync("ReceiveProcessingState", state);
                //await UpdateRFIDTag(new RFID_Tag("2Y68201955045148^4Y51118", -34));
                //await UpdateTransactionStatus("Saving RFID tag to database");
                //await Task.Delay(2000);
                //await UpdateTransactionStatus("");
            }
        }

        public async Task UpdateTransactionStatus(string status)
        {
            if (Global.HubContext != null)
                await Global.HubContext.Clients.All.SendAsync("ReceiveTransactionStatus", status);
        }

        public async Task UpdateIOChannels(List<IPort> ports)
        {
            if (Global.HubContext != null)
                await Global.HubContext.Clients.All.SendAsync("ReceiveIOPorts", ports);
        }

        public async Task UpdateReaderConnectionState(bool state)
        {
            if (Global.HubContext != null)
                await Global.HubContext.Clients.All.SendAsync("ReceiveReaderConnectionState", state);
        }

        public async Task UpdateIOConnectionState(bool state)
        {
            if (Global.HubContext != null)
                await Global.HubContext.Clients.All.SendAsync("ReceiveIOConnectionState", state);
        }

        public async Task UpdateReaderMQTTStatus(bool state)
        {
            if (Global.HubContext != null)
                await Global.HubContext.Clients.All.SendAsync("ReceiveReaderMQTTState", state);
        }

        public async Task UpdateIOMQTTStatus(bool state)
        {
            if (Global.HubContext != null)
                await Global.HubContext.Clients.All.SendAsync("ReceiveIOMQTTState", state);
        }
    }
}
