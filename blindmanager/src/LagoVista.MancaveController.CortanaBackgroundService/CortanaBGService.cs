using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;

namespace LagoVista.MancaveController.CortanaBackgroundService
{
    public sealed class CortanaBGService : IBackgroundTask
    {
        private BackgroundTaskDeferral _serviceDeferral;
        VoiceCommandServiceConnection voiceServiceConnection;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _serviceDeferral = taskInstance.GetDeferral();


            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;

            if (triggerDetails != null && triggerDetails.Name == "CortanaBGService")
            {
                try
                {
                    voiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);

                    voiceServiceConnection.VoiceCommandCompleted += VoiceCommandCompleted;

                    VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();

                    switch (voiceCommand.CommandName)
                    {
                        case "blinds":
                            {
                                foreach(var prop in voiceCommand.Properties)
                                {
                                    Debug.WriteLine(String.Format("prop {0} = {1}", prop.Key, prop.Value.ToString()));
                                }

                                var state = voiceCommand.Properties["blindState"][0];
                                var blind = voiceCommand.Properties["blindId"][0];

                                await ChangeBlindState(blind, state);
                                break;
                            }

                        // As a last resort launch the app in the foreground
                        default:
                            _serviceDeferral.Complete();
                            break;
                    }
                }
                catch(Exception)
                {
                    Debugger.Break();
                }
                finally
                {
                    if (_serviceDeferral != null)
                    {
                        //Complete the service deferral
                        _serviceDeferral.Complete();
                        _serviceDeferral = null;
                    }
                }
            }
        }

        private async Task ChangeBlindState(String blindId, String state)
        {
            // Take action and determine when the next trip to targetSender
            // Inset code here

            // Replace the hardcoded strings used here with strings 
            // appropriate for your application.

            // First, create the VoiceCommandUserMessage with the strings 
            // that Cortana will show and speak.
            var userMessage = new VoiceCommandUserMessage
            {
                DisplayMessage = String.Format("{0}ing your blinds.", state),
                SpokenMessage = string.Format("Got it, I'm going to {0} the {1} blind.", state, blindId)
            };

                
            // Create the VoiceCommandResponse from the userMessage and list    
            // of content tiles.
            var response = VoiceCommandResponse.CreateResponse(userMessage);

            //// Cortana will present a "Go to app_name" link that the user 
            //// can tap to launch the app. 
            //// Pass in a launch to enable the app to deep link to a page 
            //// relevant to the voice command.
            response.AppLaunchArgument = string.Format("something={0}", "for%20nothing");

            try
            {
                // Ask Cortana to display the user message and content tile and 
                // also speak the user message.
                

                var blindIdx = String.Empty;

                switch(blindId.ToLower())
                {
                    case "door": blindIdx = "5"; break;
                    case "southeast": blindIdx = "4"; break;
                    case "south": blindIdx = "3"; break;
                    case "southwest": blindIdx = "2"; break;
                    case "west": blindIdx = "1"; break;

                }

                if (state.ToLower() == "close")
                {
                    var uri = String.Format("http://slsys.homeip.net:9300/blind/{0}/down/20000", blindIdx);

                    var request = new HttpClient();
                    request.DefaultRequestHeaders.Add("clientsecret", "{D9F7D7C8-D752-47B3-8C16-B4F61B004A2A}");
                    await request.GetAsync(uri);
                }
                else
                {
                    var uri = String.Format("http://slsys.homeip.net:9300/blind/{0}/up", blindIdx);

                    var request = new HttpClient();
                    request.DefaultRequestHeaders.Add("clientsecret", "{D9F7D7C8-D752-47B3-8C16-B4F61B004A2A}");
                    await request.GetAsync(uri);
                }

                await voiceServiceConnection.ReportSuccessAsync(response);

            }
            catch (Exception)
            {
                Debugger.Break();
            }
        }

        private void VoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            if (_serviceDeferral != null)
            {
                // Insert your code here
                //Complete the service deferral
                _serviceDeferral.Complete();
                _serviceDeferral = null;
            }
        }
    }
}
