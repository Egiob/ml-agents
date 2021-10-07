using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;
using System.Text;
using System;

public class DiscriminatorSideChannel : SideChannel
{
    public DiscriminatorSideChannel()
    {
        ChannelId = new Guid("621f0a70-4f87-11ea-a6bf-784f4387d1f7");
    }

    protected override void OnMessageReceived(IncomingMessage msg)
    {
        var receivedString = msg.ReadString();
        Debug.Log("From Python : " + receivedString);
    }

    public void SendDiscriminatorState(List<float> discState)
    {


        using (var msgOut = new OutgoingMessage())
            {
                msgOut.WriteFloatList(discState);
                QueueMessageToSend(msgOut);
            }
        
    }
}