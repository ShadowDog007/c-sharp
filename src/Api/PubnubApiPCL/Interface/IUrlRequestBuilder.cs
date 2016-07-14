﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi.Interface
{
    public interface IUrlRequestBuilder
    {
        Uri BuildTimeRequest();

        Uri BuildPublishRequest(string channel, object originalMessage, bool storeInHistory, string jsonUserMetaData);

        Uri BuildHereNowRequest(string[] channels, string[] channelGroups, bool showUUIDList, bool includeUserState);

        Uri BuildHistoryRequest(string channel, long start, long end, int count, bool reverse, bool includeToken);

        Uri BuildGlobalHereNowRequest(bool showUUIDList, bool includeUserState);

        Uri BuildWhereNowRequest(string uuid);

        Uri BuildGrantAccessRequest(string channelsCommaDelimited, string channelGroupsCommaDelimited, string authKeysCommaDelimited, bool read, bool write, bool manage, int ttl);

        Uri BuildAuditAccessRequest(string channel, string channelGroup, string authKeysCommaDelimited);

        Uri BuildGetUserStateRequest(string channelsCommaDelimited, string channelGroupsCommaDelimited, string uuid);

        Uri BuildSetUserStateRequest(string channelsCommaDelimited, string channelGroupsCommaDelimited, string uuid, string jsonUserState);

    }
}
