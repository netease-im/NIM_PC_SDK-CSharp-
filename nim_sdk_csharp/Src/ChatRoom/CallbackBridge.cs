﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NIMChatRoom
{
    //如果错误码为kResRoomLocalNeedRequestAgain，聊天室重连机制结束，则需要向IM服务器重新请求进入该聊天室权限
    public delegate void ChatRoomLoginDelegate(NIMChatRoomLoginStep loginStep, NIM.ResponseCode errorCode, ChatRoomInfo roomInfo, MemberInfo memberInfo);

    public delegate void ExitChatRoomDelegate(long roomId, NIM.ResponseCode errorCode, NIMChatRoomExitReason reason);

    public delegate void QueryMembersResultDelegate(long roomId, NIM.ResponseCode errorCode, MemberInfo[] members);

    public delegate void QueryMessageHistoryResultDelegate(long roomId, NIM.ResponseCode errorCode, Message[] messages);

    public delegate void SetMemberPropertyDelegate(long roomId,NIM.ResponseCode errorCode,MemberInfo info);

    public delegate void CloseRoomDelegate(long roomId, NIM.ResponseCode errorCode);

    public delegate void RemoveMemberDelegate(long roomId, NIM.ResponseCode errorCode);

    public delegate void GetRoomInfoDelegate(long roomId,NIM.ResponseCode errorCode,ChatRoomInfo info);

    public delegate void TempMuteMemberDelegate(long roomId, NIM.ResponseCode errorCode, MemberInfo info);

    internal static class CallbackBridge
    {
        public static readonly NimChatroomGetMembersCbFunc OnQueryChatMembersCompleted = (roomId, errorCode, result, jsonExtension, userData) =>
        {
            MemberInfo[] members = null;
            if (errorCode == (int)NIM.ResponseCode.kNIMResSuccess)
            {
                members = NimUtility.Json.JsonParser.Deserialize<MemberInfo[]>(result);
            }
            NimUtility.DelegateConverter.InvokeOnce<QueryMembersResultDelegate>(userData, roomId, (NIM.ResponseCode)errorCode, members);
        };

        public static readonly NimChatroomGetMsgCbFunc OnQueryMsgHistoryCompleted = (roomId, errorCode, result, jsonExtension, userData) =>
        {
            Message[] messages = null;
            var code = (NIM.ResponseCode) errorCode;
            if (code == NIM.ResponseCode.kNIMResSuccess)
            {
                messages = NimUtility.Json.JsonParser.Deserialize<Message[]>(result);
            }
            NimUtility.DelegateConverter.InvokeOnce<QueryMessageHistoryResultDelegate>(userData, roomId, code, messages);
        };

        public static readonly NimChatroomSetMemberAttributeCbFunc OnSetMemberPropertyCompleted = (roomId, errorCode, result, jsonExtension, userData) =>
        {
            MemberInfo mi = null;
            var code = (NIM.ResponseCode) errorCode;
            if (code == NIM.ResponseCode.kNIMResSuccess)
                mi = NimUtility.Json.JsonParser.Deserialize<MemberInfo>(result);
            NimUtility.DelegateConverter.InvokeOnce<SetMemberPropertyDelegate>(userData, roomId, code, mi);
        };

        public static readonly NimChatroomCloseCbFunc OnRoomClosed = (roomId, errorCode, jsonExtension, userData) =>
        {
            NimUtility.DelegateConverter.InvokeOnce<CloseRoomDelegate>(userData, roomId, (NIM.ResponseCode) errorCode);
        };

        public static readonly NimChatroomGetInfoCbFunc OnGetRoomInfoCompleted = (roomId, errorCode, result, jsonExtension, userData) =>
        {
            ChatRoomInfo roomInfo = null;
            var code = (NIM.ResponseCode) errorCode;
            if (code == NIM.ResponseCode.kNIMResSuccess)
                roomInfo = NimUtility.Json.JsonParser.Deserialize<ChatRoomInfo>(result);
            NimUtility.DelegateConverter.InvokeOnce<GetRoomInfoDelegate>(userData, roomId, code, roomInfo);
        };

        public static readonly NimChatroomKickMemberCbFunc OnRemoveMemberCompleted = (roomId, errorCode, jsonExtension, userData) =>
        {
            NimUtility.DelegateConverter.InvokeOnce<RemoveMemberDelegate>(userData, roomId, (NIM.ResponseCode)errorCode);
        };

        public static readonly nim_chatroom_temp_mute_member_cb_func TempMuteMemberCallback = (roomId, resCode, result, jsonExt, userData) =>
        {
            if (userData != IntPtr.Zero)
            {
                var info = MemberInfo.Deserialize(result);
                NimUtility.DelegateConverter.InvokeOnce<TempMuteMemberDelegate>(userData, roomId, (NIM.ResponseCode)resCode, info);
            }
        };
    }
}
