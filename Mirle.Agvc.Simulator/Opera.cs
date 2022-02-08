using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using com.mirle.iibg3k0.ttc.Common;
using com.mirle.iibg3k0.ttc.Common.TCPIP;
using Google.Protobuf.Collections;
using Mirle.Agvc.Simulator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.App;

namespace Mirle.Agvc.Simulator
{
    class Opera
    {
        private EventHandler<string> OnCmdSend;
        private EnumOperaType operaType;
        private LoggerAgent theLoggerAgent;
        private CMDCancelType cmdCanceltype = CMDCancelType.CmdNone;
        private ID_144_STATUS_CHANGE_REP theVehicleInfo;
        private TcpIpAgent ServerClientAgent;
        private ID_36_TRANS_EVENT_RESPONSE recent36Response;
        private ID_31_TRANS_REQUEST recent31Cmd;
        private ID_37_TRANS_CANCEL_REQUEST iD_37_TRANS_Cmd;
        private MiddlerConfigs middlerConfigs;
        private MiddleAgent agent;

        private SCApplication scApp = null;

        private object sendRecv_LockObj = new object();

        public Opera(MiddleAgent agent)
        {
            scApp = SCApplication.getInstance();
            this.agent = agent;
        }

        public bool Selected_Operate_Type(EnumOperaType selectedOpera)
        {
            bool isSuccess = true;
            operaType = selectedOpera;
            isSuccess = isSuccess && operaType == selectedOpera;
            return isSuccess;
        }

