﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PubnubApi;
using Windows.UI.Core;
using Windows.UI;
using Windows.UI.Popups;
using System.Threading.Tasks;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WindowsUniversalApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class PubnubOperation : Page //ignore codacy check
    {
        string channel = "";
        string channelGroup = "";
        PubnubConfigData data;
        static Pubnub pubnub;
        static PNConfiguration config;
        SubscribeCallbackExt listener;

        Popup publishPopup;
        Popup hereNowPopup;
        Popup whereNowPopup;
        Popup globalHereNowPopup;
        Popup userStatePopup;
        Popup changeUUIDPopup;


        public PubnubOperation()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            data = e.Parameter as PubnubConfigData;

            if (data != null)
            {
                config = new PNConfiguration();
                config.PublishKey = data.publishKey;
                config.SubscribeKey = data.subscribeKey;
                config.SecretKey = data.secretKey;
                config.CipherKey = data.cipherKey;
                config.Secure = data.ssl;
                config.Origin = data.origin;
                config.Uuid = data.sessionUUID;
                config.AuthKey = data.authKey;
                config.PresenceTimeout = data.presenceHeartbeat;
                config.SubscribeTimeout = data.subscribeTimeout;
                config.NonSubscribeRequestTimeout = data.nonSubscribeTimeout;
                config.UseClassicHttpWebRequest = true;

                config.PubnubLog = new PlatformPubnubLog();
                config.LogVerbosity = PNLogVerbosity.BODY;
                pubnub = new Pubnub(config);
                listener = new SubscribeCallbackExt(
                    async (o, m) =>
                    {
                        await DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)).ConfigureAwait(false);
                    },
                    async (o, p) =>
                    {
                        await DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(p)).ConfigureAwait(false);
                    },
                    async (o, s) =>
                    {
                        await DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(s)).ConfigureAwait(false);
                    });
            }

        }

        private void btnTime_Click(object sender, RoutedEventArgs e)
        {
            pubnub.Time().Async(new PNTimeResultExt(
                async (r, s) =>
                {
                    if (r != null)
                    {
                        await DisplayMessageInTextBox(r.Timetoken.ToString()).ConfigureAwait(false);
                    }
                }));
        }

        private void btnSubscribe_Click(object sender, RoutedEventArgs e)
        {
            channel = txtChannel.Text;
            channelGroup = txtChannelGroup.Text;
            pubnub.AddListener(listener);
            pubnub.Subscribe<string>()
                .Channels(new [] { channel })
                .ChannelGroups(new [] { channelGroup })
                .WithPresence()
                .Execute();
        }

        private void btnUnsubscribe_Click(object sender, RoutedEventArgs e)
        {
            channel = txtChannel.Text;
            channelGroup = txtChannelGroup.Text;
            pubnub.Unsubscribe<string>()
                .Channels(new [] { channel })
                .ChannelGroups(new [] { channelGroup })
                .Execute();
            pubnub.RemoveListener(listener);
        }

        private void btnPublish_Click(object sender, RoutedEventArgs e)
        {
            channel = txtChannel.Text;
            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel publishStackPanel = new StackPanel();
            publishStackPanel.Background = new SolidColorBrush(Colors.Blue);
            publishStackPanel.Width = 320;
            publishStackPanel.Height = 550;

            publishPopup = new Popup();
            publishPopup.Height = 550;
            publishPopup.Width = 320;
            publishPopup.VerticalOffset = 100;
            publishPopup.HorizontalOffset = 10;


            PublishMessageUserControl control = new PublishMessageUserControl();
            publishStackPanel.Children.Add(control);
            border.Child = publishStackPanel;

            publishPopup.Child = border;
            publishPopup.IsOpen = true;

            publishPopup.Closed += (senderPopup, argsPopup) =>
            {
                if (control.IsOKButtonEntered)
                {
                    RadioButton radNormalPublish = control.FindName("radNormalPublish") as RadioButton;
                    if (radNormalPublish != null && radNormalPublish.IsChecked.Value)
                    {
                        TextBox txtPublish = control.FindName("txtPublish") as TextBox;
                        string publishMsg = (txtPublish != null) ? txtPublish.Text : "";

                        CheckBox chkStoreInHistory = control.FindName("chkStoreInHistory") as CheckBox;
                        bool storeInHistory = (chkStoreInHistory != null) ? chkStoreInHistory.IsChecked.Value : true;

                        if (publishMsg != "")
                        {
                            double doubleData;
                            int intData;
                            if (int.TryParse(publishMsg, out intData)) //capture numeric data
                            {
                                pubnub.Publish()
                                .Channel(channel)
                                .Message(intData)
                                .ShouldStore(storeInHistory)
                                .Async(new PNPublishResultExt(
                                    async (r, s) =>
                                    {
                                        if (r != null)
                                        {
                                            await DisplayMessageInTextBox(r.Timetoken.ToString()).ConfigureAwait(false);
                                        }
                                    }));
                            }
                            else if (double.TryParse(publishMsg, out doubleData)) //capture numeric data
                            {
                                pubnub.Publish()
                                .Channel(channel)
                                .Message(doubleData)
                                .ShouldStore(storeInHistory)
                                .Async(new PNPublishResultExt(
                                    async (r, s) =>
                                    {
                                        if (r != null)
                                        {
                                            await DisplayMessageInTextBox(r.Timetoken.ToString()).ConfigureAwait(false);
                                        }
                                    }));
                            }
                            else
                            {
                                pubnub.Publish()
                                .Channel(channel)
                                .Message(publishMsg)
                                .ShouldStore(storeInHistory)
                                .Async(new PNPublishResultExt(
                                    async (r, s) =>
                                    {
                                        if (r != null)
                                        {
                                            await DisplayMessageInTextBox(r.Timetoken.ToString()).ConfigureAwait(false);
                                        }
                                    }));
                            }
                        }
                    }

                }
                publishPopup = null;
                this.IsEnabled = true;
            };

        }

        private async void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            channel = txtChannel.Text;
            await DisplayMessageInTextBox("Running Detailed History:").ConfigureAwait(false);
            pubnub.History()
                .Channel(channel)
                .Count(100)
                .Async(new PNHistoryResultExt(
                    async (r, s) =>
                    {
                        if (r != null)
                        {
                            await DisplayMessageInTextBox("Message Count = " + r.Messages.Count.ToString()).ConfigureAwait(false);
                        }
                    }));
        }

        private void btnGlobalHereNow_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel globalHerenowStackPanel = new StackPanel();
            globalHerenowStackPanel.Background = new SolidColorBrush(Colors.Blue);
            globalHerenowStackPanel.Width = 300;
            globalHerenowStackPanel.Height = 300;

            globalHereNowPopup = new Popup();
            globalHereNowPopup.Height = 300;
            globalHereNowPopup.Width = 300;

            globalHereNowPopup.HorizontalOffset = 10;
            globalHereNowPopup.VerticalOffset = 100;

            HereNowOptionsUserControl control = new HereNowOptionsUserControl();
            globalHerenowStackPanel.Children.Add(control);
            border.Child = globalHerenowStackPanel;

            globalHereNowPopup.Child = border;
            globalHereNowPopup.IsOpen = true;

            globalHereNowPopup.Closed += async (senderPopup, argsPopup) =>
            {
                if (control.IsOKButtonEntered)
                {
                    CheckBox chkShowUUID = control.FindName("chkHereNowShowUUID") as CheckBox;
                    bool showUUID = (chkShowUUID != null) ? chkShowUUID.IsChecked.Value : false;

                    CheckBox chkIncludeUserState = control.FindName("chkHereIncludeUserState") as CheckBox;
                    bool includeState = (chkIncludeUserState != null) ? chkIncludeUserState.IsChecked.Value : false;

                    await DisplayMessageInTextBox("Running GlobalHereNow:").ConfigureAwait(false);
                    pubnub.HereNow()
                    .IncludeUUIDs(showUUID)
                    .IncludeState(includeState)
                    .Async(new PNHereNowResultEx(
                        async (r, s) =>
                        {
                            if (r != null)
                            {
                                await DisplayMessageInTextBox("TotalChannels = " + r.TotalChannels.ToString()).ConfigureAwait(false);
                                await DisplayMessageInTextBox("TotalOccupancy = " + r.TotalOccupancy.ToString()).ConfigureAwait(false);
                            }
                        }));
                }
                globalHereNowPopup = null;
                this.IsEnabled = true;
            };
        }

        private void btnHereNow_Click(object sender, RoutedEventArgs e)
        {
            channel = txtChannel.Text;
            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel herenowStackPanel = new StackPanel();
            herenowStackPanel.Background = new SolidColorBrush(Colors.Blue);
            herenowStackPanel.Width = 300;
            herenowStackPanel.Height = 300;

            hereNowPopup = new Popup();
            hereNowPopup.Height = 300;
            hereNowPopup.Width = 300;

            hereNowPopup.HorizontalOffset = 10;
            hereNowPopup.VerticalOffset = 100;


            HereNowOptionsUserControl control = new HereNowOptionsUserControl();
            herenowStackPanel.Children.Add(control);
            border.Child = herenowStackPanel;

            hereNowPopup.Child = border;
            hereNowPopup.IsOpen = true;

            hereNowPopup.Closed += async (senderPopup, argsPopup) =>
            {
                if (control.IsOKButtonEntered)
                {
                    CheckBox chkShowUUID = control.FindName("chkHereNowShowUUID") as CheckBox;
                    bool showUUID = (chkShowUUID != null) ? chkShowUUID.IsChecked.Value : false;

                    CheckBox chkIncludeUserState = control.FindName("chkHereIncludeUserState") as CheckBox;
                    bool includeState = (chkIncludeUserState != null) ? chkIncludeUserState.IsChecked.Value : false;

                    await DisplayMessageInTextBox("Running HereNow:").ConfigureAwait(false);
                    pubnub.HereNow()
                    .Channels(new[] { channel })
                    .IncludeUUIDs(showUUID)
                    .IncludeState(includeState)
                    .Async(new PNHereNowResultEx(
                        async (r, s) =>
                        {
                            if (r != null)
                            {
                                await DisplayMessageInTextBox("TotalChannels = " + r.TotalChannels.ToString()).ConfigureAwait(false);
                                await DisplayMessageInTextBox("TotalOccupancy = " + r.TotalOccupancy.ToString()).ConfigureAwait(false);
                            }
                        }));
                }
                hereNowPopup = null;
                this.IsEnabled = true;
            };
        }

        private void btnPAMChannel_Click(object sender, RoutedEventArgs e)
        {
            channel = txtChannel.Text;

            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel pamChannelStackPanel = new StackPanel();
            pamChannelStackPanel.Background = new SolidColorBrush(Colors.Blue);
            pamChannelStackPanel.Width = 400;
            pamChannelStackPanel.Height = 600;

            Popup pamChannelPopup = new Popup();
            pamChannelPopup.Height = 300;
            pamChannelPopup.Width = 300;

            pamChannelPopup.HorizontalOffset = 10;
            pamChannelPopup.VerticalOffset = 100;

            PAMChannelUserControl control = new PAMChannelUserControl();

            TextBox txtPAMChannelName = control.FindName("txtChannelName") as TextBox;
            if (txtPAMChannelName != null)
            {
                txtPAMChannelName.Text = channel;
            }

            pamChannelStackPanel.Children.Add(control);
            border.Child = pamChannelStackPanel;

            pamChannelPopup.Child = border;
            pamChannelPopup.IsOpen = true;

            pamChannelPopup.Closed += async (senderPopup, argsPopup) =>
            {
                if (control.IsOKButtonEntered)
                {
                    string pamUserChannelName = "";
                    string pamAuthKey = "";
                    txtPAMChannelName = control.FindName("txtChannelName") as TextBox;
                    if (txtPAMChannelName != null)
                    {
                        pamUserChannelName = txtPAMChannelName.Text.Trim();
                    }

                    TextBox txtAuthKey = control.FindName("txtAuthKey") as TextBox;
                    if (txtAuthKey != null)
                    {
                        pamAuthKey = txtAuthKey.Text;
                    }

                    RadioButton radGrantPAMChannel = control.FindName("radGrantChannel") as RadioButton;
                    if (radGrantPAMChannel != null && radGrantPAMChannel.IsChecked.Value)
                    {
                        await DisplayMessageInTextBox("Running GrantAccess:").ConfigureAwait(false);
                        int ttlInMinutes = 1440;
                        pubnub.Grant()
                        .Channels(new[] { pamUserChannelName })
                        .AuthKeys(new[] { pamAuthKey })
                        .Read(true)
                        .Write(true)
                        .TTL(ttlInMinutes)
                        .Async(new PNAccessManagerGrantResultExt(async (r, s) =>
                        {
                            if (r != null)
                            {
                                await DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)).ConfigureAwait(false);
                            }
                        }));
                    }

                    RadioButton radAuditChannel = control.FindName("radAuditChannel") as RadioButton;
                    if (radAuditChannel != null && radAuditChannel.IsChecked.Value)
                    {
                        await DisplayMessageInTextBox("Running AuditAccess:").ConfigureAwait(false);
                        pubnub.Audit()
                        .Channel(pamUserChannelName)
                        .AuthKeys(new[] { pamAuthKey })
                        .Async(new PNAccessManagerAuditResultExt(
                            async (r, s) =>
                            {
                                if (r != null)
                                {
                                    await DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)).ConfigureAwait(false);
                                }
                            }));
                    }

                    RadioButton radRevokeChannel = control.FindName("radRevokeChannel") as RadioButton;
                    if (radRevokeChannel != null && radRevokeChannel.IsChecked.Value)
                    {
                        await DisplayMessageInTextBox("Running Revoke Access:").ConfigureAwait(false);
                        await Task.Run(() =>
                         {
                             pubnub.Grant()
                             .Channels(new[] { pamUserChannelName })
                             .AuthKeys(new[] { pamAuthKey })
                             .Read(false)
                             .Write(false)
                             .Async(new PNAccessManagerGrantResultExt(async (r, s) =>
                             {
                                 if (r != null)
                                 {
                                     await DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)).ConfigureAwait(false);
                                 }
                             }));
                         }).ConfigureAwait(false);
                    }

                }
                pamChannelPopup = null;
                this.IsEnabled = true;
            };

        }

        private void btnPAMChannelGroup_Click(object sender, RoutedEventArgs e)
        {
            channelGroup = txtChannelGroup.Text;

            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel pamChannelGroupStackPanel = new StackPanel();
            pamChannelGroupStackPanel.Background = new SolidColorBrush(Colors.Blue);
            pamChannelGroupStackPanel.Width = 400;
            pamChannelGroupStackPanel.Height = 600;

            Popup pamChannelGroupPopup = new Popup();
            pamChannelGroupPopup.Height = 300;
            pamChannelGroupPopup.Width = 300;

            pamChannelGroupPopup.HorizontalOffset = 10;
            pamChannelGroupPopup.VerticalOffset = 100;

            PAMChannelGroupUserControl control = new PAMChannelGroupUserControl();

            TextBox txtPAMChannelGroup = control.FindName("txtChannelGroup") as TextBox;
            if (txtPAMChannelGroup != null)
            {
                txtPAMChannelGroup.Text = channelGroup;
            }

            pamChannelGroupStackPanel.Children.Add(control);
            border.Child = pamChannelGroupStackPanel;

            pamChannelGroupPopup.Child = border;
            pamChannelGroupPopup.IsOpen = true;

            pamChannelGroupPopup.Closed += async (senderPopup, argsPopup) =>
            {
                if (control.IsOKButtonEntered)
                {
                    string pamUserChannelGroup = "";
                    string pamAuthKey = "";
                    int ttlInMinutes = 1440;
                    txtPAMChannelGroup = control.FindName("txtChannelGroup") as TextBox;
                    if (txtPAMChannelGroup != null)
                    {
                        pamUserChannelGroup = txtPAMChannelGroup.Text;

                        TextBox txtAuthKey = control.FindName("txtAuthKey") as TextBox;
                        if (txtAuthKey != null)
                        {
                            pamAuthKey = txtAuthKey.Text;
                        }

                        RadioButton radGrantPAMChannelGroup = control.FindName("radGrantChannelGroup") as RadioButton;
                        if (radGrantPAMChannelGroup != null && radGrantPAMChannelGroup.IsChecked.Value)
                        {
                            await DisplayMessageInTextBox("Running ChannelGroupGrantAccess:").ConfigureAwait(false);
                            pubnub.Grant()
                            .ChannelGroups(new[] { pamUserChannelGroup })
                            .AuthKeys(new[] { pamAuthKey })
                            .Read(true)
                            .Write(true)
                            .TTL(ttlInMinutes)
                            .Async(new PNAccessManagerGrantResultExt(async (r, s) =>
                            {
                                if (r != null)
                                {
                                    await DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)).ConfigureAwait(false);
                                }
                            }));
                        }

                        RadioButton radAuditPAMChannelGroup = control.FindName("radAuditChannelGroup") as RadioButton;
                        if (radAuditPAMChannelGroup != null && radAuditPAMChannelGroup.IsChecked.Value)
                        {
                            await DisplayMessageInTextBox("Running ChannelGroupAuditAccess:").ConfigureAwait(false);
                            pubnub.Audit()
                            .ChannelGroup(pamUserChannelGroup)
                            .AuthKeys(new[] { pamAuthKey })
                            .Async(new PNAccessManagerAuditResultExt(
                                async (r, s) =>
                                {
                                    if (r != null)
                                    {
                                        await DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)).ConfigureAwait(false);
                                    }
                                }));
                        }

                        RadioButton radRevokePAMChannelGroup = control.FindName("radRevokeChannelGroup") as RadioButton;
                        if (radRevokePAMChannelGroup != null && radRevokePAMChannelGroup.IsChecked.Value)
                        {
                            await DisplayMessageInTextBox("Running ChannelGroup Revoke Access:").ConfigureAwait(false);
                            pubnub.Grant()
                            .ChannelGroups(new[] { pamUserChannelGroup })
                            .AuthKeys(new[] { pamAuthKey })
                            .Read(false)
                            .Write(false)
                            .TTL(ttlInMinutes)
                            .Async(new PNAccessManagerGrantResultExt(async (r, s) =>
                            {
                                if (r != null)
                                {
                                    await DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)).ConfigureAwait(false);
                                }
                            }));
                        }
                    }
                }
                pamChannelGroupPopup = null;
                this.IsEnabled = true;
            };

        }

        private void btnUserState_Click(object sender, RoutedEventArgs e)
        {
            channel = txtChannel.Text;
            channelGroup = txtChannelGroup.Text;
            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel userStateStackPanel = new StackPanel();
            userStateStackPanel.Background = new SolidColorBrush(Colors.Blue);

            userStatePopup = new Popup();
            userStatePopup.Height = 300;
            userStatePopup.Width = 300;

            userStatePopup.HorizontalOffset = 10;
            userStatePopup.VerticalOffset = 100;

            UserStateUserControl control = new UserStateUserControl();
            TextBox txtGetUserStateUUID = control.FindName("txtGetUserStateUUID") as TextBox;
            if (txtGetUserStateUUID != null)
            {
                txtGetUserStateUUID.Text = data.sessionUUID;
            }

            userStateStackPanel.Children.Add(control);
            border.Child = userStateStackPanel;

            userStatePopup.Child = border;
            userStatePopup.IsOpen = true;

            userStatePopup.Closed += async (senderPopup, argsPopup) =>
            {
                if (control.IsOKButtonEntered)
                {
                    if (control.IsSetUserState)
                    {
                        string userStateKey1 = "";
                        string userStateValue1 = "";
                        TextBox txtSetUserStateKey1 = control.FindName("txtKey1") as TextBox;
                        if (txtSetUserStateKey1 != null)
                        {
                            userStateKey1 = txtSetUserStateKey1.Text;
                        }
                        TextBox txtSetUserStateVal1 = control.FindName("txtValue1") as TextBox;
                        if (txtSetUserStateVal1 != null)
                        {
                            userStateValue1 = txtSetUserStateVal1.Text;
                        }

                        await DisplayMessageInTextBox("Running Set User State:").ConfigureAwait(false);

                        int valueInt;
                        double valueDouble;
                        if (Int32.TryParse(userStateValue1, out valueInt))
                        {
                            Dictionary<string, object> dicInt = new Dictionary<string, object>();
                            dicInt.Add(userStateKey1, valueInt);

                            pubnub.SetPresenceState()
                            .Channels(new[] { channel })
                            .ChannelGroups(new[] { channelGroup })
                            .State(dicInt)
                            .Async(new PNSetStateResultExt(
                                async (r, s) =>
                                {
                                    await DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)).ConfigureAwait(false);
                                }));
                        }
                        else if (Double.TryParse(userStateValue1, out valueDouble))
                        {
                            Dictionary<string, object> dicDouble = new Dictionary<string, object>();
                            dicDouble.Add(userStateKey1, valueDouble);

                            pubnub.SetPresenceState()
                            .Channels(new[] { channel })
                            .ChannelGroups(new[] { channelGroup })
                            .State(dicDouble)
                            .Async(new PNSetStateResultExt(
                                async (r, s) =>
                                {
                                    await DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)).ConfigureAwait(false);
                                }));
                        }
                        else
                        {
                            Dictionary<string, object> dicObj = new Dictionary<string, object>();
                            dicObj.Add(userStateKey1, userStateValue1);

                            pubnub.SetPresenceState()
                            .Channels(new[] { channel })
                            .ChannelGroups(new[] { channelGroup })
                            .State(dicObj)
                            .Async(new PNSetStateResultExt(
                                async (r, s) =>
                                {
                                    await DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)).ConfigureAwait(false);
                                }));
                        }
                    }
                    else if (control.IsGetUserState)
                    {
                        txtGetUserStateUUID = control.FindName("txtGetUserStateUUID") as TextBox;
                        if (txtGetUserStateUUID != null)
                        {
                            await DisplayMessageInTextBox("Running Get User State:").ConfigureAwait(false);
                            string userStateUUID = txtGetUserStateUUID.Text.Trim();

                            pubnub.GetPresenceState()
                            .Channels(new[] { channel })
                            .ChannelGroups(new[] { channelGroup })
                            .Uuid(userStateUUID)
                            .Async(new PNGetStateResultExt(
                                async (r, s) =>
                                {
                                    await DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)).ConfigureAwait(false);
                                }));
                        }
                    }
                }
                userStatePopup = null;
                this.IsEnabled = true;
            };
        }

        private void btnWhereNow_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel whereNowStackPanel = new StackPanel();
            whereNowStackPanel.Background = new SolidColorBrush(Colors.Blue);
            whereNowStackPanel.Width = 300;
            whereNowStackPanel.Height = 300;

            whereNowPopup = new Popup();
            whereNowPopup.Height = 300;
            whereNowPopup.Width = 300;

            whereNowPopup.HorizontalOffset = 10;
            whereNowPopup.VerticalOffset = 100;

            WhereNowUserControl control = new WhereNowUserControl();
            TextBox txtWhereNowUUID = control.FindName("txtWhereNowUUID") as TextBox;
            if (txtWhereNowUUID != null)
            {
                txtWhereNowUUID.Text = data.sessionUUID;
            }

            whereNowStackPanel.Children.Add(control);
            border.Child = whereNowStackPanel;

            whereNowPopup.Child = border;
            whereNowPopup.IsOpen = true;

            whereNowPopup.Closed += async (senderPopup, argsPopup) =>
            {
                if (control.IsOKButtonEntered)
                {
                    TextBox txtWhereNowUUIDConfirm = control.FindName("txtWhereNowUUID") as TextBox;
                    if (txtWhereNowUUIDConfirm != null)
                    {
                        string whereNowUUID = txtWhereNowUUIDConfirm.Text.Trim();

                        await DisplayMessageInTextBox("Running WhereNow:").ConfigureAwait(false);

                        pubnub.WhereNow()
                        .Uuid(whereNowUUID)
                        .Async(new PNWhereNowResultExt(async (r, s) =>
                        {
                            await DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)).ConfigureAwait(false);
                        }));
                    }

                }
                whereNowPopup = null;
                this.IsEnabled = true;
            };
        }

        private void btnChangeUUID_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel changeUUIDStackPanel = new StackPanel();
            changeUUIDStackPanel.Background = new SolidColorBrush(Colors.Blue);
            changeUUIDStackPanel.Width = 400;
            changeUUIDStackPanel.Height = 300;

            changeUUIDPopup = new Popup();
            changeUUIDPopup.Height = 300;
            changeUUIDPopup.Width = 300;

            changeUUIDPopup.HorizontalOffset = 10;
            changeUUIDPopup.VerticalOffset = 100;

            ChangeUUIDUserControl control = new ChangeUUIDUserControl();
            TextBlock tbCurrentUUID = control.FindName("lblCurrentUUID") as TextBlock;
            if (tbCurrentUUID != null)
            {
                tbCurrentUUID.Text = data.sessionUUID;
            }

            changeUUIDStackPanel.Children.Add(control);
            border.Child = changeUUIDStackPanel;

            changeUUIDPopup.Child = border;
            changeUUIDPopup.IsOpen = true;

            changeUUIDPopup.Closed += (senderPopup, argsPopup) =>
            {
                if (!control.IsCancelledButton)
                {
                    TextBox txtNewUUID = control.FindName("txtNewUUID") as TextBox;
                    if (txtNewUUID != null)
                    {
                        System.Threading.Tasks.Task.Run(() =>
                        {
                            data.sessionUUID = txtNewUUID.Text;
                            pubnub.ChangeUUID(data.sessionUUID);
                        });
                    }
                }
                changeUUIDPopup = null;
                this.IsEnabled = true;
            };

        }

        private void btnChannelGroup_Click(object sender, RoutedEventArgs e)
        {
            channelGroup = txtChannelGroup.Text;

            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel channelGroupStackPanel = new StackPanel();
            channelGroupStackPanel.Background = new SolidColorBrush(Colors.Blue);
            channelGroupStackPanel.Width = 400;
            channelGroupStackPanel.Height = 600;

            Popup channelGroupPopup = new Popup();
            channelGroupPopup.Height = 300;
            channelGroupPopup.Width = 300;

            channelGroupPopup.HorizontalOffset = 10;
            channelGroupPopup.VerticalOffset = 100;

            ChannelGroupUserControl control = new ChannelGroupUserControl();

            TextBox txtPAMChannelGroup = control.FindName("txtChannelGroup") as TextBox;
            if (txtPAMChannelGroup != null)
            {
                txtPAMChannelGroup.Text = channelGroup;
            }
            TextBox txtChannelName = control.FindName("txtChannelName") as TextBox;
            if (txtChannelName != null)
            {
                txtChannelName.Text = "ch1";
            }

            channelGroupStackPanel.Children.Add(control);
            border.Child = channelGroupStackPanel;

            channelGroupPopup.Child = border;
            channelGroupPopup.IsOpen = true;

            channelGroupPopup.Closed += async (senderPopup, argsPopup) =>
            {
                if (control.IsOKButtonEntered)
                {
                    string userChannelGroup = "";
                    string userChannelName = "";

                    txtPAMChannelGroup = control.FindName("txtChannelGroup") as TextBox;
                    if (txtPAMChannelGroup != null)
                    {
                        userChannelGroup = txtPAMChannelGroup.Text;

                        txtChannelName = control.FindName("txtChannelName") as TextBox;
                        if (txtChannelName != null)
                        {
                            userChannelName = txtChannelName.Text;
                        }
                        RadioButton radGetChannelsOfChannelGroup = control.FindName("radGetChannelsOfChannelGroup") as RadioButton;
                        if (radGetChannelsOfChannelGroup != null && radGetChannelsOfChannelGroup.IsChecked.Value)
                        {
                            await DisplayMessageInTextBox("Running GetChannelsForChannelGroup:").ConfigureAwait(false);
                            pubnub.ListChannelsForChannelGroup()
                            .ChannelGroup(userChannelGroup)
                            .Async(new PNChannelGroupsAllChannelsResultExt(async (r, s) =>
                            {
                                await DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)).ConfigureAwait(false);
                            }));
                        }

                        RadioButton radAddChannelToChannelGroup = control.FindName("radAddChannelToChannelGroup") as RadioButton;
                        if (radAddChannelToChannelGroup != null && radAddChannelToChannelGroup.IsChecked.Value)
                        {
                            await DisplayMessageInTextBox("Running AddChannelsToChannelGroup:").ConfigureAwait(false);
                            pubnub.AddChannelsToChannelGroup()
                            .Channels(new[] { userChannelName })
                            .ChannelGroup(userChannelGroup)
                            .Async(new PNChannelGroupsAddChannelResultExt(async (r, s) =>
                            {
                                await DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)).ConfigureAwait(false);
                            }));
                        }

                        RadioButton radRemoveChannelFromChannelGroup = control.FindName("radRemoveChannelFromChannelGroup") as RadioButton;
                        if (radRemoveChannelFromChannelGroup != null && radRemoveChannelFromChannelGroup.IsChecked.Value)
                        {
                            await DisplayMessageInTextBox("Running RemoveChannelsFromChannelGroup:").ConfigureAwait(false);
                            pubnub.RemoveChannelsFromChannelGroup()
                            .Channels(new[] { userChannelName })
                            .ChannelGroup(userChannelGroup)
                            .Async(new PNChannelGroupsRemoveChannelResultExt(async (r, s) =>
                            {
                                await DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)).ConfigureAwait(false);
                            }));
                        }
                    }
                }
                channelGroupPopup = null;
                this.IsEnabled = true;
            };

        }

        private void btnDisconnectRetry_Click(object sender, RoutedEventArgs e)
        {
            pubnub.TerminateCurrentSubscriberRequest();
        }

        private async Task DisplayMessageInTextBox(string msg)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                string modifiedMsg = "";
                if (msg.Length > 200)
                {
                    modifiedMsg = string.Concat(msg.Substring(0, 200), "..(truncated)");
                }
                else
                {
                    modifiedMsg = msg;
                }

                if (txtResult.Text.Length > 200)
                {
                    txtResult.Text = string.Concat("(Truncated)..\n", txtResult.Text.Remove(0, 200));
                }

                txtResult.Text += modifiedMsg + "\n";
                txtResult.Select(txtResult.Text.Length - 1, 1);
            });
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            PubnubCleanup();
        }

        static void PubnubCleanup()
        {
            if (pubnub != null)
            {
                pubnub.Destroy();
                pubnub = null;
            }
        }

        private async void txtResult_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            MessageDialog messageDialog = new MessageDialog("Confirm Delete");

            messageDialog.Commands.Add(new UICommand("Delete", new UICommandInvokedHandler(this.CommandInvokedHandler)));
            messageDialog.Commands.Add(new UICommand("Cancel", new UICommandInvokedHandler(this.CommandInvokedHandler)));

            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 0;

            // Set the command to be invoked when escape is pressed
            messageDialog.CancelCommandIndex = 1;

            // Show the message dialog
            await messageDialog.ShowAsync();

        }

        private void CommandInvokedHandler(IUICommand command)
        {
            if (command.Label == "Delete")
            {
                txtResult.Text = "";
            }
        }
    }

    public class PlatformPubnubLog : IPubnubLog
    {
        public void WriteToLog(string log)
        {
            System.Diagnostics.Debug.WriteLine(log);
        }
    }


    public class DemoSubscribeCallback : SubscribeCallback
    {
        readonly Action<string> callback;
        readonly Pubnub pubnub = new Pubnub(null);
        public DemoSubscribeCallback(Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void Message<T>(Pubnub pubnub, PNMessageResult<T> message)
        {
            if (message != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(message));
            }
        }

        public override void Presence(Pubnub pubnub, PNPresenceEventResult presence)
        {
            if (presence != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(presence));
            }
        }

        public override void Status(Pubnub pubnub, PNStatus status)
        {
            string msg = string.Format("Operation: {0}; Category: {1};  StatusCode: {2}", status.Operation, status.Category, status.StatusCode);
            this.callback(msg);

            if (status.Category == PNStatusCategory.PNUnexpectedDisconnectCategory)
            {
                // This event happens when radio / connectivity is lost
            }
            else if (status.Category == PNStatusCategory.PNConnectedCategory)
            {
                //Debug.WriteLine("CONNECTED {0} Channels = {1}, ChannelGroups = {2}", status.StatusCode, string.Join(",", status.AffectedChannels), string.Join(",", status.AffectedChannelGroups));
                // Connect event. You can do stuff like publish, and know you'll get it.
                // Or just use the connected event to confirm you are subscribed for
                // UI / internal notifications, etc

            }
            else if (status.Category == PNStatusCategory.PNReconnectedCategory)
            {
                //Debug.WriteLine("RE-CONNECTED {0} Channels = {1}, ChannelGroups = {2}", status.StatusCode, string.Join(",", status.AffectedChannels), string.Join(",", status.AffectedChannelGroups));
                // Happens as part of our regular operation. This event happens when
                // radio / connectivity is lost, then regained.
            }
            else if (status.Category == PNStatusCategory.PNDecryptionErrorCategory)
            {
                // Handle messsage decryption error. Probably client configured to
                // encrypt messages and on live data feed it received plain text.
            }
        }
    }

}