        public bool StartFeedBack31(ID_31_TRANS_REQUEST transRequest, ID_144_STATUS_CHANGE_REP theVehicleInfo,
                                    LoggerAgent theLoggerAgent, TcpIpAgent ServerClientAgent, EventHandler<string> OnCmdSend)
        {
            try
            {
                cmdCanceltype = CMDCancelType.CmdNone;
                recent31Cmd = transRequest;
                WaitForOneSecond();
                SetBasicInform(transRequest, theVehicleInfo, theLoggerAgent, ServerClientAgent, OnCmdSend);
                switch (operaType)
                {
                    case EnumOperaType.NormalComplete:
                        NormalComplete(transRequest);
                        break;
                    case EnumOperaType.CancelComplete:
                        CancelComplete(transRequest);
                        break;
                    case EnumOperaType.AbortComplete:
                        AbortComplete(transRequest);
                        break;
                    case EnumOperaType.Abnormal_BcrReadFail:
                        Abnormal_BcrReadFail(transRequest);
                        break;
                    case EnumOperaType.Abnormal_BcrMismatch:
                        Abnormal_BcrMismatch(transRequest);
                        break;
                    case EnumOperaType.Abnormal_BcrDuplicate:
                        Abnormal_BcrDuplicate(transRequest);
                        break;
                    case EnumOperaType.Abnormal_DoubleStorage:
                        Abnormal_DoubleStorage(transRequest);
                        break;
                    case EnumOperaType.Abnormal_EmptyRetrieval:
                        Abnormal_EmptyRetrieval(transRequest);
                        break;
                    case EnumOperaType.InterlockError:
                        InterlockError(transRequest);
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
                return false;
            }
        }

        public bool StartFeedBack37(ID_37_TRANS_CANCEL_REQUEST transRequest, ID_144_STATUS_CHANGE_REP theVehicleInfo,
                                    LoggerAgent theLoggerAgent, TcpIpAgent ServerClientAgent, EventHandler<string> OnCmdSend)
        {
            try
            {
                iD_37_TRANS_Cmd = Deep_Clone<ID_37_TRANS_CANCEL_REQUEST>(transRequest);
                switch (iD_37_TRANS_Cmd.ActType)
                {
                    case CMDCancelType.CmdCancel:
                        cmdCanceltype = CMDCancelType.CmdCancel;
                        break;
                    case CMDCancelType.CmdAbort:
                        cmdCanceltype = CMDCancelType.CmdAbort;
                        break;
                }
                WaitForOneSecond();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private com.mirle.iibg3k0.ttc.Common.TrxTcpIp.ReturnCode snedRecv<TSource>(WrapperMessage wrapper, out TSource stRecv, out string rtnMsg)
        {
            lock (sendRecv_LockObj)
            {
                TrxTcpIp.ReturnCode returncode;
                bool retrySendRecv = true;
                stRecv = default(TSource);
                rtnMsg = "";
                returncode = TrxTcpIp.ReturnCode.SendDataFail;
                while (retrySendRecv == true)
                {
                    returncode = ServerClientAgent.TrxTcpIp.sendRecv_Google(wrapper, out stRecv, out rtnMsg);
                    retrySendRecv = false;
                }
                return returncode;
            }
        }

        private static void WaitForOneSecond()
        {
            SpinWait.SpinUntil(() => false, 1000);
        }

        private void SetBasicInform(ID_31_TRANS_REQUEST transRequest, ID_144_STATUS_CHANGE_REP theVehicleInfo, LoggerAgent theLoggerAgent, TcpIpAgent ServerClientAgent, EventHandler<string> OnCmdSend)
        {
            this.theLoggerAgent = theLoggerAgent;
            this.theVehicleInfo = theVehicleInfo;
            this.ServerClientAgent = ServerClientAgent;
            this.OnCmdSend = OnCmdSend;

            theVehicleInfo.CmdID = transRequest.CmdID;
            theVehicleInfo.BOXID = transRequest.BOXID;
            theVehicleInfo.CSTID = transRequest.CSTID;
            theVehicleInfo.LOTID = transRequest.LOTID;
        }
        // All OK
        private void NormalComplete(ID_31_TRANS_REQUEST transRequest)
        {
            switch (transRequest.ActType)
            {
                case ActiveType.Move:
                    ChangeTheAddressSectionToUnloadAddress(transRequest);
                    Send_Cmd132_TransferCompleteReport(transRequest);
                    break;
                case ActiveType.Cyclemove:
                    agent.IsCycleMove = true;
                    SetVehicleStatus(VHActionStatus.Commanding);
                    Send_Cmd144_StatusChangeReport(false);
                    ChangeTheAddressSectionByCycleMoveMode(transRequest);
                    break;
                case ActiveType.Load:
                    LoadProcess(transRequest);
                    Send_Cmd136_TransferEventReport(EventType.Bcrread, true, transRequest);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd132_TransferCompleteReport(transRequest);
                    break;
                case ActiveType.Unload:
                    UnloadProcess(transRequest);
                    Send_Cmd132_TransferCompleteReport(transRequest);
                    break;
                case ActiveType.Loadunload:
                    SetVehicleStatus(VHActionStatus.Commanding);
                    Send_Cmd144_StatusChangeReport(false);

                    LoadProcess(transRequest);
                    Send_Cmd136_TransferEventReport(EventType.Bcrread, true, transRequest);
                    SetVehicleCSTStatus(true);
                    Send_Cmd144_StatusChangeReport(true);
                    SpinWait.SpinUntil(() => false, 200);
                    UnloadProcess(transRequest);
                    SetVehicleCSTStatus(false);
                    Send_Cmd144_StatusChangeReport(false);

                    Send_Cmd132_TransferCompleteReport(transRequest);
                    SetVehicleStatus(VHActionStatus.NoCommand);
                    Send_Cmd144_StatusChangeReport(false);
                    break;
                case ActiveType.Scan:
                    LoadProcess(transRequest);
                    Send_Cmd136_TransferEventReport(EventType.Bcrread, true, transRequest);
                    SpinWait.SpinUntil(() => false, 1000);
                    UnloadProcess(transRequest);
                    Send_Cmd132_TransferCompleteReport(transRequest);
                    break;
            }

        }

        private void SetVehicleStatus(VHActionStatus vhActionStatus)
        {
            switch (vhActionStatus)
            {
                case VHActionStatus.Commanding:
                    theVehicleInfo.ActionStatus = VHActionStatus.Commanding;
                    break;
                case VHActionStatus.NoCommand:
                    theVehicleInfo.ActionStatus = VHActionStatus.NoCommand;
                    theVehicleInfo.CmdID = "";
                    theVehicleInfo.BOXID = "";
                    theVehicleInfo.CSTID = "";
                    theVehicleInfo.LOTID = "";
                    break;
            }

        }

        private void SetVehicleCSTStatus(bool hasCST)
        {
            theVehicleInfo.HasCst = hasCST?VhLoadCarrierStatus.Exist:VhLoadCarrierStatus.NotExist;
        }

        //TESTING
        private void CancelComplete(ID_31_TRANS_REQUEST transRequest)
        {
            switch (transRequest.ActType)
            {
                case ActiveType.Move:
                    ChangeTheAddressSectionToUnloadAddress(transRequest);
                    while (cmdCanceltype == CMDCancelType.CmdNone)
                    {
                        SpinWait.SpinUntil(() => false, 1000);
                    }

                    Send_Cmd132_TransferCompleteReport(transRequest);
                    break;
                case ActiveType.Load:
                    ChangeTheAddressSectionToUnloadAddress(transRequest);
                    while (cmdCanceltype == CMDCancelType.CmdNone)
                    {
                        SpinWait.SpinUntil(() => false, 1000);
                    }
                    Send_Cmd132_TransferCompleteReport(transRequest);
                    break;
                case ActiveType.Unload:
                    while (cmdCanceltype == CMDCancelType.CmdNone)
                    {
                        SpinWait.SpinUntil(() => false, 1000);
                    }
                    UnloadProcess(transRequest);
                    Send_Cmd132_TransferCompleteReport(transRequest);
                    break;
                case ActiveType.Loadunload:
                    while (cmdCanceltype == CMDCancelType.CmdNone)
                    {
                        SpinWait.SpinUntil(() => false, 1000);
                    }
                    Send_Cmd132_TransferCompleteReport(transRequest);
                    break;
                case ActiveType.Scan:
                    ChangeTheAddressSectionToUnloadAddress(transRequest);
                    while (cmdCanceltype == CMDCancelType.CmdNone)
                    {
                        SpinWait.SpinUntil(() => false, 1000);
                    }
                    Send_Cmd132_TransferCompleteReport(transRequest);
                    break;
            }
        }
        //TESTING
        private void AbortComplete(ID_31_TRANS_REQUEST transRequest)
        {
            switch (transRequest.ActType)
            {
                case ActiveType.Unload:
                    while (cmdCanceltype == CMDCancelType.CmdNone)
                    {
                        SpinWait.SpinUntil(() => false, 1000);
                    }
                    Send_Cmd132_TransferCompleteReport(transRequest);
                    break;
                case ActiveType.Loadunload:
                    LoadProcess(transRequest);
                    Send_Cmd136_TransferEventReport(EventType.Bcrread, true, transRequest);
                    SpinWait.SpinUntil(() => false, 1000);
                    while (cmdCanceltype == CMDCancelType.CmdNone)
                    {
                        SpinWait.SpinUntil(() => false, 1000);
                    }
                    Send_Cmd132_TransferCompleteReport(transRequest);
                    break;
                case ActiveType.Scan:
                    LoadProcess(transRequest);
                    Send_Cmd136_TransferEventReport(EventType.Bcrread, true, transRequest);
                    SpinWait.SpinUntil(() => false, 1000);
                    while (cmdCanceltype == CMDCancelType.CmdNone)
                    {
                        SpinWait.SpinUntil(() => false, 1000);
                    }
                    Send_Cmd132_TransferCompleteReport(transRequest);
                    break;
            }
        }
        // Testing
        private void InterlockError(ID_31_TRANS_REQUEST transRequest)
        {
            switch (transRequest.ActType)
            {
                case ActiveType.Move:
                    ChangeTheAddressSectionToUnloadAddress(transRequest);
                    Send_Cmd132_TransferCompleteReport(transRequest);
                    break;
                case ActiveType.Load:
                    Send_Cmd136_TransferEventReport(EventType.LoadArrivals, true, transRequest);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd132_TransferCompleteReport(transRequest);
                    break;
                case ActiveType.Unload:
                    if (transRequest.ActType == ActiveType.Scan)
                    {
                        // Didn't have the unload move , but need to unload the box to the same place where it was loaded.
                    }
                    else
                    {
                        ChangeTheAddressSectionToUnloadAddress(transRequest);
                    }
                    Send_Cmd136_TransferEventReport(EventType.UnloadArrivals, true, transRequest);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd132_TransferCompleteReport(transRequest);
                    break;
                case ActiveType.Loadunload:
                    if (transRequest.GuideAddressStartToLoad.Count > 0)
                    {
                        if (!scApp.isMatche(transRequest.GuideAddressStartToLoad[0], theVehicleInfo.CurrentAdrID))
                        {
                            ChangeTheAddressSectionToLoadAddress(transRequest);
                        }
                    }
                    Send_Cmd136_TransferEventReport(EventType.LoadArrivals, true, transRequest);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd132_TransferCompleteReport(transRequest);
                    break;
                case ActiveType.Scan:
                    ChangeTheAddressSectionToLoadAddress(transRequest);
                    Send_Cmd136_TransferEventReport(EventType.LoadArrivals, true, transRequest);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd132_TransferCompleteReport(transRequest);
                    break;
            }

        }
        // Testing
        private void Abnormal_BcrReadFail(ID_31_TRANS_REQUEST transRequest)
        {
            LoadProcess(transRequest);
            Send_Cmd136_TransferEventReport(EventType.Bcrread, true, transRequest, BCRReadResult.BcrReadFail);
            ID_36_TRANS_EVENT_RESPONSE the36Response = Deep_Clone<ID_36_TRANS_EVENT_RESPONSE>(recent36Response);
            if (the36Response.RenameBOXID != "")
            {
                theVehicleInfo.CarBoxID = the36Response.RenameBOXID;
            }
            if (the36Response.ReplyActiveType == CMDCancelType.CmdNone)
            {
                UnloadProcess(transRequest);
                Send_Cmd132_TransferCompleteReport(transRequest);
            }
            else if (the36Response.ReplyActiveType == CMDCancelType.CmdCancelIdReadFailed)
            {
                Send_Cmd132_TransferCompleteReport(transRequest, CMDCancelType.CmdCancelIdReadFailed);
            }
        }

        private void Abnormal_BcrMismatch(ID_31_TRANS_REQUEST transRequest)
        {
            LoadProcess(transRequest);
            Send_Cmd136_TransferEventReport(EventType.Bcrread, true, transRequest, BCRReadResult.BcrMisMatch);
            ID_36_TRANS_EVENT_RESPONSE the36Response = Deep_Clone<ID_36_TRANS_EVENT_RESPONSE>(recent36Response);
            if (the36Response.RenameBOXID != "")
            {
                theVehicleInfo.CarBoxID = the36Response.RenameBOXID;
            }
            if (the36Response.ReplyActiveType == CMDCancelType.CmdNone)
            {
                UnloadProcess(transRequest);
                Send_Cmd132_TransferCompleteReport(transRequest);
            }
            else if (the36Response.ReplyActiveType == CMDCancelType.CmdCancelIdMismatch)
            {
                Send_Cmd132_TransferCompleteReport(transRequest, CMDCancelType.CmdCancelIdMismatch);
            }
        }

        private void Abnormal_BcrDuplicate(ID_31_TRANS_REQUEST transRequest)
        {
            // Here has nothing to do.
        }

        private void Abnormal_DoubleStorage(ID_31_TRANS_REQUEST transRequest)
        {
            LoadProcess(transRequest);
            Send_Cmd136_TransferEventReport(EventType.Bcrread, true, transRequest);
            SpinWait.SpinUntil(() => false, 1000);
            if (transRequest.ActType == ActiveType.Scan)
            {
                // Didn't have the unload move , but need to unload the box to the same place where it was loaded.
            }
            else
            {
                ChangeTheAddressSectionToUnloadAddress(transRequest);
            }
            Send_Cmd136_TransferEventReport(EventType.UnloadArrivals, true, transRequest);
            Send_Cmd136_TransferEventReport(EventType.DoubleStorage, true, transRequest);
            SpinWait.SpinUntil(() => false, 1000);
            Send_Cmd132_TransferCompleteReport(transRequest);

        }

        private void DoubleStorageLoop(ID_31_TRANS_REQUEST transRequest)
        {
            Send_Cmd136_TransferEventReport(EventType.DoubleStorage, true, transRequest);
            ID_36_TRANS_EVENT_RESPONSE the36Response = Deep_Clone<ID_36_TRANS_EVENT_RESPONSE>(recent36Response);
            if (the36Response.ReplyActiveType == CMDCancelType.CmdNone)
            {
                //do nothing.
                while (cmdCanceltype == CMDCancelType.CmdNone)
                {
                    SpinWait.SpinUntil(() => false, 1000);
                }
                Send_Cmd132_TransferCompleteReport(transRequest);
            }
            else if (the36Response.ReplyActiveType == CMDCancelType.CmdRetry)
            {
                DoubleStorageLoop(transRequest);
            }
        }

        private void Abnormal_EmptyRetrieval(ID_31_TRANS_REQUEST transRequest)
        {
            ChangeTheAddressSectionToLoadAddress(transRequest);
            Send_Cmd136_TransferEventReport(EventType.LoadArrivals, true, transRequest);
            SpinWait.SpinUntil(() => false, 1000);
            //EmptyRetrievalLoop(transRequest);
            Send_Cmd136_TransferEventReport(EventType.EmptyRetrieval, true, transRequest);
            Send_Cmd132_TransferCompleteReport(transRequest);
        }

        private void EmptyRetrievalLoop(ID_31_TRANS_REQUEST transRequest)
        {
            Send_Cmd136_TransferEventReport(EventType.EmptyRetrieval, true, transRequest);
            ID_36_TRANS_EVENT_RESPONSE the36Response = Deep_Clone<ID_36_TRANS_EVENT_RESPONSE>(recent36Response);
            if (the36Response.ReplyActiveType == CMDCancelType.CmdNone)
            {
                //do nothing.
                while (cmdCanceltype == CMDCancelType.CmdNone)
                {
                    SpinWait.SpinUntil(() => false, 1000);
                }
                Send_Cmd132_TransferCompleteReport(transRequest);
            }
            else if (the36Response.ReplyActiveType == CMDCancelType.CmdRetry)
            {
                EmptyRetrievalLoop(transRequest);
            }
        }

        private void LoadProcess(ID_31_TRANS_REQUEST transRequest)
        {
            if (transRequest.GuideAddressStartToLoad.Count > 0)
            {
                if (!scApp.isMatche(transRequest.GuideAddressStartToLoad[transRequest.GuideAddressStartToLoad.Count - 1], theVehicleInfo.CurrentAdrID))
                {
                    ChangeTheAddressSectionToLoadAddress(transRequest);
                }
            }
            Send_Cmd136_TransferEventReport(EventType.LoadArrivals, true, transRequest);
            SpinWait.SpinUntil(() => false, 300);
            Send_Cmd136_TransferEventReport(EventType.Vhloading, true, transRequest);
            SpinWait.SpinUntil(() => false, 300);
            Send_Cmd136_TransferEventReport(EventType.LoadComplete, true, transRequest);
            SpinWait.SpinUntil(() => false, 300);
            Send_Cmd144_StatusChangeReport(true);
        }

        private void UnloadProcess(ID_31_TRANS_REQUEST transRequest)
        {
            if (transRequest.ActType == ActiveType.Scan)
            {
                // Didn't have the unload move , but need to unload the box to the same place where it was loaded.
            }
            else
            {
                ChangeTheAddressSectionToUnloadAddress(transRequest);
            }
            Send_Cmd136_TransferEventReport(EventType.UnloadArrivals, true, transRequest);
            SpinWait.SpinUntil(() => false, 300);
            Send_Cmd136_TransferEventReport(EventType.Vhunloading, true, transRequest);
            SpinWait.SpinUntil(() => false, 300);
            Send_Cmd136_TransferEventReport(EventType.UnloadComplete, true, transRequest);
            SpinWait.SpinUntil(() => false, 300);
            Send_Cmd144_StatusChangeReport(false);
        }

        private void ChangeTheAddressSectionToUnloadAddress(ID_31_TRANS_REQUEST transRequest)
        {
            if (transRequest.GuideSectionsToDestination.Count > 0)
            {
                int sectionAddressCount = 0;
                for (sectionAddressCount = 0; sectionAddressCount < transRequest.GuideSectionsToDestination.Count;)
                {
                    if (!scApp.checkVhAddress(agent.RemotePort().ToString(), transRequest.GuideAddressToDestination[sectionAddressCount]))
                    {
                        SpinWait.SpinUntil(() => false, 2000);
                        continue;
                    }

                    string[] reserveSections = { transRequest.GuideSectionsToDestination[sectionAddressCount] };
                    DriveDirction[] reserveDirection = { DriveDirction.DriveDirForward };

                    Send_Cmd136_TransferEventReserveReport(EventType.ReserveReq, reserveSections, reserveDirection, true, transRequest);
                    if (recent36Response.IsReserveSuccess == ReserveResult.Success)
                    {
                        theVehicleInfo.CurrentSecID = transRequest.GuideSectionsToDestination[sectionAddressCount];
                        theVehicleInfo.CurrentAdrID = transRequest.GuideAddressToDestination[sectionAddressCount];
                        theVehicleInfo.ActionStatus = VHActionStatus.Commanding;

                        scApp.updateVhAddress(agent.RemotePort().ToString(), theVehicleInfo.CurrentAdrID);

                        //Send_Cmd144_StatusChangeReport(false);
                        Send_Cmd134_TransferEventReport();
                        SpinWait.SpinUntil(() => false, 2000);

                        sectionAddressCount++;
                    }

                    if (cmdCanceltype == CMDCancelType.CmdCancel || cmdCanceltype == CMDCancelType.CmdAbort)
                    {
                        theVehicleInfo.ActionStatus = VHActionStatus.NoCommand;
                        Send_Cmd144_StatusChangeReport(false);
                        Send_Cmd134_TransferEventReport();

                        return;
                    }
                }

                /*theVehicleInfo.CurrentAdrID = transRequest.ToAdr;
                theVehicleInfo.CurrentSecID = transRequest.GuideSectionsToDestination[sectionAddressCount - 1];
                theVehicleInfo.ActionStatus = VHActionStatus.NoCommand;

                scApp.updateVhAddress(agent.RemotePort().ToString(), theVehicleInfo.CurrentAdrID);

                Send_Cmd144_StatusChangeReport(false);
                Send_Cmd134_TransferEventReport();
                SpinWait.SpinUntil(() => false, 500);*/
            }
        }

        private void ChangeTheAddressSectionByCycleMoveMode(ID_31_TRANS_REQUEST transRequest)
        {
            if (!theVehicleInfo.CurrentAdrID.Trim().Equals("") || !theVehicleInfo.CurrentSecID.Trim().Equals(""))
            {
                while (agent.IsCycleMove) {
                    SectionData cur_section = scApp.SectionDataBLL.loadSectionByID(theVehicleInfo.CurrentSecID);
                    SectionData next_section = scApp.SectionDataBLL.loadSectionByFrom_Add_ID(cur_section.TO_ADR_ID);

                    if (cmdCanceltype == CMDCancelType.CmdCancel || cmdCanceltype == CMDCancelType.CmdAbort)
                    {
                        theVehicleInfo.ActionStatus = VHActionStatus.NoCommand;
                        Send_Cmd144_StatusChangeReport(false);
                        Send_Cmd134_TransferEventReport();

                        break;
                    }

                    if (!scApp.checkVhAddress(agent.RemotePort().ToString(), cur_section.TO_ADR_ID))
                    {
                        SpinWait.SpinUntil(() => false, 2000);
                    }
                    else {
                        string[] reserveSections = { next_section.SEC_ID };
                        DriveDirction[] reserveDirection = { DriveDirction.DriveDirForward };

                        Send_Cmd136_TransferEventReserveReport(EventType.ReserveReq, reserveSections, reserveDirection, false);
                        if (recent36Response.IsReserveSuccess == ReserveResult.Success)
                        {
                            theVehicleInfo.CurrentSecID = next_section.SEC_ID;
                            theVehicleInfo.CurrentAdrID = next_section.TO_ADR_ID;

                            scApp.updateVhAddress(agent.RemotePort().ToString(), theVehicleInfo.CurrentAdrID);

                            Send_Cmd134_TransferEventReport();
                            SpinWait.SpinUntil(() => false, 500);

                            if (!agent.destination.Equals(string.Empty) && theVehicleInfo.CurrentAdrID.Equals(agent.destination))
                            {
                                theVehicleInfo.CmdID = transRequest.CmdID.Trim();
                                SetVehicleStatus(VHActionStatus.NoCommand);
                                Send_Cmd132_TransferCompleteReport(transRequest);
                                break;
                            }

                            if (agent.destination.Equals(string.Empty))
                            {
                                AddressData addressData = scApp.AddressDataBLL.loadAddressByID(theVehicleInfo.CurrentAdrID);
                                if (addressData.SNED_ZONE.Equals("Y") && addressData.ZONE_ID != null)
                                {
                                    Send_Cmd136_ZoneCommandReq(EventType.ZoneCommandReq, addressData.ZONE_ID);
                                    if (!recent36Response.ZoneCommandPortID.Trim().Equals(""))
                                    {
                                        agent.destination = recent36Response.RenameLOTID;
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }

        private void ChangeTheAddressSectionToLoadAddress(ID_31_TRANS_REQUEST transRequest)
        {
            if (transRequest.GuideSectionsStartToLoad.Count > 0)
            {
                for (int sectionAddressCount = 0; sectionAddressCount < transRequest.GuideSectionsStartToLoad.Count;)
                {
                    if (!scApp.checkVhAddress(agent.RemotePort().ToString(), transRequest.GuideAddressStartToLoad[sectionAddressCount]))
                    {
                        SpinWait.SpinUntil(() => false, 2000);
                        continue;
                    }

                    string[] reserveSections = { transRequest.GuideSectionsStartToLoad[sectionAddressCount] };
                    DriveDirction[] reserveDirection = { DriveDirction.DriveDirForward };

                    Send_Cmd136_TransferEventReserveReport(EventType.ReserveReq, reserveSections, reserveDirection, true, transRequest);
                    if (recent36Response.IsReserveSuccess == ReserveResult.Success)
                    {
                        theVehicleInfo.CurrentSecID = transRequest.GuideSectionsStartToLoad[sectionAddressCount];
                        theVehicleInfo.CurrentAdrID = transRequest.GuideAddressStartToLoad[sectionAddressCount];

                        scApp.updateVhAddress(agent.RemotePort().ToString(), theVehicleInfo.CurrentAdrID);

                        Send_Cmd134_TransferEventReport();
                        SpinWait.SpinUntil(() => false, 3000);

                        sectionAddressCount++;
                    }

                    if (cmdCanceltype == CMDCancelType.CmdCancel || cmdCanceltype == CMDCancelType.CmdAbort)
                    {
                        theVehicleInfo.ActionStatus = VHActionStatus.NoCommand;
                        Send_Cmd144_StatusChangeReport(false);
                        Send_Cmd134_TransferEventReport();

                        return;
                    }
                }

                theVehicleInfo.CurrentAdrID = transRequest.LoadAdr;
                scApp.updateVhAddress(agent.RemotePort().ToString(), theVehicleInfo.CurrentAdrID);
                Send_Cmd134_TransferEventReport();
                SpinWait.SpinUntil(() => false, 250);
            }
        }
        public void Send_Cmd144_StatusChangeReport(bool hasCstBoxorNot)
        {
            try
            {
                ID_144_STATUS_CHANGE_REP iD_144_SendToOHTC = new ID_144_STATUS_CHANGE_REP();
                iD_144_SendToOHTC = Deep_Clone<ID_144_STATUS_CHANGE_REP>(theVehicleInfo);
                if (hasCstBoxorNot == true)
                {
                    setHaveCstBox144(iD_144_SendToOHTC);
                }
                else if (hasCstBoxorNot == false)
                {
                    setEmptyCstBox144(iD_144_SendToOHTC);
                }
                iD_144_SendToOHTC.SecDistance = 50;
                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.StatueChangeRepFieldNumber;
                wrappers.StatueChangeRep = iD_144_SendToOHTC;

                ServerClientAgent.TrxTcpIp.SendGoogleMsg(wrappers);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }

        }
        public void Send_Cmd134_TransferEventReport()
        {
            BlockData blockData = scApp.BlockDataBLL.loadBlockByID(theVehicleInfo.CurrentAdrID.Substring(0, 1) == "1" ? theVehicleInfo.CurrentAdrID : theVehicleInfo.CurrentSecID);
            double VhXAxis = Convert.ToDouble(blockData.XAxis);
            double VhYAxis = Convert.ToDouble(blockData.YAxis);
            double VhAngle = Convert.ToDouble(blockData.Angle);


            try
            {
                ID_134_TRANS_EVENT_REP iD_134_TRANS_EVENT_REP = new ID_134_TRANS_EVENT_REP();
                iD_134_TRANS_EVENT_REP.EventType = EventType.AdrPass;
                iD_134_TRANS_EVENT_REP.CurrentAdrID = theVehicleInfo.CurrentAdrID;
                iD_134_TRANS_EVENT_REP.CurrentSecID = theVehicleInfo.CurrentSecID;
                iD_134_TRANS_EVENT_REP.SecDistance = 50;
                iD_134_TRANS_EVENT_REP.XAxis = VhXAxis;
                iD_134_TRANS_EVENT_REP.YAxis = VhYAxis;
                iD_134_TRANS_EVENT_REP.Angle = VhAngle;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TransEventRepFieldNumber;
                wrappers.TransEventRep = iD_134_TRANS_EVENT_REP;

                ServerClientAgent.TrxTcpIp.SendGoogleMsg(wrappers);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        private void Send_Cmd136_ZoneCommandReq(EventType eventType,String Zone_ID)
        {
            try
            {
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                ID_36_TRANS_EVENT_RESPONSE iD_36_TRANS_EVENT_RESPONSE = new ID_36_TRANS_EVENT_RESPONSE();
                
                iD_136_TRANS_EVENT_REP.EventType = eventType;
                iD_136_TRANS_EVENT_REP.CurrentAdrID = theVehicleInfo.CurrentAdrID;
                iD_136_TRANS_EVENT_REP.CurrentSecID = theVehicleInfo.CurrentSecID;
                iD_136_TRANS_EVENT_REP.SecDistance = 0;
                iD_136_TRANS_EVENT_REP.ZoneCommandID = Zone_ID;
                //iD_136_TRANS_EVENT_REP.ZoneCommandID = "123";

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;

                SendCommandWrapper(wrappers, out iD_36_TRANS_EVENT_RESPONSE, false);
                recent36Response = iD_36_TRANS_EVENT_RESPONSE;
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }


        private void Send_Cmd136_TransferEventReport(EventType eventType, bool isCmd = false,
                                ID_31_TRANS_REQUEST now31Cmd = null, BCRReadResult bCRReadResult = BCRReadResult.BcrNormal)
        {
            try
            {
                string loadPortId = "";
                string unloadPortId = "";
                if (isCmd == false)
                {
                    // do nothing
                }
                else if (isCmd == true)
                {
                    loadPortId = now31Cmd.LoadPortID;
                    unloadPortId = now31Cmd.UnloadPortID;
                }
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                ID_36_TRANS_EVENT_RESPONSE iD_36_TRANS_EVENT_RESPONSE = new ID_36_TRANS_EVENT_RESPONSE();
                Set136Inform(eventType, bCRReadResult, loadPortId, unloadPortId, iD_136_TRANS_EVENT_REP);

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;

                SendCommandWrapper(wrappers, out iD_36_TRANS_EVENT_RESPONSE, false);
                recent36Response = iD_36_TRANS_EVENT_RESPONSE;
                //if (iD_36_TRANS_EVENT_RESPONSE.ReplyActiveType != CMDCancelType.CmdNone)
                //{
                //    switch (iD_36_TRANS_EVENT_RESPONSE.ReplyActiveType)
                //    {
                //        case CMDCancelType.CmdCancelIdReadFailed:
                //            Send_Cmd132_TransferCompleteReport(recent31Cmd);
                //            break;
                //        case CMDCancelType.CmdCancelIdMismatch:
                //            Send_Cmd132_TransferCompleteReport(recent31Cmd);
                //            break;
                //        case CMDCancelType.CmdCancelIdReadDuplicate:

                //            break;
                //        case CMDCancelType.CmdCancelIdReadForceFinish:

                //            break;
                //        case CMDCancelType.CmdAbort:

                //            break;
                //        case CMDCancelType.CmdCancel:

                //            break;
                //        case CMDCancelType.CmdRetry:
                //            //recent36Response = Deep_Clone<ID_36_TRANS_EVENT_RESPONSE>(iD_36_TRANS_EVENT_RESPONSE);
                //            //recent36Response = iD_36_TRANS_EVENT_RESPONSE;
                //            break;
                //        case CMDCancelType.CmdNone:
                //            //recent36Response = Deep_Clone<ID_36_TRANS_EVENT_RESPONSE>(iD_36_TRANS_EVENT_RESPONSE);
                //            //recent36Response = iD_36_TRANS_EVENT_RESPONSE;
                //            break;
                //    }
                //}
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        private void Send_Cmd136_TransferEventReserveReport(EventType eventType, string[] reserveSections, DriveDirction[] reserveDirections, bool isCmd = false,
                                ID_31_TRANS_REQUEST now31Cmd = null)
        {
            try
            {
                string loadPortId = "";
                string unloadPortId = "";
                if (isCmd == false)
                {
                    // do nothing
                }
                else if (isCmd == true)
                {
                    loadPortId = now31Cmd.LoadPortID;
                    unloadPortId = now31Cmd.UnloadPortID;
                }
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                ID_36_TRANS_EVENT_RESPONSE iD_36_TRANS_EVENT_RESPONSE = new ID_36_TRANS_EVENT_RESPONSE();

                iD_136_TRANS_EVENT_REP.EventType = eventType;
                iD_136_TRANS_EVENT_REP.BOXID = theVehicleInfo.BOXID;
                iD_136_TRANS_EVENT_REP.CurrentAdrID = theVehicleInfo.CurrentAdrID;
                iD_136_TRANS_EVENT_REP.CurrentSecID = theVehicleInfo.CurrentSecID;
                iD_136_TRANS_EVENT_REP.CSTID = theVehicleInfo.CSTID;
                iD_136_TRANS_EVENT_REP.LOTID = theVehicleInfo.LOTID;

                iD_136_TRANS_EVENT_REP.SecDistance = 0;
                iD_136_TRANS_EVENT_REP.LoadPortID = loadPortId;
                iD_136_TRANS_EVENT_REP.UnloadPortID = unloadPortId;

                GetReserveInfo(reserveSections, reserveDirections, iD_136_TRANS_EVENT_REP.ReserveInfos);

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;

                SendCommandWrapper(wrappers, out iD_36_TRANS_EVENT_RESPONSE, false);
                recent36Response = iD_36_TRANS_EVENT_RESPONSE;
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        private void GetReserveInfo(string[] reserveSections, DriveDirction[] reserveDirections, RepeatedField<ReserveInfo> reserveInfos)
        {
            if (reserveSections.Length > 0)
            {
                for (int i = 0; i < reserveSections.Length; i++)
                {
                    ReserveInfo reserveInfo = new ReserveInfo();
                    reserveInfo.ReserveSectionID = reserveSections[i];
                    reserveInfo.DriveDirction = reserveDirections[i];

                    reserveInfos.Add(reserveInfo);
                }
            }
        }

        private void Set136Inform(EventType eventType, BCRReadResult bCRReadResult, string loadPortId, string unloadPortId, ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP)
        {
            string Mismatch_ID = Global_Param.MismatchID;
            string BOXID = "";
            switch (bCRReadResult)
            {
                case BCRReadResult.BcrMisMatch:
                    BOXID = Mismatch_ID;
                    break;
                case BCRReadResult.BcrReadFail:
                    BOXID = "";
                    break;
                case BCRReadResult.BcrNormal:
                    BOXID = theVehicleInfo.BOXID;
                    break;
            }
            iD_136_TRANS_EVENT_REP.EventType = eventType;
            iD_136_TRANS_EVENT_REP.BOXID = BOXID;
            iD_136_TRANS_EVENT_REP.CurrentAdrID = theVehicleInfo.CurrentAdrID;
            iD_136_TRANS_EVENT_REP.CurrentSecID = theVehicleInfo.CurrentSecID;
            iD_136_TRANS_EVENT_REP.CSTID = theVehicleInfo.CSTID;
            iD_136_TRANS_EVENT_REP.LOTID = theVehicleInfo.LOTID;

            iD_136_TRANS_EVENT_REP.SecDistance = 0;
            iD_136_TRANS_EVENT_REP.BCRReadResult = bCRReadResult;
            iD_136_TRANS_EVENT_REP.LoadPortID = loadPortId;
            iD_136_TRANS_EVENT_REP.UnloadPortID = unloadPortId;
        }

        private void Send_Cmd132_TransferCompleteReport(ID_31_TRANS_REQUEST transRequest, CMDCancelType cMDCancelType = CMDCancelType.CmdNone)
        {
            try
            {
                ID_132_TRANS_COMPLETE_REPORT iD_132_TRANS_COMPLETE_REPORT = new ID_132_TRANS_COMPLETE_REPORT();
                ID_32_TRANS_COMPLETE_RESPONSE iD_32_TRANS_COMPLETE_RESPONSE = new ID_32_TRANS_COMPLETE_RESPONSE();
                if (operaType == EnumOperaType.InterlockError)
                {
                    iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusInterlockError;
                }
                else if (cmdCanceltype == CMDCancelType.CmdCancel || cmdCanceltype == CMDCancelType.CmdAbort)
                {
                    if (cmdCanceltype == CMDCancelType.CmdCancel)
                    {
                        iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusCancel;
                    }
                    if (cmdCanceltype == CMDCancelType.CmdAbort)
                    {
                        iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusAbort;
                    }
                }
                else
                {
                    switch (transRequest.ActType)
                    {
                        case ActiveType.Move:
                            if (cMDCancelType == CMDCancelType.CmdNone)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusMove;
                                setEmptyCarrier132(iD_132_TRANS_COMPLETE_REPORT);
                            }
                            break;
                        case ActiveType.Cyclemove:
                            if (cMDCancelType == CMDCancelType.CmdNone)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusCycleMove;
                                setEmptyCarrier132(iD_132_TRANS_COMPLETE_REPORT);
                                agent.IsCycleMove = false;
                                agent.destination = string.Empty;
                            }
                            break;
                        case ActiveType.Load:
                            if (cMDCancelType == CMDCancelType.CmdNone)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusLoad;
                                setHaveCstBox132(iD_132_TRANS_COMPLETE_REPORT, cMDCancelType);
                            }
                            else if (cMDCancelType == CMDCancelType.CmdCancelIdReadFailed)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIdreadFailed;
                                setHaveCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                                iD_132_TRANS_COMPLETE_REPORT.CarBoxID = theVehicleInfo.CarBoxID;
                            }
                            else if (cMDCancelType == CMDCancelType.CmdCancelIdMismatch)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIdmisMatch;
                                setHaveCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                                iD_132_TRANS_COMPLETE_REPORT.CarBoxID = theVehicleInfo.CarBoxID;
                            }
                            break;
                        case ActiveType.Unload:
                            if (cMDCancelType == CMDCancelType.CmdNone)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusUnload;
                                setEmptyCarrier132(iD_132_TRANS_COMPLETE_REPORT);
                            }
                            break;
                        case ActiveType.Loadunload:
                            if (cMDCancelType == CMDCancelType.CmdNone)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusLoadunload;
                                setEmptyCarrier132(iD_132_TRANS_COMPLETE_REPORT);
                            }
                            else if (cMDCancelType == CMDCancelType.CmdCancelIdReadFailed)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIdreadFailed;
                                setHaveCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                                iD_132_TRANS_COMPLETE_REPORT.CarBoxID = theVehicleInfo.CarBoxID;
                            }
                            else if (cMDCancelType == CMDCancelType.CmdCancelIdMismatch)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIdmisMatch;
                                setHaveCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                                iD_132_TRANS_COMPLETE_REPORT.CarBoxID = theVehicleInfo.CarBoxID;
                            }
                            break;
                        case ActiveType.Scan:
                            if (cMDCancelType == CMDCancelType.CmdNone)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusScan;
                                setEmptyCarrier132(iD_132_TRANS_COMPLETE_REPORT);
                            }
                            break;
                    }
                }
                iD_132_TRANS_COMPLETE_REPORT.CurrentSecID = theVehicleInfo.CurrentSecID;
                iD_132_TRANS_COMPLETE_REPORT.CurrentAdrID = theVehicleInfo.CurrentAdrID;
                iD_132_TRANS_COMPLETE_REPORT.CmdDistance = 0;
                iD_132_TRANS_COMPLETE_REPORT.CmdID = transRequest.CmdID;
                iD_132_TRANS_COMPLETE_REPORT.BOXID = theVehicleInfo.BOXID;
                iD_132_TRANS_COMPLETE_REPORT.CSTID = theVehicleInfo.CSTID;
                iD_132_TRANS_COMPLETE_REPORT.LOTID = theVehicleInfo.LOTID;


                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TranCmpRepFieldNumber;
                wrappers.TranCmpRep = iD_132_TRANS_COMPLETE_REPORT;

                SendCommandWrapper(wrappers, out iD_32_TRANS_COMPLETE_RESPONSE);

                //Chris Add 20211102
                theVehicleInfo.CmdID = "";
                Send_Cmd144_StatusChangeReport(false);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        private void setHaveCstBox132(ID_132_TRANS_COMPLETE_REPORT iD_132_TRANS_COMPLETE_REPORT, CMDCancelType cMDCancelType = CMDCancelType.CmdNone)
        {
            iD_132_TRANS_COMPLETE_REPORT.HasBox = VhLoadCarrierStatus.Exist;
            iD_132_TRANS_COMPLETE_REPORT.CarBoxID = theVehicleInfo.BOXID;
            iD_132_TRANS_COMPLETE_REPORT.HasCst = VhLoadCarrierStatus.Exist;
            iD_132_TRANS_COMPLETE_REPORT.CarCstID = theVehicleInfo.CSTID;
        }

        private static void setEmptyCarrier132(ID_132_TRANS_COMPLETE_REPORT iD_132_TRANS_COMPLETE_REPORT)
        {
            iD_132_TRANS_COMPLETE_REPORT.HasBox = VhLoadCarrierStatus.NotExist;
            iD_132_TRANS_COMPLETE_REPORT.CarBoxID = "";
            iD_132_TRANS_COMPLETE_REPORT.HasCst = VhLoadCarrierStatus.NotExist;
            iD_132_TRANS_COMPLETE_REPORT.CarCstID = "";
        }

        private void setHaveCstBox144(ID_144_STATUS_CHANGE_REP iD_144_STATUS_CHANGE_REP)
        {
            iD_144_STATUS_CHANGE_REP.HasBox = VhLoadCarrierStatus.Exist;
            iD_144_STATUS_CHANGE_REP.CarBoxID = theVehicleInfo.BOXID;
            iD_144_STATUS_CHANGE_REP.HasCst = VhLoadCarrierStatus.Exist;
            iD_144_STATUS_CHANGE_REP.CarCstID = theVehicleInfo.CSTID;
        }
        private static void setEmptyCstBox144(ID_144_STATUS_CHANGE_REP iD_144_STATUS_CHANGE_REP)
        {
            iD_144_STATUS_CHANGE_REP.HasBox = VhLoadCarrierStatus.NotExist;
            iD_144_STATUS_CHANGE_REP.CarBoxID = "";
            iD_144_STATUS_CHANGE_REP.HasCst = VhLoadCarrierStatus.NotExist;
            iD_144_STATUS_CHANGE_REP.CarCstID = "";
        }

        private void SendCommandWrapper<TSource>(WrapperMessage wrapper, out TSource stRecv, bool isReply = false)
        {
            string rtnMsg = string.Empty;
            string msg = $"[SEND] [ID = {wrapper.ID}][SeqNum = {wrapper.SeqNum}] " + wrapper.ToString();
            OnCmdSend?.Invoke(this, msg);
            theLoggerAgent.LogMsg("Comm", new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                 , msg));
            com.mirle.iibg3k0.ttc.Common.TrxTcpIp.ReturnCode result = snedRecv(wrapper, out stRecv, out rtnMsg);
        }
        /// <summary>
        /// Deep_Clone : Using for clone all the object by [Serializable]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="RealObject"></param>
        /// <returns></returns>
        public T Deep_Clone<T>(T RealObject)
        {
            try
            {
                using (Stream objectStream = new MemoryStream())
                {
                    //利用 System.Runtime.Serialization序列化与反序列化完成引用对象的复制     
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(objectStream, RealObject);
                    objectStream.Seek(0, SeekOrigin.Begin);
                    return (T)formatter.Deserialize(objectStream);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message + Environment.NewLine + ex.StackTrace;
                return RealObject;
            }
        }

        public string GetPortAddress(string port_id)
        {
            string sResult = string.Empty;
            bool bRtnCode = false;
            string trx_name = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string type_id = "O";

            try
            {
                string[] action_targets = new string[]
                {
                    "QxPortStation",
                    "GetPortStationAddress"
                };
                StringBuilder sb = new StringBuilder();
                sb.Append($"{nameof(trx_name)}={trx_name}").Append("&");
                sb.Append($"{nameof(type_id)}={type_id}").Append("&");
                sb.Append($"{nameof(port_id)}={port_id}");
                byte[] byteArray = Encoding.UTF8.GetBytes(sb.ToString());
                sResult = scApp.mgoApp.getWebClientManager().PostInfoToServer(action_targets, byteArray);

            }
            catch (Exception ex)
            {
                //logger.Error(ex, "Exception");
            }

            return sResult;
        }

    }
}
