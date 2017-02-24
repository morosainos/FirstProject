using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections.Generic;
using System.Text;
using KSC.GMF.Business;
using KSC.GMF.Data.Special;
using KSC.GMF.Protomatter;
using Plumbing.Interfaces;
using System.Web.Script.Serialization;
using KSC.GMF.WebApplications.RelationshipManager;
using KSC.GMF.WebappLib;
using KSC.GMF.WebappLib.GMFWebControls.EditControls;
using AjaxControlToolkit;

namespace KSC.GMF.WebApplications.Resources.UserControls
{
    public partial class ChangeReasonReview : System.Web.UI.UserControl
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ChangeReason));

        #region Statics and Constants

        private const string COMPANY_TYPE = "C";
        private const string RELATIONSHIP_TYPE = "R";
        private const string SECURITY_TYPE = "S";

        private const string PROTO_COMPANY_TYPE = "C";
        private const string PROTO_RELATIONSHIP_TYPE = "R";
        private const string WORK_TABLE_TYPE = "W";

        private const string COMPANY_ORDER_DATA = "C1D";
        private const string COMPANY_ORDER_NAME = "C2N";
        private const string COMPANY_ORDER_ADDR = "C3A";
        private const string COMPANY_ORDER_CONT = "C4C";
        private const string COMPANY_ORDER_SVC = "C5S";
        private const string COMPANY_ORDER_TYPE = "C6T";
        private const string COMPANY_ORDER_UNQI = "C7I";
        private const string COMPANY_LDATA_NAME = "29";
        private const string COMPANY_LDATA_ADDR = "33";
        private const string COMPANY_LDATA_CONT = "32";
        private const string COMPANY_LDATA_SVC = "53";
        private const string COMPANY_LDATA_TYPE = "3";
        private const string COMPANY_LDATA_UNQI = "17";
        private const string RELATIONSHIP_ORDER_DATA = "R1D";
        private const string SECURITY_ORDER_DATA = "S1D";
        private const string SECURITY_ORDER_NAME = "S2N";
        private const string SECURITY_ORDER_UNQI = "S3I";
        private const string SECURITY_ORDER_XCHG = "S4X";
        private const string RELATIONSHIP_LDATA_SUBTYPE = "37";
        private const string SECURITY_LDATA_NAME = "52";
        private const string SECURITY_LDATA_UNQI = "15";
        private const string SECURITY_LDATA_XCHG = "48";
        private const string CLEAR_KSC = "CLEAR KSC";
        private const string EMPTY_ROW = "- Select -";
        private const string PHSICAL_ADDRESS_COUNTRY = "Physical Address - Country";
        private const string PHSICAL_ADDRESS_STATE = "Physical Address - State Province Name";
        private const string PHSICAL_ADDRESS_LINE_ONE = "Physical Address - Addr Line One";
        private const string PHSICAL_ADDRESS_LINE_TWO = "Physical Address - Addr Line Two";
        private const string REGISTRY_ADDRESS_COUNTRY = "Registered Address - Country";
        private const string REGISTRY_ADDRESS_STATE = "Registered Address - State Province Name";
        private const string REGISTRY_ADDRESS_LINE_ONE = "Registered Address - Addr Line One";
        private const string REGISTRY_ADDRESS_LINE_TWO = "Registered Address - Addr Line Two";
        private const string PHSICAL_ADDRESS_CITY = "Physical Address - City";
        private const string REGISTRY_ADDRESS_CITY = "Registered Address - City";
        private const string PHSICAL_ADDRESS_PC = "Physical Address - Postal Code";
        private const string REGISTRY_ADDRESS_PC = "Registered Address - Postal Code";
        private const string LEGAL_STRUCTURE = "Industry Type - Legal Structure";
        private const string FISCAL_YEAR_END = "Fiscal Year End";
        private const string NAICS = "NAICS";
        private const string NAICSSUB = "NAICSSUB";
        private const string NACE = "NACE";
        private const string SIC = "SIC";
        private const string GICS = "GICS";
        private const string SELECT = "- Select -";
        private const string DEFAULT_STRING = "Default";
        private string[] IND_GROUP = new string[] { ChangeReasonEditor.IND_NAICS, ChangeReasonEditor.IND_SIC1, ChangeReasonEditor.IND_NACE, ChangeReasonEditor.IND_GICS, ChangeReasonEditor.IND_NAICS_SUB };
        Dictionary<string, List<UpdatedValuesEntity>> updatedValuesMap = new Dictionary<string, List<UpdatedValuesEntity>>();
        Dictionary<string, List<int>> inputMap = new Dictionary<string, List<int>>();
        List<UpdatedValuesEntity> updateValuesList = new List<UpdatedValuesEntity>();
        Dictionary<string, UpdatedValuesEntity> proUpdatedValuesMap = new Dictionary<string, UpdatedValuesEntity>();
        Dictionary<string, int> proInputMap = new Dictionary<string, int>();

        private ProtoGraph graphToAddCha = null;
        private Dictionary<long, ProtoCompany> proComs = null;
        private Dictionary<long, ProtoRelationship> proRels = null;
        private long removeNode = 0;
        private string dataType = string.Empty;
        private List<bool> addressChangedList = new List<bool>();

        #endregion

        private string userId;
        private string selfLoad;
        private static int historyIndexForProto;
        private static List<DataRow> comTypeRecordList = new List<DataRow>();
        private static List<DataRow> comAddrRecordList = new List<DataRow>();

        protected void Page_Load(object sender, EventArgs e)
        {
            log.Debug("change reason review entry.");
            revchgEvents.ItemDataBound += new DataGridItemEventHandler(revchgEvents_ItemDataBound);
            if ("show" == selfLoad)
            {
                UpdateProgressSaving.Visible = true;
            }
            else
            {
                UpdateProgressSaving.Visible = false;
            }
        }

        public string UserId
        {
            get
            {
                return userId;
            }
            set
            {
                userId = value;
            }
        }

        public String SelfLoad
        {
            get
            {
                return selfLoad;
            }
            set
            {
                selfLoad = value;
            }
        }

        public ProtoGraph GraphToAddCha
        {
            get
            {
                return graphToAddCha;
            }
            set
            {
                graphToAddCha = value;
            }
        }

        class UpdatedValuesEntity
        {
            public string attribute { get; set; }
            public string item { get; set; }
            public string beforeValue { get; set; }
            public string beforeCode { get; set; }
            public string updatedValue { get; set; }
            public long changeKey { get; set; }
            public long masterKey { get; set; }
            public long gmfId { get; set; }
            public int fid { get; set; }
            public int listRow { get; set; }
            public string listName { get; set; }
            public string value { get; set; }
            public string bValue { get; set; }
        }

        public void showPop(long hoID, string gmfID)
        {
            DataTable workqEventsRec = new DataTable();
            // workqEventsRec.Columns.Add(new DataColumn("heOID", typeof(string)));
            workqEventsRec.Columns.Add(new DataColumn("filedChange", typeof(string)));
            workqEventsRec.Columns.Add(new DataColumn("origanleValue", typeof(string)));
            workqEventsRec.Columns.Add(new DataColumn("newValue", typeof(string)));
            workqEventsRec.Columns.Add(new DataColumn("updatedValue", typeof(string)));

            DataTable allEventsData = getAllChangeRecordsByEventId(hoID);
            List<DataRow> rowLst = null;
            IsIndustryTypeNoSeq.Value = "true";
            Dictionary<string, bool> dynamicNaicssubMap = new Dictionary<string, bool>();
            dynamicNaicssubMap.Add("isNaicssubInChange", false);
            dynamicNaicssubMap.Add("isNaicssubVisible", false);
            Dictionary<string, bool> isAddrNoSeqMap = new Dictionary<string, bool>();
            isAddrNoSeqMap.Add("isPAddrNoSeq", true);
            isAddrNoSeqMap.Add("isRAddrNoSeq", true);
            JavaScriptSerializer jsSer = new JavaScriptSerializer();
            DynamicNaicssub.Value = jsSer.Serialize(dynamicNaicssubMap);
            IsAddrNoSeq.Value = jsSer.Serialize(isAddrNoSeqMap);
            if (allEventsData.Rows.Count > 0)
            {
                GMFID.Value = gmfID;
                rowLst = new List<DataRow>();
                foreach (DataRow workRec in allEventsData.Rows)
                {
                    rowLst.Add(workRec);
                }
                showAllChanges(hoID, rowLst, workqEventsRec);
                if (revchgEvents.Columns[3].Visible == false)
                {
                    revchgEvents.Columns[3].Visible = true;
                }
            }

            if (workqEventsRec.Rows.Count > 0)
            {
                revchgEvents.DataSource = workqEventsRec;
                revchgEvents.DataBind();
                revchgEvents.Style.Add("display", "block");
                EventID.Value = hoID.ToString();
                GMFID.Value = gmfID;
            }
            else
            {
                revchgEvents.Style.Add("display", "none");
            }

            setListValues();
            changeReasonRevPopup.Show();
            CrrUpdate.Update();
        }

        public void showPopForProto(string hoID, string gmfid, ProtoGraph graph)
        {
            DataTable workqEventsRec = new DataTable();
            // workqEventsRec.Columns.Add(new DataColumn("heOID", typeof(string)));
            workqEventsRec.Columns.Add(new DataColumn("filedChange", typeof(string)));
            workqEventsRec.Columns.Add(new DataColumn("origanleValue", typeof(string)));
            workqEventsRec.Columns.Add(new DataColumn("newValue", typeof(string)));
            workqEventsRec.Columns.Add(new DataColumn("updatedValue", typeof(string)));

            long protoId = Convert.ToInt32(hoID.Split(':')[0]);
            int protoIndex = Convert.ToInt32(hoID.Split(':')[1]);
            historyIndexForProto = protoIndex;
            IsIndustryTypeNoSeq.Value = "true";
            if (null != graph && null != graph.CreatedEntities && graph.CreatedEntities.Count > 0)
            {
                proComs = graph.CreatedEntities;
            }

            if (null != graph && null != graph.ModifiedRels && graph.ModifiedRels.Count > 0)
            {
                proRels = graph.ModifiedRels;
            }

            if (null != proComs)
            {
                if (proComs.ContainsKey(protoId))
                {
                    ProtoCompany company = proComs[protoId];
                    if (null != company && null != company.History && company.History.Count > 0)
                    {
                        ProtoCompany preCom = null;
                        ProtoCompany currCom = company.History[protoIndex];
                        if (protoIndex > 0)
                        {
                            preCom = company.History[protoIndex - 1];
                        }
                        processProtoCom(workqEventsRec, preCom, currCom);

                        if (revchgEvents.Columns[3].Visible == false)
                        {
                            revchgEvents.Columns[3].Visible = true;
                        }

                        // process source references
                        JavaScriptSerializer jsSer = new JavaScriptSerializer();

                        ChangeResonSource existingCha = currCom.ComChReasonSource;

                        String existingSrc = jsSer.Serialize(existingCha);
                        ChangeRSRevjson.Value = existingSrc;
                        ChangeRSBefJson.Value = existingSrc;
                        String updatedValueStr = jsSer.Serialize(proUpdatedValuesMap);
                        UpdatedValues.Value = updatedValueStr;
                        String inputValueStr = jsSer.Serialize(proInputMap);
                        InputValues.Value = inputValueStr;
                        Dictionary<string, bool> dynamicNaicssubMap = new Dictionary<string, bool>();
                        dynamicNaicssubMap.Add("isNaicssubInChange", false);
                        dynamicNaicssubMap.Add("isNaicssubVisible", false);
                        DynamicNaicssub.Value = jsSer.Serialize(dynamicNaicssubMap);
                    }
                }
            }

            if (null != proRels)
            {
                if (proRels.ContainsKey(protoId))
                {
                    ProtoRelationship protoRel = proRels[protoId];
                    if (null != protoRel && null != protoRel.History && protoRel.History.Count > 0)
                    {
                        ProtoRelationship preRel = null;
                        ProtoRelationship currRel = protoRel.History[protoIndex];
                        if (protoIndex > 0)
                        {
                            preRel = protoRel.History[protoIndex - 1];
                        }
                        processProtoRel(workqEventsRec, preRel, currRel);

                        if (revchgEvents.Columns[3].Visible == true)
                        {
                            revchgEvents.Columns[3].Visible = false;
                        }

                        // process source references
                        JavaScriptSerializer jsSer = new JavaScriptSerializer();

                        ChangeResonSource existingCha = currRel.RelChReasonSource;

                        String existingSrc = jsSer.Serialize(existingCha);
                        ChangeRSRevjson.Value = existingSrc;
                    }
                }
            }

            if (workqEventsRec.Rows.Count > 0)
            {
                revchgEvents.DataSource = workqEventsRec;
                revchgEvents.DataBind();
                revchgEvents.Style.Add("display", "block");
                EventID.Value = hoID;
                GMFID.Value = gmfid;
            }
            else
            {
                revchgEvents.Style.Add("display", "none");
            }

            setListValues();
            changeReasonRevPopup.Show();
            CrrUpdate.Update();
        }
        protected DataTable getAllChangeRecordsByEventId(long eventId)
        {
            #region mainSql
            StringBuilder mainSql = new StringBuilder();
            mainSql.Append("select heOID, '");
            mainSql.Append(COMPANY_TYPE);
            mainSql.Append("' heType, hcdGMFID heGMFID, heReason, heSource, heUserID, heChgDate, '");
            mainSql.Append(COMPANY_ORDER_DATA);
            mainSql.Append("' subOrder, '' subType, hcdID subId, hcdAction subAction, hcdFID heFID, 1 typeOrder, '' ldDesc ");
            mainSql.Append("from tChgEvent inner join tChgCData on heOID = hcdEventID where heOID = @EventId and tChgEvent.heActive = 1  UNION ALL ");
            mainSql.Append("select chgData.*, nType.ldOrder typeOrder, nType.ldDesc from (select heOID, '");
            mainSql.Append(COMPANY_TYPE);
            mainSql.Append("' heType, hcnGMFID heGMFID, heReason, heSource, heUserID, heChgDate, '");
            mainSql.Append(COMPANY_ORDER_NAME);
            mainSql.Append("' subOrder, case when hcnTypeAft is null or rtrim(hcnTypeAft) = '' then hcnTypeBef else hcnTypeAft end subType, hcnID subId, hcnAction subAction, hcnFID heFID ");
            mainSql.Append("from tChgEvent inner join tChgCName on heOID = hcnEventID where heOID = @EventId and tChgEvent.heActive = 1 ) chgData left join (select * from tListData where ldListId = ");
            mainSql.Append(COMPANY_LDATA_NAME);
            mainSql.Append(") nType on subType = ldValue UNION ALL ");
            mainSql.Append("select chgData.*, aType.ldId typeOrder, aType.ldDesc from (select heOID, '");
            mainSql.Append(COMPANY_TYPE);
            mainSql.Append("' heType, hcaGMFID heGMFID, heReason, heSource, heUserID, heChgDate, '");
            mainSql.Append(COMPANY_ORDER_ADDR);
            mainSql.Append("' subOrder, case when hcaTypeAft is null or rtrim(hcaTypeAft)='' then hcaTypeBef else hcaTypeAft end subType, hcaID subId, hcaAction subAction, hcaFID heFID ");
            mainSql.Append("from tChgEvent inner join tChgCAddr on heOID = hcaEventID where heOID = @EventId and tChgEvent.heActive = 1 ) chgData left join (select * from tListData where ldListId = ");
            mainSql.Append(COMPANY_LDATA_ADDR);
            mainSql.Append(") aType on subType = ldValue UNION ALL ");
            mainSql.Append("select chgData.*, cType.ldId typeOrder, cType.ldDesc from (select heOID, '");
            mainSql.Append(COMPANY_TYPE);
            mainSql.Append("' heType, hccGMFID heGMFID, heReason, heSource, heUserID, heChgDate, '");
            mainSql.Append(COMPANY_ORDER_CONT);
            mainSql.Append("' subOrder, case when hccTypeAft is null or rtrim(hccTypeAft)='' then hccTypeBef else hccTypeAft end subType, hccID subId, hccAction subAction, hccFID heFID ");
            mainSql.Append("from tChgEvent inner join tChgCContct on heOID = hccEventID where heOID = @EventId and tChgEvent.heActive = 1 ) chgData left join (select * from tListData where ldListId = ");
            mainSql.Append(COMPANY_LDATA_CONT);
            mainSql.Append(") cType on subType = ldValue UNION ALL ");
            mainSql.Append("select chgData.*, sType.ldId typeOrder, sType.ldDesc from (select heOID, '");
            mainSql.Append(COMPANY_TYPE);
            mainSql.Append("' heType, hcsGMFID heGMFID, heReason, heSource, heUserID, heChgDate, '");
            mainSql.Append(COMPANY_ORDER_SVC);
            mainSql.Append("' subOrder, case when hcsTypeAft is null or rtrim(hcsTypeAft)='' then hcsTypeBef else hcsTypeAft end subType, hcsID subId, hcsAction subAction, hcsFID heFID ");
            mainSql.Append("from tChgEvent inner join tChgCSvc on heOID = hcsEventID where heOID = @EventId and tChgEvent.heActive = 1 ) chgData left join (select * from tListData where ldListId = ");
            mainSql.Append(COMPANY_LDATA_SVC);
            mainSql.Append(") sType on subType = ldValue UNION ALL ");
            mainSql.Append("select chgData.*, tType.ldId typeOrder, tType.ldDesc from (select heOID, '");
            mainSql.Append(COMPANY_TYPE);
            mainSql.Append("' heType, hctGMFID heGMFID, heReason, heSource, heUserID, heChgDate, '");
            mainSql.Append(COMPANY_ORDER_TYPE);
            mainSql.Append("' subOrder, case when hctTypeAft is null or rtrim(hctTypeAft)='' then hctTypeBef else hctTypeAft end subType, hctID subId, hctAction subAction, hctFID heFID ");
            mainSql.Append("from tChgEvent inner join tChgCType on heOID = hctEventID where heOID = @EventId and tChgEvent.heActive = 1 ) chgData left join (select * from tListData where ldListId = ");
            mainSql.Append(COMPANY_LDATA_TYPE);
            mainSql.Append(") tType on subType = ldValue UNION ALL ");
            mainSql.Append("select chgData.*, iType.ldId typeOrder, iType.ldDesc from (select heOID, '");
            mainSql.Append(COMPANY_TYPE);
            mainSql.Append("' heType, hciGMFID heGMFID, heReason, heSource, heUserID, heChgDate, '");
            mainSql.Append(COMPANY_ORDER_UNQI);
            mainSql.Append("' subOrder, case when hciTypeAft is null or rtrim(hciTypeAft)='' then hciTypeBef else hciTypeAft end subType, hciID subId, hciAction subAction, hciFID heFID ");
            mainSql.Append("from tChgEvent inner join tChgCUnqIds on heOID = hciEventID where heOID = @EventId and tChgEvent.heActive = 1 ) chgData left join (select * from tListData where ldListId = ");
            mainSql.Append(COMPANY_LDATA_UNQI);
            mainSql.Append(") iType on subType = ldValue UNION ALL ");
            mainSql.Append("select chgData.*, rType.ldOrder typeOrder, rType.ldDesc from (select heOID, '");
            mainSql.Append(RELATIONSHIP_TYPE);
            mainSql.Append("' heType, hrdGMFID heGMFID, heReason, heSource, heUserID, heChgDate, '");
            mainSql.Append(RELATIONSHIP_ORDER_DATA);
            mainSql.Append("' subOrder, case when hrdSubTypeAft is null or rtrim(hrdSubTypeAft)='' then hrdSubTypeBef else hrdSubTypeAft end subType, hrdID subId, hrdAction subAction, hrdFID heFID ");
            mainSql.Append("from tChgEvent inner join tChgRData on heOID = hrdEventID where heOID = @EventId and tChgEvent.heActive = 1 ) chgData left join (select * from tListData where ldListId = ");
            mainSql.Append(RELATIONSHIP_LDATA_SUBTYPE);
            mainSql.Append(") rType on subType = ldValue UNION ALL ");
            mainSql.Append("select heOID, '");
            mainSql.Append(SECURITY_TYPE);
            mainSql.Append("' heType, hsdGMFID heGMFID, heReason, heSource, heUserID, heChgDate, '");
            mainSql.Append(SECURITY_ORDER_DATA);
            mainSql.Append("' subOrder,  '' subType, hsdID subId, hsdAction subAction, hsdFID heFID, 1 typeOrder, '' ldDesc ");
            mainSql.Append("from tChgEvent inner join tChgSData on heOID = hsdEventID where heOID = @EventId and tChgEvent.heActive = 1  UNION ALL ");
            mainSql.Append("select chgData.*, nType.ldId typeOrder, nType.ldDesc from (select heOID, '");
            mainSql.Append(SECURITY_TYPE);
            mainSql.Append("' heType, hsnGMFID heGMFID, heReason, heSource, heUserID, heChgDate, '");
            mainSql.Append(SECURITY_ORDER_NAME);
            mainSql.Append("' subOrder, case when hsnTypeAft is null or rtrim(hsnTypeAft) = '' then hsnTypeBef else hsnTypeAft end subType, hsnID subId, hsnAction subAction, hsnFID heFID ");
            mainSql.Append("from tChgEvent inner join tChgSName on heOID = hsnEventID where heOID = @EventId and tChgEvent.heActive = 1 ) chgData left join (select * from tListData where ldListId = ");
            mainSql.Append(SECURITY_LDATA_NAME);
            mainSql.Append(") nType on subType = ldValue UNION ALL ");
            mainSql.Append("select chgData.*, iType.ldId typeOrder, iType.ldDesc from (select heOID, '");
            mainSql.Append(SECURITY_TYPE);
            mainSql.Append("' heType, hsiGMFID heGMFID, heReason, heSource, heUserID, heChgDate, '");
            mainSql.Append(SECURITY_ORDER_UNQI);
            mainSql.Append("' subOrder, case when hsiTypeAft is null or rtrim(hsiTypeAft) = '' then hsiTypeBef else hsiTypeAft end subType, hsiID subId, hsiAction subAction, hsiFID heFID ");
            mainSql.Append("from tChgEvent inner join tChgSUnqIds on heOID = hsiEventID where heOID = @EventId and tChgEvent.heActive = 1 ) chgData left join (select * from tListData where ldListId = ");
            mainSql.Append(SECURITY_LDATA_UNQI);
            mainSql.Append(") iType on subType = ldValue UNION ALL ");
            mainSql.Append("select chgData.*, xType.ldOrder typeOrder, xType.ldDesc from (select heOID, '");
            mainSql.Append(SECURITY_TYPE);
            mainSql.Append("' heType, hsxGMFID heGMFID, heReason, heSource, heUserID, heChgDate, '");
            mainSql.Append(SECURITY_ORDER_XCHG);
            mainSql.Append("' subOrder, case when hsxTypeAft is null or rtrim(hsxTypeAft) = '' then hsxTypeBef else hsxTypeAft end subType, hsxID subId, hsxAction subAction, hsxFID heFID ");
            mainSql.Append("from tChgEvent inner join tChgSXchg on heOID = hsxEventID where heOID = @EventId and tChgEvent.heActive = 1 ) chgData left join (select * from tListData where ldListId = ");
            mainSql.Append(SECURITY_LDATA_XCHG);
            mainSql.Append(") xType on subType = ldValue ");
            mainSql.Append("order by heGMFID, heChgDate, subOrder, typeOrder, subId");

            #endregion

            IGridDefinition workQChangeEvents = new GridDefinition();
            workQChangeEvents.Sql = mainSql.ToString();
            workQChangeEvents.AddParameter("EventId", SqlDbType.BigInt, eventId);
            return workQChangeEvents.GetDataTable();
        }
        #region Company process
        private void processComData(DataTable contentItems, string key, string type)
        {
            DataRow contentRow;
            string befValue, aftValue;
            CompanyDataChangeCollection dataCol = new CompanyDataChangeCollection();
            CompanyDataChange dataChg = dataCol.GetByPrimaryKey(long.Parse(key));

            if (null != dataChg)
            {
                befValue = dataChg.IsExpiredBeforeNull ? "" : dataChg.ExpiredBefore.ToString();
                aftValue = dataChg.IsExpiredAfterNull ? "" : dataChg.ExpiredAfter.ToString();
                if (checkChange(befValue, aftValue))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Is Expired";
                    contentRow[1] = befValue;
                    contentRow[2] = aftValue;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "Expired", key, 0, contentItems.Rows.Count - 1, dataChg.CompanyGMFID, dataChg.FeedID, null);
                }
                befValue = dataChg.IsExpiredDateBeforeNull ? "" : dataChg.ExpiredDateBefore.ToString();
                aftValue = dataChg.IsExpiredDateAfterNull ? "" : dataChg.ExpiredDateAfter.ToString();
                if (checkChange(befValue, aftValue))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Date of expiration";
                    contentRow[1] = befValue;
                    contentRow[2] = aftValue;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "ExpiredDate", key, 0, contentItems.Rows.Count - 1, dataChg.CompanyGMFID, dataChg.FeedID, null);
                }
                if (checkChange(dataChg.ExpiredReasonBefore, dataChg.ExpiredReasonAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Reason for expiration";
                    contentRow[1] = dataChg.ExpiredReasonBefore;
                    contentRow[2] = dataChg.ExpiredReasonAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "ExpiredReason", key, 0, contentItems.Rows.Count - 1, dataChg.CompanyGMFID, dataChg.FeedID, null);
                }
                befValue = dataChg.IsIsBranchBeforeNull ? "" : dataChg.IsBranchBefore.ToString();
                aftValue = dataChg.IsIsBranchAfterNull ? "" : dataChg.IsBranchAfter.ToString();
                if (checkChange(befValue, aftValue))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Is a Branch";
                    contentRow[1] = befValue;
                    contentRow[2] = aftValue;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "IsBranch", key, 0, contentItems.Rows.Count - 1, dataChg.CompanyGMFID, dataChg.FeedID, null);
                }
                befValue = dataChg.IsIsPublicBeforeNull ? "" : dataChg.IsPublicBefore.ToString();
                aftValue = dataChg.IsIsPublicAfterNull ? "" : dataChg.IsPublicAfter.ToString();
                if (checkChange(befValue, aftValue))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Is Publicly traded";
                    contentRow[1] = befValue;
                    contentRow[2] = aftValue;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "IsPublic", key, 0, contentItems.Rows.Count - 1, dataChg.CompanyGMFID, dataChg.FeedID, null);
                }
                befValue = dataChg.IsIsIssuerBeforeNull ? "" : dataChg.IsIssuerBefore.ToString();
                aftValue = dataChg.IsIsIssuerAfterNull ? "" : dataChg.IsIssuerAfter.ToString();
                if (checkChange(befValue, aftValue))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Is an Issuer of Security";
                    contentRow[1] = befValue;
                    contentRow[2] = aftValue;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "IsIssuer", key, 0, contentItems.Rows.Count - 1, dataChg.CompanyGMFID, dataChg.FeedID, null);
                }
                if (checkChange(dataChg.FiscalYearEndBefore, dataChg.FiscalYearEndAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Fiscal Year End";
                    contentRow[1] = dataChg.FiscalYearEndBefore;
                    contentRow[2] = dataChg.FiscalYearEndAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "FiscalYearEnd", key, 0, contentItems.Rows.Count - 1, dataChg.CompanyGMFID, dataChg.FeedID, null);
                }
                if (checkChange(dataChg.StatusBefore, dataChg.StatusAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Status";
                    contentRow[1] = dataChg.StatusBefore;
                    contentRow[2] = dataChg.StatusAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "Status", key, 0, contentItems.Rows.Count - 1, dataChg.CompanyGMFID, dataChg.FeedID, null);
                }
            }
            dataCol.Dispose();
        }

        private bool checkChange(string a, string b)
        {
            string aStr, bStr;
            if (null == a)
                aStr = "";
            else
                aStr = a.Trim();
            if (null == b)
                bStr = "";
            else
                bStr = b.Trim();
            return !aStr.Equals(bStr);

        }

        private void processComName(DataTable contentItems, string key, string typeDesc, string type)
        {
            DataRow contentRow;
            CompanyNameChangeCollection nameCol = new CompanyNameChangeCollection();
            long longKey = long.Parse(key);

            CompanyNameChange nameChg = nameCol.GetByPrimaryKey(longKey);
            if (null != nameChg)
            {
                if (checkChange(nameChg.NameBefore, nameChg.NameAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = typeDesc;
                    contentRow[1] = nameChg.NameBefore;
                    contentRow[2] = nameChg.NameAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "Name", key, nameChg.CompanyNameID, contentItems.Rows.Count - 1, 0, 0, null);
                }
            }
            nameCol.Dispose();
        }

        private void processComAddr(DataTable contentItems, string key, string typeDesc, string type, List<DataRow> industryTypeRow)
        {
            DataRow contentRow;
            CompanyAddressChangeCollection addrCol = new CompanyAddressChangeCollection();
            long longKey = long.Parse(key);
            CompanyAddressChange addrChg = addrCol.GetByPrimaryKey(longKey);
            JavaScriptSerializer jsSer = new JavaScriptSerializer();
            Dictionary<string, bool> isAddrNoSeqMap = jsSer.Deserialize<Dictionary<string, bool>>(this.IsAddrNoSeq.Value);

            if (null != addrChg)
            {
                if (checkChange(addrChg.Address1Before, addrChg.Address1After))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = typeDesc + " - Addr Line One";
                    contentRow[1] = addrChg.Address1Before;
                    contentRow[2] = addrChg.Address1After;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, contentRow[0].ToString(), key, addrChg.CompanyAddressID, contentItems.Rows.Count - 1, 0, 0, null);
                }
                if (checkChange(addrChg.Address2Before, addrChg.Address2After))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = typeDesc + " - Addr Line Two";
                    contentRow[1] = addrChg.Address2Before;
                    contentRow[2] = addrChg.Address2After;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, contentRow[0].ToString(), key, addrChg.CompanyAddressID, contentItems.Rows.Count - 1, 0, 0, null);
                }
                if (checkChange(addrChg.CityBefore, addrChg.CityAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = typeDesc + " - City";
                    contentRow[1] = addrChg.CityBefore;
                    contentRow[2] = addrChg.CityAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, contentRow[0].ToString(), key, addrChg.CompanyAddressID, contentItems.Rows.Count - 1, 0, 0, null);
                }
                if (checkChange(addrChg.CountryBefore, addrChg.CountryAfter) &&
                    ("Registered Address".Equals(typeDesc) && isAddrNoSeqMap["isRAddrNoSeq"] || "Physical Address".Equals(typeDesc) && isAddrNoSeqMap["isPAddrNoSeq"]))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = typeDesc + " - State Province Name";
                    contentRow[1] = addrChg.StateProvinceNameBefore;
                    contentRow[2] = addrChg.StateProvinceNameAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, contentRow[0].ToString(), key, addrChg.CompanyAddressID, contentItems.Rows.Count - 1, 0, 0, null);
                }
                else
                {
                    if (checkChange(addrChg.StateProvinceNameBefore, addrChg.StateProvinceNameAfter))
                    {
                        contentRow = contentItems.NewRow();
                        contentRow[0] = typeDesc + " - State Province Name";
                        contentRow[1] = addrChg.StateProvinceNameBefore;
                        contentRow[2] = addrChg.StateProvinceNameAfter;
                        contentItems.Rows.Add(contentRow);
                        gatherValues(type, contentRow, contentRow[0].ToString(), key, addrChg.CompanyAddressID, contentItems.Rows.Count - 1, 0, 0, null);
                    }
                }
                if (checkChange(addrChg.CountryBefore, addrChg.CountryAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = typeDesc + " - Country";
                    contentRow[1] = addrChg.CountryBefore;
                    contentRow[2] = addrChg.CountryAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, contentRow[0].ToString(), key, addrChg.CompanyAddressID, contentItems.Rows.Count - 1, 0, 0, null);
                }
                if (checkChange(addrChg.PostalCodeBefore, addrChg.PostalCodeAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = typeDesc + " - Postal Code";
                    contentRow[1] = addrChg.PostalCodeBefore;
                    contentRow[2] = addrChg.PostalCodeAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, contentRow[0].ToString(), key, addrChg.CompanyAddressID, contentItems.Rows.Count - 1, 0, 0, null);
                }
                //If legal structure is in change, then display it at last of Address group.
                if (industryTypeRow.Count > 0)
                {
                    DataRow eventRec = industryTypeRow[0];
                    string subId = eventRec["subId"].ToString();
                    CompanyTypeChangeCollection typeCol = new CompanyTypeChangeCollection();
                    CompanyTypeChange typeChg = typeCol.GetByPrimaryKey(long.Parse(subId));
                    if (null != typeChg)
                    {
                        contentRow = contentItems.NewRow();
                        contentRow[0] = "Industry Type - Legal Structure";
                        contentRow[1] = typeChg.CodeBefore;
                        contentRow[2] = typeChg.CodeAfter;
                        contentItems.Rows.Add(contentRow);
                        gatherValues(COMPANY_ORDER_TYPE, contentRow, contentRow[0].ToString(), subId, typeChg.CompanyTypeID, contentItems.Rows.Count - 1, 0, 0, null);
                    }
                    industryTypeRow.Clear();
                    typeCol.Dispose();
                }
                else if (checkChange(addrChg.CountryBefore, addrChg.CountryAfter))
                {
                    //If legal structure is not in change but registered country is in, then display it at last of Address group.
                    if ("Registered Address".Equals(typeDesc) && isAddrNoSeqMap["isRAddrNoSeq"])
                    {
                        contentRow = contentItems.NewRow();
                        contentRow[0] = "Industry Type - Legal Structure";
                        contentRow[1] = getComTypeCode("STRUCTURE");
                        contentRow[2] = getComTypeCode("STRUCTURE");
                        contentItems.Rows.Add(contentRow);
                    }
                }
            }
            addrCol.Dispose();
        }

        private void processComContact(DataTable contentItems, string key, string typeDesc, string type)
        {
            DataRow contentRow;
            CompanyContactChangeCollection contCol = new CompanyContactChangeCollection();
            CompanyContactChange contChg = contCol.GetByPrimaryKey(long.Parse(key));
            if (null != contChg)
            {
                if (checkChange(contChg.MethodBefore, contChg.MethodAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Contact Method - " + typeDesc;
                    contentRow[1] = contChg.MethodBefore;
                    contentRow[2] = contChg.MethodAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "Method", key, contChg.CompanyContactID, contentItems.Rows.Count - 1, 0, 0, null);
                }
            }
            contCol.Dispose();
        }

        private void processComService(DataTable contentItems, string key, string typeDesc, string type)
        {
            DataRow contentRow;
            CompanyServiceChangeCollection servCol = new CompanyServiceChangeCollection();
            CompanyServiceChange servChg = servCol.GetByPrimaryKey(long.Parse(key));
            if (null != servChg)
            {
                if (checkChange(servChg.NameBefore, servChg.NameAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Service - " + typeDesc;
                    contentRow[1] = servChg.NameBefore;
                    contentRow[2] = servChg.NameAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "Name", key, servChg.CompanyServiceID, contentItems.Rows.Count - 1, 0, 0, null);
                }
            }
            servCol.Dispose();
        }

        private void processComType(DataTable contentItems, string key, string typeDesc, string type, List<DataRow> industryTypeRow)
        {
            DataRow contentRow;
            CompanyTypeChangeCollection typeCol = new CompanyTypeChangeCollection();
            CompanyTypeChange typeChg = typeCol.GetByPrimaryKey(long.Parse(key));
            if (null != typeChg)
            {
                string befValue = null != typeChg.CodeBefore ? typeChg.CodeBefore : "";
                if (befValue.ToUpper().Equals("DEFAULT") || CLEAR_KSC.Equals(befValue.ToUpper())) befValue = "";
                string aftValue = null != typeChg.CodeAfter ? typeChg.CodeAfter : "";
                if (aftValue.ToUpper().Equals("DEFAULT") || CLEAR_KSC.Equals(aftValue.ToUpper())) aftValue = "";
                if (checkChange(typeChg.CodeBefore, typeChg.CodeAfter))
                {
                    string subType = string.IsNullOrEmpty(typeChg.TypeAfter) ? typeChg.TypeBefore : typeChg.TypeAfter;

                    contentRow = contentItems.NewRow();
                    //if ("ENTITY".Equals(subType) || "STRUCTURE".Equals(subType))
                    if ("STRUCTURE".Equals(subType))
                    {
                        if (industryTypeRow.Count == 1)
                        {
                            contentRow = contentItems.NewRow();
                            contentRow[0] = "Industry Type - " + typeDesc;
                            contentRow[1] = typeChg.CodeBefore;
                            contentRow[2] = typeChg.CodeAfter;
                            contentItems.Rows.Add(contentRow);
                            gatherValues(type, contentRow, contentRow[0].ToString(), key, typeChg.CompanyTypeID, contentItems.Rows.Count - 1, 0, 0, null);
                            industryTypeRow.Clear();
                        }
                        typeCol.Dispose();
                        return;
                    }
                    else if ("ENTITY".Equals(subType))
                    {
                        contentRow[0] = "Industry Type - " + typeDesc;
                    }
                    else
                    {
                        contentRow[0] = "Industry Type - " + subType;
                    }
                    contentRow[1] = typeChg.CodeBefore;
                    contentRow[2] = typeChg.CodeAfter;
                    int indexOfIndustryTypeRow = -1;
                    if (bool.Parse(IsIndustryTypeNoSeq.Value))
                    {
                        switch (subType)
                        {
                            case "NAICS": indexOfIndustryTypeRow = 0; break;
                            case "SIC1": indexOfIndustryTypeRow = 1; break;
                            case "NACE": indexOfIndustryTypeRow = 2; break;
                            case "GICS": indexOfIndustryTypeRow = 3; break;
                            case "NAICSSUB": indexOfIndustryTypeRow = 4; break;
                        }
                        addRowForIndustryType(contentItems, contentRow, indexOfIndustryTypeRow, industryTypeRow);
                    }
                    else
                    {
                        contentItems.Rows.Add(contentRow);
                        gatherValues(type, contentRow, contentRow[0].ToString(), key, typeChg.CompanyTypeID, contentItems.Rows.Count - 1, 0, 0, null);
                    }
                    if ("ENTITY".Equals(subType) || bool.Parse(IsIndustryTypeNoSeq.Value))
                    {
                        if (industryTypeRow.Count > 0)
                        {
                            int indexOfNaics = contentItems.Rows.IndexOf(industryTypeRow[0]);
                            gatherValues(type, contentRow, contentRow[0].ToString(), key, typeChg.CompanyTypeID, indexOfNaics + indexOfIndustryTypeRow, 0, 0, null);
                        }
                        else
                        {
                            gatherValues(type, contentRow, contentRow[0].ToString(), key, typeChg.CompanyTypeID, contentItems.Rows.Count - 1, 0, 0, null);
                        }
                    }
                }
            }
            typeCol.Dispose();
        }

        private void addRowForIndustryType(DataTable contentItems, DataRow contentRow, int indexOfIndustryTypeRow, List<DataRow> industryTypeRow)
        {
            int index = 0;
            if (industryTypeRow.Count == 5 && indexOfIndustryTypeRow > 0)
            {
                index = contentItems.Rows.IndexOf(industryTypeRow[indexOfIndustryTypeRow]);
                contentItems.Rows.RemoveAt(index);
                contentItems.Rows.InsertAt(contentRow, index);
            }
            else if (indexOfIndustryTypeRow == 0)
            {
                //If NAICS is in change , child type should all be shown in popup.
                industryTypeRow.Insert(0, contentRow);
                contentItems.Rows.Add(contentRow);
                for (int i = 1; i < IND_GROUP.Length; i++)
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = IND_GROUP[i];
                    contentRow[1] = "";
                    contentRow[2] = "";
                    industryTypeRow.Insert(i, contentRow);
                    contentItems.Rows.Add(contentRow);
                }
            }
            else
            {
                contentItems.Rows.Add(contentRow);
            }
        }

        private void processComUnqi(DataTable contentItems, string key, string typeDesc, string type)
        {
            DataRow contentRow;
            CompanyUniqueIDChangeCollection unqiCol = new CompanyUniqueIDChangeCollection();
            CompanyUniqueIDChange unqiChg = unqiCol.GetByPrimaryKey(long.Parse(key));
            if (null != unqiChg)
            {
                string befValue = unqiChg.TypeBefore;
                string aftValue = unqiChg.TypeAfter;
                string idenType = unqiChg.ImportTypeBefore == null ? unqiChg.ImportTypeAfter : unqiChg.ImportTypeBefore;
                if (checkChange(befValue, aftValue))
                {
                    contentRow = contentItems.NewRow();
                    if (idenType.Equals("KSC_ID1"))
                    {
                        contentRow[0] = ChangeReasonEditor.IDEN1_TYPE;
                    }
                    else
                    {
                        contentRow[0] = ChangeReasonEditor.IDEN2_TYPE;
                    }
                    string befListDataDescription = "";
                    string aftListDataDescription = "";
                    if (!string.IsNullOrEmpty(befValue))
                    {
                        befListDataDescription = List.TranslateToDescription(List.COMPANY_UNIQUE_ID_TYPES, befValue);
                    }
                    if (!String.IsNullOrEmpty(befListDataDescription))
                    {
                        befValue = befListDataDescription;
                    }
                    if (!string.IsNullOrEmpty(aftValue))
                    {
                        aftListDataDescription = List.TranslateToDescription(List.COMPANY_UNIQUE_ID_TYPES, aftValue);
                    }
                    if (!String.IsNullOrEmpty(aftListDataDescription))
                    {
                        aftValue = aftListDataDescription;
                    }
                    contentRow[1] = befValue;
                    contentRow[2] = aftValue;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "Type", key, unqiChg.CompanyUniqueID, contentItems.Rows.Count - 1, 0, 0, unqiChg.TypeBefore);
                }
                if (checkChange(unqiChg.CodeBefore, unqiChg.CodeAfter))
                {
                    string subType = string.IsNullOrEmpty(unqiChg.TypeAfter) ? unqiChg.TypeBefore : unqiChg.TypeAfter;

                    contentRow = contentItems.NewRow();
                    if (idenType.Equals("KSC_ID1"))
                    {
                        if (!string.IsNullOrEmpty(subType))
                        {
                            contentRow[0] = ChangeReasonEditor.IDEN1_VALUE_SUB + typeDesc;
                        }
                        else
                        {
                            contentRow[0] = ChangeReasonEditor.IDEN1_VALUE_SUB + subType;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(subType))
                        {
                            contentRow[0] = ChangeReasonEditor.IDEN2_VALUE_SUB + typeDesc;
                        }
                        else
                        {
                            contentRow[0] = ChangeReasonEditor.IDEN2_VALUE_SUB + subType;
                        }
                    }
                    contentRow[1] = unqiChg.CodeBefore;
                    contentRow[2] = unqiChg.CodeAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "Code", key, unqiChg.CompanyUniqueID, contentItems.Rows.Count - 1, 0, 0, null);
                }
            }
            unqiCol.Dispose();
        }

        private void gatherValues(string type, DataRow contentRow, string item, string key, long masterKey, int rowCnt, long gmfid, int fid, string dbValue)
        {

            UpdatedValuesEntity val = new UpdatedValuesEntity();
            val.attribute = type;
            val.item = item;
            if (string.IsNullOrEmpty(dbValue))
            {
                val.beforeValue = contentRow[1].ToString();
            }
            else
            {
                val.beforeValue = dbValue;
            }
            val.updatedValue = contentRow[2].ToString();
            val.changeKey = long.Parse(key);
            val.masterKey = masterKey;
            val.gmfId = gmfid;
            val.fid = fid;
            List<UpdatedValuesEntity> updatedValuesList = new List<UpdatedValuesEntity>();
            if (updatedValuesMap.ContainsKey(key))
            {
                updatedValuesList = updatedValuesMap[key];
                updatedValuesMap.Remove(key);
                updatedValuesList.Add(val);
                updatedValuesMap.Add(key, updatedValuesList);
            }
            else
            {
                updatedValuesList.Add(val);
                updatedValuesMap.Add(key, updatedValuesList);
            }
            matchKeyInput(key, rowCnt);
        }

        private void gatherValues(string type, DataRow contentRow, int rowCnt, string value, string bValue)
        {
            UpdatedValuesEntity val = new UpdatedValuesEntity();
            val.attribute = type;
            val.item = contentRow[0].ToString();
            val.beforeValue = contentRow[1].ToString();
            val.updatedValue = contentRow[2].ToString();
            val.value = value;
            val.bValue = bValue;
            List<UpdatedValuesEntity> updatedValuesList = new List<UpdatedValuesEntity>();
            if (!proUpdatedValuesMap.ContainsKey(val.item))
            {
                proUpdatedValuesMap.Add(val.item, val);
                proInputMap.Add(val.item, rowCnt);
            }
        }
        #endregion

        #region Security process
        private void processSecData(DataTable contentItems, string key, string typeDesc, string type)
        {
            DataRow contentRow;
            string befValue, aftValue;
            SecurityDataChangeCollection secDataCol = new SecurityDataChangeCollection();
            SecurityDataChange dataChg = secDataCol.GetByPrimaryKey(long.Parse(key));
            if (null != dataChg)
            {
                if (checkChange(dataChg.GMFStatusBefore, dataChg.GMFStatusAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "GMF record status";
                    contentRow[1] = dataChg.GMFStatusBefore;
                    contentRow[2] = dataChg.GMFStatusAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "GMF record status", key, 0, contentItems.Rows.Count - 1, dataChg.SecurityGMFID, dataChg.FeedID, null);
                }
                befValue = dataChg.IsExpiredBeforeNull ? "" : dataChg.ExpiredBefore.ToString();
                aftValue = dataChg.IsExpiredAfterNull ? "" : dataChg.ExpiredAfter.ToString();
                if (checkChange(befValue, aftValue))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Is Expired";
                    contentRow[1] = befValue;
                    contentRow[2] = aftValue;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "Expired", key, 0, contentItems.Rows.Count - 1, dataChg.SecurityGMFID, dataChg.FeedID, null);
                }
                befValue = dataChg.IsExpiredDateBeforeNull ? "" : dataChg.ExpiredDateBefore.ToString();
                aftValue = dataChg.IsExpiredDateAfterNull ? "" : dataChg.ExpiredDateAfter.ToString();
                if (checkChange(befValue, aftValue))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Date of expiration";
                    contentRow[1] = befValue;
                    contentRow[2] = aftValue;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "ExpiredDate", key, 0, contentItems.Rows.Count - 1, dataChg.SecurityGMFID, dataChg.FeedID, null);
                }
                if (checkChange(dataChg.ExpiredReasonBefore, dataChg.ExpiredReasonAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Reason for expiration";
                    contentRow[1] = dataChg.ExpiredReasonBefore;
                    contentRow[2] = dataChg.ExpiredReasonAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "ExpiredReason", key, 0, contentItems.Rows.Count - 1, dataChg.SecurityGMFID, dataChg.FeedID, null);
                }
                if (checkChange(dataChg.Issuer1Before, dataChg.Issuer1After))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Name of Issuer1";
                    contentRow[1] = dataChg.Issuer1Before;
                    contentRow[2] = dataChg.Issuer1After;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "Name of Issuer1", key, 0, contentItems.Rows.Count - 1, dataChg.SecurityGMFID, dataChg.FeedID, null);
                }
                if (checkChange(dataChg.Description1Before, dataChg.Description1After))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Description of Issuer1";
                    contentRow[1] = dataChg.Description1Before;
                    contentRow[2] = dataChg.Description1After;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "Description of Issuer1", key, 0, contentItems.Rows.Count - 1, dataChg.SecurityGMFID, dataChg.FeedID, null);
                }
                if (checkChange(dataChg.CorporateActionBefore, dataChg.CorporateActionAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Corporate Action Reason";
                    contentRow[1] = dataChg.CorporateActionBefore;
                    contentRow[2] = dataChg.CorporateActionAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "Corporate Action Reason", key, 0, contentItems.Rows.Count - 1, dataChg.SecurityGMFID, dataChg.FeedID, null);
                }
                if (checkChange(dataChg.FundManagerBefore, dataChg.FundManagerAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Fund Management";
                    contentRow[1] = dataChg.FundManagerBefore;
                    contentRow[2] = dataChg.FundManagerAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "Fund Management", key, 0, contentItems.Rows.Count - 1, dataChg.SecurityGMFID, dataChg.FeedID, null);
                }
                if (checkChange(dataChg.FundAdvisorBefore, dataChg.FundAdvisorAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Fund Advisor";
                    contentRow[1] = dataChg.FundAdvisorBefore;
                    contentRow[2] = dataChg.FundAdvisorAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "Fund Advisor", key, 0, contentItems.Rows.Count - 1, dataChg.SecurityGMFID, dataChg.FeedID, null);
                }
                if (checkChange(dataChg.FundFamilyBefore, dataChg.FundFamilyAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Fund Family";
                    contentRow[1] = dataChg.FundFamilyBefore;
                    contentRow[2] = dataChg.FundFamilyAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "Fund Family", key, 0, contentItems.Rows.Count - 1, dataChg.SecurityGMFID, dataChg.FeedID, null);
                }
                if (checkChange(dataChg.FundLoadBefore, dataChg.FundLoadAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Fund Load";
                    contentRow[1] = dataChg.FundLoadBefore;
                    contentRow[2] = dataChg.FundLoadAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "Fund Load", key, 0, contentItems.Rows.Count - 1, dataChg.SecurityGMFID, dataChg.FeedID, null);
                }
                if (checkChange(dataChg.FundFYEBefore, dataChg.FundFYEAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Fund Fiscal Year End";
                    contentRow[1] = dataChg.FundFYEBefore;
                    contentRow[2] = dataChg.FundFYEAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "Fund Fiscal Year End", key, 0, contentItems.Rows.Count - 1, dataChg.SecurityGMFID, dataChg.FeedID, null);
                }
                if (checkChange(dataChg.ClassBefore, dataChg.ClassAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Security Class";
                    contentRow[1] = dataChg.ClassBefore;
                    contentRow[2] = dataChg.ClassAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "Security Class", key, 0, contentItems.Rows.Count - 1, dataChg.SecurityGMFID, dataChg.FeedID, null);
                }
                if (checkChange(dataChg.TypeBefore, dataChg.TypeAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Security Type";
                    contentRow[1] = dataChg.TypeBefore;
                    contentRow[2] = dataChg.TypeAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "Security Type", key, 0, contentItems.Rows.Count - 1, dataChg.SecurityGMFID, dataChg.FeedID, null);
                }
                befValue = dataChg.IsDefaultValueBeforeNull ? "" : dataChg.DefaultValueBefore.ToString();
                aftValue = dataChg.IsDefaultValueAfterNull ? "" : dataChg.DefaultValueAfter.ToString();
                if (checkChange(befValue, aftValue))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Default Value";
                    contentRow[1] = befValue;
                    contentRow[2] = aftValue;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "Default Value", key, 0, contentItems.Rows.Count - 1, dataChg.SecurityGMFID, dataChg.FeedID, null);
                }
                if (checkChange(dataChg.PrimaryExchangeBefore, dataChg.PrimaryExchangeAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Primary Exchange";
                    contentRow[1] = dataChg.PrimaryExchangeBefore;
                    contentRow[2] = dataChg.PrimaryExchangeAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "Primary Exchange", key, 0, contentItems.Rows.Count - 1, dataChg.SecurityGMFID, dataChg.FeedID, null);
                }
                if (checkChange(dataChg.CountryOfIncorporationBefore, dataChg.CountryOfIncorporationAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = "Country of Incorporation";
                    contentRow[1] = dataChg.CountryOfIncorporationBefore;
                    contentRow[2] = dataChg.CountryOfIncorporationAfter;
                    contentItems.Rows.Add(contentRow);
                    gatherValues(type, contentRow, "Country of Incorporation", key, 0, contentItems.Rows.Count - 1, dataChg.SecurityGMFID, dataChg.FeedID, null);
                }
            }
            secDataCol.Dispose();
        }

        private void processSecName(DataTable contentItems, string key, string typeDesc)
        {
            DataRow contentRow;
            SecurityNameChangeCollection secNameCol = new SecurityNameChangeCollection();
            SecurityNameChange nameChg = secNameCol.GetByPrimaryKey(long.Parse(key));
            if (null != nameChg)
            {
                if (checkChange(nameChg.NameBefore, nameChg.NameAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = typeDesc;
                    contentRow[1] = nameChg.NameBefore;
                    contentRow[2] = nameChg.NameAfter;
                    contentItems.Rows.Add(contentRow);
                }
            }
            secNameCol.Dispose();
        }

        private void processSecUnqi(DataTable contentItems, string key, string typeDesc)
        {
            DataRow contentRow;
            SecurityUniqueIDChangeCollection secUnqiCol = new SecurityUniqueIDChangeCollection();
            SecurityUniqueIDChange unqiChg = secUnqiCol.GetByPrimaryKey(long.Parse(key));
            if (null != unqiChg)
            {
                if (checkChange(unqiChg.CodeBefore, unqiChg.CodeAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = typeDesc;
                    contentRow[1] = unqiChg.CodeBefore;
                    contentRow[2] = unqiChg.CodeAfter;
                    contentItems.Rows.Add(contentRow);
                }
            }
            secUnqiCol.Dispose();
        }

        private void processSecExchange(DataTable contentItems, string key, string typeDesc)
        {
            DataRow contentRow;
            string befValue, aftValue;
            SecurityExchangeChangeCollection secExchgCol = new SecurityExchangeChangeCollection();
            SecurityExchangeChange exchgChg = secExchgCol.GetByPrimaryKey(long.Parse(key));
            if (null != exchgChg)
            {
                if (checkChange(exchgChg.ExchangeBefore, exchgChg.ExchangeAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = typeDesc + " - Exchange";
                    contentRow[1] = exchgChg.ExchangeBefore;
                    contentRow[2] = exchgChg.ExchangeAfter;
                    contentItems.Rows.Add(contentRow);
                }
                if (checkChange(exchgChg.TradeSymbolBefore, exchgChg.TradeSymbolAfter))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = typeDesc + " - Trade Symbol";
                    contentRow[1] = exchgChg.TradeSymbolBefore;
                    contentRow[2] = exchgChg.TradeSymbolAfter;
                    contentItems.Rows.Add(contentRow);
                }
                befValue = exchgChg.IsExchangeExpiredBeforeNull ? "" : exchgChg.ExchangeExpiredBefore.ToString();
                aftValue = exchgChg.IsExchangeExpiredAfterNull ? "" : exchgChg.ExchangeExpiredAfter.ToString();
                if (checkChange(befValue, aftValue))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = typeDesc + " - Is Expired";
                    contentRow[1] = befValue;
                    contentRow[2] = aftValue;
                    contentItems.Rows.Add(contentRow);
                }
                befValue = exchgChg.IsExchangeExpireDateBeforeNull ? "" : exchgChg.ExchangeExpireDateBefore.ToString();
                aftValue = exchgChg.IsExchangeExpireDateAfterNull ? "" : exchgChg.ExchangeExpireDateAfter.ToString();
                if (checkChange(befValue, aftValue))
                {
                    contentRow = contentItems.NewRow();
                    contentRow[0] = typeDesc + " - Expired Date";
                    contentRow[1] = befValue;
                    contentRow[2] = aftValue;
                    contentItems.Rows.Add(contentRow);
                }
            }
            secExchgCol.Dispose();
        }

        #endregion

        private void processRelData(DataTable contentItems, string key, string typeDesc, string type)
        {
            DataRow contentRow;
            string befValue, aftValue;
            RelationshipDataChangeCollection relCol = new RelationshipDataChangeCollection();
            RelationshipDataChange relChg = relCol.GetByPrimaryKey(long.Parse(key));
            if (null != relChg)
            {
                if (ChangeEventCollection.CHG_ACTION_UPDATE.Equals(relChg.Action.Trim()))
                {
                    string relType = relChg.SubTypeAfter;
                    befValue = relChg.IsPercentBeforeNull ? "" : relChg.PercentBefore.ToString();
                    aftValue = relChg.IsPercentAfterNull ? "" : relChg.PercentAfter.ToString();

                    if (checkChange(befValue, aftValue))
                    {
                        contentRow = contentItems.NewRow();
                        contentRow[0] = "Percentage";
                        contentRow[1] = befValue;
                        contentRow[2] = aftValue;
                        contentItems.Rows.Add(contentRow);
                        gatherValues(type, contentRow, "Percentage", key, 0, contentItems.Rows.Count - 1, relChg.RelationshipGMFID, relChg.FeedID, null);
                    }
                }
            }
            relCol.Dispose();
        }

        /// <summary>
        /// Revert proto com
        /// </summary>
        /// <param name="pre"></param>
        /// <param name="current"></param>
        private bool RevertProtoCom()
        {
            string eventId = this.EventID.Value;
            long gmfid = Convert.ToInt64(eventId.Split(':')[0]);
            int index = Convert.ToInt32(eventId.Split(':')[1]);
            removeNode = gmfid;
            if (null != graphToAddCha && null != graphToAddCha.CreatedEntities && graphToAddCha.CreatedEntities.Count > 0)
            {
                proComs = graphToAddCha.CreatedEntities;
            }

            if (null != graphToAddCha && null != graphToAddCha.ModifiedRels && graphToAddCha.ModifiedRels.Count > 0)
            {
                proRels = graphToAddCha.ModifiedRels;
            }

            if (null != proComs)
            {
                if (proComs.ContainsKey(gmfid))
                {
                    ProtoCompany company = proComs[gmfid];
                    if (null != company && null != company.History && company.History.Count > 0)
                    {
                        if (index == company.History.Count - 1)
                        {
                            company.History.RemoveAt(index);
                            if (index != 0)
                            {
                                company.clone(company.History[index - 1], company);
                            }
                            else if (index == 0)
                            {
                                if (null != proRels)
                                {
                                    foreach (long key in proRels.Keys)
                                    {
                                        ProtoRelationship proRel = proRels[key];
                                        if (proRel.ParentGMFID == gmfid)
                                        {
                                            removeNode = proRel.ChildGMFID;
                                        }
                                        else if (proRel.ChildGMFID == gmfid)
                                        {
                                            removeNode = proRel.ParentGMFID;
                                        }
                                    }
                                }
                                graphToAddCha.DeleteCreatedEntity(gmfid);
                            }
                        }
                        else
                        {
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                    }
                }
            }

            if (null != proRels)
            {
                if (proRels.ContainsKey(gmfid))
                {
                    ProtoRelationship proRel = proRels[gmfid];
                    if (null != proRel && null != proRel.History && proRel.History.Count > 0)
                    {
                        if (index == proRel.History.Count - 1)
                        {
                            proRel.History.RemoveAt(index);
                            long removeGMFID = 0;
                            // remove proto
                            if (proRel.ChildGMFID < 0 && proRel.ParentGMFID < 0)
                            {
                                if (proRel.ChildGMFID < proRel.ParentGMFID)
                                {
                                    removeGMFID = proRel.ChildGMFID;
                                    removeNode = proRel.ParentGMFID;
                                }
                                else
                                {
                                    removeGMFID = proRel.ParentGMFID;
                                    removeNode = proRel.ChildGMFID;
                                }
                            }
                            else if (proRel.ChildGMFID < 0 && proRel.ParentGMFID > 0)
                            {
                                removeGMFID = proRel.ChildGMFID;
                                removeNode = proRel.ParentGMFID;
                            }
                            else if (proRel.ParentGMFID < 0 && proRel.ChildGMFID > 0)
                            {
                                removeGMFID = proRel.ParentGMFID;
                                removeNode = proRel.ChildGMFID;
                            }

                            graphToAddCha.DeleteRelationship(proRel.ChildGMFID, proRel.ParentGMFID, proRel.SubType, true);
                            //if (0 != removeGMFID)
                            //{
                            //    if (proComs.ContainsKey(removeGMFID))
                            //    {
                            //        graphToAddCha.DeleteCreatedEntity(removeGMFID);
                            //    }
                            //}
                        }
                        else
                        {
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                    }
                }
            }

            if (!graphToAddCha.Entities.ContainsKey(removeNode))
            {
                foreach (long key in graphToAddCha.Entities.Keys)
                {
                    removeNode = graphToAddCha.Entities[key].KINS;
                    break;
                }
            }
            return true;
        }

        #region protoGraph process
        private void processProtoCom(DataTable contentItems, ProtoCompany pre, ProtoCompany current)
        {
            DataRow contentRow;
            string befValue, aftValue;

            befValue = (null != pre && null != pre.IsExpired) ? pre.IsExpired : "";
            aftValue = (null != current && null != current.IsExpired) ? current.IsExpired : "";
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = "Is Expired";
                if ("Active".Equals(befValue)) befValue = "False";
                else if ("Expired".Equals(befValue)) befValue = "True";
                if ("Active".Equals(aftValue)) aftValue = "False";
                else if ("Expired".Equals(aftValue)) aftValue = "True";
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_DATA, contentRow, contentItems.Rows.Count - 1, null, null);
            }
            befValue = (null != pre && null != pre.FiscalYearEnd) ? pre.FiscalYearEnd : "";
            aftValue = (null != current && null != current.FiscalYearEnd) ? current.FiscalYearEnd : "";
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = "Fiscal Year End";
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_DATA, contentRow, contentItems.Rows.Count - 1, null, null);
            }
            befValue = (null != pre && null != pre.LegalName) ? pre.LegalName : "";
            aftValue = (null != current && null != current.LegalName) ? current.LegalName : "";
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = "Legal Name";
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_NAME, contentRow, contentItems.Rows.Count - 1, null, null);
            }
            befValue = (null != pre && null != pre.Alias) ? pre.Alias : "";
            aftValue = (null != current && null != current.Alias) ? current.Alias : "";
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = "Alias";
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_NAME, contentRow, contentItems.Rows.Count - 1, null, null);
            }

            #region Proto Address
            befValue = (null != pre && null != pre.AddressLine1) ? pre.AddressLine1 : "";
            aftValue = (null != current && null != current.AddressLine1) ? current.AddressLine1 : "";
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = PHSICAL_ADDRESS_LINE_ONE;
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_ADDR, contentRow, contentItems.Rows.Count - 1, null, null);
            }
            befValue = (null != pre && null != pre.AddressLine2) ? pre.AddressLine2 : "";
            aftValue = (null != current && null != current.AddressLine2) ? current.AddressLine2 : "";
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = PHSICAL_ADDRESS_LINE_TWO;
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_ADDR, contentRow, contentItems.Rows.Count - 1, null, null);
            }
            befValue = (null != pre && null != pre.CityName) ? pre.CityName : "";
            aftValue = (null != current && null != current.CityName) ? current.CityName : "";
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = PHSICAL_ADDRESS_CITY;
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_ADDR, contentRow, contentItems.Rows.Count - 1, null, null);
            }
            befValue = (null != pre && null != pre.StateProvinceName) ? pre.StateProvinceName : "";
            if (!string.IsNullOrEmpty(befValue) && !string.IsNullOrEmpty(pre.StateProvinceDesc))
            {
                befValue = pre.StateProvinceDesc;
            }
            aftValue = (null != current && null != current.StateProvinceName) ? current.StateProvinceName : "";
            if (!string.IsNullOrEmpty(aftValue) && !string.IsNullOrEmpty(current.StateProvinceDesc))
            {
                aftValue = current.StateProvinceDesc;
            }
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = PHSICAL_ADDRESS_STATE;
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_ADDR, contentRow, contentItems.Rows.Count - 1, null, null);
            }
            befValue = (null != pre && null != pre.CountryOfIncorporation) ? pre.CountryOfIncorporation : "";
            if (!string.IsNullOrEmpty(befValue) && !string.IsNullOrEmpty(pre.CountryDesc))
            {
                befValue = pre.CountryDesc;
            }
            aftValue = (null != current && null != current.CountryOfIncorporation) ? current.CountryOfIncorporation : "";
            if (!string.IsNullOrEmpty(aftValue) && !string.IsNullOrEmpty(current.CountryDesc))
            {
                aftValue = current.CountryDesc;
            }
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = PHSICAL_ADDRESS_COUNTRY;
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_ADDR, contentRow, contentItems.Rows.Count - 1, null, null);
            }
            befValue = (null != pre && null != pre.PostalCode) ? pre.PostalCode : "";
            aftValue = (null != current && null != current.PostalCode) ? current.PostalCode : "";
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = PHSICAL_ADDRESS_PC;
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_ADDR, contentRow, contentItems.Rows.Count - 1, null, null);
            }
            befValue = (null != pre && null != pre.RegAddressLine1) ? pre.RegAddressLine1 : "";
            aftValue = (null != current && null != current.RegAddressLine1) ? current.RegAddressLine1 : "";
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = REGISTRY_ADDRESS_LINE_ONE;
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_ADDR, contentRow, contentItems.Rows.Count - 1, null, null);
            }
            befValue = (null != pre && null != pre.RegAddressLine2) ? pre.RegAddressLine2 : "";
            aftValue = (null != current && null != current.RegAddressLine2) ? current.RegAddressLine2 : "";
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = REGISTRY_ADDRESS_LINE_TWO;
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_ADDR, contentRow, contentItems.Rows.Count - 1, null, null);
            }
            befValue = (null != pre && null != pre.RegCityName) ? pre.RegCityName : "";
            aftValue = (null != current && null != current.RegCityName) ? current.RegCityName : "";
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = REGISTRY_ADDRESS_CITY;
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_ADDR, contentRow, contentItems.Rows.Count - 1, null, null);
            }
            befValue = (null != pre && null != pre.RegStateProvinceName) ? pre.RegStateProvinceName : "";
            if (!string.IsNullOrEmpty(befValue) && !string.IsNullOrEmpty(pre.RegStateProvinceDesc))
            {
                befValue = pre.RegStateProvinceDesc;
            }
            aftValue = (null != current && null != current.RegStateProvinceName) ? current.RegStateProvinceName : "";
            if (!string.IsNullOrEmpty(aftValue) && !string.IsNullOrEmpty(current.RegStateProvinceDesc))
            {
                aftValue = current.RegStateProvinceDesc;
            }
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = REGISTRY_ADDRESS_STATE;
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_ADDR, contentRow, contentItems.Rows.Count - 1, null, null);
            }
            befValue = (null != pre && null != pre.RegCountry) ? pre.RegCountry : "";
            if (befValue.ToUpper().Equals("DEFAULT")) befValue = "";
            if (!string.IsNullOrEmpty(befValue) && !string.IsNullOrEmpty(pre.RegCountryDesc))
            {
                befValue = pre.RegCountryDesc;
            }
            aftValue = (null != current && null != current.RegCountry) ? current.RegCountry : "";
            if (aftValue.ToUpper().Equals("DEFAULT")) aftValue = "";
            if (!string.IsNullOrEmpty(aftValue) && !string.IsNullOrEmpty(current.RegCountryDesc))
            {
                aftValue = current.RegCountryDesc;
            }
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = REGISTRY_ADDRESS_COUNTRY;
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_ADDR, contentRow, contentItems.Rows.Count - 1, null, null);
            }
            befValue = (null != pre && null != pre.RegPostalCode) ? pre.RegPostalCode : "";
            aftValue = (null != current && null != current.RegPostalCode) ? current.RegPostalCode : "";
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = REGISTRY_ADDRESS_PC;
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_ADDR, contentRow, contentItems.Rows.Count - 1, null, null);
            }
            #endregion

            #region Proto Company Type
            befValue = (null != pre && null != pre.CompanyType) ? pre.CompanyType : "";
            aftValue = (null != current && null != current.CompanyType) ? current.CompanyType : "";
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = "Industry Type - Entity Classification";
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_TYPE, contentRow, contentItems.Rows.Count - 1, null, null);
            }
            befValue = (null != pre && null != pre.NAICS) ? pre.NAICS : "";
            aftValue = (null != current && null != current.NAICS) ? current.NAICS : "";
            if (befValue.ToUpper().Equals("DEFAULT") || CLEAR_KSC.Equals(befValue.ToUpper())) befValue = "";
            if (aftValue.ToUpper().Equals("DEFAULT") || CLEAR_KSC.Equals(aftValue.ToUpper())) aftValue = "";
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = "Industry Type - NAICS";
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_TYPE, contentRow, contentItems.Rows.Count - 1, null, null);
            }
            befValue = (null != pre && null != pre.SIC) ? pre.SIC : "";
            aftValue = (null != current && null != current.SIC) ? current.SIC : "";
            if (befValue.ToUpper().Equals("DEFAULT") || CLEAR_KSC.Equals(befValue.ToUpper())) befValue = "";
            if (aftValue.ToUpper().Equals("DEFAULT") || CLEAR_KSC.Equals(aftValue.ToUpper())) aftValue = "";
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = "Industry Type - SIC1";
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_TYPE, contentRow, contentItems.Rows.Count - 1, null, null);
            }
            befValue = (null != pre && null != pre.NACE) ? pre.NACE : "";
            aftValue = (null != current && null != current.NACE) ? current.NACE : "";
            if (befValue.ToUpper().Equals("DEFAULT") || CLEAR_KSC.Equals(befValue.ToUpper())) befValue = "";
            if (aftValue.ToUpper().Equals("DEFAULT") || CLEAR_KSC.Equals(aftValue.ToUpper())) aftValue = "";
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = "Industry Type - NACE";
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_TYPE, contentRow, contentItems.Rows.Count - 1, null, null);
            }
            befValue = (null != pre && null != pre.NAICSSub) ? pre.NAICSSub : "";
            aftValue = (null != current && null != current.NAICSSub) ? current.NAICSSub : "";
            if (befValue.ToUpper().Equals("DEFAULT") || CLEAR_KSC.Equals(befValue.ToUpper())) befValue = "";
            if (aftValue.ToUpper().Equals("DEFAULT") || CLEAR_KSC.Equals(aftValue.ToUpper())) aftValue = "";
            //if (checkChange(befValue, aftValue))
            //{
            contentRow = contentItems.NewRow();
            contentRow[0] = "Industry Type - NAICSSUB";
            contentRow[1] = befValue;
            contentRow[2] = aftValue;
            contentItems.Rows.Add(contentRow);
            gatherValues(COMPANY_ORDER_TYPE, contentRow, contentItems.Rows.Count - 1, null, null);
            //}
            befValue = (null != pre && null != pre.GICS) ? pre.GICS : "";
            aftValue = (null != current && null != current.GICS) ? current.GICS : "";
            if (befValue.ToUpper().Equals("DEFAULT") || CLEAR_KSC.Equals(befValue.ToUpper())) befValue = "";
            if (aftValue.ToUpper().Equals("DEFAULT") || CLEAR_KSC.Equals(aftValue.ToUpper())) aftValue = "";
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = "Industry Type - GICS";
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_TYPE, contentRow, contentItems.Rows.Count - 1, null, null);
            }
            befValue = (null != pre && null != pre.LegalStructure) ? pre.LegalStructure : "";
            if (befValue.ToUpper().Equals("DEFAULT") || CLEAR_KSC.Equals(befValue.ToUpper())) befValue = "";
            aftValue = (null != current && null != current.LegalStructure) ? current.LegalStructure : "";
            if (aftValue.ToUpper().Equals("DEFAULT") || CLEAR_KSC.Equals(aftValue.ToUpper())) aftValue = "";
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = LEGAL_STRUCTURE;
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_TYPE, contentRow, contentItems.Rows.Count - 1, null, null);
            }
            #endregion

            #region Proto Unique Identifier
            string value = "";
            string bvalue = "";
            befValue = (null != pre && null != pre.Identifier1Type) ? pre.Identifier1Type : "";
            if (befValue.ToUpper().Equals("DEFAULT")) befValue = "";
            bvalue = befValue;
            if (null != pre && !string.IsNullOrEmpty(pre.Identifier1TypeDesc))
            {
                befValue = pre.Identifier1TypeDesc;
            }
            aftValue = (null != current && null != current.Identifier1Type) ? current.Identifier1Type : "";
            value = aftValue;
            if (aftValue.ToUpper().Equals("DEFAULT")) aftValue = "";
            if (null != current && !string.IsNullOrEmpty(current.Identifier1TypeDesc))
            {
                aftValue = current.Identifier1TypeDesc;
            }
            string identifierType = string.IsNullOrEmpty(aftValue) ? befValue : aftValue;
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = ChangeReasonEditor.IDEN1_TYPE;
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_UNQI, contentRow, contentItems.Rows.Count - 1, value, bvalue);
            }
            befValue = (null != pre && null != pre.Identifier1Value) ? pre.Identifier1Value : "";
            aftValue = (null != current && null != current.Identifier1Value) ? current.Identifier1Value : "";
            bvalue = befValue;
            value = aftValue;
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = ChangeReasonEditor.IDEN1_VALUE_SUB + identifierType;
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_UNQI, contentRow, contentItems.Rows.Count - 1, value, bvalue);
            }

            befValue = (null != pre && null != pre.Identifier2Type) ? pre.Identifier2Type : "";
            if (befValue.ToUpper().Equals("DEFAULT")) befValue = "";
            bvalue = befValue;
            if (null != pre && !string.IsNullOrEmpty(pre.Identifier2TypeDesc))
            {
                befValue = pre.Identifier2TypeDesc;
            }
            aftValue = (null != current && null != current.Identifier2Type) ? current.Identifier2Type : "";
            if (aftValue.ToUpper().Equals("DEFAULT")) aftValue = "";
            value = aftValue;
            if (null != current && !string.IsNullOrEmpty(current.Identifier2TypeDesc))
            {
                aftValue = current.Identifier2TypeDesc;
            }
            identifierType = string.IsNullOrEmpty(aftValue) ? befValue : aftValue;
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = ChangeReasonEditor.IDEN2_TYPE;
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_UNQI, contentRow, contentItems.Rows.Count - 1, value, bvalue);
            }
            befValue = (null != pre && null != pre.Identifier2Value) ? pre.Identifier2Value : "";
            aftValue = (null != current && null != current.Identifier2Value) ? current.Identifier2Value : "";
            value = aftValue;
            bvalue = befValue;
            if (checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = ChangeReasonEditor.IDEN2_VALUE_SUB + identifierType;
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
                gatherValues(COMPANY_ORDER_UNQI, contentRow, contentItems.Rows.Count - 1, value, bvalue);
            }
            #endregion
        }

        private void processProtoRel(DataTable contentItems, ProtoRelationship pre, ProtoRelationship current)
        {
            DataRow contentRow;
            long befValueId, aftValueId;
            string befValue, aftValue, relType;

            befValue = (null != pre && null != pre.SubType) ? pre.SubType : "";
            aftValue = (null != current && null != current.SubType) ? current.SubType : "";
            relType = string.IsNullOrEmpty(aftValue) ? befValue : aftValue;
            if (!string.IsNullOrEmpty(befValue) && !string.IsNullOrEmpty(aftValue) && checkChange(befValue, aftValue))
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = "Relationship Type";
                contentRow[1] = befValue;
                contentRow[2] = aftValue;
                contentItems.Rows.Add(contentRow);
            }
            befValueId = (null != pre && null != pre.ChildGMFID) ? pre.ChildGMFID : 0;
            aftValueId = (null != current && null != current.ChildGMFID) ? current.ChildGMFID : 0;
            befValue = (null != pre && null != pre.ChildGMFID) ? pre.ChildGMFID.ToString() : "";
            aftValue = (null != current && null != current.ChildGMFID) ? current.ChildGMFID.ToString() : "";
            if (befValueId != aftValueId)
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = relType + " - Child GMFID";
                if (befValueId > 0)
                {
                    contentRow[1] = KSC.GMF.WebappLib.Support.GMFIDLink.createGMFIDLink(befValueId); ;
                }
                else
                {
                    contentRow[1] = befValue;
                }
                if (aftValueId > 0)
                {
                    contentRow[2] = KSC.GMF.WebappLib.Support.GMFIDLink.createGMFIDLink(aftValueId); ;
                }
                else
                {
                    contentRow[2] = aftValue;
                }
                contentItems.Rows.Add(contentRow);
            }
            befValueId = (null != pre && null != pre.ParentGMFID) ? pre.ParentGMFID : 0;
            aftValueId = (null != current && null != current.ParentGMFID) ? current.ParentGMFID : 0;
            befValue = (null != pre && null != pre.ParentGMFID) ? pre.ParentGMFID.ToString() : "";
            aftValue = (null != current && null != current.ParentGMFID) ? current.ParentGMFID.ToString() : "";
            if (befValueId != aftValueId)
            {
                contentRow = contentItems.NewRow();
                contentRow[0] = relType + " - Parent GMFID";
                if (befValueId > 0)
                {
                    contentRow[1] = KSC.GMF.WebappLib.Support.GMFIDLink.createGMFIDLink(befValueId); ;
                }
                else
                {
                    contentRow[1] = befValue;
                }
                if (aftValueId > 0)
                {
                    contentRow[2] = KSC.GMF.WebappLib.Support.GMFIDLink.createGMFIDLink(aftValueId); ;
                }
                else
                {
                    contentRow[2] = aftValue;
                }
                contentItems.Rows.Add(contentRow);
            }
        }

        #endregion

        private void validateSubsequentPopup(string currGMFId, string currEventId)
        {
            JavaScriptSerializer jsSer = new JavaScriptSerializer();
            Dictionary<string, bool> isAddrNoSeqMap = jsSer.Deserialize<Dictionary<string, bool>>(this.IsAddrNoSeq.Value);
            TransactionManager tm = new TransactionManager();
            tm.BeginTransaction();
            if (comTypeRecordList.Count > 0)
            {
                foreach (DataRow rec in comTypeRecordList)
                {
                    if (ValidateSubsequentChange("tChgCType", "hctOID", "hctEventID", "hctID", getComTypeOID(rec["subType"].ToString(), currGMFId),
                        long.Parse(currEventId), long.Parse(rec["subId"].ToString()), tm) > 0)
                    {
                        IsIndustryTypeNoSeq.Value = "false";
                        break;
                    }
                }
            }
            if (comAddrRecordList.Count > 0)
            {
                foreach (DataRow rec in comAddrRecordList)
                {
                    string type = rec["subType"].ToString();
                    string subId = rec["subId"].ToString();
                    if (!"STRUCTURE".Equals(type))
                    {
                        if (ValidateSubsequentChange("tchgCAddr", "hcaOID", "hcaEventID", "hcaID", getComAddrOID(rec["subType"].ToString(), currGMFId),
                            long.Parse(currEventId), long.Parse(subId), tm) > 0)
                        {
                            if ("REGISTERED".Equals(type))
                            {
                                isAddrNoSeqMap["isRAddrNoSeq"] = false;
                            }
                            else
                            {
                                isAddrNoSeqMap["isPAddrNoSeq"] = false;
                            }
                            continue;
                        }
                    }
                    else
                    {
                        if (ValidateSubsequentChange("tChgCType", "hctOID", "hctEventID", "hctID", getComTypeOID(rec["subType"].ToString(), currGMFId),
                            long.Parse(currEventId), long.Parse(subId), tm) > 0)
                        {
                            isAddrNoSeqMap["isRAddrNoSeq"] = false;
                            continue;
                        }
                    }
                }
            }
            IsAddrNoSeq.Value = jsSer.Serialize(isAddrNoSeqMap);
            tm.Dispose();
        }

        private void showAllChanges(long eventId, List<DataRow> allEventsData, DataTable eventTable)
        {
            string currGMFId = allEventsData[0]["heGMFID"].ToString();
            string currEventId = allEventsData[0]["heOID"].ToString();
            string changeReason = allEventsData[0]["heReason"].ToString();
            comTypeRecordList.Clear();
            comAddrRecordList.Clear();
            List<DataRow> industryTypeRow = new List<DataRow>();
            List<DataRow> registeredAddrRow = new List<DataRow>();
            foreach (DataRow rec in allEventsData)
            {
                string type = rec["subType"].ToString();
                if (COMPANY_ORDER_TYPE.Equals(rec["subOrder"].ToString()))
                {
                    if ("STRUCTURE".Equals(type))
                    {
                        comAddrRecordList.Add(rec);
                        industryTypeRow.Add(rec);
                    }
                    else if (!"ENTITY".Equals(type))
                    {
                        comTypeRecordList.Add(rec);
                    }
                }
                else if (COMPANY_ORDER_ADDR.Equals(rec["subOrder"].ToString()))
                {
                    comAddrRecordList.Add(rec);
                }
            }

            validateSubsequentPopup(currGMFId, currEventId);

            foreach (DataRow eventRec in allEventsData)
            {
                string subOrder = eventRec["subOrder"].ToString();
                SubOrder.Value = subOrder;
                string subId = eventRec["subId"].ToString();
                string typeDesc = eventRec["ldDesc"].ToString();
                dataType = subOrder;
                if (COMPANY_ORDER_DATA.Equals(subOrder))
                {
                    processComData(eventTable, subId, COMPANY_ORDER_DATA);
                }
                else if (COMPANY_ORDER_NAME.Equals(subOrder))
                {
                    processComName(eventTable, subId, typeDesc, COMPANY_ORDER_NAME);
                }
                else if (COMPANY_ORDER_ADDR.Equals(subOrder))
                {
                    processComAddr(eventTable, subId, typeDesc, COMPANY_ORDER_ADDR, industryTypeRow);
                }
                else if (COMPANY_ORDER_CONT.Equals(subOrder))
                {
                    processComContact(eventTable, subId, typeDesc, COMPANY_ORDER_CONT);
                }
                else if (COMPANY_ORDER_SVC.Equals(subOrder))
                {
                    processComService(eventTable, subId, typeDesc, COMPANY_ORDER_SVC);
                }
                else if (COMPANY_ORDER_TYPE.Equals(subOrder))
                {
                    processComType(eventTable, subId, typeDesc, COMPANY_ORDER_TYPE, industryTypeRow);
                }
                else if (COMPANY_ORDER_UNQI.Equals(subOrder))
                {
                    processComUnqi(eventTable, subId, typeDesc, COMPANY_ORDER_UNQI);
                }
                else if (SECURITY_ORDER_DATA.Equals(subOrder))
                {
                    processSecData(eventTable, subId, typeDesc, SECURITY_ORDER_DATA);
                }
                else if (SECURITY_ORDER_NAME.Equals(subOrder))
                {
                    processSecName(eventTable, subId, typeDesc);
                }
                else if (SECURITY_ORDER_UNQI.Equals(subOrder))
                {
                    processSecUnqi(eventTable, subId, typeDesc);
                }
                else if (SECURITY_ORDER_XCHG.Equals(subOrder))
                {
                    processSecExchange(eventTable, subId, typeDesc);
                }
                else if (RELATIONSHIP_ORDER_DATA.Equals(subOrder))
                {
                    processRelData(eventTable, subId, typeDesc, RELATIONSHIP_ORDER_DATA);
                }
            }

            // process source references
            JavaScriptSerializer jsSer = new JavaScriptSerializer();
            ChangeSourceReferencesCollection crs = new ChangeSourceReferencesCollection();
            crs.GetByChangeEventID(long.Parse(currEventId));

            ArrayList changeSourcesList = crs.ChangeSourceReferencesList;
            List<string> sourceTypes = new List<string>();
            List<string> sourceUrls = new List<string>();

            foreach (ChangeSourceReferences chaSource in changeSourcesList)
            {
                sourceTypes.Add(chaSource.Source);
                sourceUrls.Add(chaSource.ReferenceURL);
            }
            crs.Dispose();
            ChangeEventCollection ces = new ChangeEventCollection();
            ChangeEvent ceObj = ces.GetByPrimaryKey(eventId);
            ces.Dispose();
            string eventComment = ceObj.Comments;

            ChangeResonSource existingCha = new ChangeResonSource();
            existingCha.changeReason = changeReason;
            existingCha.changeSourceTypes = sourceTypes;
            existingCha.changeSourceUrls = sourceUrls;
            if (!string.IsNullOrEmpty(eventComment))
            {
                existingCha.showComment = "show";
                existingCha.changeComments = eventComment;
            }
            else
            {
                existingCha.showComment = "";
                existingCha.changeComments = "";
            }
            string existingSrc = jsSer.Serialize(existingCha);
            ChangeRSRevjson.Value = existingSrc;
            ChangeRSBefJson.Value = existingSrc;
            string updatedValueStr = jsSer.Serialize(updatedValuesMap);
            UpdatedValues.Value = updatedValueStr;
            string inputValueStr = jsSer.Serialize(inputMap);
            InputValues.Value = inputValueStr;
            string updatedValueListStr = jsSer.Serialize(updateValuesList);
            UpdateValuesListHidden.Value = updatedValueListStr;
            addressChangedList.Add(false);
            addressChangedList.Add(false);
            AddressChanged.Value = jsSer.Serialize(addressChangedList);
            crs.Dispose();
        }

        /// <summary>
        /// Get values of input in Update Values text.
        /// </summary>
        /// <returns></returns>
        private List<List<string>> getInputValues()
        {
            List<List<string>> returnList = new List<List<string>>();
            foreach (DataGridItem eachRow in revchgEvents.Items)
            {
                Control ctrl = eachRow.FindControl("updatedValueLst");
                ListDataDropDown updatedValueList = (ListDataDropDown)ctrl;
                if (null != updatedValueList.SelectedItem)
                {
                    if (SELECT.Equals(updatedValueList.SelectedItem.Text) || List.LIST_CONTROL_DEFAULT_VALUE.Equals(updatedValueList.SelectedValue))
                    {
                        List<string> lst = new List<string>();
                        lst.Add(string.Empty);
                        returnList.Add(lst);
                    }
                    else
                    {
                        string header = eachRow.Cells[0].Text;
                        if (header.Contains("Country") || header.Contains("State Province"))
                        {
                            List<string> lst = new List<string>();
                            lst.Add(updatedValueList.SelectedItem.Text);
                            lst.Add(updatedValueList.SelectedValue);
                            returnList.Add(lst);
                        }
                        else if (header.Contains("Security Class"))
                        {
                            string secClass = updatedValueList.SelectedItem.Text.Substring(0, updatedValueList.SelectedItem.Text.IndexOf("-")).Trim();
                            List<string> lst = new List<string>();
                            lst.Add(secClass);
                            returnList.Add(lst);
                        }
                        else if (header.Contains("Security Type"))
                        {
                            List<string> lst = new List<string>();
                            lst.Add(updatedValueList.SelectedValue);
                            returnList.Add(lst);
                        }
                        else if (header.Contains(ChangeReasonEditor.IDEN1_TYPE) || header.Contains(ChangeReasonEditor.IDEN2_TYPE))
                        {
                            List<string> lst = new List<string>();
                            lst.Add(updatedValueList.SelectedValue);
                            lst.Add(updatedValueList.SelectedItem.Text);
                            returnList.Add(lst);
                        }
                        else
                        {
                            List<string> lst = new List<string>();
                            lst.Add(updatedValueList.SelectedValue);
                            returnList.Add(lst);
                        }
                    }
                }
                else
                {
                    Control ctrl2 = eachRow.FindControl("updatedValueMs");
                    MonthDaySelectorControl updatedValueMs = (MonthDaySelectorControl)ctrl2;
                    if (null != updatedValueMs.SelectedMonth)
                    {
                        List<string> lst = new List<string>();
                        lst.Add(updatedValueMs.GetSelectedDateString());
                        returnList.Add(lst);
                    }
                    else
                    {
                        TextBox updatedValueTxtBox = (TextBox)eachRow.FindControl("updatedValue");
                        if ("&nbsp;".Equals(updatedValueTxtBox.Text))
                        {
                            List<string> lst = new List<string>();
                            lst.Add(string.Empty);
                            returnList.Add(lst);
                        }
                        else
                        {
                            List<string> lst = new List<string>();
                            lst.Add(updatedValueTxtBox.Text);
                            returnList.Add(lst);
                        }
                    }
                }
            }
            return returnList;
        }

        /// <summary>
        /// Get values of input in Update Values text.
        /// </summary>
        /// <returns></returns>
        private string getValueForDesc(string desc)
        {
            string returnValue = string.Empty;
            foreach (DataGridItem eachRow in revchgEvents.Items)
            {
                Control ctrl = eachRow.FindControl("updatedValueLst");
                ListDataDropDown updatedValueList = (ListDataDropDown)ctrl;
                if (null != updatedValueList.Items)
                {
                    if (null != updatedValueList.Items.FindByText(desc))
                    {
                        returnValue = updatedValueList.Items.FindByText(desc).Value;
                    }
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Check if Class or Type of security matches.
        /// </summary>
        /// <returns></returns>
        private bool checkSecurityClassTypeMatch()
        {
            DataGridItemCollection col = revchgEvents.Items;
            string securityClass = string.Empty;
            string securityType = string.Empty;
            string securityClassDes = string.Empty;
            string securityTypeDes = string.Empty;
            for (int i = 0; i < col.Count; i++)
            {
                DataGridItem eachRow = revchgEvents.Items[i];
                string header = eachRow.Cells[0].Text;
                string newValue = eachRow.Cells[2].Text;
                if (header.Contains("Security Class"))
                {
                    ListDataDropDown updatedValueList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                    securityClass = updatedValueList.SelectedValue;
                    securityClassDes = updatedValueList.SelectedItem.Text;
                }
                else if (header.Contains("Security Type"))
                {
                    ListDataDropDown updatedValueList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                    securityType = updatedValueList.SelectedValue;
                    securityTypeDes = updatedValueList.SelectedItem.Text;
                }

                if (!string.IsNullOrEmpty(securityClass) && !string.IsNullOrEmpty(securityType))
                {
                    break;
                }
            }

            if (!securityTypeDes.Contains(securityClass))
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Check for mandatory child whose parent is in change and itself has no change.
        /// </summary>
        private bool isOldAndNewValueEmpty(string oldValue, string newValue)
        {
            return ((string.IsNullOrEmpty(oldValue) || "&nbsp;".Equals(oldValue)) &&
                (string.IsNullOrEmpty(newValue) || "&nbsp;".Equals(newValue)));
        }

        /// <summary>
        /// Bind different controls to each row.
        /// </summary>
        private void setListValues()
        {

            DataGridItemCollection col = revchgEvents.Items;
            int securityClassRow = 0;
            string selectedNAICSValue = string.Empty;
            JavaScriptSerializer jsSer = new JavaScriptSerializer();
            Dictionary<string, bool> isAddrNoSeqMap = jsSer.Deserialize<Dictionary<string, bool>>(this.IsAddrNoSeq.Value);

            for (int i = 0; i < col.Count; i++)
            {
                UpdatedValuesEntity entity = new UpdatedValuesEntity();

                DataGridItem eachRow = revchgEvents.Items[i];
                string header = eachRow.Cells[0].Text;
                string oldValue = eachRow.Cells[1].Text;
                string newValue = eachRow.Cells[2].Text;
                if ("&nbsp;".Equals(newValue))
                {
                    newValue = string.Empty;
                }

                if (header.Contains(PHSICAL_ADDRESS_COUNTRY))
                {
                    ((TextBox)eachRow.FindControl("updatedValue")).Visible = false;
                    ((MonthDaySelectorControl)eachRow.FindControl("updatedValueMs")).Visible = false;
                    if (long.Parse(this.GMFID.Value) < 0)
                    {
                        ((CompareValidator)eachRow.FindControl("CompareValidator_updatedValueLst")).Enabled = true;
                        ((CompareValidator)eachRow.FindControl("CompareValidator_updatedValueLst")).ErrorMessage = "Country is required.";
                    }
                    ListDataDropDown updatedValueList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                    updatedValueList.EmptyRowText = EMPTY_ROW;
                    updatedValueList.ListName = "ISOCountry";
                    updatedValueList.forceRebind = true;
                    updatedValueList.BindList();
                    int selectIndex = 0;
                    for (int x = 0; x < updatedValueList.Items.Count; x++)
                    {
                        ListItem item = updatedValueList.Items[x];
                        if (item.Text.Equals(newValue))
                        {
                            selectIndex = x;
                            break;
                        }
                    }
                    updatedValueList.SelectedIndex = selectIndex;
                    updatedValueList.Visible = isAddrNoSeqMap["isPAddrNoSeq"];
                    this.Country_onBlur.Text = PHSICAL_ADDRESS_COUNTRY;
                    string eventHandler = Page.ClientScript.GetPostBackEventReference(this.Country_onBlur, "");
                    updatedValueList.Attributes.Add("onchange", eventHandler);
                    entity.listName = PHSICAL_ADDRESS_COUNTRY;
                    entity.listRow = i;
                    updateValuesList.Add(entity);
                }
                else if (header.Contains(PHSICAL_ADDRESS_STATE))
                {
                    ((TextBox)eachRow.FindControl("updatedValue")).Visible = false;
                    ((MonthDaySelectorControl)eachRow.FindControl("updatedValueMs")).Visible = false;
                    if (long.Parse(this.GMFID.Value) < 0)
                    {
                        ((CompareValidator)eachRow.FindControl("CompareValidator_updatedValueLst")).Enabled = true;
                        ((CompareValidator)eachRow.FindControl("CompareValidator_updatedValueLst")).ErrorMessage = "State Province is required.";
                    }
                    ListDataDropDown updatedValueList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                    updatedValueList.EmptyRowText = EMPTY_ROW;
                    updatedValueList.ListName = getSubListDataForCountry("", ChangeReasonEditor.PA_STATE);
                    if (long.Parse(this.GMFID.Value) < 0 && null != proComs)
                    {
                        int index = int.Parse(this.GMFID.Value);
                        if (null != proComs[index] && null != proComs[index].CountryOfIncorporation)
                        {
                            updatedValueList.ListName = getSubListDataForCountry(proComs[index].History[historyIndexForProto].CountryOfIncorporation, ChangeReasonEditor.PA_STATE);
                        }
                    }
                    int selectIndex = 0;
                    if (null != updatedValueList.ListName && !"EmptyList".Equals(updatedValueList.ListName))
                    {
                        updatedValueList.forceRebind = true;
                        updatedValueList.BindList();
                        for (int x = 0; x < updatedValueList.Items.Count; x++)
                        {
                            ListItem item = updatedValueList.Items[x];
                            if (item.Text.Equals(newValue))
                            {
                                selectIndex = x;
                                break;
                            }
                        }
                    }
                    else
                    {
                        updatedValueList.forceRebind = false;
                        updatedValueList.Enabled = false;
                    }
                    updatedValueList.SelectedIndex = selectIndex;
                    updatedValueList.Visible = isAddrNoSeqMap["isPAddrNoSeq"];
                    entity.listName = PHSICAL_ADDRESS_STATE;
                    entity.listRow = i;
                    updateValuesList.Add(entity);
                    this.PState_onChange.Text = PHSICAL_ADDRESS_STATE;
                    string eventHandler = Page.ClientScript.GetPostBackEventReference(this.PState_onChange, "");
                    updatedValueList.Attributes.Add("onchange", eventHandler);
                }
                else if (header.Contains(REGISTRY_ADDRESS_COUNTRY))
                {
                    ((TextBox)eachRow.FindControl("updatedValue")).Visible = false;
                    ((MonthDaySelectorControl)eachRow.FindControl("updatedValueMs")).Visible = false;
                    ListDataDropDown updatedValueList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                    updatedValueList.EmptyRowText = EMPTY_ROW;
                    updatedValueList.ListName = "ISOCountry";
                    updatedValueList.forceRebind = true;
                    updatedValueList.BindList();
                    int selectIndex = 0;
                    for (int x = 0; x < updatedValueList.Items.Count; x++)
                    {
                        ListItem item = updatedValueList.Items[x];
                        if (item.Text.Equals(newValue))
                        {
                            selectIndex = x;
                            break;
                        }
                    }
                    updatedValueList.SelectedIndex = selectIndex;
                    updatedValueList.Visible = isAddrNoSeqMap["isRAddrNoSeq"];
                    this.ResCountry_onBlur.Text = REGISTRY_ADDRESS_COUNTRY;
                    string eventHandler = Page.ClientScript.GetPostBackEventReference(this.ResCountry_onBlur, "");
                    updatedValueList.Attributes.Add("onchange", eventHandler);
                    entity.listName = REGISTRY_ADDRESS_COUNTRY;
                    entity.listRow = i;
                    updateValuesList.Add(entity);
                }
                else if (header.Contains(REGISTRY_ADDRESS_STATE))
                {
                    ((TextBox)eachRow.FindControl("updatedValue")).Visible = false;
                    ((MonthDaySelectorControl)eachRow.FindControl("updatedValueMs")).Visible = false;
                    ListDataDropDown updatedValueList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                    updatedValueList.EmptyRowText = EMPTY_ROW;
                    updatedValueList.ListName = getSubListDataForCountry("", ChangeReasonEditor.RA_STATE);
                    updatedValueList.Visible = isAddrNoSeqMap["isRAddrNoSeq"];
                    if (long.Parse(this.GMFID.Value) < 0 && null != proComs)
                    {
                        int index = int.Parse(this.GMFID.Value);
                        if (null != proComs[index] && null != proComs[index].RegCountry)
                        {
                            updatedValueList.ListName = getSubListDataForCountry(proComs[index].History[historyIndexForProto].RegCountry, ChangeReasonEditor.RA_STATE);
                        }
                    }
                    int selectIndex = 0;
                    if (null != updatedValueList.ListName && !"EmptyList".Equals(updatedValueList.ListName))
                    {
                        updatedValueList.forceRebind = true;
                        updatedValueList.BindList();
                        for (int x = 0; x < updatedValueList.Items.Count; x++)
                        {
                            ListItem item = updatedValueList.Items[x];
                            if (item.Text.Equals(newValue))
                            {
                                selectIndex = x;
                                break;
                            }
                        }
                    }
                    else
                    {
                        updatedValueList.forceRebind = false;
                        updatedValueList.Enabled = false;
                    }
                    updatedValueList.SelectedIndex = selectIndex;
                    entity.listName = REGISTRY_ADDRESS_STATE;
                    entity.listRow = i;
                    updateValuesList.Add(entity);
                    this.RState_onChange.Text = REGISTRY_ADDRESS_STATE;
                    string eventHandler = Page.ClientScript.GetPostBackEventReference(this.RState_onChange, "");
                    updatedValueList.Attributes.Add("onchange", eventHandler);
                }
                else if (header.Contains(LEGAL_STRUCTURE))
                {
                    ((TextBox)eachRow.FindControl("updatedValue")).Visible = false;
                    ((MonthDaySelectorControl)eachRow.FindControl("updatedValueMs")).Visible = false;
                    ListDataDropDown updatedValueList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                    updatedValueList.EmptyRowText = EMPTY_ROW;
                    updatedValueList.ListName = getSubListDataForCountry("", ChangeReasonEditor.LEGAL_STRUCTURE);
                    updatedValueList.Visible = isAddrNoSeqMap["isRAddrNoSeq"];
                    if (long.Parse(this.GMFID.Value) < 0 && null != proComs)
                    {
                        int index = int.Parse(this.GMFID.Value);
                        if (null != proComs[index] && null != proComs[index].RegCountry)
                        {
                            updatedValueList.ListName = getSubListDataForCountry(proComs[index].History[historyIndexForProto].RegCountry, ChangeReasonEditor.LEGAL_STRUCTURE);
                        }
                    }
                    if (null != updatedValueList.ListName && !"EmptyList".Equals(updatedValueList.ListName))
                    {
                        updatedValueList.Enabled = true;
                        updatedValueList.forceRebind = true;
                        updatedValueList.BindList();
                        updatedValueList.SelectItem(newValue);
                    }
                    else
                    {
                        updatedValueList.Enabled = false;
                    }
                    entity.listName = LEGAL_STRUCTURE;
                    entity.listRow = i;
                    updateValuesList.Add(entity);
                    this.LegalStruc_onChange.Text = LEGAL_STRUCTURE;
                    string eventHandler = Page.ClientScript.GetPostBackEventReference(this.LegalStruc_onChange, "");
                    updatedValueList.Attributes.Add("onchange", eventHandler);
                }
                else if (header.Contains(NAICSSUB))
                {
                    ((TextBox)eachRow.FindControl("updatedValue")).Visible = false;
                    ((CompareValidator)eachRow.FindControl("CompareValidator_updatedValueLst")).Enabled = bool.Parse(IsIndustryTypeNoSeq.Value);
                    ((CompareValidator)eachRow.FindControl("CompareValidator_updatedValueLst")).ErrorMessage = NAICSSUB + " is required.";
                    ((MonthDaySelectorControl)eachRow.FindControl("updatedValueMs")).Visible = false;
                    ListDataDropDown updatedValueList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                    updatedValueList.ListFormat = KSC.GMF.Business.List.ListFormatType.ValueDescription;
                    updatedValueList.EmptyRowText = EMPTY_ROW;
                    String naicsSubClassList = string.Empty;
                    updatedValueList.ListName = getSubListDataForNAICS("");
                    updatedValueList.Visible = bool.Parse(IsIndustryTypeNoSeq.Value);
                    if (!string.IsNullOrEmpty(updatedValueList.ListName) && !"EmptyList".Equals(updatedValueList.ListName))
                    {
                        updatedValueList.forceRebind = true;
                        updatedValueList.BindList();
                        updatedValueList.SelectItem(newValue);
                        updatedValueList.ListFormat = List.ListFormatType.Description;
                        updatedValueList.Visible = bool.Parse(IsIndustryTypeNoSeq.Value);
                        ((CompareValidator)eachRow.FindControl("CompareValidator_updatedValueLst")).Enabled = bool.Parse(IsIndustryTypeNoSeq.Value);
                    }
                    else
                    {
                        updatedValueList.forceRebind = false;
                        ((CompareValidator)eachRow.FindControl("CompareValidator_updatedValueLst")).Enabled = false;
                        updatedValueList.Visible = false;
                    }
                    entity.listName = NAICSSUB;
                    entity.listRow = i;
                    updateValuesList.Add(entity);

                }
                else if (header.Contains(NAICS))
                {
                    ((TextBox)eachRow.FindControl("updatedValue")).Visible = false;
                    ((CompareValidator)eachRow.FindControl("CompareValidator_updatedValueLst")).Enabled = bool.Parse(IsIndustryTypeNoSeq.Value);
                    ((CompareValidator)eachRow.FindControl("CompareValidator_updatedValueLst")).ErrorMessage = NAICS + " is required.";
                    ((MonthDaySelectorControl)eachRow.FindControl("updatedValueMs")).Visible = false;
                    ListDataDropDown updatedValueList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                    this.NAICS_onBlur.Text = NAICS;
                    string eventHandler = Page.ClientScript.GetPostBackEventReference(this.NAICS_onBlur, "");
                    updatedValueList.Attributes.Add("onchange", eventHandler);
                    updatedValueList.ListFormat = KSC.GMF.Business.List.ListFormatType.ValueDescription;
                    updatedValueList.EmptyRowText = EMPTY_ROW;
                    updatedValueList.ListName = List.NAICS_CODES;
                    updatedValueList.forceRebind = true;
                    updatedValueList.BindList();
                    updatedValueList.SelectItem(newValue);
                    updatedValueList.Visible = bool.Parse(IsIndustryTypeNoSeq.Value);
                    entity.listName = NAICS;
                    entity.listRow = i;
                    updateValuesList.Add(entity);
                    selectedNAICSValue = newValue;

                }
                else if (header.Contains(SIC))
                {
                    ((TextBox)eachRow.FindControl("updatedValue")).Visible = false;
                    ((CompareValidator)eachRow.FindControl("CompareValidator_updatedValueLst")).Enabled = bool.Parse(IsIndustryTypeNoSeq.Value);
                    ((CompareValidator)eachRow.FindControl("CompareValidator_updatedValueLst")).ErrorMessage = SIC + " is required.";
                    ((MonthDaySelectorControl)eachRow.FindControl("updatedValueMs")).Visible = false;
                    ListDataDropDown updatedValueList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                    updatedValueList.EmptyRowText = EMPTY_ROW;
                    updatedValueList.ListFormat = KSC.GMF.Business.List.ListFormatType.ValueDescription;
                    updatedValueList.ListName = "fakedataSICCodes";
                    updatedValueList.SelectedNaicsValue = getComTypeCode(NAICS);
                    if (null == updatedValueList.SelectedNaicsValue && long.Parse(this.GMFID.Value) < 0)
                    {
                        updatedValueList.ListName = "SICCodes";
                    }
                    updatedValueList.forceRebind = true;
                    updatedValueList.BindList();
                    if (isOldAndNewValueEmpty(oldValue, newValue))
                    {
                        eachRow.Cells[1].Text = getComTypeCode("SIC1");
                        eachRow.Cells[2].Text = getComTypeCode("SIC1");
                        newValue = eachRow.Cells[2].Text;
                    }
                    updatedValueList.SelectItem(newValue);
                    updatedValueList.Visible = bool.Parse(IsIndustryTypeNoSeq.Value);
                    entity.listName = SIC;
                    entity.listRow = i;
                    updateValuesList.Add(entity);

                }
                else if (header.Contains(NACE))
                {
                    ((TextBox)eachRow.FindControl("updatedValue")).Visible = false;
                    ((CompareValidator)eachRow.FindControl("CompareValidator_updatedValueLst")).Enabled = bool.Parse(IsIndustryTypeNoSeq.Value);
                    ((CompareValidator)eachRow.FindControl("CompareValidator_updatedValueLst")).ErrorMessage = NACE + " is required.";
                    ((MonthDaySelectorControl)eachRow.FindControl("updatedValueMs")).Visible = false;
                    ListDataDropDown updatedValueList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                    updatedValueList.EmptyRowText = EMPTY_ROW;
                    updatedValueList.ListFormat = KSC.GMF.Business.List.ListFormatType.ValueDescription;
                    updatedValueList.ListName = "fakedataNACECodes";
                    updatedValueList.SelectedNaicsValue = getComTypeCode(NAICS);
                    if (null == updatedValueList.SelectedNaicsValue && long.Parse(this.GMFID.Value) < 0)
                    {
                        updatedValueList.ListName = "NACECodes";
                    }
                    updatedValueList.forceRebind = true;
                    updatedValueList.BindList();
                    if (isOldAndNewValueEmpty(oldValue, newValue))
                    {
                        eachRow.Cells[1].Text = getComTypeCode(NACE);
                        eachRow.Cells[2].Text = getComTypeCode(NACE);
                        newValue = eachRow.Cells[2].Text;
                    }
                    updatedValueList.SelectItem(newValue);
                    updatedValueList.Visible = bool.Parse(IsIndustryTypeNoSeq.Value);
                    entity.listName = NACE;
                    entity.listRow = i;
                    updateValuesList.Add(entity);
                }
                else if (header.Contains(GICS))
                {
                    ((TextBox)eachRow.FindControl("updatedValue")).Visible = false;
                    ((CompareValidator)eachRow.FindControl("CompareValidator_updatedValueLst")).Enabled = false;
                    ((MonthDaySelectorControl)eachRow.FindControl("updatedValueMs")).Visible = false;
                    ListDataDropDown updatedValueList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                    updatedValueList.EmptyRowText = EMPTY_ROW;
                    updatedValueList.ListFormat = KSC.GMF.Business.List.ListFormatType.ValueDescription;
                    updatedValueList.ListName = "fakedataEditGICSCodes";
                    updatedValueList.SelectedNaicsValue = getComTypeCode(NAICS);
                    if (null == updatedValueList.SelectedNaicsValue && long.Parse(this.GMFID.Value) < 0)
                    {
                        updatedValueList.ListName = "GICSCodes";
                    }
                    updatedValueList.forceRebind = true;
                    updatedValueList.BindList();
                    if (isOldAndNewValueEmpty(oldValue, newValue))
                    {
                        eachRow.Cells[1].Text = getComTypeCode(GICS);
                        eachRow.Cells[2].Text = getComTypeCode(GICS);
                        newValue = eachRow.Cells[2].Text;
                    }
                    if (string.Empty.Equals(newValue))
                    {
                        updatedValueList.SelectedIndex = 0;
                    }
                    else
                    {
                        updatedValueList.SelectItem(newValue);
                    }
                    updatedValueList.Visible = bool.Parse(IsIndustryTypeNoSeq.Value);
                    entity.listName = GICS;
                    entity.listRow = i;
                    updateValuesList.Add(entity);
                }
                else if (header.Contains("Entity Classification"))
                {
                    ((TextBox)eachRow.FindControl("updatedValue")).Visible = false;
                    ((CompareValidator)eachRow.FindControl("CompareValidator_updatedValueLst")).Enabled = true;
                    ((CompareValidator)eachRow.FindControl("CompareValidator_updatedValueLst")).ErrorMessage = "Entity Classification is required.";
                    ((MonthDaySelectorControl)eachRow.FindControl("updatedValueMs")).Visible = false;
                    ListDataDropDown updatedValueList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                    updatedValueList.EmptyRowText = EMPTY_ROW;
                    updatedValueList.ListName = List.COMPANY_TYPE_ENTITY_CODES;
                    updatedValueList.forceRebind = true;
                    updatedValueList.BindList();
                    updatedValueList.SelectItem(newValue);
                }
                else if (header.Contains("Reason for expiration"))
                {
                    ((TextBox)eachRow.FindControl("updatedValue")).Visible = false;
                    ((MonthDaySelectorControl)eachRow.FindControl("updatedValueMs")).Visible = false;
                    ListDataDropDown updatedValueList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                    updatedValueList.EmptyRowText = EMPTY_ROW;
                    updatedValueList.Enabled = false;
                    updatedValueList.ListName = "Reason for expiration";
                    updatedValueList.forceRebind = true;
                    updatedValueList.BindList();
                    entity.listName = "Reason for expiration";
                    entity.listRow = i;
                    updateValuesList.Add(entity);
                    int selectIndex = 0;
                    if (newValue.Equals("ACTIVE"))
                    {
                        selectIndex = 1;
                    }
                    else if (newValue.Equals("OB"))
                    {
                        selectIndex = 2;
                    }
                    else if (newValue.Equals("NOTRADE"))
                    {
                        selectIndex = 4;
                    }
                    else
                    {
                        selectIndex = 0;
                    }
                    updatedValueList.SelectedIndex = selectIndex;
                }
                else if (header.Contains("Is Expired"))
                {
                    ((TextBox)eachRow.FindControl("updatedValue")).Visible = false;
                    ((CompareValidator)eachRow.FindControl("CompareValidator_updatedValueLst")).Enabled = true;
                    ((CompareValidator)eachRow.FindControl("CompareValidator_updatedValueLst")).ErrorMessage = "Is Expired is required.";
                    ((MonthDaySelectorControl)eachRow.FindControl("updatedValueMs")).Visible = false;
                    ListDataDropDown updatedValueList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                    updatedValueList.EmptyRowText = EMPTY_ROW;
                    if (SECURITY_ORDER_DATA.Equals(dataType))
                    {
                        updatedValueList.ListName = "Sec Is Expired";
                        this.IsExpired_onBlur.Text = "Sec Is Expired";
                    }
                    else
                    {
                        updatedValueList.ListName = "Is Expired";
                        this.IsExpired_onBlur.Text = "Is Expired";
                    }
                    string eventHandler = Page.ClientScript.GetPostBackEventReference(this.IsExpired_onBlur, "");
                    updatedValueList.Attributes.Add("onchange", eventHandler);
                    updatedValueList.forceRebind = true;
                    updatedValueList.BindList();
                    entity.listName = "Is Expired";
                    entity.listRow = i;
                    updateValuesList.Add(entity);
                    int selectIndex = 0;
                    if (newValue.Equals("False"))
                    {
                        selectIndex = 1;
                    }
                    else if (newValue.Equals("True"))
                    {
                        selectIndex = 2;
                    }
                    else
                    {
                        selectIndex = 0;
                    }
                    updatedValueList.SelectedIndex = selectIndex;
                }
                else if (header.Contains("Is Public"))
                {
                    ((TextBox)eachRow.FindControl("updatedValue")).Visible = false;
                    ((MonthDaySelectorControl)eachRow.FindControl("updatedValueMs")).Visible = false;
                    ListDataDropDown updatedValueList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                    updatedValueList.EmptyRowText = EMPTY_ROW;
                    updatedValueList.ListName = "Is Public";
                    updatedValueList.forceRebind = true;
                    updatedValueList.BindList();
                    int selectIndex = 0;
                    if (newValue.Equals("False"))
                    {
                        selectIndex = 2;
                    }
                    else if (newValue.Equals("True"))
                    {
                        selectIndex = 1;
                    }
                    else
                    {
                        selectIndex = 0;
                    }
                    updatedValueList.SelectedIndex = selectIndex;
                }
                else if (header.Contains("Fiscal Year End"))
                {
                    ((TextBox)eachRow.FindControl("updatedValue")).Visible = false;
                    ((ListDataDropDown)eachRow.FindControl("updatedValueLst")).Visible = false;
                    MonthDaySelectorControl updatedValueMs = (MonthDaySelectorControl)eachRow.FindControl("updatedValueMs");
                    updatedValueMs.SetSelectedDateByString(newValue);
                }
                else if (header.Contains("Unique Identifier") && header.Contains("Type"))
                {
                    ((TextBox)eachRow.FindControl("updatedValue")).Visible = false;
                    ((MonthDaySelectorControl)eachRow.FindControl("updatedValueMs")).Visible = false;
                    ListDataDropDown updatedValueList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                    updatedValueList.EmptyRowText = EMPTY_ROW;
                    updatedValueList.ListName = List.COMPANY_UNIQUE_ID_TYPES;
                    updatedValueList.forceRebind = true;
                    updatedValueList.BindList();
                    int selectIndex = 0;
                    for (int x = 0; x < updatedValueList.Items.Count; x++)
                    {
                        ListItem item = updatedValueList.Items[x];
                        if (item.Text.Equals(newValue))
                        {
                            selectIndex = x;
                            break;
                        }
                    }
                    updatedValueList.SelectedIndex = selectIndex;
                }
                else if (header.Contains("Date of expiration"))
                {
                    TextBox updatedValueTxtBox = (TextBox)eachRow.FindControl("updatedValue");
                    if ("&nbsp;".Equals(newValue))
                    {
                        updatedValueTxtBox.Text = string.Empty;
                    }
                    else
                    {
                        updatedValueTxtBox.Text = newValue;
                    }
                    updatedValueTxtBox.Enabled = false;
                    ((ListDataDropDown)eachRow.FindControl("updatedValueLst")).Visible = false;
                    ((MonthDaySelectorControl)eachRow.FindControl("updatedValueMs")).Visible = false;
                    entity.listName = "Date of expiration";
                    entity.listRow = i;
                    updateValuesList.Add(entity);
                }
                else if (header.Contains("Security Class"))
                {
                    ((TextBox)eachRow.FindControl("updatedValue")).Visible = false;
                    ((MonthDaySelectorControl)eachRow.FindControl("updatedValueMs")).Visible = false;
                    ListDataDropDown updatedValueList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                    entity.listName = "Security Class";
                    entity.listRow = i;
                    updateValuesList.Add(entity);
                    this.SecurityClass_onBlur.Text = "Security Class";
                    string eventHandler = Page.ClientScript.GetPostBackEventReference(this.SecurityClass_onBlur, "");
                    updatedValueList.Attributes.Add("onchange", eventHandler);
                    securityClassRow = i;

                }
                else if (header.Contains("Security Type"))
                {
                    ((TextBox)eachRow.FindControl("updatedValue")).Visible = false;
                    ((MonthDaySelectorControl)eachRow.FindControl("updatedValueMs")).Visible = false;
                    ListDataDropDown updatedValueList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                    entity.listName = "Security Type";
                    entity.listRow = i;
                    updateValuesList.Add(entity);
                    ArrayList listdata = new ArrayList(List.QuickGetList("SecurityType").ListData);
                    foreach (ListData ld in listdata)
                    {
                        if (log.IsDebugEnabled)
                            log.Debug("Iterating through ArrayList ListData ld: " + ld);

                        ld.Supplement2 = ld.Supplement1 + " - " + ld.Description;
                    }
                    updatedValueList.DisplayEmptyRow = true;
                    updatedValueList.EmptyRowText = EMPTY_ROW;
                    updatedValueList.DataSource = listdata;
                    updatedValueList.DataTextField = "Supplement2";
                    updatedValueList.DataValueField = "Value";
                    updatedValueList.DataBind();
                    int selectIndex = 0;
                    for (int x = 0; x < updatedValueList.Items.Count; x++)
                    {
                        ListItem item = updatedValueList.Items[x];
                        if (item.Value.Equals(newValue))
                        {
                            selectIndex = x;
                            break;
                        }
                    }
                    updatedValueList.SelectedIndex = selectIndex;
                    updatedValueList.Enabled = false;
                    //set security class
                    DataGridItem securityClass = revchgEvents.Items[securityClassRow];
                    ListDataDropDown updatedValueList2 = (ListDataDropDown)securityClass.FindControl("updatedValueLst");
                    updatedValueList2.DisplayEmptyRow = true;
                    updatedValueList2.EmptyRowText = EMPTY_ROW;
                    updatedValueList2.DataSource = listdata;
                    updatedValueList2.DataTextField = "Supplement2";
                    updatedValueList2.DataValueField = "Supplement2";
                    updatedValueList2.DataBind();
                    updatedValueList2.SelectedIndex = selectIndex;
                }
                else if (header.Contains("Default Value"))
                {
                    ((TextBox)eachRow.FindControl("updatedValue")).Visible = false;
                    ((MonthDaySelectorControl)eachRow.FindControl("updatedValueMs")).Visible = false;
                    ListDataDropDown updatedValueList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                    entity.listName = "Default Value";
                    entity.listRow = i;
                    updateValuesList.Add(entity);
                    ArrayList listdata = new ArrayList(List.QuickGetList("ClassDefaultValue").ListData);
                    updatedValueList.DataSource = listdata;
                    updatedValueList.DataTextField = "Description";
                    updatedValueList.DataValueField = "Value";
                    updatedValueList.DataBind();
                    int selectIndex = 0;
                    for (int x = 0; x < updatedValueList.Items.Count; x++)
                    {
                        ListItem item = updatedValueList.Items[x];
                        if (item.Value.Equals(newValue))
                        {
                            selectIndex = x;
                            break;
                        }
                    }
                    updatedValueList.SelectedIndex = selectIndex;
                }
                else if (long.Parse(this.GMFID.Value) < 0 && (header.Contains(ChangeReasonEditor.LEGAL_NAME) || header.Contains(ChangeReasonEditor.PA_CITY)))
                {
                    string filed = header.ToString();
                    TextBox updatedValueTxtBox = (TextBox)eachRow.FindControl("updatedValue");
                    ((RequiredFieldValidator)eachRow.FindControl("RequiredFieldValidator")).Enabled = true;
                    ((RequiredFieldValidator)eachRow.FindControl("RequiredFieldValidator")).ErrorMessage = filed + " is requried";
                    if ("&nbsp;".Equals(newValue))
                    {
                        updatedValueTxtBox.Text = string.Empty;
                    }
                    else
                    {

                        updatedValueTxtBox.Text = newValue;
                    }
                    ((ListDataDropDown)eachRow.FindControl("updatedValueLst")).Visible = false;
                    ((MonthDaySelectorControl)eachRow.FindControl("updatedValueMs")).Visible = false;
                }
                else
                {
                    TextBox updatedValueTxtBox = (TextBox)eachRow.FindControl("updatedValue");
                    if ("&nbsp;".Equals(newValue))
                    {
                        updatedValueTxtBox.Text = string.Empty;
                    }
                    else
                    {

                        updatedValueTxtBox.Text = newValue;
                    }
                    ((ListDataDropDown)eachRow.FindControl("updatedValueLst")).Visible = false;
                    ((MonthDaySelectorControl)eachRow.FindControl("updatedValueMs")).Visible = false;
                    if (header.Contains("Percentage"))
                    {
                        ((FilteredTextBoxExtender)eachRow.FindControl("validatePercent")).Enabled = true;
                    }
                    else if (header.Contains(PHSICAL_ADDRESS_LINE_ONE))
                    {
                        this.PAddrOne_onChange.Text = PHSICAL_ADDRESS_LINE_ONE;
                        string eventHandler = Page.ClientScript.GetPostBackEventReference(this.PAddrOne_onChange, "");
                        updatedValueTxtBox.Attributes.Add("onchange", eventHandler);
                        entity.listName = PHSICAL_ADDRESS_LINE_ONE;
                        entity.listRow = i;
                        updateValuesList.Add(entity);
                    }
                    else if (header.Contains(PHSICAL_ADDRESS_LINE_TWO))
                    {
                        this.PAddrTwo_onChange.Text = PHSICAL_ADDRESS_LINE_TWO;
                        string eventHandler = Page.ClientScript.GetPostBackEventReference(this.PAddrTwo_onChange, "");
                        updatedValueTxtBox.Attributes.Add("onchange", eventHandler);
                        entity.listName = PHSICAL_ADDRESS_LINE_TWO;
                        entity.listRow = i;
                        updateValuesList.Add(entity);
                    }
                    else if (header.Contains(PHSICAL_ADDRESS_CITY))
                    {
                        this.PCity_onChange.Text = PHSICAL_ADDRESS_CITY;
                        string eventHandler = Page.ClientScript.GetPostBackEventReference(this.PCity_onChange, "");
                        updatedValueTxtBox.Attributes.Add("onchange", eventHandler);
                        entity.listName = PHSICAL_ADDRESS_CITY;
                        entity.listRow = i;
                        updateValuesList.Add(entity);
                    }
                    else if (header.Contains(REGISTRY_ADDRESS_LINE_ONE))
                    {
                        this.RAddrOne_onChange.Text = REGISTRY_ADDRESS_LINE_ONE;
                        string eventHandler = Page.ClientScript.GetPostBackEventReference(this.RAddrOne_onChange, "");
                        updatedValueTxtBox.Attributes.Add("onchange", eventHandler);
                        entity.listName = REGISTRY_ADDRESS_LINE_ONE;
                        entity.listRow = i;
                        updateValuesList.Add(entity);
                    }
                    else if (header.Contains(REGISTRY_ADDRESS_LINE_TWO))
                    {
                        this.RAddrTwo_onChange.Text = REGISTRY_ADDRESS_LINE_TWO;
                        string eventHandler = Page.ClientScript.GetPostBackEventReference(this.RAddrTwo_onChange, "");
                        updatedValueTxtBox.Attributes.Add("onchange", eventHandler);
                        entity.listName = REGISTRY_ADDRESS_LINE_TWO;
                        entity.listRow = i;
                        updateValuesList.Add(entity);
                    }
                    else if (header.Contains(REGISTRY_ADDRESS_CITY))
                    {
                        this.RCity_onChange.Text = REGISTRY_ADDRESS_CITY;
                        string eventHandler = Page.ClientScript.GetPostBackEventReference(this.RCity_onChange, "");
                        updatedValueTxtBox.Attributes.Add("onchange", eventHandler);
                        entity.listName = REGISTRY_ADDRESS_CITY;
                        entity.listRow = i;
                        updateValuesList.Add(entity);
                    }
                }


            }

            String updatedValueListStr = jsSer.Serialize(updateValuesList);
            UpdateValuesListHidden.Value = updatedValueListStr;
        }
        /// <summary>
        /// Save
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void SaveButton_Click(object sender, EventArgs e)
        {
            string eventId = EventID.Value;
            string chgRSjon = ChangeRSRevjson.Value;

            if (Page.IsValid && !String.IsNullOrEmpty(chgRSjon))
            {
                JavaScriptSerializer jsSer = new JavaScriptSerializer();
                ChangeResonSource crsObj = jsSer.Deserialize<ChangeResonSource>(chgRSjon);
                if (eventId.Contains(":"))
                {
                    bool ifValid = saveProtoGraph(eventId, crsObj);

                    if (ifValid)
                    {
                        Research2 topPage = this.Page as Research2;
                        if (null != topPage)
                        {
                            topPage.RefreshChangeReasonHistroy(long.Parse(this.GMFID.Value));
                        }
                        else
                        {
                            refreshTopPage(ACTION_SAVE_REASON);
                        }
                        revchgEvents.Style.Add("display", "none");
                        changeReasonRevPopup.Hide();
                        CrrUpdate.Update();
                    }
                    else
                    {
                        CrrUpdate.Update();
                        ChangeRSBefJson.Value = chgRSjon;
                    }
                }
                else
                {
                    log.Debug("Beginning transaction");
                    TransactionManager tm = new TransactionManager();
                    try
                    {
                        //JavaScriptSerializer jsSer = new JavaScriptSerializer();
                        //ChangeResonSource crsObj = jsSer.Deserialize<ChangeResonSource>(chgRSjon);

                        tm.BeginTransaction();

                        ChangeEventCollection ceCol = tm.ChangeEventCollection;
                        ChangeEvent oldCE = ceCol.GetByPrimaryKey(long.Parse(EventID.Value));
                        ChangeEvent newDefault = convertCE(oldCE, crsObj);

                        DataTable allEventsData = getAllChangeRecordsByEventId(long.Parse(EventID.Value));

                        bool ifValid = saveUpdateValues(newDefault, tm, allEventsData);

                        if (ifValid)
                        {
                            tm.CommitTransaction();
                            ChangeRSBefJson.Value = chgRSjon;
                            Research2 topPage = this.Page as Research2;
                            if (null != topPage)
                            {
                                topPage.RefreshChangeReasonHistroy(long.Parse(this.GMFID.Value));
                            }
                            else
                            {
                                refreshTopPage(ACTION_SAVE_REASON);
                            }

                            revchgEvents.Style.Add("display", "none");
                            changeReasonRevPopup.Hide();
                            CrrUpdate.Update();
                        }
                        else
                        {
                            CrrUpdate.Update();
                            tm.RollbackTransaction();
                            ChangeRSBefJson.Value = chgRSjon;
                        }
                        tm.Dispose();
                    }
                    catch (Exception)
                    {
                        tm.RollbackTransaction();
                        tm.Dispose();
                        throw;
                    }
                }
            }
        }

        private void saveAllChanges(List<DataRow> allEventsData, long newEventId, TransactionManager tm)
        {
            for (int i = 0; i < allEventsData.Count; i++)
            {
                DataRow eventRec = allEventsData[i];
                string subOrder = eventRec["subOrder"].ToString();
                long subId = long.Parse(eventRec["subId"].ToString());
                List<List<string>> input = getInputValues();

                if (COMPANY_ORDER_DATA.Equals(subOrder))
                {
                    saveComData(subId, newEventId, tm);
                }
                else if (COMPANY_ORDER_NAME.Equals(subOrder))
                {
                    saveComName(subId, newEventId, tm);
                }
                else if (COMPANY_ORDER_ADDR.Equals(subOrder))
                {
                    saveComAddr(subId, newEventId, tm);
                }
                else if (COMPANY_ORDER_CONT.Equals(subOrder))
                {
                    saveComContact(subId, newEventId, tm);
                }
                else if (COMPANY_ORDER_SVC.Equals(subOrder))
                {
                    saveComService(subId, newEventId, tm);
                }
                else if (COMPANY_ORDER_TYPE.Equals(subOrder))
                {
                    saveComType(subId, newEventId, tm);
                }
                else if (COMPANY_ORDER_UNQI.Equals(subOrder))
                {
                    saveComUnqi(subId, newEventId, tm);
                }
                else if (SECURITY_ORDER_DATA.Equals(subOrder))
                {
                    saveSecData(subId, newEventId, tm);
                }
                else if (SECURITY_ORDER_NAME.Equals(subOrder))
                {
                    saveSecName(subId, newEventId, tm);
                }
                else if (SECURITY_ORDER_UNQI.Equals(subOrder))
                {
                    saveSecUnqi(subId, newEventId, tm);
                }
                else if (SECURITY_ORDER_XCHG.Equals(subOrder))
                {
                    saveSecExchange(subId, newEventId, tm);
                }
                else if (RELATIONSHIP_ORDER_DATA.Equals(subOrder))
                {
                    saveRelData(subId, newEventId, tm);
                }
            }
        }

        #region Save all the changes

        private void saveComData(long key, long newEventId, TransactionManager tm)
        {
            CompanyDataChangeCollection dataCol = tm.CompanyDataChangeCollection;
            CompanyDataChange dataChg = dataCol.GetByPrimaryKey(key);
            if (null != dataChg)
            {
                dataChg.ChangeEventID = newEventId;
                dataCol.Update(dataChg);
            }
            dataCol.Dispose();
        }

        private void saveComName(long key, long newEventId, TransactionManager tm)
        {
            CompanyNameChangeCollection nameCol = tm.CompanyNameChangeCollection;
            CompanyNameChange nameChg = nameCol.GetByPrimaryKey(key);
            if (null != nameChg)
            {
                nameChg.ChangeEventID = newEventId;
                nameCol.Update(nameChg);
            }
            nameCol.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="input"></param>
        private void matchKeyInput(string key, int row)
        {
            List<int> list = new List<int>();

            if (inputMap.ContainsKey(key))
            {
                list = inputMap[key];
                inputMap.Remove(key);
                list.Add(row);
                inputMap.Add(key, list);
            }
            else
            {
                list.Add(row);
                inputMap.Add(key, list);
            }
        }

        private void saveComAddr(long key, long newEventId, TransactionManager tm)
        {
            CompanyAddressChangeCollection addrCol = tm.CompanyAddressChangeCollection;
            CompanyAddressChange addrChg = addrCol.GetByPrimaryKey(key);
            if (null != addrChg)
            {
                addrChg.ChangeEventID = newEventId;
                addrCol.Update(addrChg);
            }
            addrCol.Dispose();
        }

        private void saveComContact(long key, long newEventId, TransactionManager tm)
        {
            CompanyContactChangeCollection contCol = tm.CompanyContactChangeCollection;
            CompanyContactChange contChg = contCol.GetByPrimaryKey(key);
            if (null != contChg)
            {
                contChg.ChangeEventID = newEventId;
                contCol.Update(contChg);
            }
            contCol.Dispose();
        }

        private void saveComService(long key, long newEventId, TransactionManager tm)
        {
            CompanyServiceChangeCollection servCol = tm.CompanyServiceChangeCollection;
            CompanyServiceChange servChg = servCol.GetByPrimaryKey(key);
            if (null != servChg)
            {
                servChg.ChangeEventID = newEventId;
                servCol.Update(servChg);
            }
            servCol.Dispose();
        }

        private void saveComType(long key, long newEventId, TransactionManager tm)
        {
            CompanyTypeChangeCollection typeCol = tm.CompanyTypeChangeCollection;
            CompanyTypeChange typeChg = typeCol.GetByPrimaryKey(key);
            if (null != typeChg)
            {
                typeChg.ChangeEventID = newEventId;
                typeCol.Update(typeChg);
            }
            typeCol.Dispose();
        }

        private void saveComUnqi(long key, long newEventId, TransactionManager tm)
        {
            CompanyUniqueIDChangeCollection unqiCol = tm.CompanyUniqueIDChangeCollection;
            CompanyUniqueIDChange unqiChg = unqiCol.GetByPrimaryKey(key);
            if (null != unqiChg)
            {
                unqiChg.ChangeEventID = newEventId;
                unqiCol.Update(unqiChg);
            }
            unqiCol.Dispose();
        }

        private void saveSecData(long key, long newEventId, TransactionManager tm)
        {
            SecurityDataChangeCollection secDataCol = tm.SecurityDataChangeCollection;
            SecurityDataChange dataChg = secDataCol.GetByPrimaryKey(key);
            if (null != dataChg)
            {
                dataChg.ChangeEventID = newEventId;
                secDataCol.Update(dataChg);
            }
            secDataCol.Dispose();
        }

        private void saveSecName(long key, long newEventId, TransactionManager tm)
        {
            SecurityNameChangeCollection secNameCol = tm.SecurityNameChangeCollection;
            SecurityNameChange nameChg = secNameCol.GetByPrimaryKey(key);
            if (null != nameChg)
            {
                nameChg.ChangeEventID = newEventId;
                secNameCol.Update(nameChg);
            }
            secNameCol.Dispose();
        }

        private void saveSecUnqi(long key, long newEventId, TransactionManager tm)
        {
            SecurityUniqueIDChangeCollection secUnqiCol = tm.SecurityUniqueIDChangeCollection;
            SecurityUniqueIDChange unqiChg = secUnqiCol.GetByPrimaryKey(key);
            if (null != unqiChg)
            {
                unqiChg.ChangeEventID = newEventId;
                secUnqiCol.Update(unqiChg);
            }
            secUnqiCol.Dispose();
        }

        private void saveSecExchange(long key, long newEventId, TransactionManager tm)
        {
            SecurityExchangeChangeCollection secExchgCol = tm.SecurityExchangeChangeCollection;
            SecurityExchangeChange exchgChg = secExchgCol.GetByPrimaryKey(key);
            if (null != exchgChg)
            {
                exchgChg.ChangeEventID = newEventId;
                secExchgCol.Update(exchgChg);
            }
            secExchgCol.Dispose();
        }

        private void saveRelData(long key, long newEventId, TransactionManager tm)
        {
            RelationshipDataChangeCollection relCol = tm.RelationshipDataChangeCollection;
            RelationshipDataChange relChg = relCol.GetByPrimaryKey(key);
            if (null != relChg)
            {
                relChg.ChangeEventID = newEventId;
                relCol.Update(relChg);
            }
            relCol.Dispose();
        }

        private bool saveProtoGraph(string hoId, ChangeResonSource crsObj)
        {
            bool ifValid = false;
            long gmfid = Convert.ToInt64(hoId.Split(':')[0]);
            int index = Convert.ToInt32(hoId.Split(':')[1]);

            if (null != graphToAddCha && null != graphToAddCha.CreatedEntities && graphToAddCha.CreatedEntities.Count > 0)
            {
                proComs = graphToAddCha.CreatedEntities;
            }

            if (null != graphToAddCha && null != graphToAddCha.ModifiedRels && graphToAddCha.ModifiedRels.Count > 0)
            {
                proRels = graphToAddCha.ModifiedRels;
            }

            JavaScriptSerializer jsSer = new JavaScriptSerializer();
            Dictionary<string, UpdatedValuesEntity> proDataMap = jsSer.Deserialize<Dictionary<string, UpdatedValuesEntity>>(this.UpdatedValues.Value);
            Dictionary<string, int> proInputValuesMap = jsSer.Deserialize<Dictionary<string, int>>(this.InputValues.Value);

            if (null != proComs)
            {
                if (proComs.ContainsKey(gmfid))
                {
                    ProtoCompany company = proComs[gmfid];
                    if (null != company && null != company.History && company.History.Count > 0)
                    {
                        ifValid = saveProtoCom(company, index, proDataMap, proInputValuesMap, crsObj);
                    }
                }
            }

            if (null != proRels)
            {
                if (proRels.ContainsKey(gmfid))
                {
                    ProtoRelationship proRel = proRels[gmfid];
                    if (null != proRel && null != proRel.History && proRel.History.Count > 0)
                    {
                        ProtoRelationship currRel = proRel.History[index];
                        currRel.DefaultChangeReason = crsObj.changeReason;
                        currRel.DefaultChangeSource = crsObj.changeSourceTypes[0];
                        currRel.RelChReasonSource = crsObj;
                        ifValid = true;
                    }
                }
            }
            return ifValid;
        }

        private bool saveProtoCom(ProtoCompany proCom, int index, Dictionary<string, UpdatedValuesEntity> proDataMap, Dictionary<string, int> proInputValuesMap, ChangeResonSource crsObj)
        {
            bool ifChanged = false;
            bool naicsChanged = false;
            JavaScriptSerializer jsSer = new JavaScriptSerializer();
            Dictionary<string, bool> dynamicNaicssubMap = jsSer.Deserialize<Dictionary<string, bool>>(this.DynamicNaicssub.Value);
            foreach (string key in proDataMap.Keys)
            {
                UpdatedValuesEntity update = proDataMap[key];
                int inputIndex = proInputValuesMap[key];
                List<List<string>> allInputLst = getInputValues();
                string inputValue = allInputLst[inputIndex][0];

                if (inputValue.Equals(update.beforeValue) || ((null != update.bValue) && inputValue.Equals(update.bValue)))
                {
                    if (ChangeReasonEditor.LEGAL_STRUCTURE.Equals(update.item) && ("EmptyList".Equals(getSubListDataForCountry("", ChangeReasonEditor.LEGAL_STRUCTURE)) || null == getSubListDataForCountry("", ChangeReasonEditor.LEGAL_STRUCTURE)))
                    {
                        ifChanged = true;
                    }
                    else
                    {
                        if (ChangeReasonEditor.IND_NAICS_SUB.Equals(update.item))
                        {
                            if (dynamicNaicssubMap["isNaicssubVisible"])
                            {
                                ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('" + update.item + " change could not be made due to new value equals old value.');", true);
                                return false;
                            }
                        }
                        else if (ChangeReasonEditor.ENTITY_TYPE.Equals(update.item))
                        {
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('" + update.item + " change could not be made due to new value equals old value.');", true);
                            return false;
                        }
                        else if (!naicsChanged)
                        {
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('" + update.item + " change could not be made due to new value equals old value.');", true);
                            return false;
                        }
                        else
                        {
                            ifChanged = true;
                        }
                    }
                }
                else if (!inputValue.Equals(update.updatedValue))
                {
                    ifChanged = true;
                    if (ChangeReasonEditor.IND_NAICS.Equals(update.item))
                    {
                        naicsChanged = true;
                    }
                }
            }

            if (ifChanged)
            {
                #region check the edit subsequence
                foreach (string key in proDataMap.Keys)
                {
                    UpdatedValuesEntity update = proDataMap[key];
                    int inputIndex = proInputValuesMap[key];
                    List<List<string>> allInputLst = getInputValues();
                    string inputValue = allInputLst[inputIndex][0];
                    string inputCode = string.Empty;
                    bool hasSubsequent = false;
                    if (allInputLst[inputIndex].Count > 1)
                    {
                        inputCode = allInputLst[inputIndex][1];
                    }

                    if (ChangeReasonEditor.IS_EXPIRED.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.IsExpired, proCom.History[index].IsExpired))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].IsExpired, proCom.History[index].IsExpired))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.FYE.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.FiscalYearEnd, proCom.History[index].FiscalYearEnd))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].FiscalYearEnd, proCom.History[index].FiscalYearEnd))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.LEGAL_NAME.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.LegalName, proCom.History[index].LegalName))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].LegalName, proCom.History[index].LegalName))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.ALIAS_NAME.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.Alias, proCom.History[index].Alias))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].Alias, proCom.History[index].Alias))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.PA_LINE_ONE.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.AddressLine1, proCom.History[index].AddressLine1))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].AddressLine1, proCom.History[index].AddressLine1))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.PA_LINE_TWO.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.AddressLine2, proCom.History[index].AddressLine2))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].AddressLine2, proCom.History[index].AddressLine2))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.PA_CITY.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.CityName, proCom.History[index].CityName))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].CityName, proCom.History[index].CityName))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.PA_STATE.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.StateProvinceDesc, proCom.History[index].StateProvinceDesc))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].StateProvinceDesc, proCom.History[index].StateProvinceDesc))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.RA_COUNTRY.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.CountryDesc, proCom.History[index].CountryDesc))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].CountryDesc, proCom.History[index].CountryDesc))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.PA_POSTAL_CODE.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.PostalCode, proCom.History[index].PostalCode))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].PostalCode, proCom.History[index].PostalCode))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.RA_LINE_ONE.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.RegAddressLine1, proCom.History[index].RegAddressLine1))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].RegAddressLine1, proCom.History[index].RegAddressLine1))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.RA_LINE_TWO.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.RegAddressLine2, proCom.History[index].RegAddressLine2))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].RegAddressLine2, proCom.History[index].RegAddressLine2))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.RA_CITY.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.RegCityName, proCom.History[index].RegCityName))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].RegCityName, proCom.History[index].RegCityName))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.RA_STATE.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.RegStateProvinceDesc, proCom.History[index].RegStateProvinceDesc))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].RegStateProvinceDesc, proCom.History[index].RegStateProvinceDesc))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.RA_COUNTRY.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.RegCountryDesc, proCom.History[index].RegCountryDesc))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].RegCountryDesc, proCom.History[index].RegCountryDesc))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.RA_POSTAL_CODE.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.RegPostalCode, proCom.History[index].RegPostalCode))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].RegPostalCode, proCom.History[index].RegPostalCode))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.IND_NAICS.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.NAICS, proCom.History[index].NAICS))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].NAICS, proCom.History[index].NAICS))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.IND_SIC1.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.SIC, proCom.History[index].SIC))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].SIC, proCom.History[index].SIC))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.IND_NACE.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.NACE, proCom.History[index].NACE))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].NACE, proCom.History[index].NACE))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.IND_NAICS_SUB.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.NAICSSub, proCom.History[index].NAICSSub))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].NAICSSub, proCom.History[index].NAICSSub))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.IND_GICS.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.GICS, proCom.History[index].GICS))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].GICS, proCom.History[index].GICS))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.LEGAL_STRUCTURE.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.LegalStructure, proCom.History[index].LegalStructure))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].LegalStructure, proCom.History[index].LegalStructure))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if ((ChangeReasonEditor.IDEN1_VALUE_SUB + "Type").Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.Identifier1Type, proCom.History[index].Identifier1Type))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].Identifier1Type, proCom.History[index].Identifier1Type))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if ((ChangeReasonEditor.IDEN2_VALUE_SUB + "Type").Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.Identifier2Type, proCom.History[index].Identifier2Type))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].Identifier2Type, proCom.History[index].Identifier2Type))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (!(ChangeReasonEditor.IDEN1_VALUE_SUB + "Type").Equals(update.item) && update.item.Contains(ChangeReasonEditor.IDEN1_VALUE_SUB))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.Identifier1Value, proCom.History[index].Identifier1Value))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].Identifier1Value, proCom.History[index].Identifier1Value))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (!(ChangeReasonEditor.IDEN2_VALUE_SUB + "Type").Equals(update.item) && update.item.Contains(ChangeReasonEditor.IDEN2_VALUE_SUB))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.Identifier2Value, proCom.History[index].Identifier2Value))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].Identifier2Value, proCom.History[index].Identifier2Value))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ChangeReasonEditor.ENTITY_TYPE.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            if (checkChange(proCom.CompanyType, proCom.History[index].CompanyType))
                            {
                                hasSubsequent = true;
                            }
                            else
                            {
                                for (int i = index + 1; i < proCom.History.Count; i++)
                                {
                                    if (!checkChange(proCom.History[i].CompanyType, proCom.History[index].CompanyType))
                                    {
                                        hasSubsequent = true;
                                    }
                                }
                            }
                        }
                    }

                    if (true == hasSubsequent)
                    {
                        ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be saved due to subsequent change.');", true);
                        return false;
                    }
                }
                #endregion

                foreach (string key in proDataMap.Keys)
                {
                    UpdatedValuesEntity update = proDataMap[key];
                    int inputIndex = proInputValuesMap[key];
                    List<List<string>> allInputLst = getInputValues();
                    string inputValue = allInputLst[inputIndex][0];
                    string inputCode = string.Empty;
                    if (allInputLst[inputIndex].Count > 1)
                    {
                        inputCode = allInputLst[inputIndex][1];
                    }
                    if (ChangeReasonEditor.IS_EXPIRED.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].IsExpired = inputValue;
                                proCom.IsExpired = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.FYE.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].FiscalYearEnd = inputValue;
                                proCom.FiscalYearEnd = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.LEGAL_NAME.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].LegalName = inputValue;
                                proCom.LegalName = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.ALIAS_NAME.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].Alias = inputValue;
                                proCom.Alias = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.PA_LINE_ONE.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].AddressLine1 = inputValue;
                                proCom.AddressLine1 = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.PA_LINE_TWO.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].AddressLine2 = inputValue;
                                proCom.AddressLine2 = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.PA_CITY.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].CityName = inputValue;
                                proCom.CityName = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.PA_STATE.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].StateProvinceName = inputCode;
                                proCom.History[i].StateProvinceDesc = inputValue;
                                proCom.StateProvinceName = inputCode;
                                proCom.StateProvinceDesc = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.PA_COUNTRY.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].CountryOfIncorporation = inputCode;
                                proCom.History[i].CountryDesc = inputValue;
                                proCom.CountryOfIncorporation = inputCode;
                                proCom.CountryDesc = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.PA_POSTAL_CODE.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].PostalCode = inputValue;
                                proCom.PostalCode = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.RA_LINE_ONE.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].RegAddressLine1 = inputValue;
                                proCom.RegAddressLine1 = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.RA_LINE_TWO.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].RegAddressLine2 = inputValue;
                                proCom.RegAddressLine2 = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.RA_CITY.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].RegCityName = inputValue;
                                proCom.RegCityName = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.RA_STATE.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].RegStateProvinceName = inputCode;
                                proCom.History[i].RegStateProvinceDesc = inputValue;
                                proCom.RegStateProvinceName = inputCode;
                                proCom.RegStateProvinceDesc = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.RA_COUNTRY.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].RegCountry = inputCode;
                                proCom.History[i].RegCountryDesc = inputValue;
                                proCom.RegCountry = inputCode;
                                proCom.RegCountryDesc = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.RA_POSTAL_CODE.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].RegPostalCode = inputValue;
                                proCom.RegPostalCode = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.IND_NAICS.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].NAICS = inputValue;
                                proCom.NAICS = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.IND_SIC1.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].SIC = inputValue;
                                proCom.SIC = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.IND_NACE.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].NACE = inputValue;
                                proCom.NACE = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.IND_NAICS_SUB.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].NAICSSub = inputValue;
                                proCom.NAICSSub = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.IND_GICS.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].GICS = inputValue;
                                proCom.GICS = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.LEGAL_STRUCTURE.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].LegalStructure = inputValue;
                                proCom.LegalStructure = inputValue;
                            }
                        }
                    }
                    else if ((ChangeReasonEditor.IDEN1_VALUE_SUB + "Type").Equals(update.item))
                    {
                        if (!inputValue.Equals(update.value))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].Identifier1Type = inputValue;
                                proCom.Identifier1Type = inputValue;
                                proCom.History[i].Identifier1TypeDesc = inputCode;
                                proCom.Identifier1TypeDesc = inputCode;
                            }
                        }
                    }
                    else if ((ChangeReasonEditor.IDEN2_VALUE_SUB + "Type").Equals(update.item))
                    {
                        if (!inputValue.Equals(update.value))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].Identifier2Type = inputValue;
                                proCom.Identifier2Type = inputValue;
                                proCom.History[i].Identifier2TypeDesc = inputCode;
                                proCom.Identifier2TypeDesc = inputCode;
                            }
                        }
                    }
                    else if (!(ChangeReasonEditor.IDEN1_VALUE_SUB + "Type").Equals(update.item) && update.item.Contains(ChangeReasonEditor.IDEN1_VALUE_SUB))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].Identifier1Value = inputValue;
                                proCom.Identifier1Value = inputValue;
                            }
                        }
                    }
                    else if (!(ChangeReasonEditor.IDEN2_VALUE_SUB + "Type").Equals(update.item) && update.item.Contains(ChangeReasonEditor.IDEN2_VALUE_SUB))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].Identifier2Value = inputValue;
                                proCom.Identifier2Value = inputValue;
                            }
                        }
                    }
                    else if (ChangeReasonEditor.ENTITY_TYPE.Equals(update.item))
                    {
                        if (!inputValue.Equals(update.updatedValue))
                        {
                            for (int i = index; i < proCom.History.Count; i++)
                            {
                                proCom.History[i].CompanyType = inputValue;
                                proCom.CompanyType = inputValue;
                            }
                        }
                    }
                }
            }
            if (ifChangeReasonChanged(ChangeRSRevjson, ChangeRSBefJson))
            {
                proCom.History[index].DefaultChangeReason = crsObj.changeReason;
                proCom.History[index].DefaultChangeSource = crsObj.changeSourceTypes[0];
                proCom.History[index].ComChReasonSource = crsObj;
            }
            else
            {
                if (!ifChanged)
                {
                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "debugger;alert('There is not any change!');", true);
                    return false;
                }
            }
            return true;
        }

        #endregion

        private ChangeEvent convertCE(ChangeEvent oldChangeEvent, ChangeResonSource cs)
        {
            ChangeEvent ce = new ChangeEvent();
            ce.GMFID = oldChangeEvent.GMFID;
            ce.FeedID = oldChangeEvent.FeedID;
            ce.Reason = cs.changeReason;
            ce.Source = cs.changeSourceTypes[0];
            ce.ChangeUserID = userId;
            ce.WorkQId = oldChangeEvent.WorkQId;
            ce.CurrentStepID = oldChangeEvent.CurrentStepID;
            ce.ChangeDate = oldChangeEvent.ChangeDate;
            if (String.IsNullOrEmpty(userId))
            {
                User currentUser = (User)Session["CurrentUser"];
                ce.ChangeUserID = currentUser.Signon;
            }
            ce.Comments = cs.changeComments;
            return ce;
        }

        /// <summary>
        /// Button Revert click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void RevertButton_OnClickHandler(object sender, EventArgs e)
        {

            TransactionManager tm = new TransactionManager();
            tm.BeginTransaction();
            log.Debug("RevertButton_OnClickHandler entry.");

            //Revert master record.
            bool ifValid = Revert(tm);

            if (ifValid)
            {
                tm.CommitTransaction();
                //refresh frame.
                Research2 topPage = this.Page as Research2;
                if (null != topPage)
                {
                    if (removeNode == 0)
                    {
                        removeNode = long.Parse(this.GMFID.Value);
                    }
                    topPage.RefreshChangeReasonHistroy(removeNode);
                }
                else
                {
                    refreshTopPage(ACTION_REVERT);
                }
            }
            else
            {
                tm.RollbackTransaction();
            }
            tm.Dispose();
            changeReasonRevPopup.Hide();

            CrrUpdate.Update();
        }

        public const string ACTION_SAVE_REASON = "saveReason";
        public const string ACTION_REVERT = "revert";

        public delegate void RefreshHistoryEventHandler(object sender, CommandEventArgs e);

        /// <summary>
        /// Event that is fired finish this page.
        /// </summary>
        public event RefreshHistoryEventHandler RefreshHistory;
        private void refreshTopPage(String action)
        {
            if (null != RefreshHistory)
            {
                RefreshHistory(this, new CommandEventArgs(action, null));
            }
        }

        protected void revchgEvents_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            JavaScriptSerializer jsSer = new JavaScriptSerializer();
            Dictionary<string, bool> dynamicNaicssubMap = jsSer.Deserialize<Dictionary<string, bool>>(this.DynamicNaicssub.Value);
            if (e.Item.ItemType == ListItemType.Header)
            {
            }
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                if (!isOldAndNewValueEmpty(e.Item.Cells[1].Text, e.Item.Cells[2].Text))
                {
                    if (string.IsNullOrEmpty(e.Item.Cells[1].Text) || "&nbsp;".Equals(e.Item.Cells[1].Text))
                    {
                        e.Item.Cells[2].Style.Add("color", "Green");
                    }
                    if (string.IsNullOrEmpty(e.Item.Cells[2].Text) || "&nbsp;".Equals(e.Item.Cells[2].Text))
                    {
                        e.Item.Cells[1].Style.Add("color", "Red");
                    }
                }
            }
            if (e.Item.Cells[0].Text.Equals("Industry Type - NAICSSUB"))
            {
                if (e.Item.Cells[1].Text.Equals(e.Item.Cells[2].Text))
                {
                    e.Item.Visible = false;
                    dynamicNaicssubMap["isNaicssubInChange"] = false;
                }
                else
                {
                    dynamicNaicssubMap["isNaicssubInChange"] = true;
                }
                DynamicNaicssub.Value = jsSer.Serialize(dynamicNaicssubMap);
            }
        }

        /// <summary>
        /// Logic of revert
        /// </summary>
        /// <param name="changeEventId"></param>
        /// <param name="companyGMFID"></param>
        /// <param name="feedID"></param>
        /// <param name="tm"></param>
        /// <param name="ce"></param>
        private bool Revert(TransactionManager tm)
        {

            bool ifValid = false;

            if (EventID.Value.Contains(":"))
            {
                ifValid = RevertProtoCom();
                return ifValid;
            }

            int feedId = 2;
            long changeEventId = long.Parse(EventID.Value);
            long companyGMFID = long.Parse(GMFID.Value);
            ChangeEventCollection cec = tm.ChangeEventCollection;
            ChangeEvent ce = cec.GetByPrimaryKey(changeEventId);

            if (SubOrder.Value.StartsWith("C"))
            {
                ifValid = RevertCompany(changeEventId, companyGMFID, feedId, tm, ce);
            }
            else if (SubOrder.Value.StartsWith("S"))
            {
                ifValid = RevertSecurity(changeEventId, companyGMFID, feedId, tm, ce);
            }
            else if (SubOrder.Value.StartsWith("R"))
            {
                ifValid = RevertRelationship(changeEventId, companyGMFID, feedId, tm, ce);
            }

            return ifValid;
        }

        /// <summary>
        /// Sort by ID
        /// </summary>
        class SortDataChange : IComparer
        {
            int IComparer.Compare(object a, object b)
            {
                return (((CompanyDataChange)a).ID.CompareTo(((CompanyDataChange)b).ID));
            }
        }

        /// <summary>
        /// Sort by ID
        /// </summary>
        class SortSecDataChange : IComparer
        {
            int IComparer.Compare(object a, object b)
            {
                return (((SecurityDataChange)a).ID.CompareTo(((SecurityDataChange)b).ID));
            }
        }

        /// <summary>
        /// Sort by ID
        /// </summary>
        class SortRelDataChange : IComparer
        {
            int IComparer.Compare(object a, object b)
            {
                return (((RelationshipDataChange)a).ID.CompareTo(((RelationshipDataChange)b).ID));
            }
        }

        /// <summary>
        /// Revert Relationship
        /// </summary>
        /// <param name="changeEventId"></param>
        /// <param name="relGMFID"></param>
        /// <param name="feedID"></param>
        /// <param name="tm"></param>
        /// <param name="ce"></param>
        private bool RevertRelationship(long changeEventId, long relGMFID, int feedID, TransactionManager tm, ChangeEvent ce)
        {
            RelationshipDataCollection cac = tm.RelationshipDataCollection;
            RelationshipData updateData = cac.GetByPrimaryKey(relGMFID, feedID);

            RelationshipDataChangeCollection relChange = tm.RelationshipDataChangeCollection;
            relChange.GetByChangeEventID(changeEventId);
            ArrayList datachangeList = relChange.RelationshipDataChangeList;

            if (null != datachangeList && datachangeList.Count != 0)
            {
                SortRelDataChange sort = new SortRelDataChange();
                datachangeList.Sort(sort);

                RelationshipDataChange data = (RelationshipDataChange)datachangeList[0];

                if (null != updateData && ValidateSubsequentChangeForComData("Rel", relGMFID, feedID, changeEventId, data.ID, tm) > 0)
                {
                    cac.Dispose();
                    relChange.Dispose();
                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                    return false;
                }
                else
                {

                    if (data.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_DELETE))
                    {
                        updateData = new RelationshipData();
                        updateData.FeedID = 2;
                        updateData.RelationshipGMFID = relGMFID;
                        if (!data.IsChildGMFIDBeforeNull)
                        {
                            updateData.ChildGMFID = data.ChildGMFIDBefore;
                        }
                        if (!data.IsDUPGMFIDBeforeNull)
                        {
                            updateData.DUPGMFID = data.DUPGMFIDBefore;
                        }
                        else
                        {
                            updateData.IsDUPGMFIDNull = true;
                        }
                        if (!data.IsExpiredBeforeNull)
                        {
                            updateData.Expired = data.ExpiredBefore;
                        }
                        if (!data.IsExpiredDateBeforeNull)
                        {
                            updateData.ExpiredDate = data.ExpiredDateBefore;
                        }
                        else
                        {
                            updateData.IsExpiredDateNull = true;
                        }
                        updateData.ExpiredReason = data.ExpiredReasonBefore;

                        if (!data.IsGUPGMFIDBeforeNull)
                        {
                            updateData.GUPGMFID = data.GUPGMFIDBefore;
                        }
                        else
                        {
                            updateData.IsGUPGMFIDNull = true;
                        }
                        if (!data.IsInactiveBeforeNull)
                        {
                            updateData.Inactive = data.InactiveBefore;
                        }
                        else
                        {
                            updateData.IsInactiveNull = true;
                        }
                        if (!data.IsInactiveDateBeforeNull)
                        {
                            updateData.InactiveDate = data.InactiveDateBefore;
                        }
                        else
                        {
                            updateData.IsInactiveDateNull = true;
                        }
                        updateData.InactiveReason = data.InactiveReasonBefore;

                        if (!data.IsParentGMFIDBeforeNull)
                        {
                            updateData.ParentGMFID = data.ParentGMFIDBefore;
                        }
                        if (!data.IsPercentBeforeNull)
                        {
                            updateData.Percent = data.PercentBefore;
                        }
                        else
                        {
                            updateData.IsPercentNull = true;
                        }

                        updateData.SubType = data.SubTypeBefore;
                        cac.Insert(updateData, ce, tm);

                        // updateAllChangeRecordsForOID(updateData.RelationshipGMFID, tm, "RelationshipDataChange");

                    }
                    else if (data.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_UPDATE))
                    {
                        if (updateData == null)
                        {

                            cac.Dispose();
                            relChange.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                        if (!data.IsChildGMFIDBeforeNull)
                        {
                            updateData.ChildGMFID = data.ChildGMFIDBefore;
                        }
                        if (!data.IsDUPGMFIDBeforeNull)
                        {
                            updateData.DUPGMFID = data.DUPGMFIDBefore;
                        }
                        else
                        {
                            updateData.IsDUPGMFIDNull = true;
                        }
                        if (!data.IsExpiredBeforeNull)
                        {
                            updateData.Expired = data.ExpiredBefore;
                        }
                        if (!data.IsExpiredDateBeforeNull)
                        {
                            updateData.ExpiredDate = data.ExpiredDateBefore;
                        }
                        else
                        {
                            updateData.IsExpiredDateNull = true;
                        }
                        updateData.ExpiredReason = data.ExpiredReasonBefore;

                        if (!data.IsGUPGMFIDBeforeNull)
                        {
                            updateData.GUPGMFID = data.GUPGMFIDBefore;
                        }
                        else
                        {
                            updateData.IsGUPGMFIDNull = true;
                        }
                        if (!data.IsInactiveBeforeNull)
                        {
                            updateData.Inactive = data.InactiveBefore;
                        }
                        else
                        {
                            updateData.IsInactiveNull = true;
                        }
                        if (!data.IsInactiveDateBeforeNull)
                        {
                            updateData.InactiveDate = data.InactiveDateBefore;
                        }
                        else
                        {
                            updateData.IsInactiveDateNull = true;
                        }
                        updateData.InactiveReason = data.InactiveReasonBefore;

                        if (!data.IsParentGMFIDBeforeNull)
                        {
                            updateData.ParentGMFID = data.ParentGMFIDBefore;
                        }
                        if (!data.IsPercentBeforeNull)
                        {
                            updateData.Percent = data.PercentBefore;
                        }
                        else
                        {
                            updateData.IsPercentNull = true;
                        }

                        updateData.SubType = data.SubTypeBefore;

                        cac.Update(updateData, ce, tm);
                    }
                    else
                    {
                        if (null != updateData)
                        {
                            cac.Delete(updateData, ce, tm);
                        }
                        else
                        {
                            cac.Dispose();
                            relChange.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                    }
                }
                cac.Dispose();
            }
            relChange.Dispose();

            InactivateChangeRecord(ce, tm);

            return true;
        }

        /// <summary>
        /// Revert Security
        /// </summary>
        /// <param name="changeEventId"></param>
        /// <param name="securityGMFID"></param>
        /// <param name="feedID"></param>
        /// <param name="tm"></param>
        /// <param name="ce"></param>
        private bool RevertSecurity(long changeEventId, long securityGMFID, int feedID, TransactionManager tm, ChangeEvent ce)
        {
            SecurityExchangeChangeCollection exchange = tm.SecurityExchangeChangeCollection;
            exchange.GetByChangeEventID(changeEventId);
            ArrayList exchangeList = exchange.SecurityExchangeChangeList;
            for (int i = 0; i < exchangeList.Count; i++)
            {
                SecurityExchangeChange data = (SecurityExchangeChange)exchangeList[i];

                SecurityExchangeCollection cac = tm.SecurityExchangeCollection;
                SecurityExchange exdata = cac.GetByPrimaryKey(data.SecurityExchangeID);

                if (null != exdata && ValidateSubsequentChange("tChgSXchg", "hsxOID", "hsxEventID", "hsxID", exdata.OID, changeEventId, data.ID, tm) > 0)
                {
                    exchange.Dispose();
                    cac.Dispose();
                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                    return false;
                }
                else
                {

                    if (data.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_DELETE))
                    {
                        if (ValidateSubsequentChangeForDel("tChgSXchg", "hsxgmfid", "hsxfid", "hsxEventID", "hsxID", securityGMFID, changeEventId, data.ID, tm) > 0)
                        {
                            exchange.Dispose();
                            cac.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                        exdata = new SecurityExchange();
                        exdata.FeedID = 2;
                        exdata.SecurityGMFID = securityGMFID;
                        exdata.TradeSymbol = data.TradeSymbolBefore;
                        exdata.Type = data.TypeBefore;
                        exdata.Exchange = data.ExchangeBefore;

                        if (!data.IsExchangeExpiredBeforeNull)
                        {
                            exdata.ExchangeExpired = data.ExchangeExpiredBefore;
                        }
                        else
                        {
                            exdata.IsExchangeExpireDateNull = true;
                        }

                        if (!data.IsExchangeExpireDateBeforeNull)
                        {
                            exdata.ExchangeExpireDate = data.ExchangeExpireDateBefore;
                        }
                        else
                        {
                            exdata.IsExchangeExpireDateNull = true;
                        }
                        cac.Insert(exdata, ce, tm);

                        updateAllChangeRecordsForOID(exdata.OID, data.SecurityExchangeID, tm, "SecurityExchangeChange");

                    }
                    else if (data.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_UPDATE))
                    {
                        if (null == exdata)
                        {

                            exchange.Dispose();
                            cac.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }

                        exdata.TradeSymbol = data.TradeSymbolBefore;
                        exdata.Type = data.TypeBefore;
                        exdata.Exchange = data.ExchangeBefore;

                        if (!data.IsExchangeExpiredBeforeNull)
                        {
                            exdata.ExchangeExpired = data.ExchangeExpiredBefore;
                        }
                        else
                        {
                            exdata.IsExchangeExpireDateNull = true;
                        }

                        if (!data.IsExchangeExpireDateBeforeNull)
                        {
                            exdata.ExchangeExpireDate = data.ExchangeExpireDateBefore;
                        }
                        else
                        {
                            exdata.IsExchangeExpireDateNull = true;
                        }

                        cac.Update(exdata, ce, tm);
                    }
                    else
                    {
                        if (null != exdata)
                        {
                            cac.Delete(exdata, ce, tm);
                        }
                        else
                        {

                            exchange.Dispose();
                            cac.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                    }
                }
                cac.Dispose();
            }
            exchange.Dispose();


            SecurityNameChangeCollection nameChange = tm.SecurityNameChangeCollection;
            nameChange.GetByChangeEventID(changeEventId);
            ArrayList nameList = nameChange.SecurityNameChangeList;
            for (int i = 0; i < nameList.Count; i++)
            {
                SecurityNameChange data = (SecurityNameChange)nameList[i];

                SecurityNameCollection cac = tm.SecurityNameCollection;
                SecurityName updateData = cac.GetByPrimaryKey(data.SecurityNameID);

                if (null != updateData && ValidateSubsequentChange("tChgSName", "hsnOID", "hsnEventID", "hsnID", updateData.OID, changeEventId, data.ID, tm) > 0)
                {
                    nameChange.Dispose();
                    cac.Dispose();
                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                    return false;
                }
                else
                {

                    if (data.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_DELETE))
                    {
                        if (ValidateSubsequentChangeForDel("tChgSName", "hsngmfid", "hsnfid", "hsnEventID", "hsnID", securityGMFID, changeEventId, data.ID, tm) > 0)
                        {
                            nameChange.Dispose();
                            cac.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                        updateData = new SecurityName();
                        updateData.FeedID = 2;
                        updateData.SecurityGMFID = securityGMFID;
                        updateData.Name = data.NameBefore;
                        updateData.Type = data.TypeBefore;
                        cac.Insert(updateData, ce, tm);

                        updateAllChangeRecordsForOID(updateData.OID, data.SecurityNameID, tm, "SecurityNameChange");

                    }
                    else if (data.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_UPDATE))
                    {
                        if (updateData == null)
                        {

                            nameChange.Dispose();
                            cac.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                        updateData.Name = data.NameBefore;
                        updateData.Type = data.TypeBefore;

                        cac.Update(updateData, ce, tm);
                    }
                    else
                    {
                        if (null != updateData)
                        {
                            cac.Delete(updateData, ce, tm);
                        }
                        else
                        {

                            nameChange.Dispose();
                            cac.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                    }
                }
                cac.Dispose();
            }
            nameChange.Dispose();

            SecurityUniqueIDChangeCollection uniqueIDChange = tm.SecurityUniqueIDChangeCollection;
            uniqueIDChange.GetByChangeEventID(changeEventId);
            ArrayList uniqueIDList = uniqueIDChange.SecurityUniqueIDChangeList;
            for (int i = 0; i < uniqueIDList.Count; i++)
            {
                SecurityUniqueIDChange data = (SecurityUniqueIDChange)uniqueIDList[i];

                SecurityUniqueIDCollection cac = tm.SecurityUniqueIDCollection;
                SecurityUniqueID updateData = cac.GetByPrimaryKey(data.SecurityUniqueID);

                if (null != updateData && ValidateSubsequentChange("tChgSUnqIds", "hsiOID", "hsiEventID", "hsiID", updateData.OID, changeEventId, data.ID, tm) > 0)
                {
                    uniqueIDChange.Dispose();
                    cac.Dispose();
                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                    return false;
                }
                else
                {

                    if (data.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_DELETE))
                    {
                        if (ValidateSubsequentChangeForDel("tChgSUnqIds", "hsigmfid", "hsifid", "hsiEventID", "hsiID", securityGMFID, changeEventId, data.ID, tm) > 0)
                        {
                            uniqueIDChange.Dispose();
                            cac.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                        updateData = new SecurityUniqueID();
                        updateData.FeedID = 2;
                        updateData.SecurityGMFID = securityGMFID;
                        updateData.Code = data.CodeBefore;
                        updateData.Type = data.TypeBefore;
                        cac.Insert(updateData, ce, tm);

                        updateAllChangeRecordsForOID(updateData.OID, data.SecurityUniqueID, tm, "SecurityUniqueIDChange");

                    }
                    else if (data.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_UPDATE))
                    {
                        if (updateData == null)
                        {
                            uniqueIDChange.Dispose();
                            cac.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                        updateData.Code = data.CodeBefore;
                        updateData.Type = data.TypeBefore;
                        cac.Update(updateData, ce, tm);
                    }
                    else
                    {
                        if (null != updateData)
                        {
                            cac.Delete(updateData, ce, tm);
                        }
                        else
                        {
                            uniqueIDChange.Dispose();
                            cac.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                    }
                }
                cac.Dispose();
            }
            uniqueIDChange.Dispose();


            SecurityDataCollection cdc = tm.SecurityDataCollection;
            SecurityData comdata = cdc.GetByPrimaryKey(securityGMFID, feedID);

            SecurityDataChangeCollection datachange = tm.SecurityDataChangeCollection;
            datachange.GetByChangeEventID(changeEventId);
            ArrayList datachangeList = datachange.SecurityDataChangeList;

            if (null != datachangeList && datachangeList.Count != 0)
            {
                SortSecDataChange sort = new SortSecDataChange();
                datachangeList.Sort(sort);

                SecurityDataChange cdata = (SecurityDataChange)datachangeList[0];

                if (null != comdata && ValidateSubsequentChangeForComData("Sec", securityGMFID, feedID, changeEventId, cdata.ID, tm) > 0)
                {
                    datachange.Dispose();
                    cdc.Dispose();
                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                    return false;
                }
                else
                {
                    if (cdata.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_INSERT))
                    {
                        InactivateSecDataSubsChangeEvents(securityGMFID, feedID, tm);
                        if (null != comdata)
                        {
                            cdc.Delete(comdata, ce, tm);
                        }
                        else
                        {
                            datachange.Dispose();
                            cdc.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                    }
                    else if (cdata.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_DELETE))
                    {
                        comdata = new SecurityData();

                        comdata.FeedID = 2;
                        comdata.SecurityGMFID = securityGMFID;

                        comdata.ADRLevel = cdata.ADRLevelBefore;
                        comdata.CFI = cdata.CFIBefore;
                        comdata.Class = cdata.ClassBefore;
                        comdata.CorporateAction = cdata.CorporateActionBefore;
                        comdata.CountryOfIncorporation = cdata.CountryOfIncorporationBefore;
                        comdata.Currency = cdata.CurrencyBefore;
                        comdata.Description1 = cdata.Description1Before;
                        comdata.Description2 = cdata.Description2Before;
                        comdata.Description3 = cdata.Description3Before;
                        comdata.ExpiredReason = cdata.ExpiredReasonBefore;
                        comdata.FundAdvisor = cdata.FundAdvisorBefore;
                        comdata.FundFamily = cdata.FundFamilyBefore;
                        comdata.FundFYE = cdata.FundFYEBefore;
                        comdata.FundLoad = cdata.FundLoadBefore;
                        comdata.FundManager = cdata.FundManagerBefore;
                        comdata.GMFStatus = cdata.GMFStatusBefore;
                        comdata.InterestRate = cdata.InterestRateBefore;
                        comdata.ISINCountry = cdata.ISINCountryBefore;
                        comdata.Issuer1 = cdata.Issuer1Before;
                        comdata.Issuer2 = cdata.Issuer2Before;
                        comdata.Issuer3 = cdata.Issuer3Before;
                        comdata.ObligorName = cdata.ObligorNameBefore;
                        comdata.PrimaryExchange = cdata.PrimaryExchangeBefore;
                        comdata.ProjectName = cdata.ProjectNameBefore;
                        comdata.SeriesName = cdata.SeriesNameBefore;
                        comdata.Type = cdata.TypeBefore;

                        if (!cdata.IsDefaultValueBeforeNull)
                        {
                            comdata.DefaultValue = cdata.DefaultValueBefore;
                        }
                        else
                        {
                            comdata.IsDefaultValueNull = true;
                        }
                        if (!cdata.IsExpiredBeforeNull)
                        {
                            comdata.Expired = cdata.ExpiredBefore;
                        }
                        else
                        {
                            comdata.IsExpiredNull = true;
                        }
                        if (!cdata.IsExpiredDateBeforeNull)
                        {
                            comdata.ExpiredDate = cdata.ExpiredDateBefore;
                        }
                        else
                        {
                            comdata.IsExpiredDateNull = true;
                        }
                        if (!cdata.IsInstitutionalBeforeNull)
                        {
                            comdata.Institutional = cdata.InstitutionalBefore;
                        }
                        else
                        {
                            comdata.Institutional = true;
                        }
                        if (!cdata.IsIssueDateBeforeNull)
                        {
                            comdata.IssueDate = cdata.IssueDateBefore;
                        }
                        else
                        {
                            comdata.IsIssueDateNull = true;
                        }
                        if (!cdata.IsMaturityDateBeforeNull)
                        {
                            comdata.MaturityDate = cdata.MaturityDateBefore;
                        }
                        else
                        {
                            comdata.IsMaturityDateNull = true;
                        }
                        if (!cdata.IsIs529PlanBeforeNull)
                        {
                            comdata.Is529Plan = cdata.Is529PlanBefore;
                        }
                        else
                        {
                            comdata.Is529Plan = true;
                        }
                        cdc.Insert(comdata, ce, tm);
                        // updateAllChangeRecordsForOID(comdata.SecurityGMFID, tm, "SecurityDataChange");
                    }
                    else
                    {
                        if (null == comdata)
                        {
                            datachange.Dispose();
                            cdc.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }

                        comdata.ADRLevel = cdata.ADRLevelBefore;
                        comdata.CFI = cdata.CFIBefore;
                        comdata.Class = cdata.ClassBefore;
                        comdata.CorporateAction = cdata.CorporateActionBefore;
                        comdata.CountryOfIncorporation = cdata.CountryOfIncorporationBefore;
                        comdata.Currency = cdata.CurrencyBefore;
                        comdata.Description1 = cdata.Description1Before;
                        comdata.Description2 = cdata.Description2Before;
                        comdata.Description3 = cdata.Description3Before;
                        comdata.ExpiredReason = cdata.ExpiredReasonBefore;
                        comdata.FundAdvisor = cdata.FundAdvisorBefore;
                        comdata.FundFamily = cdata.FundFamilyBefore;
                        comdata.FundFYE = cdata.FundFYEBefore;
                        comdata.FundLoad = cdata.FundLoadBefore;
                        comdata.FundManager = cdata.FundManagerBefore;
                        comdata.GMFStatus = cdata.GMFStatusBefore;
                        comdata.InterestRate = cdata.InterestRateBefore;
                        comdata.ISINCountry = cdata.ISINCountryBefore;
                        comdata.Issuer1 = cdata.Issuer1Before;
                        comdata.Issuer2 = cdata.Issuer2Before;
                        comdata.Issuer3 = cdata.Issuer3Before;
                        comdata.ObligorName = cdata.ObligorNameBefore;
                        comdata.PrimaryExchange = cdata.PrimaryExchangeBefore;
                        comdata.ProjectName = cdata.ProjectNameBefore;
                        comdata.SeriesName = cdata.SeriesNameBefore;
                        comdata.Type = cdata.TypeBefore;

                        if (!cdata.IsDefaultValueBeforeNull)
                        {
                            comdata.DefaultValue = cdata.DefaultValueBefore;
                        }
                        else
                        {
                            comdata.IsDefaultValueNull = true;
                        }
                        if (!cdata.IsExpiredBeforeNull)
                        {
                            comdata.Expired = cdata.ExpiredBefore;
                        }
                        else
                        {
                            comdata.IsExpiredNull = true;
                        }
                        if (!cdata.IsExpiredDateBeforeNull)
                        {
                            comdata.ExpiredDate = cdata.ExpiredDateBefore;
                        }
                        else
                        {
                            comdata.IsExpiredDateNull = true;
                        }
                        if (!cdata.IsInstitutionalBeforeNull)
                        {
                            comdata.Institutional = cdata.InstitutionalBefore;
                        }
                        else
                        {
                            comdata.Institutional = true;
                        }
                        if (!cdata.IsIssueDateBeforeNull)
                        {
                            comdata.IssueDate = cdata.IssueDateBefore;
                        }
                        else
                        {
                            comdata.IsIssueDateNull = true;
                        }
                        if (!cdata.IsMaturityDateBeforeNull)
                        {
                            comdata.MaturityDate = cdata.MaturityDateBefore;
                        }
                        else
                        {
                            comdata.IsMaturityDateNull = true;
                        }
                        if (!cdata.IsIs529PlanBeforeNull)
                        {
                            comdata.Is529Plan = cdata.Is529PlanBefore;
                        }
                        else
                        {
                            comdata.Is529Plan = true;
                        }
                        cdc.Update(comdata, ce, tm);
                    }
                }
                datachange.Dispose();
            }
            cdc.Dispose();

            InactivateChangeRecord(ce, tm);

            return true;
        }


        /// <summary>
        /// If securitydata is deleted, the sub objects under if will be deleted too, but the change event
        /// of these objects are still active, this method get all sub objects of deleted data, then get all 
        /// change records by OID of it, then get all change events of the change records and inactive it.
        /// </summary>
        /// <param name="securityGMFID"></param>
        /// <param name="feedID"></param>
        /// <param name="tm"></param>
        private void InactivateSecDataSubsChangeEvents(long securityGMFID, int feedID, TransactionManager tm)
        {

            SecurityExchangeCollection sec = tm.SecurityExchangeCollection;
            sec.GetBySecurityGMFID_FeedID(securityGMFID, feedID);
            ArrayList lst = sec.SecurityExchangeList;
            for (int i = 0; i < lst.Count; i++)
            {
                SecurityExchange a = (SecurityExchange)lst[i];
                SecurityExchangeChangeCollection addc = tm.SecurityExchangeChangeCollection;
                addc.GetByOID(a.OID);
                ArrayList datachangeList = addc.SecurityExchangeChangeList;

                for (int j = 0; j < datachangeList.Count; j++)
                {
                    SecurityExchangeChange data = (SecurityExchangeChange)datachangeList[j];
                    ChangeEventCollection cec = tm.ChangeEventCollection;
                    ChangeEvent ce = cec.GetByPrimaryKey(data.ChangeEventID);
                    InactivateChangeRecord(ce, tm);
                }
                addc.Dispose();
            }
            sec.Dispose();

            SecurityNameCollection snc = tm.SecurityNameCollection;
            snc.GetBySecurityGMFID_FeedID(securityGMFID, feedID);
            ArrayList snclst = snc.SecurityNameList;
            for (int i = 0; i < snclst.Count; i++)
            {
                SecurityName a = (SecurityName)snclst[i];
                SecurityNameChangeCollection addc = tm.SecurityNameChangeCollection;
                addc.GetByOID(a.OID);
                ArrayList datachangeList = addc.SecurityNameChangeList;

                for (int j = 0; j < datachangeList.Count; j++)
                {
                    SecurityNameChange data = (SecurityNameChange)datachangeList[j];
                    ChangeEventCollection cec = tm.ChangeEventCollection;
                    ChangeEvent ce = cec.GetByPrimaryKey(data.ChangeEventID);
                    InactivateChangeRecord(ce, tm);
                }
                addc.Dispose();
            }
            snc.Dispose();

            SecurityUniqueIDCollection suc = tm.SecurityUniqueIDCollection;
            suc.GetBySecurityGMFID_FeedID(securityGMFID, feedID);
            ArrayList suclst = suc.SecurityUniqueIDList;
            for (int i = 0; i < suclst.Count; i++)
            {
                SecurityUniqueID a = (SecurityUniqueID)suclst[i];
                SecurityUniqueIDChangeCollection addc = tm.SecurityUniqueIDChangeCollection;
                addc.GetByOID(a.OID);
                ArrayList datachangeList = addc.SecurityUniqueIDChangeList;

                for (int j = 0; j < datachangeList.Count; j++)
                {
                    SecurityUniqueIDChange data = (SecurityUniqueIDChange)datachangeList[j];
                    ChangeEventCollection cec = tm.ChangeEventCollection;
                    ChangeEvent ce = cec.GetByPrimaryKey(data.ChangeEventID);
                    InactivateChangeRecord(ce, tm);
                }
                addc.Dispose();
            }
            suc.Dispose();
        }

        /// <summary>
        /// If comdata is deleted, the sub objects under if will be deleted too, but the change event
        /// of these objects are still active, this method get all sub objects of deleted data, then get all 
        /// change records by OID of it, then get all change events of the change records and inactive it.
        /// </summary>
        /// <param name="companyGMFID"></param>
        /// <param name="feedID"></param>
        /// <param name="tm"></param>
        private void InactivateComDataSubsChangeEvents(long companyGMFID, int feedID, TransactionManager tm)
        {

            CompanyAddressCollection cac = tm.CompanyAddressCollection;
            cac.GetByCompanyGMFID_FeedID(companyGMFID, feedID);
            ArrayList lst = cac.CompanyAddressList;
            for (int i = 0; i < lst.Count; i++)
            {
                CompanyAddress a = (CompanyAddress)lst[i];
                CompanyAddressChangeCollection addc = tm.CompanyAddressChangeCollection;
                addc.GetByOID(a.ID);
                ArrayList datachangeList = addc.CompanyAddressChangeList;

                for (int j = 0; j < datachangeList.Count; j++)
                {
                    CompanyAddressChange data = (CompanyAddressChange)datachangeList[j];
                    ChangeEventCollection cec = tm.ChangeEventCollection;
                    ChangeEvent ce = cec.GetByPrimaryKey(data.ChangeEventID);
                    InactivateChangeRecord(ce, tm);
                }
                addc.Dispose();
            }
            cac.Dispose();

            CompanyContactCollection ccc = tm.CompanyContactCollection;
            ccc.GetByCompanyGMFID_FeedID(companyGMFID, feedID);
            ArrayList ccclst = ccc.CompanyContactList;
            for (int i = 0; i < ccclst.Count; i++)
            {
                CompanyContact a = (CompanyContact)ccclst[i];
                CompanyContactChangeCollection addc = tm.CompanyContactChangeCollection;
                addc.GetByOID(a.ID);
                ArrayList datachangeList = addc.CompanyContactChangeList;

                for (int j = 0; j < datachangeList.Count; j++)
                {
                    CompanyContactChange data = (CompanyContactChange)datachangeList[j];
                    ChangeEventCollection cec = tm.ChangeEventCollection;
                    ChangeEvent ce = cec.GetByPrimaryKey(data.ChangeEventID);
                    InactivateChangeRecord(ce, tm);
                }
                addc.Dispose();
            }
            ccc.Dispose();

            CompanyNameCollection cnc = tm.CompanyNameCollection;
            cnc.GetByCompanyGMFID_FeedID(companyGMFID, feedID);
            ArrayList cnclst = cnc.CompanyNameList;
            for (int i = 0; i < cnclst.Count; i++)
            {
                CompanyName a = (CompanyName)cnclst[i];
                CompanyNameChangeCollection addc = tm.CompanyNameChangeCollection;
                addc.GetByOID(a.ID);
                ArrayList datachangeList = addc.CompanyNameChangeList;

                for (int j = 0; j < datachangeList.Count; j++)
                {
                    CompanyNameChange data = (CompanyNameChange)datachangeList[j];
                    ChangeEventCollection cec = tm.ChangeEventCollection;
                    ChangeEvent ce = cec.GetByPrimaryKey(data.ChangeEventID);
                    InactivateChangeRecord(ce, tm);
                }
                addc.Dispose();
            }
            cnc.Dispose();

            CompanyServiceCollection csc = tm.CompanyServiceCollection;
            csc.GetByCompanyGMFID_FeedID(companyGMFID, feedID);
            ArrayList csclst = csc.CompanyServiceList;
            for (int i = 0; i < csclst.Count; i++)
            {
                CompanyService a = (CompanyService)csclst[i];
                CompanyServiceChangeCollection addc = tm.CompanyServiceChangeCollection;
                addc.GetByOID(a.ID);
                ArrayList datachangeList = addc.CompanyServiceChangeList;

                for (int j = 0; j < datachangeList.Count; j++)
                {
                    CompanyServiceChange data = (CompanyServiceChange)datachangeList[j];
                    ChangeEventCollection cec = tm.ChangeEventCollection;
                    ChangeEvent ce = cec.GetByPrimaryKey(data.ChangeEventID);
                    InactivateChangeRecord(ce, tm);
                }
                addc.Dispose();
            }
            csc.Dispose();

            CompanyTypeCollection ctc = tm.CompanyTypeCollection;
            ctc.GetByCompanyGMFID_FeedID(companyGMFID, feedID);
            ArrayList ctclst = ctc.CompanyTypeList;
            for (int i = 0; i < ctclst.Count; i++)
            {
                CompanyType a = (CompanyType)ctclst[i];
                CompanyTypeChangeCollection addc = tm.CompanyTypeChangeCollection;
                addc.GetByOID(a.ID);
                ArrayList datachangeList = addc.CompanyTypeChangeList;

                for (int j = 0; j < datachangeList.Count; j++)
                {
                    CompanyTypeChange data = (CompanyTypeChange)datachangeList[j];
                    ChangeEventCollection cec = tm.ChangeEventCollection;
                    ChangeEvent ce = cec.GetByPrimaryKey(data.ChangeEventID);
                    InactivateChangeRecord(ce, tm);
                }
                addc.Dispose();
            }
            ctc.Dispose();

            CompanyUniqueIDCollection cuc = tm.CompanyUniqueIDCollection;
            cuc.GetByCompanyGMFID_FeedID(companyGMFID, feedID);
            ArrayList cuclst = cuc.CompanyUniqueIDList;
            for (int i = 0; i < cuclst.Count; i++)
            {
                CompanyUniqueID a = (CompanyUniqueID)cuclst[i];
                CompanyUniqueIDChangeCollection addc = tm.CompanyUniqueIDChangeCollection;
                addc.GetByOID(a.ID);
                ArrayList datachangeList = addc.CompanyUniqueIDChangeList;

                for (int j = 0; j < datachangeList.Count; j++)
                {
                    CompanyUniqueIDChange data = (CompanyUniqueIDChange)datachangeList[j];
                    ChangeEventCollection cec = tm.ChangeEventCollection;
                    ChangeEvent ce = cec.GetByPrimaryKey(data.ChangeEventID);
                    InactivateChangeRecord(ce, tm);
                }
                addc.Dispose();
            }
            cuc.Dispose();


        }

        /// <summary>
        /// Revert Company
        /// </summary>
        /// <param name="changeEventId"></param>
        /// <param name="companyGMFID"></param>
        /// <param name="feedID"></param>
        /// <param name="tm"></param>
        /// <param name="ce"></param>
        private bool RevertCompany(long changeEventId, long companyGMFID, int feedID, TransactionManager tm, ChangeEvent ce)
        {
            CompanyAddressChangeCollection addrchange = tm.CompanyAddressChangeCollection;
            addrchange.GetByChangeEventID(changeEventId);
            ArrayList addrchangeList = addrchange.CompanyAddressChangeList;
            for (int i = 0; i < addrchangeList.Count; i++)
            {
                CompanyAddressChange data = (CompanyAddressChange)addrchangeList[i];

                CompanyAddressCollection cac = tm.CompanyAddressCollection;
                CompanyAddress adddata = cac.GetByPrimaryKey(data.CompanyAddressID);

                if (null != adddata && ValidateSubsequentChange("tChgCAddr", "hcaOID", "hcaEventID", "hcaID", adddata.ID, changeEventId, data.ID, tm) > 0)
                {
                    addrchange.Dispose();
                    cac.Dispose();
                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                    return false;
                }
                else
                {

                    if (data.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_DELETE))
                    {
                        if (ValidateSubsequentChangeForDel("tChgCAddr", "hcagmfid", "hcafid", "hcaEventID", "hcaID", companyGMFID, changeEventId, data.ID, tm) > 0)
                        {
                            addrchange.Dispose();
                            cac.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }

                        adddata = new CompanyAddress();
                        adddata.FeedID = 2;
                        adddata.CompanyGMFID = companyGMFID;
                        adddata.Address1 = data.Address1Before;
                        adddata.Address2 = data.Address2Before;
                        adddata.City = data.CityBefore;
                        adddata.CityCode = data.CityCodeBefore;
                        adddata.ContinentCode = data.ContinentCodeBefore;
                        adddata.Country = data.CountryBefore;
                        adddata.CountryCode = data.CountryCodeBefore;
                        adddata.County = data.CountyBefore;
                        adddata.CountyCode = data.CountyCodeBefore;
                        adddata.PostalCode = data.PostalCodeBefore;
                        adddata.SrcCountryCode = data.SrcCountryCodeBefore;
                        adddata.StateProvinceAbbrev = data.StateProvinceAbbrevBefore;
                        adddata.StateProvinceName = data.StateProvinceNameBefore;
                        adddata.Type = data.TypeBefore;

                        cac.Insert(adddata, ce, tm);

                        updateAllChangeRecordsForOID(adddata.ID, data.CompanyAddressID, tm, "CompanyAddressChange");

                    }
                    else if (data.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_UPDATE))
                    {
                        if (null == adddata)
                        {
                            addrchange.Dispose();
                            cac.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }

                        adddata.Address1 = data.Address1Before;
                        adddata.Address2 = data.Address2Before;
                        adddata.City = data.CityBefore;
                        adddata.CityCode = data.CityCodeBefore;
                        adddata.ContinentCode = data.ContinentCodeBefore;
                        adddata.Country = data.CountryBefore;
                        adddata.CountryCode = data.CountryCodeBefore;
                        //adddata.County = data.CountyBefore;
                        //adddata.CountyCode = data.CountyCodeBefore;
                        adddata.PostalCode = data.PostalCodeBefore;
                        adddata.SrcCountryCode = data.SrcCountryCodeBefore;
                        adddata.StateProvinceAbbrev = data.StateProvinceAbbrevBefore;
                        adddata.StateProvinceName = data.StateProvinceNameBefore;
                        adddata.Type = data.TypeBefore;

                        if (string.IsNullOrEmpty(data.Address1Before) && string.IsNullOrEmpty(data.Address2Before) &&
                            string.IsNullOrEmpty(data.CityBefore) && string.IsNullOrEmpty(data.CityCodeBefore) &&
                            string.IsNullOrEmpty(data.ContinentCodeBefore) && string.IsNullOrEmpty(data.CountryBefore) &&
                            string.IsNullOrEmpty(data.CountryCodeBefore) && string.IsNullOrEmpty(data.PostalCodeBefore) &&
                            string.IsNullOrEmpty(data.SrcCountryCodeBefore) && string.IsNullOrEmpty(data.StateProvinceAbbrevBefore) &&
                            string.IsNullOrEmpty(data.StateProvinceNameBefore) && string.IsNullOrEmpty(data.TypeBefore))
                        {
                            cac.Delete(adddata, ce, tm);
                        }
                        else
                        {
                            cac.Update(adddata, ce, tm);
                        }
                    }
                    else
                    {
                        if (null != adddata)
                        {
                            cac.Delete(adddata, ce, tm);
                        }
                        else
                        {
                            cac.Dispose();
                            addrchange.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                    }
                }
                cac.Dispose();
            }
            addrchange.Dispose();

            CompanyContactChangeCollection cntchange = tm.CompanyContactChangeCollection;
            cntchange.GetByChangeEventID(changeEventId);
            ArrayList cntchangeList = cntchange.CompanyContactChangeList;
            for (int i = 0; i < cntchangeList.Count; i++)
            {
                CompanyContactChange data = (CompanyContactChange)cntchangeList[i];

                CompanyContactCollection ccc = tm.CompanyContactCollection;
                CompanyContact ccdata = ccc.GetByPrimaryKey(data.CompanyContactID);

                if (null != ccdata && ValidateSubsequentChange("tChgCContct", "hccOID", "hccEventID", "hccID", ccdata.ID, changeEventId, data.ID, tm) > 0)
                {
                    cntchange.Dispose();
                    ccc.Dispose();
                    tm.RollbackTransaction();
                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                    return false;
                }
                else
                {
                    if (data.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_DELETE))
                    {
                        if (ValidateSubsequentChangeForDel("tChgCContct", "hccgmfid", "hccfid", "hccEventID", "hccID", companyGMFID, changeEventId, data.ID, tm) > 0)
                        {
                            cntchange.Dispose();
                            ccc.Dispose();
                            tm.RollbackTransaction();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                        ccdata = new CompanyContact();
                        ccdata.FeedID = 2;
                        ccdata.CompanyGMFID = companyGMFID;
                        ccdata.Method = data.MethodBefore;
                        ccdata.Type = data.TypeBefore;
                        ccc.Insert(ccdata, ce, tm);
                        updateAllChangeRecordsForOID(ccdata.ID, data.CompanyContactID, tm, "CompanyContactChange");
                    }
                    else if (data.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_UPDATE))
                    {
                        if (null == ccdata)
                        {
                            cntchange.Dispose();
                            ccc.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                        ccdata.Method = data.MethodBefore;
                        ccdata.Type = data.TypeBefore;
                        ccc.Update(ccdata, ce, tm);
                    }
                    else
                    {
                        if (null != ccdata)
                        {
                            ccc.Delete(ccdata, ce, tm);
                        }
                        else
                        {
                            ccc.Dispose();
                            cntchange.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                    }
                }
                ccc.Dispose();
            }
            cntchange.Dispose();

            CompanyNameChangeCollection namechange = tm.CompanyNameChangeCollection;
            namechange.GetByChangeEventID(changeEventId);
            ArrayList nameList = namechange.CompanyNameChangeList;
            for (int i = 0; i < nameList.Count; i++)
            {
                CompanyNameChange data = (CompanyNameChange)nameList[i];
                CompanyNameCollection ccc = tm.CompanyNameCollection;
                CompanyName ccdata = ccc.GetByPrimaryKey(data.CompanyNameID);


                if (null != ccdata && ValidateSubsequentChange("tChgCName", "hcnOID", "hcnEventID", "hcnID", ccdata.ID, changeEventId, data.ID, tm) > 0)
                {
                    namechange.Dispose();
                    ccc.Dispose();
                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                    return false;
                }
                else
                {
                    if (data.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_DELETE))
                    {
                        if (ValidateSubsequentChangeForDel("tChgCName", "hcngmfid", "hcnfid", "hcnEventID", "hcnID", companyGMFID, changeEventId, data.ID, tm) > 0)
                        {
                            namechange.Dispose();
                            ccc.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                        ccdata = new CompanyName();
                        ccdata.FeedID = 2;
                        ccdata.CompanyGMFID = companyGMFID;
                        if (!data.IsTypeIncrementBeforeNull)
                        {
                            ccdata.TypeIncrement = data.TypeIncrementBefore;
                        }
                        ccdata.ImportType = data.ImportTypeBefore;
                        ccdata.Name = data.NameBefore;
                        ccdata.Type = data.TypeBefore;
                        ccc.Insert(ccdata, ce, tm);
                        updateAllChangeRecordsForOID(ccdata.ID, data.CompanyNameID, tm, "CompanyNameChange");

                        CompanyDataCollection cdc2 = tm.CompanyDataCollection;
                        CompanyData comdata2 = cdc2.GetByPrimaryKey(companyGMFID, feedID);
                        if (null != comdata2)
                        {
                            comdata2.LegalName = ccdata.Name;
                            cdc2.Update(comdata2, ce, tm);
                            cdc2.Dispose();
                        }
                    }
                    else if (data.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_UPDATE))
                    {
                        if (null == ccdata)
                        {
                            namechange.Dispose();
                            ccc.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                        if (!data.IsTypeIncrementBeforeNull)
                        {
                            ccdata.TypeIncrement = data.TypeIncrementBefore;
                        }
                        ccdata.ImportType = data.ImportTypeBefore;
                        ccdata.Name = data.NameBefore;
                        ccdata.Type = data.TypeBefore;
                        ccc.Update(ccdata, ce, tm);

                        CompanyDataCollection cdc2 = tm.CompanyDataCollection;
                        CompanyData comdata2 = cdc2.GetByPrimaryKey(companyGMFID, feedID);
                        if (null != comdata2)
                        {
                            comdata2.LegalName = ccdata.Name;
                            cdc2.Update(comdata2, ce, tm);
                            cdc2.Dispose();
                        }
                    }
                    else
                    {
                        if (null != ccdata)
                        {
                            ccc.Delete(ccdata, ce, tm);

                            CompanyDataCollection cdc2 = tm.CompanyDataCollection;
                            CompanyData comdata2 = cdc2.GetByPrimaryKey(companyGMFID, feedID);
                            if (null != comdata2)
                            {
                                comdata2.LegalName = string.Empty;
                                cdc2.Update(comdata2, ce, tm);
                                cdc2.Dispose();
                            }
                        }
                        else
                        {
                            ccc.Dispose();
                            namechange.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                    }
                }
                ccc.Dispose();

            }
            namechange.Dispose();

            CompanyServiceChangeCollection svchange = tm.CompanyServiceChangeCollection;
            svchange.GetByChangeEventID(changeEventId);
            ArrayList svchangeList = svchange.CompanyServiceChangeList;
            for (int i = 0; i < svchangeList.Count; i++)
            {
                CompanyServiceChange data = (CompanyServiceChange)svchangeList[i];

                CompanyServiceCollection ccc = tm.CompanyServiceCollection;
                CompanyService ccdata = ccc.GetByPrimaryKey(data.CompanyServiceID);

                if (null != ccdata && ValidateSubsequentChange("tChgCSvc", "hcsOID", "hcsEventID", "hcsID", ccdata.ID, changeEventId, data.ID, tm) > 0)
                {
                    svchange.Dispose();
                    ccc.Dispose();
                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                    return false;
                }
                else
                {
                    if (data.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_DELETE))
                    {
                        if (ValidateSubsequentChangeForDel("tChgCSvc", "hcsgmfid", "hcsfid", "hcsEventID", "hcsID", companyGMFID, changeEventId, data.ID, tm) > 0)
                        {
                            svchange.Dispose();
                            ccc.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                        ccdata = new CompanyService();
                        ccdata.FeedID = 2;
                        ccdata.CompanyGMFID = companyGMFID;
                        ccdata.ImportType = data.ImportTypeBefore;
                        if (!data.IsTypeIncrementBeforeNull)
                        {
                            ccdata.TypeIncrement = data.TypeIncrementBefore;
                        }
                        ccdata.Name = data.NameBefore;
                        ccdata.Type = data.TypeBefore;
                        ccc.Insert(ccdata, ce, tm);
                        updateAllChangeRecordsForOID(ccdata.ID, data.CompanyServiceID, tm, "CompanyServiceChange");
                    }
                    else if (data.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_UPDATE))
                    {
                        if (null == ccdata)
                        {
                            svchange.Dispose();
                            ccc.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                        ccdata.ImportType = data.ImportTypeBefore;
                        if (!data.IsTypeIncrementBeforeNull)
                        {
                            ccdata.TypeIncrement = data.TypeIncrementBefore;
                        }
                        ccdata.Name = data.NameBefore;
                        ccdata.Type = data.TypeBefore;
                        ccc.Update(ccdata, ce, tm);
                    }
                    else
                    {
                        if (null != ccdata)
                        {
                            ccc.Delete(ccdata, ce, tm);
                        }
                        else
                        {
                            ccc.Dispose();
                            svchange.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                    }
                }

                ccc.Dispose();
            }
            svchange.Dispose();

            CompanyTypeChangeCollection ctchange = tm.CompanyTypeChangeCollection;
            ctchange.GetByChangeEventID(changeEventId);
            ArrayList ctchangeList = ctchange.CompanyTypeChangeList;
            CompanyTypeCollection ctc = tm.CompanyTypeCollection;
            for (int i = 0; i < ctchangeList.Count; i++)
            {
                CompanyTypeChange data = (CompanyTypeChange)ctchangeList[i];
                CompanyType ccdata = ctc.GetByPrimaryKey(data.CompanyTypeID);

                if (null != ccdata && ValidateSubsequentChange("tChgCType", "hctOID", "hctEventID", "hctID", ccdata.ID, changeEventId, data.ID, tm) > 0)
                {
                    ctchange.Dispose();
                    ctc.Dispose();
                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                    return false;
                }
                else
                {

                    if (data.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_DELETE))
                    {
                        //if (ValidateSubsequentChangeForDel("tChgCType", "hctgmfid", "hctfid", "hctEventID", "hctID", companyGMFID, changeEventId, data.ID, tm) > 0)
                        if (ValidateSubsequentChangeForDelIndustry("tChgCType", "hctgmfid", "hctfid", "hctEventID", "hctID", companyGMFID, changeEventId,
                            data.ID, tm, "hctType", data.TypeBefore) > 0)
                        {
                            ctchange.Dispose();
                            ctc.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                        ccdata = new CompanyType();
                        ccdata.FeedID = 2;
                        ccdata.CompanyGMFID = companyGMFID;
                        if (!data.IsTypeIncrementBeforeNull)
                        {
                            ccdata.TypeIncrement = data.TypeIncrementBefore;
                        }
                        ccdata.ImportType = data.ImportTypeBefore;
                        ccdata.Type = data.TypeBefore;
                        ccdata.Code = data.CodeBefore;
                        ctc.Insert(ccdata, ce, tm);
                        updateAllChangeRecordsForOID(ccdata.ID, data.CompanyTypeID, tm, "CompanyTypeChange");
                    }
                    else if (data.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_UPDATE))
                    {
                        if (null == ccdata)
                        {
                            ctchange.Dispose();
                            ctc.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                        if (!data.IsTypeIncrementBeforeNull)
                        {
                            ccdata.TypeIncrement = data.TypeIncrementBefore;
                        }
                        ccdata.ImportType = data.ImportTypeBefore;
                        ccdata.Type = data.TypeBefore;
                        ccdata.Code = data.CodeBefore;
                        if (string.IsNullOrEmpty(data.CodeBefore))
                        {
                            //Treat revert to update to null as delete.
                            ctc.Delete(ccdata, ce, tm);
                        }
                        else
                        {
                            ctc.Update(ccdata, ce, tm);
                        }
                    }
                    else
                    {
                        if (null != ccdata)
                        {
                            ctc.Delete(ccdata, ce, tm);
                        }
                        else
                        {
                            ctchange.Dispose();
                            ctc.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                    }
                }
            }
            ctc.Dispose();
            ctchange.Dispose();

            CompanyUniqueIDChangeCollection cuchange = tm.CompanyUniqueIDChangeCollection;
            cuchange.GetByChangeEventID(changeEventId);
            ArrayList cuchangeList = cuchange.CompanyUniqueIDChangeList;
            for (int i = 0; i < cuchangeList.Count; i++)
            {
                CompanyUniqueIDChange data = (CompanyUniqueIDChange)cuchangeList[i];

                CompanyUniqueIDCollection ccc = tm.CompanyUniqueIDCollection;
                CompanyUniqueID ccdata = ccc.GetByPrimaryKey(data.CompanyUniqueID);


                if (null != ccdata && ValidateSubsequentChange("tChgCUnqIds", "hciOID", "hciEventID", "hciID", ccdata.ID, changeEventId, data.ID, tm) > 0)
                {
                    cuchange.Dispose();
                    ccc.Dispose();
                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                    return false;
                }
                else
                {
                    if (data.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_DELETE))
                    {
                        if (ValidateSubsequentChangeForDel("tChgCUnqIds", "hcigmfid", "hcifid", "hciEventID", "hciID", companyGMFID, changeEventId, data.ID, tm) > 0)
                        {
                            cuchange.Dispose();
                            ccc.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                        ccdata = new CompanyUniqueID();
                        ccdata.FeedID = 2;
                        ccdata.CompanyGMFID = companyGMFID;
                        if (!data.IsRulesBeforeNull)
                        {
                            ccdata.Rules = data.RulesBefore;
                        }
                        ccdata.ImportType = data.ImportTypeBefore;
                        if (!data.IsRulesBeforeNull)
                        {
                            ccdata.NotRules = data.NotRulesBefore;
                        }
                        ccdata.Type = data.TypeBefore;
                        ccdata.Code = data.CodeBefore;
                        ccc.Insert(ccdata, ce, tm);
                        updateAllChangeRecordsForOID(ccdata.ID, data.CompanyUniqueID, tm, "CompanyUniqueIDChange");
                    }
                    else if (data.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_UPDATE))
                    {
                        if (null == ccdata)
                        {
                            cuchange.Dispose();
                            ccc.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                        if (!data.IsRulesBeforeNull)
                        {
                            ccdata.Rules = data.RulesBefore;
                        }
                        ccdata.ImportType = data.ImportTypeBefore;
                        if (!data.IsRulesBeforeNull)
                        {
                            ccdata.NotRules = data.NotRulesBefore;
                        }
                        ccdata.Type = data.TypeBefore;
                        ccdata.Code = data.CodeBefore;
                        if (string.IsNullOrEmpty(data.CodeBefore))
                        {
                            //Treat revert to update to null as delete.
                            ccc.Delete(ccdata, ce, tm);
                        }
                        else
                        {
                            ccc.Update(ccdata, ce, tm);
                        }
                    }
                    else
                    {
                        if (null != ccdata)
                        {
                            ccc.Delete(ccdata, ce, tm);
                        }
                        else
                        {
                            ccc.Dispose();
                            cuchange.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                    }
                }
                ccc.Dispose();
            }
            cuchange.Dispose();

            CompanyDataCollection cdc = tm.CompanyDataCollection;
            CompanyData comdata = cdc.GetByPrimaryKey(companyGMFID, feedID);

            CompanyDataChangeCollection datachange = tm.CompanyDataChangeCollection;
            datachange.GetByChangeEventID(changeEventId);
            ArrayList datachangeList = datachange.CompanyDataChangeList;

            if (null != datachangeList && datachangeList.Count != 0)
            {
                SortDataChange sort = new SortDataChange();
                datachangeList.Sort(sort);

                CompanyDataChange cdata = (CompanyDataChange)datachangeList[0];

                if (null != comdata && ValidateSubsequentChangeForComData("Com", companyGMFID, feedID, changeEventId, cdata.ID, tm) > 0)
                {
                    datachange.Dispose();
                    cdc.Dispose();
                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                    return false;
                }
                else
                {
                    if (cdata.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_INSERT))
                    {
                        InactivateComDataSubsChangeEvents(companyGMFID, feedID, tm);
                        if (null != comdata)
                        {
                            cdc.Delete(comdata, ce, tm);
                        }
                        else
                        {
                            datachange.Dispose();
                            cdc.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }
                    }
                    else if (cdata.Action.Trim().Equals(ChangeEventCollection.CHG_ACTION_DELETE))
                    {
                        comdata = new CompanyData();

                        comdata.FeedID = 2;
                        comdata.CompanyGMFID = companyGMFID;
                        if (!cdata.IsDomesticUltimateParentBeforeNull)
                        {
                            comdata.DomesticUltimateParent = cdata.DomesticUltimateParentBefore;
                        }
                        else
                        {
                            comdata.IsDomesticUltimateParentNull = true;
                        }
                        comdata.DUNSChangeCode = cdata.DUNSChangeCodeBefore;
                        if (!cdata.IsEmployeeCountBeforeNull)
                        {
                            comdata.EmployeeCount = cdata.EmployeeCountBefore;
                        }
                        else
                        {
                            comdata.IsEmployeeCountNull = true;
                        }
                        if (!cdata.IsEmployeeTotalBeforeNull)
                        {
                            comdata.EmployeeTotal = cdata.EmployeeTotalBefore;
                        }
                        else
                        {
                            comdata.IsEmployeeTotalNull = true;
                        }
                        comdata.ExecName = cdata.ExecNameBefore;
                        comdata.ExecTitle = cdata.ExecTitleBefore;
                        if (!cdata.IsExpiredBeforeNull)
                        {
                            comdata.Expired = cdata.ExpiredBefore;
                        }
                        else
                        {
                            comdata.IsExpiredNull = true;
                        }
                        if (!cdata.IsExpiredDateBeforeNull)
                        {
                            comdata.ExpiredDate = cdata.ExpiredDateBefore;
                        }
                        else
                        {
                            comdata.IsExpiredDateNull = true;
                        }
                        comdata.ExpiredReason = cdata.ExpiredReasonBefore;

                        if (!cdata.IsFamilyTreeValidDateBeforeNull)
                        {
                            comdata.FamilyTreeValidDate = cdata.FamilyTreeValidDateBefore;
                        }
                        else
                        {
                            comdata.IsFamilyTreeValidDateNull = true;
                        }
                        comdata.FiscalYearEnd = cdata.FiscalYearEndBefore;

                        if (!cdata.IsGlobalUltimateParentBeforeNull)
                        {
                            comdata.GlobalUltimateParent = cdata.GlobalUltimateParentBefore;
                        }
                        else
                        {
                            comdata.IsGlobalUltimateParentNull = true;
                        }

                        if (!cdata.IsIsBranchBeforeNull)
                        {
                            comdata.IsBranch = cdata.IsBranchBefore;
                        }
                        else
                        {
                            comdata.IsIsBranchNull = true;
                        }


                        if (!cdata.IsIsIssuerBeforeNull)
                        {
                            comdata.IsIssuer = cdata.IsIssuerBefore;
                        }
                        else
                        {
                            comdata.IsIsIssuerNull = true;
                        }
                        if (!cdata.IsIsParentBeforeNull)
                        {
                            comdata.IsParent = cdata.IsParentBefore;
                        }
                        else
                        {
                            comdata.IsIsParentNull = true;
                        }
                        if (!cdata.IsIsPublicBeforeNull)
                        {
                            comdata.IsPublic = cdata.IsPublicBefore;
                        }
                        else
                        {
                            comdata.IsIsPublicNull = true;
                        }
                        if (!cdata.IsResearchDateBeforeNull)
                        {
                            comdata.ResearchDate = cdata.ResearchDateBefore;
                        }
                        else
                        {
                            comdata.IsResearchDateNull = true;
                        }
                        if (!cdata.IsUSDSalesBeforeNull)
                        {
                            comdata.USDSales = cdata.USDSalesBefore;
                        }
                        else
                        {
                            comdata.IsUSDSalesNull = true;
                        }
                        comdata.LineOfBusiness = cdata.LineOfBusinessBefore;
                        comdata.LocalCurrency = cdata.LocalCurrencyBefore;
                        if (!cdata.IsLocalSalesBeforeNull)
                        {
                            comdata.LocalSales = cdata.LocalSalesBefore;
                        }
                        else
                        {
                            comdata.IsLocalSalesNull = true;
                        }
                        comdata.Marketability = cdata.MarketabilityBefore;

                        comdata.OriginalDUNS = cdata.OriginalDUNSBefore;
                        if (!cdata.IsOwnerReviewCycleBeforeNull)
                        {
                            comdata.OwnerReviewCycle = cdata.OwnerReviewCycleBefore;

                        }
                        else
                        {
                            comdata.IsOwnerReviewCycleNull = true;
                        }
                        if (!cdata.IsOwnerVerifyBeforeNull)
                        {
                            comdata.OwnerVerify = cdata.OwnerVerifyBefore;
                        }
                        else
                        {
                            comdata.IsOwnerVerifyNull = true;
                        }
                        comdata.ParentInformation1 = cdata.ParentInformation1Before;
                        comdata.ParentInformation2 = cdata.ParentInformation2Before;
                        if (!cdata.IsResearchDateBeforeNull)
                        {
                            comdata.ResearchDate = cdata.ResearchDateBefore;
                        }
                        else
                        {
                            comdata.IsResearchDateNull = true;
                        }
                        comdata.StateProvinceIncorp = cdata.StateProvinceIncorpBefore;
                        comdata.Status = cdata.StatusBefore;
                        comdata.YearStarted = cdata.YearStartedBefore;
                        cdc.Insert(comdata, ce, tm);
                        //updateAllChangeRecordsForOID(comdata.CompanyGMFID, tm, "CompanyDataChange");
                    }
                    else
                    {

                        if (null == comdata)
                        {
                            datachange.Dispose();
                            cdc.Dispose();
                            ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be reverted due to subsequent change.');", true);
                            return false;
                        }

                        if (!cdata.IsDomesticUltimateParentBeforeNull)
                        {
                            comdata.DomesticUltimateParent = cdata.DomesticUltimateParentBefore;
                        }
                        else
                        {
                            comdata.IsDomesticUltimateParentNull = true;
                        }
                        comdata.DUNSChangeCode = cdata.DUNSChangeCodeBefore;
                        if (!cdata.IsEmployeeCountBeforeNull)
                        {
                            comdata.EmployeeCount = cdata.EmployeeCountBefore;
                        }
                        else
                        {
                            comdata.IsEmployeeCountNull = true;
                        }
                        if (!cdata.IsEmployeeTotalBeforeNull)
                        {
                            comdata.EmployeeTotal = cdata.EmployeeTotalBefore;
                        }
                        else
                        {
                            comdata.IsEmployeeTotalNull = true;
                        }
                        comdata.ExecName = cdata.ExecNameBefore;
                        comdata.ExecTitle = cdata.ExecTitleBefore;
                        if (!cdata.IsExpiredBeforeNull)
                        {
                            comdata.Expired = cdata.ExpiredBefore;
                        }
                        else
                        {
                            comdata.IsExpiredNull = true;
                        }
                        if (!cdata.IsExpiredDateBeforeNull)
                        {
                            comdata.ExpiredDate = cdata.ExpiredDateBefore;
                        }
                        else
                        {
                            comdata.IsExpiredDateNull = true;
                        }
                        comdata.ExpiredReason = cdata.ExpiredReasonBefore;

                        if (!cdata.IsFamilyTreeValidDateBeforeNull)
                        {
                            comdata.FamilyTreeValidDate = cdata.FamilyTreeValidDateBefore;
                        }
                        else
                        {
                            comdata.IsFamilyTreeValidDateNull = true;
                        }
                        comdata.FiscalYearEnd = cdata.FiscalYearEndBefore;

                        if (!cdata.IsGlobalUltimateParentBeforeNull)
                        {
                            comdata.GlobalUltimateParent = cdata.GlobalUltimateParentBefore;
                        }
                        else
                        {
                            comdata.IsGlobalUltimateParentNull = true;
                        }

                        if (!cdata.IsIsBranchBeforeNull)
                        {
                            comdata.IsBranch = cdata.IsBranchBefore;
                        }
                        else
                        {
                            comdata.IsIsBranchNull = true;
                        }


                        if (!cdata.IsIsIssuerBeforeNull)
                        {
                            comdata.IsIssuer = cdata.IsIssuerBefore;
                        }
                        else
                        {
                            comdata.IsIsIssuerNull = true;
                        }
                        if (!cdata.IsIsParentBeforeNull)
                        {
                            comdata.IsParent = cdata.IsParentBefore;
                        }
                        else
                        {
                            comdata.IsIsParentNull = true;
                        }
                        if (!cdata.IsIsPublicBeforeNull)
                        {
                            comdata.IsPublic = cdata.IsPublicBefore;
                        }
                        else
                        {
                            comdata.IsIsPublicNull = true;
                        }
                        if (!cdata.IsResearchDateBeforeNull)
                        {
                            comdata.ResearchDate = cdata.ResearchDateBefore;
                        }
                        else
                        {
                            comdata.IsResearchDateNull = true;
                        }
                        if (!cdata.IsUSDSalesBeforeNull)
                        {
                            comdata.USDSales = cdata.USDSalesBefore;
                        }
                        else
                        {
                            comdata.IsUSDSalesNull = true;
                        }
                        comdata.LineOfBusiness = cdata.LineOfBusinessBefore;
                        comdata.LocalCurrency = cdata.LocalCurrencyBefore;
                        if (!cdata.IsLocalSalesBeforeNull)
                        {
                            comdata.LocalSales = cdata.LocalSalesBefore;
                        }
                        else
                        {
                            comdata.IsLocalSalesNull = true;
                        }
                        comdata.Marketability = cdata.MarketabilityBefore;

                        comdata.OriginalDUNS = cdata.OriginalDUNSBefore;
                        if (!cdata.IsOwnerReviewCycleBeforeNull)
                        {
                            comdata.OwnerReviewCycle = cdata.OwnerReviewCycleBefore;

                        }
                        else
                        {
                            comdata.IsOwnerReviewCycleNull = true;
                        }
                        if (!cdata.IsOwnerVerifyBeforeNull)
                        {
                            comdata.OwnerVerify = cdata.OwnerVerifyBefore;
                        }
                        else
                        {
                            comdata.IsOwnerVerifyNull = true;
                        }
                        comdata.ParentInformation1 = cdata.ParentInformation1Before;
                        comdata.ParentInformation2 = cdata.ParentInformation2Before;
                        if (!cdata.IsResearchDateBeforeNull)
                        {
                            comdata.ResearchDate = cdata.ResearchDateBefore;
                        }
                        else
                        {
                            comdata.IsResearchDateNull = true;
                        }
                        comdata.StateProvinceIncorp = cdata.StateProvinceIncorpBefore;
                        comdata.Status = cdata.StatusBefore;
                        comdata.YearStarted = cdata.YearStartedBefore;
                        cdc.Update(comdata, ce, tm);
                    }
                }
                datachange.Dispose();
            }
            cdc.Dispose();

            InactivateChangeRecord(ce, tm);

            return true;
        }

        /// <summary>
        /// Inactivate change event
        /// </summary>
        /// <param name="ce"></param>
        /// <param name="tm"></param>
        private void InactivateChangeRecord(ChangeEvent ce, TransactionManager tm)
        {
            ce.ChangeEventActive = false;
            ChangeEventCollection cec = tm.ChangeEventCollection;
            cec.Update(ce);
        }

        /// <summary>
        /// Validate subsequent change for CompanyData
        /// </summary>
        /// <param name="gmfId"></param>
        /// <param name="feedId"></param>
        /// <param name="eventId"></param>
        /// <param name="id"></param>
        /// <param name="tm"></param>
        /// <returns></returns>
        private int ValidateSubsequentChangeForComData(string dataType, long gmfId, int feedId, long eventId, long id, TransactionManager tm)
        {
            string sqlStr = "";
            if ("Sec".Equals(dataType))
            {
                sqlStr = "SELECT count(*) FROM tChgSData INNER JOIN tChgEvent on hsdEventID = heOID and tChgEvent.heActive = 1" +
                      " WHERE" +
                      " hsdGMFID = @GMFID" +
                      " and hsdFID = @FID" +
                      " and hsdEventID <> @EVTID" +
                      " and hsdID > @ID";
            }
            else if ("Com".Equals(dataType))
            {
                sqlStr = "SELECT count(*) FROM tChgCData INNER JOIN tChgEvent on hcdEventID = heOID and tChgEvent.heActive = 1" +
                    " WHERE" +
                    " hcdGMFID = @GMFID" +
                    " and hcdFID = @FID" +
                    " and hcdEventID <> @EVTID" +
                    " and hcdID > @ID";
            }
            else if ("Rel".Equals(dataType))
            {
                sqlStr = "SELECT count(*) FROM tChgRData INNER JOIN tChgEvent on hrdEventID = heOID and tChgEvent.heActive = 1" +
                    " WHERE" +
                    " hrdGMFID = @GMFID" +
                    " and hrdFID = @FID" +
                    " and hrdEventID <> @EVTID" +
                    " and hrdID > @ID";
            }

            SqlCommand cmd = new SqlCommand(sqlStr, (SqlConnection)tm.Connection);
            cmd.Parameters.Add("@GMFID", SqlDbType.BigInt);
            cmd.Parameters["@GMFID"].Value = gmfId;
            cmd.Parameters.Add("@FID", SqlDbType.BigInt);
            cmd.Parameters["@FID"].Value = feedId;
            cmd.Parameters.Add("@EVTID", SqlDbType.BigInt);
            cmd.Parameters["@EVTID"].Value = eventId;
            cmd.Parameters.Add("@ID", SqlDbType.BigInt);
            cmd.Parameters["@ID"].Value = id;
            cmd.Transaction = (SqlTransaction)tm.Transaction;
            string cnt = cmd.ExecuteScalar().ToString();
            return int.Parse(cnt);
        }

        /// <summary>
        /// Validate subsequent change for all items except CompanyData
        /// </summary>
        /// <param name="table"></param>
        /// <param name="oidCol"></param>
        /// <param name="evtCol"></param>
        /// <param name="idCol"></param>
        /// <param name="oid"></param>
        /// <param name="eventId"></param>
        /// <param name="id"></param>
        /// <param name="tm"></param>
        /// <returns></returns>
        private int ValidateSubsequentChange(string table, string oidCol, string evtCol, string idCol, long oid, long eventId, long id, TransactionManager tm)
        {
            string sqlStr = "SELECT count(*) FROM " + table +
                " INNER JOIN tChgEvent on tChgEvent.heOID = " +
                evtCol + " and tChgEvent.heActive = 1" +
                " WHERE " + oidCol +
                " = @OID" +
                " and " + evtCol + " <> @EVTID" +
                " and " + idCol + " > @ID";

            SqlCommand cmd = new SqlCommand(sqlStr, (SqlConnection)tm.Connection);
            cmd.Parameters.Add("@OID", SqlDbType.BigInt);
            cmd.Parameters["@OID"].Value = oid;
            cmd.Parameters.Add("@EVTID", SqlDbType.BigInt);
            cmd.Parameters["@EVTID"].Value = eventId;
            cmd.Parameters.Add("@ID", SqlDbType.BigInt);
            cmd.Parameters["@ID"].Value = id;
            cmd.Transaction = (SqlTransaction)tm.Transaction;
            string cnt = cmd.ExecuteScalar().ToString();
            return int.Parse(cnt);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="oidCol"></param>
        /// <param name="evtCol"></param>
        /// <param name="idCol"></param>
        /// <param name="oid"></param>
        /// <param name="eventId"></param>
        /// <param name="id"></param>
        /// <param name="tm"></param>
        /// <returns></returns>
        private int ValidateSubsequentChangeForDel(string table, string gmfidCol, string fidCol, string evtCol, string idCol, long gmfid, long eventId, long id, TransactionManager tm)
        {
            string sqlStr = "SELECT count(*) FROM " + table +
                " INNER JOIN tChgEvent on tChgEvent.heOID = " +
                evtCol + " and tChgEvent.heActive = 1" +
                " WHERE " + gmfidCol +
                " = @GMFID" +
                " and " + fidCol + " = 2" +
                " and " + evtCol + " <> @EVTID" +
                " and " + idCol + " > @ID";

            SqlCommand cmd = new SqlCommand(sqlStr, (SqlConnection)tm.Connection);
            cmd.Parameters.Add("@GMFID", SqlDbType.BigInt);
            cmd.Parameters["@GMFID"].Value = gmfid;
            cmd.Parameters.Add("@EVTID", SqlDbType.BigInt);
            cmd.Parameters["@EVTID"].Value = eventId;
            cmd.Parameters.Add("@ID", SqlDbType.BigInt);
            cmd.Parameters["@ID"].Value = id;
            cmd.Transaction = (SqlTransaction)tm.Transaction;
            string cnt = cmd.ExecuteScalar().ToString();
            return int.Parse(cnt);

        }

        private int ValidateSubsequentChangeForDelIndustry(string table, string gmfidCol, string fidCol, string evtCol, string idCol, long gmfid, long eventId, long id, TransactionManager tm, string typeCol, string type)
        {
            string sqlStr = "SELECT count(*) FROM " + table +
                " INNER JOIN tChgEvent on tChgEvent.heOID = " +
                evtCol + " and tChgEvent.heActive = 1" +
                " WHERE " + gmfidCol +
                " = @GMFID" +
                " and " + fidCol + " = 2" +
                " and " + evtCol + " <> @EVTID" +
                " and " + idCol + " > @ID" + " and ( " + typeCol + "Bef = @TYPE" + " or " + typeCol + "Aft = @TYPE )";

            SqlCommand cmd = new SqlCommand(sqlStr, (SqlConnection)tm.Connection);
            cmd.Parameters.Add("@GMFID", SqlDbType.BigInt);
            cmd.Parameters["@GMFID"].Value = gmfid;
            cmd.Parameters.Add("@EVTID", SqlDbType.BigInt);
            cmd.Parameters["@EVTID"].Value = eventId;
            cmd.Parameters.Add("@ID", SqlDbType.BigInt);
            cmd.Parameters["@ID"].Value = id;
            cmd.Parameters.Add("@TYPE", SqlDbType.VarChar);
            cmd.Parameters["@TYPE"].Value = type;
            cmd.Transaction = (SqlTransaction)tm.Transaction;
            string cnt = cmd.ExecuteScalar().ToString();
            return int.Parse(cnt);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="description"></param>
        /// <param name="tm"></param>
        /// <returns></returns>
        private string getListName(string description)
        {
            KSC.GMF.Data.GMFDB db = new KSC.GMF.Data.GMFDB();
            string sqlStr = "select lsName from tlist inner join tlistdata on lsID = ldListID and lddesc = @DES";

            SqlCommand cmd = new SqlCommand(sqlStr, (SqlConnection)db.Connection);
            cmd.Parameters.Add("@DES", SqlDbType.VarChar);
            cmd.Parameters["@DES"].Value = description;
            object o = cmd.ExecuteScalar();
            db.Dispose();
            if (null == o)
            {
                return null;
            }
            return o.ToString();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        private string getSubListDataForCountry(string countryName, string listName)
        {
            string selectedList = String.Empty;
            if (string.IsNullOrEmpty(countryName))
            {
                KSC.GMF.Data.GMFDB db = new KSC.GMF.Data.GMFDB();
                string sqlStr = "select TOP 1 ldValue from tlistdata inner join tlist on ldListId = lsid inner join (select top 1 hcacountryaft from  tchgcaddr where hcagmfid = @GMFID and hcafid = @FID and hcaTypeAft = @TYPE order by hcaid desc) t2 on t2.hcacountryaft = lddesc and lsName = @COUNTRY";
                SqlCommand cmd = new SqlCommand(sqlStr, (SqlConnection)db.Connection);
                cmd.Parameters.Add("@GMFID", SqlDbType.BigInt);
                cmd.Parameters["@GMFID"].Value = long.Parse(this.GMFID.Value);
                cmd.Parameters.Add("@FID", SqlDbType.BigInt);
                cmd.Parameters["@FID"].Value = 2;
                //cmd.Parameters.Add("@EVTID", SqlDbType.BigInt);
                //cmd.Parameters["@EVTID"].Value = long.Parse(this.EventID.Value);
                cmd.Parameters.Add("@TYPE", SqlDbType.VarChar);
                if (listName.Equals(ChangeReasonEditor.LEGAL_STRUCTURE) || listName.Equals(ChangeReasonEditor.RA_STATE))
                {
                    cmd.Parameters["@TYPE"].Value = "REGISTERED";
                }
                else
                {
                    cmd.Parameters["@TYPE"].Value = "PHYSICAL";
                }
                cmd.Parameters.Add("@COUNTRY", SqlDbType.VarChar);
                cmd.Parameters["@COUNTRY"].Value = "ISOCountry";
                object o = cmd.ExecuteScalar();
                db.Dispose();
                if (null == o)
                {
                    return null;
                }
                countryName = o.ToString();
                db.Dispose();
            }
            if (listName.Equals(ChangeReasonEditor.LEGAL_STRUCTURE))
            {
                selectedList = (!string.IsNullOrEmpty(countryName)) ? List.TranslateToSupplement3(List.ISOCOUNTRY_LISTNAME, countryName) : "EmptyList";
            }
            else if (listName.Equals(ChangeReasonEditor.RA_STATE) || listName.Equals(ChangeReasonEditor.PA_STATE))
            {
                selectedList = (!string.IsNullOrEmpty(countryName)) ? List.TranslateToSupplement2(List.ISOCOUNTRY_LISTNAME, countryName) : "EmptyList";
            }
            selectedList = selectedList ?? "EmptyList";
            return selectedList;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        private string getSubListDataForNAICS(string naicsValue)
        {
            string selectedList = String.Empty;
            if (string.IsNullOrEmpty(naicsValue))
            {
                if (long.Parse(this.GMFID.Value) > 0)
                {
                    KSC.GMF.Data.GMFDB db = new KSC.GMF.Data.GMFDB();
                    string sqlStr = "select TOP 1 hctcodeaft from tchgctype where hctgmfid = @GMFID and hctfid = @FID and hctTypeAft = @TYPE order by hctid desc";
                    SqlCommand cmd = new SqlCommand(sqlStr, (SqlConnection)db.Connection);
                    cmd.Parameters.Add("@GMFID", SqlDbType.BigInt);
                    cmd.Parameters["@GMFID"].Value = long.Parse(this.GMFID.Value);
                    cmd.Parameters.Add("@FID", SqlDbType.BigInt);
                    cmd.Parameters["@FID"].Value = 2;
                    cmd.Parameters.Add("@TYPE", SqlDbType.VarChar);
                    cmd.Parameters["@TYPE"].Value = "NAICS";
                    object o = cmd.ExecuteScalar();
                    db.Dispose();
                    if (null == o)
                    {
                        return null;
                    }
                    naicsValue = o.ToString();
                    db.Dispose();
                }
                else
                {
                    naicsValue = getComTypeCode(NAICS);
                }
            }

            selectedList = (!string.IsNullOrEmpty(naicsValue)) ? List.TranslateToSupplement1(List.NAICS_CODES, naicsValue) : "EmptyList";

            selectedList = selectedList ?? "EmptyList";
            return selectedList;

        }
        /// <summary>
        /// update all OID for change records
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="tm"></param>
        /// <param name="updateCat"></param>
        private void updateAllChangeRecordsForOID(long oid, long oldOid, TransactionManager tm, string updateCat)
        {
            switch (updateCat)
            {
                case "CompanyContactChange":
                    CompanyContactChangeCollection cccc = tm.CompanyContactChangeCollection;
                    cccc.GetByOID(oldOid);
                    ArrayList cccdatachangeList = cccc.CompanyContactChangeList;

                    for (int i = 0; i < cccdatachangeList.Count; i++)
                    {
                        CompanyContactChange data = (CompanyContactChange)cccdatachangeList[i];
                        data.CompanyContactID = oid;
                        cccc.Update(data);
                    }
                    cccc.Dispose();
                    break;

                case "CompanyUniqueIDChange":
                    CompanyUniqueIDChangeCollection cuic = tm.CompanyUniqueIDChangeCollection;
                    cuic.GetByOID(oldOid);
                    ArrayList cucdatachangeList = cuic.CompanyUniqueIDChangeList;

                    for (int i = 0; i < cucdatachangeList.Count; i++)
                    {
                        CompanyUniqueIDChange data = (CompanyUniqueIDChange)cucdatachangeList[i];
                        data.CompanyUniqueID = oid;
                        cuic.Update(data);
                    }
                    cuic.Dispose();
                    break;

                case "CompanyNameChange":
                    CompanyNameChangeCollection cncc = tm.CompanyNameChangeCollection;
                    cncc.GetByOID(oldOid);
                    ArrayList cncdatachangeList = cncc.CompanyNameChangeList;

                    for (int i = 0; i < cncdatachangeList.Count; i++)
                    {
                        CompanyNameChange data = (CompanyNameChange)cncdatachangeList[i];
                        data.CompanyNameID = oid;
                        cncc.Update(data);
                    }
                    cncc.Dispose();
                    break;

                case "CompanyServiceChange":
                    CompanyServiceChangeCollection cscc = tm.CompanyServiceChangeCollection;
                    cscc.GetByOID(oldOid);
                    ArrayList cscdatachangeList = cscc.CompanyServiceChangeList;

                    for (int i = 0; i < cscdatachangeList.Count; i++)
                    {
                        CompanyServiceChange data = (CompanyServiceChange)cscdatachangeList[i];
                        data.CompanyServiceID = oid;
                        cscc.Update(data);
                    }
                    cscc.Dispose();
                    break;

                case "CompanyTypeChange":
                    CompanyTypeChangeCollection ctcc = tm.CompanyTypeChangeCollection;
                    ctcc.GetByOID(oldOid);
                    ArrayList ctcdatachangeList = ctcc.CompanyTypeChangeList;

                    for (int i = 0; i < ctcdatachangeList.Count; i++)
                    {
                        CompanyTypeChange data = (CompanyTypeChange)ctcdatachangeList[i];
                        data.CompanyTypeID = oid;
                        ctcc.Update(data);
                    }
                    ctcc.Dispose();
                    break;

                case "CompanyAddressChange":
                    CompanyAddressChangeCollection addc = tm.CompanyAddressChangeCollection;
                    addc.GetByOID(oldOid);
                    ArrayList datachangeList = addc.CompanyAddressChangeList;

                    for (int i = 0; i < datachangeList.Count; i++)
                    {
                        CompanyAddressChange data = (CompanyAddressChange)datachangeList[i];
                        data.CompanyAddressID = oid;
                        addc.Update(data);
                    }
                    addc.Dispose();
                    break;

                //case "SecurityDataChange":
                //SecurityDataChangeCollection sdcc = tm.SecurityDataChangeCollection;
                //sdcc.GetByGMFID(oid);
                //ArrayList sdcdatachangeList = sdcc.SecurityDataChangeList;

                //for (int i = 0; i < sdcdatachangeList.Count; i++)
                //{
                //    SecurityDataChange data = (SecurityDataChange)sdcdatachangeList[i];
                //    data.SecurityGMFID = oid;
                //    sdcc.Update(data);
                //}
                //sdcc.Dispose();
                //break;

                case "SecurityExchangeChange":
                    SecurityExchangeChangeCollection secc = tm.SecurityExchangeChangeCollection;
                    secc.GetByOID(oldOid);
                    ArrayList secdatachangeList = secc.SecurityExchangeChangeList;

                    for (int i = 0; i < secdatachangeList.Count; i++)
                    {
                        SecurityExchangeChange data = (SecurityExchangeChange)secdatachangeList[i];
                        data.SecurityExchangeID = oid;
                        secc.Update(data);
                    }
                    secc.Dispose();
                    break;

                case "SecurityNameChange":
                    SecurityNameChangeCollection sncc = tm.SecurityNameChangeCollection;
                    sncc.GetByOID(oldOid);
                    ArrayList sncdatachangeList = sncc.SecurityNameChangeList;

                    for (int i = 0; i < sncdatachangeList.Count; i++)
                    {
                        SecurityNameChange data = (SecurityNameChange)sncdatachangeList[i];
                        data.SecurityNameID = oid;
                        sncc.Update(data);
                    }
                    sncc.Dispose();
                    break;

                case "SecurityUniqueIDChange":
                    SecurityUniqueIDChangeCollection suc = tm.SecurityUniqueIDChangeCollection;
                    suc.GetByOID(oldOid);
                    ArrayList sucdatachangeList = suc.SecurityUniqueIDChangeList;

                    for (int i = 0; i < sucdatachangeList.Count; i++)
                    {
                        SecurityUniqueIDChange data = (SecurityUniqueIDChange)sucdatachangeList[i];
                        data.SecurityUniqueID = oid;
                        suc.Update(data);
                    }
                    suc.Dispose();
                    break;

                case "RelationshipDataChange":
                    RelationshipDataChangeCollection rdc = tm.RelationshipDataChangeCollection;
                    rdc.GetByGMFID(oldOid);
                    ArrayList rdcdatachangeList = rdc.RelationshipDataChangeList;

                    for (int i = 0; i < rdcdatachangeList.Count; i++)
                    {
                        RelationshipDataChange data = (RelationshipDataChange)rdcdatachangeList[i];
                        data.RelationshipGMFID = oid;
                        rdc.Update(data);
                    }
                    rdc.Dispose();
                    break;
            }
        }

        /// <summary>
        /// Save edited new value
        /// </summary>
        /// <param name="ce"></param>
        /// <param name="tm"></param>
        private bool saveUpdateValues(ChangeEvent ce, TransactionManager tm, DataTable allEventsData)
        {
            JavaScriptSerializer jsSer = new JavaScriptSerializer();
            Dictionary<string, List<UpdatedValuesEntity>> dataMap = jsSer.Deserialize<Dictionary<string, List<UpdatedValuesEntity>>>(this.UpdatedValues.Value);
            //List<bool> addressChanged = jsSer.Deserialize<List<bool>>(this.AddressChanged.Value);
            bool ifAddressChanged = false;
            bool ifResAddressChanged = false;
            string phsicalStr = string.Empty;
            string regStr = string.Empty;
            string newPhsicalStr = string.Empty;
            string newRegStr = string.Empty;

            long allEvntId = 0;
            Dictionary<string, List<long>> newIdMap = new Dictionary<string, List<long>>();
            Dictionary<string, List<int>> inputValuesMap = jsSer.Deserialize<Dictionary<string, List<int>>>(this.InputValues.Value);
            bool ifChanged = false;
            int companyDataCnt = 0;
            List<string> skipType = new List<string>();
            bool naicsChanged = false;

            Dictionary<string, List<List<object>>> validateIds = new Dictionary<string, List<List<object>>>();
            Dictionary<string, bool> dynamicNaicssubMap = jsSer.Deserialize<Dictionary<string, bool>>(this.DynamicNaicssub.Value);

            foreach (string key in dataMap.Keys)
            {
                List<UpdatedValuesEntity> lst = dataMap[key];
                List<int> inputLst = inputValuesMap[key];
                List<List<string>> allInputLst = getInputValues();
                UpdatedValuesEntity update = null;
                for (int i = 0; i < lst.Count; i++)
                {
                    update = (UpdatedValuesEntity)lst[i];
                    string inputValue = allInputLst[inputLst[i]][0];

                    if (update.item.Contains("Percentage") && inputValue.Length < 10)
                    {
                        inputValue = inputValue.PadRight(10, '0');
                    }
                    if (!inputValue.Equals(update.updatedValue))
                    {
                        ifChanged = true;
                    }

                    if (PHSICAL_ADDRESS_COUNTRY.Equals(update.item))
                    {
                        phsicalStr = phsicalStr + update.beforeValue;
                        newPhsicalStr = newPhsicalStr + inputValue;

                    }
                    else if (REGISTRY_ADDRESS_COUNTRY.Equals(update.item))
                    {
                        regStr = regStr + update.beforeValue;
                        newRegStr = newRegStr + inputValue;
                    }
                    else if (PHSICAL_ADDRESS_STATE.Equals(update.item))
                    {
                        phsicalStr = phsicalStr + update.beforeValue;
                        newPhsicalStr = newPhsicalStr + inputValue;

                    }
                    else if (REGISTRY_ADDRESS_STATE.Equals(update.item))
                    {
                        regStr = regStr + update.beforeValue;
                        newRegStr = newRegStr + inputValue;
                    }
                    else if (PHSICAL_ADDRESS_LINE_ONE.Equals(update.item))
                    {
                        phsicalStr = phsicalStr + update.beforeValue;
                        newPhsicalStr = newPhsicalStr + inputValue;


                    }
                    else if (REGISTRY_ADDRESS_LINE_ONE.Equals(update.item))
                    {
                        regStr = regStr + update.beforeValue;
                        newRegStr = newRegStr + inputValue;

                    }
                    else if (PHSICAL_ADDRESS_LINE_TWO.Equals(update.item))
                    {
                        phsicalStr = phsicalStr + update.beforeValue;
                        newPhsicalStr = newPhsicalStr + inputValue;


                    }
                    else if (REGISTRY_ADDRESS_LINE_TWO.Equals(update.item))
                    {
                        regStr = regStr + update.beforeValue;
                        newRegStr = newRegStr + inputValue;

                    }
                    else if (PHSICAL_ADDRESS_CITY.Equals(update.item))
                    {
                        phsicalStr = phsicalStr + update.beforeValue;
                        newPhsicalStr = newPhsicalStr + inputValue;

                    }
                    else if (REGISTRY_ADDRESS_CITY.Equals(update.item))
                    {
                        regStr = regStr + update.beforeValue;
                        newRegStr = newRegStr + inputValue;

                    }
                    else if (PHSICAL_ADDRESS_PC.Equals(update.item))
                    {
                        phsicalStr = phsicalStr + update.beforeValue;
                        newPhsicalStr = newPhsicalStr + inputValue;

                    }
                    else if (REGISTRY_ADDRESS_PC.Equals(update.item))
                    {

                        regStr = regStr + update.beforeValue;
                        newRegStr = newRegStr + inputValue;
                    }
                    else if (LEGAL_STRUCTURE.Equals(update.item))
                    {

                        regStr = regStr + update.beforeValue;
                        newRegStr = newRegStr + inputValue;
                    }
                }

                if (!regStr.Equals(newRegStr))
                {
                    ifResAddressChanged = true;
                }
                if (!phsicalStr.Equals(newPhsicalStr))
                {
                    ifAddressChanged = true;
                }
            }
            if (!ifChanged && bool.Parse(IsIndustryTypeNoSeq.Value))
            {
                foreach (DataGridItem eachRow in revchgEvents.Items)
                {
                    if (eachRow.Cells[1].Text.Equals(eachRow.Cells[2].Text) && eachRow.Cells[0].Text.Contains("Industry Type"))
                    {
                        if (ChangeReasonEditor.IND_NAICS_SUB.Equals(eachRow.Cells[0].Text) && !dynamicNaicssubMap["isNaicssubVisible"])
                        {
                            continue;
                        }
                        ListDataDropDown psList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                        string value = psList.SelectedValue;
                        if (DEFAULT_STRING.Equals(value))
                        {
                            value = "";
                        }
                        //Find children is changed or not which are not in change.
                        if (!eachRow.Cells[2].Text.Equals(value))
                        {
                            ifChanged = true;
                            break;
                        }
                    }
                }
            }

            if (ifChanged)
            {
                foreach (string key in dataMap.Keys)
                {
                    List<UpdatedValuesEntity> lst = dataMap[key];
                    List<int> inputLst = inputValuesMap[key];
                    List<List<string>> allInputLst = getInputValues();
                    UpdatedValuesEntity update = (UpdatedValuesEntity)lst[0];
                    object ccdata = null;
                    object ccc = null;

                    if (COMPANY_ORDER_DATA.Equals(update.attribute))
                    {
                        ccc = (object)tm.CompanyDataCollection;
                        CompanyDataCollection tmp = (CompanyDataCollection)ccc;
                        ccdata = (object)(tmp.GetByPrimaryKey(update.gmfId, update.fid));
                        companyDataCnt = companyDataCnt + 1;
                    }
                    else if (COMPANY_ORDER_NAME.Equals(update.attribute))
                    {
                        ccc = (object)tm.CompanyNameCollection;
                        CompanyNameCollection tmp = (CompanyNameCollection)ccc;
                        ccdata = (object)(tmp.GetByPrimaryKey(update.masterKey));
                        tmp.Dispose();
                    }
                    else if (COMPANY_ORDER_ADDR.Equals(update.attribute))
                    {
                        ccc = (object)tm.CompanyAddressCollection;
                        CompanyAddressCollection tmp = (CompanyAddressCollection)ccc;
                        ccdata = (object)(tmp.GetByPrimaryKey(update.masterKey));
                        tmp.Dispose();
                    }
                    else if (COMPANY_ORDER_CONT.Equals(update.attribute))
                    {
                        ccc = (object)tm.CompanyContactCollection;
                        CompanyContactCollection tmp = (CompanyContactCollection)ccc;
                        ccdata = (object)(tmp.GetByPrimaryKey(update.masterKey));
                        tmp.Dispose();
                    }
                    else if (COMPANY_ORDER_SVC.Equals(update.attribute))
                    {
                        ccc = (object)tm.CompanyServiceCollection;
                        CompanyServiceCollection tmp = (CompanyServiceCollection)ccc;
                        ccdata = (object)(tmp.GetByPrimaryKey(update.masterKey));
                        tmp.Dispose();
                    }
                    else if (COMPANY_ORDER_TYPE.Equals(update.attribute))
                    {
                        ccc = (object)tm.CompanyTypeCollection;
                        CompanyTypeCollection tmp = (CompanyTypeCollection)ccc;
                        ccdata = (object)(tmp.GetByPrimaryKey(update.masterKey));
                        tmp.Dispose();
                    }
                    else if (COMPANY_ORDER_UNQI.Equals(update.attribute))
                    {
                        ccc = (object)tm.CompanyUniqueIDCollection;
                        CompanyUniqueIDCollection tmp = (CompanyUniqueIDCollection)ccc;
                        ccdata = (object)(tmp.GetByPrimaryKey(update.masterKey));
                        tmp.Dispose();
                    }
                    else if (SECURITY_ORDER_DATA.Equals(update.attribute))
                    {
                        ccc = (object)tm.SecurityDataCollection;
                        SecurityDataCollection tmp = (SecurityDataCollection)ccc;
                        ccdata = (object)(tmp.GetByPrimaryKey(update.gmfId, update.fid));
                        tmp.Dispose();
                    }
                    else if (RELATIONSHIP_ORDER_DATA.Equals(update.attribute))
                    {
                        ccc = (object)tm.RelationshipDataCollection;
                        RelationshipDataCollection tmp = (RelationshipDataCollection)ccc;
                        ccdata = (object)(tmp.GetByPrimaryKey(update.gmfId, update.fid));
                        tmp.Dispose();
                    }

                    // After saving,a popup window opens to let the researcher know that the proper action 
                    // would be to revert the change and then edit the existing identifier value.
                    if (null == ccdata)
                    {
                        // There's a case GICS/NAICSSUB/STRUCTURE is from value to null and in latest record GICS is forced to show with NAICS and choose null.
                        // A   null  null
                        if (ChangeReasonEditor.IND_GICS.Equals(update.item) || ChangeReasonEditor.IND_NAICS_SUB.Equals(update.item) || ChangeReasonEditor.LEGAL_STRUCTURE.Equals(update.item))
                        {
                            if (string.IsNullOrEmpty(allInputLst[inputLst[0]][0]))
                            {
                                string skiType = update.item.Substring(16);
                                if (ChangeReasonEditor.LEGAL_STRUCTURE.Equals(update.item))
                                {
                                    skiType = "STRUCTURE";
                                }
                                skipType.Add(skiType);
                                continue;
                            }
                        }
                        ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Please revert the change and then edit the existing value.');", true);
                        return false;
                    }

                    for (int i = 0; i < lst.Count; i++)
                    {
                        update = (UpdatedValuesEntity)lst[i];
                        string inputValue = allInputLst[inputLst[i]][0];
                        string inputCode = string.Empty;
                        if (allInputLst[inputLst[i]].Count > 1)
                        {
                            inputCode = allInputLst[inputLst[i]][1];
                        }

                        if (COMPANY_ORDER_NAME.Equals(update.attribute))
                        {
                            if (inputValue.Equals(update.beforeValue))
                            {
                                ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Name change could not be made due to new value equals old value.');", true);
                                return false;
                            }
                            ((CompanyName)ccdata).Name = inputValue;

                            if (!validateIds.ContainsKey(COMPANY_ORDER_NAME))
                            {
                                List<List<object>> list = new List<List<object>>();
                                List<object> longList = new List<object>();
                                longList.Add(update.masterKey);
                                list.Add(longList);
                                validateIds.Add(COMPANY_ORDER_NAME, list);
                            }
                            else
                            {
                                List<List<object>> list = validateIds[COMPANY_ORDER_NAME];
                                List<object> longList = new List<object>();
                                longList.Add(update.masterKey);
                                list.Add(longList);
                                validateIds.Remove(COMPANY_ORDER_NAME);
                                validateIds.Add(COMPANY_ORDER_NAME, list);
                            }
                        }
                        else if (COMPANY_ORDER_ADDR.Equals(update.attribute))
                        {
                            if (PHSICAL_ADDRESS_COUNTRY.Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue) && !ifAddressChanged)
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Physical Country change could not be made due to new value equals old value.');", true);
                                    return false;
                                }

                                ((CompanyAddress)ccdata).Country = inputValue;
                                ((CompanyAddress)ccdata).CountryCode = inputCode;

                            }
                            else if (REGISTRY_ADDRESS_COUNTRY.Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue) && !ifResAddressChanged)
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Registered Country change could not be made due to new value equals old value.');", true);
                                    return false;
                                }
                                ((CompanyAddress)ccdata).Country = inputValue;
                                ((CompanyAddress)ccdata).CountryCode = inputCode;

                            }
                            else if (PHSICAL_ADDRESS_STATE.Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue) && !ifAddressChanged)
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Physical StateProvinceName change could not be made due to new value equals old value.');", true);
                                    return false;
                                }

                                ((CompanyAddress)ccdata).StateProvinceName = inputValue;
                                ((CompanyAddress)ccdata).StateProvinceCode = inputCode;
                                ((CompanyAddress)ccdata).StateProvinceAbbrev = inputCode;
                            }
                            else if (REGISTRY_ADDRESS_STATE.Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue) && !ifResAddressChanged)
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Registered StateProvinceName change could not be made due to new value equals old value.');", true);
                                    return false;
                                }

                                ((CompanyAddress)ccdata).StateProvinceName = inputValue;
                                ((CompanyAddress)ccdata).StateProvinceCode = inputCode;
                                ((CompanyAddress)ccdata).StateProvinceAbbrev = inputCode;
                            }
                            else if (PHSICAL_ADDRESS_LINE_ONE.Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue) && !ifAddressChanged)
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Physical Address1 change could not be made due to new value equals old value.');", true);
                                    return false;
                                }

                                ((CompanyAddress)ccdata).Address1 = inputValue;

                            }
                            else if (REGISTRY_ADDRESS_LINE_ONE.Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue) && !ifResAddressChanged)
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Registered Address1 change could not be made due to new value equals old value.');", true);
                                    return false;
                                }

                                ((CompanyAddress)ccdata).Address1 = inputValue;

                            }
                            else if (PHSICAL_ADDRESS_LINE_TWO.Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue) && !ifAddressChanged)
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Physical Address2 change could not be made due to new value equals old value.');", true);
                                    return false;
                                }

                                ((CompanyAddress)ccdata).Address2 = inputValue;

                            }
                            else if (REGISTRY_ADDRESS_LINE_TWO.Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue) && !ifResAddressChanged)
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Registered Address2 change could not be made due to new value equals old value.');", true);
                                    return false;
                                }

                                ((CompanyAddress)ccdata).Address2 = inputValue;

                            }
                            else if (PHSICAL_ADDRESS_CITY.Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue) && !ifAddressChanged)
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Physical City change could not be made due to new value equals old value.');", true);
                                    return false;
                                }

                                ((CompanyAddress)ccdata).City = inputValue;

                            }
                            else if (REGISTRY_ADDRESS_CITY.Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue) && !ifResAddressChanged)
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Registered City change could not be made due to new value equals old value.');", true);
                                    return false;
                                }

                                ((CompanyAddress)ccdata).City = inputValue;

                            }
                            else if (PHSICAL_ADDRESS_PC.Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue) && !ifAddressChanged)
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Physical PostalCode change could not be made due to new value equals old value.');", true);
                                    return false;
                                }

                                ((CompanyAddress)ccdata).PostalCode = inputValue;

                            }
                            else if (REGISTRY_ADDRESS_PC.Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue) && !ifResAddressChanged)
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Registered PostalCode change could not be made due to new value equals old value.');", true);
                                    return false;
                                }

                                ((CompanyAddress)ccdata).PostalCode = inputValue;

                            }

                            if (!validateIds.ContainsKey(COMPANY_ORDER_ADDR))
                            {
                                List<List<object>> list = new List<List<object>>();
                                List<object> longList = new List<object>();
                                longList.Add(update.masterKey);
                                list.Add(longList);
                                validateIds.Add(COMPANY_ORDER_ADDR, list);
                            }
                            else
                            {
                                List<List<object>> list = validateIds[COMPANY_ORDER_ADDR];
                                List<object> longList = new List<object>();
                                longList.Add(update.masterKey);
                                list.Add(longList);
                                validateIds.Remove(COMPANY_ORDER_ADDR);
                                validateIds.Add(COMPANY_ORDER_ADDR, list);
                            }
                        }
                        else if (COMPANY_ORDER_CONT.Equals(update.attribute))
                        {
                            if (inputValue.Equals(update.beforeValue))
                            {
                                ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Contact change could not be made due to new value equals old value.');", true);
                                return false;
                            }

                            ((CompanyContact)ccdata).Method = inputValue;

                            if (!validateIds.ContainsKey(COMPANY_ORDER_CONT))
                            {
                                List<List<object>> list = new List<List<object>>();
                                List<object> longList = new List<object>();
                                longList.Add(update.masterKey);
                                list.Add(longList);
                                validateIds.Add(COMPANY_ORDER_CONT, list);
                            }
                            else
                            {
                                List<List<object>> list = validateIds[COMPANY_ORDER_CONT];
                                List<object> longList = new List<object>();
                                longList.Add(update.masterKey);
                                list.Add(longList);
                                validateIds.Remove(COMPANY_ORDER_CONT);
                                validateIds.Add(COMPANY_ORDER_CONT, list);
                            }

                        }
                        else if (COMPANY_ORDER_SVC.Equals(update.attribute))
                        {
                            if (inputValue.Equals(update.beforeValue))
                            {
                                ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Service change could not be made due to new value equals old value.');", true);
                                return false;
                            }

                            ((CompanyService)ccdata).Name = inputValue;

                            if (!validateIds.ContainsKey(COMPANY_ORDER_SVC))
                            {
                                List<List<object>> list = new List<List<object>>();
                                List<object> longList = new List<object>();
                                longList.Add(update.masterKey);
                                list.Add(longList);
                                validateIds.Add(COMPANY_ORDER_SVC, list);
                            }
                            else
                            {
                                List<List<object>> list = validateIds[COMPANY_ORDER_SVC];
                                List<object> longList = new List<object>();
                                longList.Add(update.masterKey);
                                list.Add(longList);
                                validateIds.Remove(COMPANY_ORDER_SVC);
                                validateIds.Add(COMPANY_ORDER_SVC, list);
                            }

                        }
                        else if (COMPANY_ORDER_TYPE.Equals(update.attribute))
                        {
                            if (LEGAL_STRUCTURE.Equals(update.item) && !ifResAddressChanged)
                            {
                                if (inputValue.Equals(update.beforeValue))
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Legal Structure change could not be made due to new value equals old value.');", true);
                                    return false;
                                }
                            }
                            else if (!LEGAL_STRUCTURE.Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue))
                                {
                                    // 1.Skip NAICSSUB in case original new update in edit new value.
                                    //                           null    A   null   result by NAICS without sub to another without sub.
                                    // 2.If NAICS changed then see child types as changed.
                                    if (!((ChangeReasonEditor.IND_NAICS_SUB.Equals(update.item) && !dynamicNaicssubMap["isNaicssubVisible"]) || naicsChanged))
                                    {
                                        ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Industry Type change could not be made due to new value equals old value.');", true);
                                        return false;
                                    }
                                }
                                else if (ChangeReasonEditor.IND_NAICS.Equals(update.item))
                                {
                                    naicsChanged = true;
                                }
                            }

                            ((CompanyType)ccdata).Code = inputValue;
                            // Don't process change like original new update in edit new value.
                            //                              A      B     A      which final result is A to A.
                            // Treat it as no change,just update data in tComType table.
                            if (!update.beforeValue.Equals(inputValue))
                            {
                                if (!validateIds.ContainsKey(COMPANY_ORDER_TYPE))
                                {
                                    List<List<object>> list = new List<List<object>>();
                                    List<object> longList = new List<object>();
                                    longList.Add(update.masterKey);
                                    list.Add(longList);
                                    validateIds.Add(COMPANY_ORDER_TYPE, list);
                                }
                                else
                                {
                                    List<List<object>> list = validateIds[COMPANY_ORDER_TYPE];
                                    List<object> longList = new List<object>();
                                    longList.Add(update.masterKey);
                                    list.Add(longList);
                                    validateIds.Remove(COMPANY_ORDER_TYPE);
                                    validateIds.Add(COMPANY_ORDER_TYPE, list);
                                }
                            }
                        }
                        else if (COMPANY_ORDER_UNQI.Equals(update.attribute))
                        {
                            if ("Type".Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue))
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Unique Type change could not be made due to new value equals old value.');", true);
                                    return false;
                                }


                                ((CompanyUniqueID)ccdata).Type = inputValue;

                            }
                            else if ("Code".Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue))
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Unique Code change could not be made due to new value equals old value.');", true);
                                    return false;
                                }

                                ((CompanyUniqueID)ccdata).Code = inputValue;

                            }

                            if (!validateIds.ContainsKey(COMPANY_ORDER_UNQI))
                            {
                                List<List<object>> list = new List<List<object>>();
                                List<object> longList = new List<object>();
                                longList.Add(update.masterKey);
                                list.Add(longList);
                                validateIds.Add(COMPANY_ORDER_UNQI, list);
                            }
                            else
                            {
                                List<List<object>> list = validateIds[COMPANY_ORDER_UNQI];
                                List<object> longList = new List<object>();
                                longList.Add(update.masterKey);
                                list.Add(longList);
                                validateIds.Remove(COMPANY_ORDER_UNQI);
                                validateIds.Add(COMPANY_ORDER_UNQI, list);
                            }
                        }
                        else if (COMPANY_ORDER_DATA.Equals(update.attribute))
                        {
                            if ("Expired".Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue))
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Expired change could not be made due to new value equals old value.');", true);
                                    return false;
                                }
                                ((CompanyData)ccdata).Expired = bool.Parse(inputValue);

                            }
                            else if ("ExpiredDate".Equals(update.item))
                            {
                                if (string.IsNullOrEmpty(inputValue))
                                {
                                    ((CompanyData)ccdata).IsExpiredDateNull = true;

                                }
                                else
                                {

                                    ((CompanyData)ccdata).ExpiredDate = DateTime.Parse(inputValue);

                                }
                            }
                            else if ("ExpiredReason".Equals(update.item))
                            {

                                if (inputValue.Equals(update.beforeValue))
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('ExpiredReason change could not be made due to new value equals old value.');", true);
                                    return false;
                                }

                                ((CompanyData)ccdata).ExpiredReason = inputValue;

                            }
                            else if ("IsBranch".Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue))
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('IsBranch change could not be made due to new value equals old value.');", true);
                                    return false;
                                }

                                if (string.IsNullOrEmpty(inputValue))
                                {
                                    ((CompanyData)ccdata).IsIsBranchNull = true;
                                }
                                else
                                {
                                    ((CompanyData)ccdata).IsBranch = bool.Parse(inputValue);
                                }

                            }
                            else if ("IsPublic".Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue))
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('IsPublic change could not be made due to new value equals old value.');", true);
                                    return false;
                                }

                                if (string.IsNullOrEmpty(inputValue))
                                {
                                    ((CompanyData)ccdata).IsIsPublicNull = true;
                                }
                                else
                                {
                                    ((CompanyData)ccdata).IsPublic = bool.Parse(inputValue);
                                }

                            }
                            else if ("IsIssuer".Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue))
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('IsIssuer change could not be made due to new value equals old value.');", true);
                                    return false;
                                }



                                if (string.IsNullOrEmpty(inputValue))
                                {
                                    ((CompanyData)ccdata).IsIsIssuerNull = true;
                                }
                                else
                                {
                                    ((CompanyData)ccdata).IsIssuer = bool.Parse(inputValue);
                                }

                            }
                            else if ("FiscalYearEnd".Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue))
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('FiscalYearEnd change could not be made due to new value equals old value.');", true);
                                    return false;
                                }


                                ((CompanyData)ccdata).FiscalYearEnd = inputValue;

                            }
                            else if ("Status".Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue))
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Status change could not be made due to new value equals old value.');", true);
                                    return false;
                                }

                                ((CompanyData)ccdata).Status = inputValue;

                            }
                            if (!validateIds.ContainsKey(COMPANY_ORDER_DATA))
                            {
                                List<List<object>> list = new List<List<object>>();
                                List<object> longList = new List<object>();
                                longList.Add(update.gmfId);
                                longList.Add(update.fid);
                                list.Add(longList);
                                validateIds.Add(COMPANY_ORDER_DATA, list);
                            }
                            else
                            {
                                List<List<object>> list = validateIds[COMPANY_ORDER_DATA];
                                List<object> longList = new List<object>();
                                longList.Add(update.gmfId);
                                longList.Add(update.fid);
                                list.Add(longList);
                                validateIds.Remove(COMPANY_ORDER_DATA);
                                validateIds.Add(COMPANY_ORDER_DATA, list);
                            }

                        }
                        else if (SECURITY_ORDER_DATA.Equals(update.attribute))
                        {
                            if ("Expired".Equals(update.item))
                            {
                                if ((inputValue.Equals(update.beforeValue)) || (string.Empty.Equals(update.beforeValue) && "Clear KSC Status".Equals(inputValue)))
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Expired change could not be made due to new value equals old value.');", true);
                                    return false;
                                }

                                if ("Clear KSC Status".Equals(inputValue))
                                {
                                    ((SecurityData)ccdata).IsExpiredNull = true;
                                }
                                else
                                {
                                    ((SecurityData)ccdata).Expired = bool.Parse(inputValue);
                                }
                            }
                            else if ("ExpiredDate".Equals(update.item))
                            {
                                if (string.IsNullOrEmpty(inputValue))
                                {
                                    ((SecurityData)ccdata).IsExpiredDateNull = true;

                                }
                                else
                                {

                                    ((SecurityData)ccdata).ExpiredDate = DateTime.Parse(inputValue);

                                }
                            }
                            else if ("ExpiredReason".Equals(update.item))
                            {

                                if (inputValue.Equals(update.beforeValue))
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('ExpiredReason change could not be made due to new value equals old value.');", true);
                                    return false;
                                }

                                ((SecurityData)ccdata).ExpiredReason = inputValue;

                            }
                            else if ("Security Class".Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue))
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Security Class could not be made due to new value equals old value.');", true);
                                    return false;
                                }
                                ((SecurityData)ccdata).Class = inputValue;

                            }
                            else if ("Security Type".Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue))
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Security Class Doesn't match to Type.');", true);
                                    return false;
                                }
                                ((SecurityData)ccdata).Type = inputValue;

                            }
                            else if ("Default Value".Equals(update.item))
                            {
                                if (inputValue.Equals(update.beforeValue))
                                {
                                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Default Value change could not be made due to new value equals old value.');", true);
                                    return false;
                                }

                                ((SecurityData)ccdata).DefaultValue = bool.Parse(inputValue);

                            }
                            if (!validateIds.ContainsKey(SECURITY_ORDER_DATA))
                            {
                                List<List<object>> list = new List<List<object>>();
                                List<object> longList = new List<object>();
                                longList.Add(update.gmfId);
                                longList.Add(update.fid);
                                list.Add(longList);
                                validateIds.Add(SECURITY_ORDER_DATA, list);
                            }
                            else
                            {
                                List<List<object>> list = validateIds[SECURITY_ORDER_DATA];
                                List<object> longList = new List<object>();
                                longList.Add(update.gmfId);
                                longList.Add(update.fid);
                                list.Add(longList);
                                validateIds.Remove(SECURITY_ORDER_DATA);
                                validateIds.Add(SECURITY_ORDER_DATA, list);
                            }
                        }
                        else if (RELATIONSHIP_ORDER_DATA.Equals(update.attribute))
                        {
                            if (inputValue.Equals(update.beforeValue))
                            {
                                ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Name change could not be made due to new value equals old value.');", true);
                                return false;
                            }

                            if (string.IsNullOrEmpty(inputValue))
                            {
                                ((RelationshipData)ccdata).IsPercentNull = true;
                            }
                            else
                            {
                                ((RelationshipData)ccdata).Percent = decimal.Parse(inputValue);
                            }
                            if (!validateIds.ContainsKey(RELATIONSHIP_ORDER_DATA))
                            {
                                List<List<object>> list = new List<List<object>>();
                                List<object> longList = new List<object>();
                                longList.Add(update.gmfId);
                                longList.Add(update.fid);
                                list.Add(longList);
                                validateIds.Add(RELATIONSHIP_ORDER_DATA, list);
                            }
                            else
                            {
                                List<List<object>> list = validateIds[RELATIONSHIP_ORDER_DATA];
                                List<object> longList = new List<object>();
                                longList.Add(update.gmfId);
                                longList.Add(update.fid);
                                list.Add(longList);
                                validateIds.Remove(RELATIONSHIP_ORDER_DATA);
                                validateIds.Add(RELATIONSHIP_ORDER_DATA, list);
                            }
                        }
                    }

                    if (allEvntId == 0)
                    {
                        ce.ID = 0;
                    }
                    else
                    {
                        ChangeEventCollection ceCol = tm.ChangeEventCollection;
                        ce = ceCol.GetByPrimaryKey(allEvntId);
                        ceCol.Dispose();
                    }

                    List<long> idlst = new List<long>();
                    if (COMPANY_ORDER_NAME.Equals(update.attribute))
                    {
                        CompanyNameCollection updc = (CompanyNameCollection)ccc;
                        CompanyName data = (CompanyName)ccdata;
                        idlst = updc.UpdateInput(data, ce, tm);
                        updc.Dispose();
                        CompanyDataCollection cdc = tm.CompanyDataCollection;
                        CompanyData comdata = cdc.GetByPrimaryKey(long.Parse(this.GMFID.Value), 2);
                        if (null != comdata)
                        {
                            comdata.LegalName = data.Name;
                            cdc.Update(comdata, ce, tm);
                            cdc.Dispose();
                        }
                    }
                    else if (COMPANY_ORDER_ADDR.Equals(update.attribute))
                    {
                        CompanyAddressCollection updc = (CompanyAddressCollection)ccc;
                        CompanyAddress data = (CompanyAddress)ccdata;
                        idlst = updc.UpdateInput(data, ce, tm);
                        updc.Dispose();
                    }
                    else if (COMPANY_ORDER_CONT.Equals(update.attribute))
                    {
                        CompanyContactCollection updc = (CompanyContactCollection)ccc;
                        CompanyContact data = (CompanyContact)ccdata;
                        idlst = updc.UpdateInput(data, ce, tm);
                        updc.Dispose();
                    }
                    else if (COMPANY_ORDER_DATA.Equals(update.attribute))
                    {
                        CompanyDataCollection updc = (CompanyDataCollection)ccc;
                        CompanyData data = (CompanyData)ccdata;
                        idlst = updc.UpdateInput(data, ce, tm);
                    }
                    else if (COMPANY_ORDER_SVC.Equals(update.attribute))
                    {
                        CompanyServiceCollection updc = (CompanyServiceCollection)ccc;
                        CompanyService data = (CompanyService)ccdata;
                        idlst = updc.UpdateInput(data, ce, tm);
                        updc.Dispose();
                    }
                    else if (COMPANY_ORDER_TYPE.Equals(update.attribute))
                    {
                        CompanyTypeCollection updc = (CompanyTypeCollection)ccc;
                        CompanyType data = (CompanyType)ccdata;
                        if (update.beforeValue.Equals(data.Code))
                        {
                            if (string.IsNullOrEmpty(update.beforeValue))
                            {
                                //Delete tComType for case like original new update in edit new value.
                                //                                 null   A     null    which final result is null to null. 
                                idlst = null;
                                updc.Delete(data);
                            }
                            else
                            {
                                //Update tComType for case like original new update in edit new value.
                                //                                 A      B     A      which final result is A to A. 
                                idlst = null;
                                updc.Update(data);
                            }
                        }
                        else if (string.IsNullOrEmpty(data.Code))
                        {
                            //Treat update to null as delete.
                            idlst = updc.DeleteInput(data, ce, tm);
                        }
                        else
                        {
                            idlst = updc.UpdateInput(data, ce, tm);
                        }
                        updc.Dispose();
                    }
                    else if (COMPANY_ORDER_UNQI.Equals(update.attribute))
                    {
                        CompanyUniqueIDCollection updc = (CompanyUniqueIDCollection)ccc;
                        CompanyUniqueID data = (CompanyUniqueID)ccdata;
                        idlst = updc.UpdateInput(data, ce, tm);
                        updc.Dispose();
                    }
                    else if (SECURITY_ORDER_DATA.Equals(update.attribute))
                    {
                        SecurityDataCollection updc = (SecurityDataCollection)ccc;
                        SecurityData data = (SecurityData)ccdata;
                        idlst = updc.UpdateInput(data, ce, tm);
                    }
                    else if (RELATIONSHIP_ORDER_DATA.Equals(update.attribute))
                    {
                        RelationshipDataCollection updc = (RelationshipDataCollection)ccc;
                        RelationshipData data = (RelationshipData)ccdata;
                        idlst = updc.UpdateInput(data, ce, tm);
                    }

                    if (null != idlst)
                    {
                        allEvntId = idlst[0];
                    }
                    if (!newIdMap.ContainsKey(key))
                    {
                        if (null != idlst)
                        {
                            newIdMap.Add(key, idlst);
                        }
                    }

                }

                if (!processMandatoryChildTypes(ce, tm, skipType))
                {
                    return false;
                }

                foreach (string key in dataMap.Keys)
                {

                    List<UpdatedValuesEntity> lst = dataMap[key];
                    List<int> inputLst = inputValuesMap[key];
                    string attr = lst[0].attribute;
                    if (COMPANY_ORDER_NAME.Equals(attr))
                    {
                        CompanyNameChangeCollection cncc = tm.CompanyNameChangeCollection;
                        CompanyNameChange cnc = new CompanyNameChange();
                        if (!newIdMap.ContainsKey(key))
                        {
                            cncc.Dispose();
                            continue;
                        }
                        List<long> idlst = newIdMap[key];
                        allEvntId = idlst[0];
                        long changeId = idlst[1];
                        cnc = cncc.GetByPrimaryKey(changeId);
                        long oldMasterKey = 0;
                        for (int i = 0; i < lst.Count; i++)
                        {
                            UpdatedValuesEntity update = (UpdatedValuesEntity)lst[i];
                            cnc.NameBefore = update.beforeValue;
                            if (validateIds.ContainsKey(COMPANY_ORDER_NAME))
                            {
                                List<List<object>> list = validateIds[COMPANY_ORDER_NAME];
                                for (int x = 0; x < list.Count; x++)
                                {
                                    List<object> lonList = list[x];
                                    if (oldMasterKey == update.masterKey)
                                    {
                                        lonList.Add(cnc.ID);
                                        lonList.Add(cnc.ChangeEventID);
                                        lonList.Add("tChgCName");
                                        lonList.Add("hcnOID");
                                        lonList.Add("hcnEventID");
                                        lonList.Add("hcnID");
                                    }
                                    else
                                    {
                                        if (lonList[0].Equals(update.masterKey))
                                        {
                                            lonList.Add(cnc.ID);
                                            lonList.Add(cnc.ChangeEventID);
                                            lonList.Add("tChgCName");
                                            lonList.Add("hcnOID");
                                            lonList.Add("hcnEventID");
                                            lonList.Add("hcnID");
                                        }
                                    }
                                }
                                oldMasterKey = update.masterKey;
                            }
                        }
                        cncc.Update(cnc);
                        cncc.Dispose();
                    }
                    else if (COMPANY_ORDER_CONT.Equals(attr))
                    {
                        CompanyContactChangeCollection cncc = tm.CompanyContactChangeCollection;
                        CompanyContactChange cnc = new CompanyContactChange();
                        if (!newIdMap.ContainsKey(key))
                        {
                            cncc.Dispose();
                            continue;
                        }
                        List<long> idlst = newIdMap[key];
                        allEvntId = idlst[0];
                        long changeId = idlst[1];
                        cnc = cncc.GetByPrimaryKey(changeId);
                        long oldMasterKey = 0;
                        for (int i = 0; i < lst.Count; i++)
                        {
                            UpdatedValuesEntity update = (UpdatedValuesEntity)lst[i];
                            cnc.MethodBefore = update.beforeValue;

                            if (validateIds.ContainsKey(COMPANY_ORDER_CONT))
                            {
                                List<List<object>> list = validateIds[COMPANY_ORDER_CONT];
                                for (int x = 0; x < list.Count; x++)
                                {
                                    List<object> lonList = list[x];
                                    if (oldMasterKey == update.masterKey)
                                    {
                                        lonList.Add(cnc.ID);
                                        lonList.Add(cnc.ChangeEventID);
                                        lonList.Add("tChgCContct");
                                        lonList.Add("hccOID");
                                        lonList.Add("hccEventID");
                                        lonList.Add("hccID");
                                    }
                                    else
                                    {
                                        if (lonList[0].Equals(update.masterKey))
                                        {
                                            lonList.Add(cnc.ID);
                                            lonList.Add(cnc.ChangeEventID);
                                            lonList.Add("tChgCContct");
                                            lonList.Add("hccOID");
                                            lonList.Add("hccEventID");
                                            lonList.Add("hccID");
                                        }
                                    }
                                }
                                oldMasterKey = update.masterKey;
                            }
                        }
                        cncc.Update(cnc);
                        cncc.Dispose();
                    }
                    else if (COMPANY_ORDER_SVC.Equals(attr))
                    {
                        CompanyServiceChangeCollection cncc = tm.CompanyServiceChangeCollection;
                        CompanyServiceChange cnc = new CompanyServiceChange();
                        if (!newIdMap.ContainsKey(key))
                        {
                            cncc.Dispose();
                            continue;
                        }
                        List<long> idlst = newIdMap[key];
                        allEvntId = idlst[0];
                        long changeId = idlst[1];
                        cnc = cncc.GetByPrimaryKey(changeId);
                        long oldMasterKey = 0;
                        for (int i = 0; i < lst.Count; i++)
                        {
                            UpdatedValuesEntity update = (UpdatedValuesEntity)lst[i];
                            cnc.NameBefore = update.beforeValue;

                            if (validateIds.ContainsKey(COMPANY_ORDER_SVC))
                            {
                                List<List<object>> list = validateIds[COMPANY_ORDER_SVC];
                                for (int x = 0; x < list.Count; x++)
                                {
                                    List<object> lonList = list[x];
                                    if (oldMasterKey == update.masterKey)
                                    {
                                        lonList.Add(cnc.ID);
                                        lonList.Add(cnc.ChangeEventID);
                                        lonList.Add("tChgCSvc");
                                        lonList.Add("hcsOID");
                                        lonList.Add("hcsEventID");
                                        lonList.Add("hcsID");
                                    }
                                    else
                                    {
                                        if (lonList[0].Equals(update.masterKey))
                                        {
                                            lonList.Add(cnc.ID);
                                            lonList.Add(cnc.ChangeEventID);
                                            lonList.Add("tChgCSvc");
                                            lonList.Add("hcsOID");
                                            lonList.Add("hcsEventID");
                                            lonList.Add("hcsID");
                                        }
                                    }
                                }
                                oldMasterKey = update.masterKey;
                            }
                        }

                        cncc.Update(cnc);
                        cncc.Dispose();
                    }
                    else if (COMPANY_ORDER_TYPE.Equals(attr))
                    {
                        CompanyTypeChangeCollection cncc = tm.CompanyTypeChangeCollection;
                        CompanyTypeChange cnc = new CompanyTypeChange();
                        if (!newIdMap.ContainsKey(key))
                        {
                            cncc.Dispose();
                            continue;
                        }
                        List<long> idlst = newIdMap[key];
                        allEvntId = idlst[0];
                        long changeId = idlst[1];
                        cnc = cncc.GetByPrimaryKey(changeId);
                        long oldMasterKey = 0;
                        for (int i = 0; i < lst.Count; i++)
                        {
                            UpdatedValuesEntity update = (UpdatedValuesEntity)lst[i];
                            cnc.CodeBefore = update.beforeValue;

                            if (validateIds.ContainsKey(COMPANY_ORDER_TYPE))
                            {
                                List<List<object>> list = validateIds[COMPANY_ORDER_TYPE];
                                for (int x = 0; x < list.Count; x++)
                                {
                                    List<object> lonList = list[x];
                                    if (oldMasterKey == update.masterKey)
                                    {
                                        lonList.Add(cnc.ID);
                                        lonList.Add(cnc.ChangeEventID);
                                        lonList.Add("tChgCType");
                                        lonList.Add("hctOID");
                                        lonList.Add("hctEventID");
                                        lonList.Add("hctID");
                                    }
                                    else
                                    {
                                        if (lonList[0].Equals(update.masterKey))
                                        {
                                            lonList.Add(cnc.ID);
                                            lonList.Add(cnc.ChangeEventID);
                                            lonList.Add("tChgCType");
                                            lonList.Add("hctOID");
                                            lonList.Add("hctEventID");
                                            lonList.Add("hctID");
                                        }
                                    }
                                }
                                oldMasterKey = update.masterKey;
                            }
                        }
                        cncc.Update(cnc);
                        cncc.Dispose();
                    }
                    else if (COMPANY_ORDER_UNQI.Equals(attr))
                    {
                        CompanyUniqueIDChangeCollection cncc = tm.CompanyUniqueIDChangeCollection;
                        CompanyUniqueIDChange cnc = new CompanyUniqueIDChange();
                        if (!newIdMap.ContainsKey(key))
                        {
                            cncc.Dispose();
                            continue;
                        }
                        List<long> idlst = newIdMap[key];
                        allEvntId = idlst[0];
                        long changeId = idlst[1];
                        cnc = cncc.GetByPrimaryKey(changeId);
                        long oldMasterKey = 0;

                        for (int i = 0; i < lst.Count; i++)
                        {
                            UpdatedValuesEntity update = (UpdatedValuesEntity)lst[i];
                            if ("Type".Equals(update.item))
                            {
                                cnc.TypeBefore = update.beforeValue;
                            }
                            else if ("Code".Equals(update.item))
                            {
                                cnc.CodeBefore = update.beforeValue;
                            }

                            if (validateIds.ContainsKey(COMPANY_ORDER_UNQI))
                            {
                                List<List<object>> list = validateIds[COMPANY_ORDER_UNQI];
                                for (int x = 0; x < list.Count; x++)
                                {
                                    List<object> lonList = list[x];
                                    if (oldMasterKey == update.masterKey)
                                    {
                                        lonList.Add(cnc.ID);
                                        lonList.Add(cnc.ChangeEventID);
                                        lonList.Add("tChgCUnqIds");
                                        lonList.Add("hciOID");
                                        lonList.Add("hciEventID");
                                        lonList.Add("hciID");
                                    }
                                    else
                                    {
                                        if (lonList[0].Equals(update.masterKey))
                                        {
                                            lonList.Add(cnc.ID);
                                            lonList.Add(cnc.ChangeEventID);
                                            lonList.Add("tChgCUnqIds");
                                            lonList.Add("hciOID");
                                            lonList.Add("hciEventID");
                                            lonList.Add("hciID");
                                        }
                                    }
                                }
                                oldMasterKey = update.masterKey;
                            }
                        }
                        cncc.Update(cnc);
                        cncc.Dispose();
                    }
                    else if (COMPANY_ORDER_DATA.Equals(attr))
                    {
                        CompanyDataChangeCollection col = tm.CompanyDataChangeCollection;
                        CompanyDataChange data = new CompanyDataChange();
                        if (!newIdMap.ContainsKey(key))
                        {
                            col.Dispose();
                            continue;
                        }
                        List<long> idlst = newIdMap[key];
                        allEvntId = idlst[0];
                        long changeId = idlst[1];
                        data = col.GetByPrimaryKey(changeId);
                        for (int i = 0; i < lst.Count; i++)
                        {
                            UpdatedValuesEntity update = (UpdatedValuesEntity)lst[i];
                            if ("Expired".Equals(update.item))
                            {
                                if (string.IsNullOrEmpty(update.beforeValue))
                                {
                                    data.IsExpiredBeforeNull = true;
                                }
                                else
                                {

                                    data.ExpiredBefore = bool.Parse(update.beforeValue);
                                }
                            }
                            else if ("ExpiredDate".Equals(update.item))
                            {
                                if (string.IsNullOrEmpty(update.beforeValue))
                                {

                                    data.IsExpiredDateBeforeNull = true;
                                }
                                else
                                {

                                    data.ExpiredDateBefore = DateTime.Parse(update.beforeValue);
                                }
                            }
                            else if ("ExpiredReason".Equals(update.item))
                            {
                                data.ExpiredReasonBefore = update.beforeValue;
                            }
                            else if ("IsBranch".Equals(update.item))
                            {
                                if (string.IsNullOrEmpty(update.beforeValue))
                                {

                                    data.IsIsBranchBeforeNull = true;
                                }
                                else
                                {

                                    data.IsBranchBefore = bool.Parse(update.beforeValue);
                                }
                            }
                            else if ("IsPublic".Equals(update.item))
                            {
                                if (string.IsNullOrEmpty(update.beforeValue))
                                {

                                    data.IsIsPublicBeforeNull = true;
                                }
                                else
                                {

                                    data.IsPublicBefore = bool.Parse(update.beforeValue);
                                }
                            }
                            else if ("IsIssuer".Equals(update.item))
                            {
                                if (string.IsNullOrEmpty(update.beforeValue))
                                {
                                    data.IsIsIssuerBeforeNull = true;
                                }
                                else
                                {
                                    data.IsIssuerBefore = bool.Parse(update.beforeValue);
                                }
                            }
                            else if ("FiscalYearEnd".Equals(update.item))
                            {
                                data.FiscalYearEndBefore = update.beforeValue;
                            }
                            else if ("Status".Equals(update.item))
                            {
                                data.StatusBefore = update.beforeValue;
                            }

                            if (validateIds.ContainsKey(COMPANY_ORDER_DATA))
                            {
                                List<List<object>> list = validateIds[COMPANY_ORDER_DATA];
                                for (int x = 0; x < list.Count; x++)
                                {
                                    List<object> lonList = list[x];
                                    lonList.Add("Com");
                                }
                            }
                        }
                        col.Update(data);
                        col.Dispose();
                    }
                    else if (COMPANY_ORDER_ADDR.Equals(attr))
                    {
                        CompanyAddressChangeCollection col = tm.CompanyAddressChangeCollection;
                        CompanyAddressChange data = new CompanyAddressChange();
                        if (!newIdMap.ContainsKey(key))
                        {
                            col.Dispose();
                            continue;
                        }
                        List<long> idlst = newIdMap[key];
                        allEvntId = idlst[0];
                        long changeId = idlst[1];
                        data = col.GetByPrimaryKey(changeId);
                        long oldMasterKey = 0;

                        for (int i = 0; i < lst.Count; i++)
                        {
                            UpdatedValuesEntity update = (UpdatedValuesEntity)lst[i];
                            if (PHSICAL_ADDRESS_LINE_ONE.Equals(update.item) || REGISTRY_ADDRESS_LINE_ONE.Equals(update.item))
                            {
                                data.Address1Before = update.beforeValue;
                            }
                            else if (PHSICAL_ADDRESS_LINE_TWO.Equals(update.item) || REGISTRY_ADDRESS_LINE_TWO.Equals(update.item))
                            {
                                data.Address2Before = update.beforeValue;
                            }
                            else if (PHSICAL_ADDRESS_CITY.Equals(update.item) || REGISTRY_ADDRESS_CITY.Equals(update.item))
                            {
                                data.CityBefore = update.beforeValue;
                            }
                            else if (PHSICAL_ADDRESS_STATE.Equals(update.item) || REGISTRY_ADDRESS_STATE.Equals(update.item))
                            {
                                data.StateProvinceNameBefore = update.beforeValue;
                                if (!string.IsNullOrEmpty(update.beforeValue))
                                {
                                    data.StateProvinceAbbrevBefore = getValueForDesc(update.beforeValue);
                                }
                                else
                                {
                                    data.StateProvinceAbbrevBefore = string.Empty;
                                }
                                data.StateProvinceCodeBefore = data.StateProvinceAbbrevBefore;
                            }
                            else if (PHSICAL_ADDRESS_COUNTRY.Equals(update.item) || REGISTRY_ADDRESS_COUNTRY.Equals(update.item))
                            {
                                data.CountryBefore = update.beforeValue;
                                if (!string.IsNullOrEmpty(update.beforeValue))
                                {
                                    data.CountryCodeBefore = getValueForDesc(update.beforeValue);
                                }
                                else
                                {
                                    data.CountryCodeBefore = string.Empty;
                                }
                            }
                            else if (PHSICAL_ADDRESS_PC.Equals(update.item) || REGISTRY_ADDRESS_PC.Equals(update.item))
                            {
                                data.PostalCodeBefore = update.beforeValue;
                            }

                            if (validateIds.ContainsKey(COMPANY_ORDER_ADDR))
                            {
                                List<List<object>> list = validateIds[COMPANY_ORDER_ADDR];
                                for (int x = 0; x < list.Count; x++)
                                {
                                    List<object> lonList = list[x];
                                    if (oldMasterKey == update.masterKey)
                                    {
                                        lonList.Add(data.ID);
                                        lonList.Add(data.ChangeEventID);
                                        lonList.Add("tChgCAddr");
                                        lonList.Add("hcaOID");
                                        lonList.Add("hcaEventID");
                                        lonList.Add("hcaID");
                                    }
                                    else
                                    {
                                        if (lonList[0].Equals(update.masterKey))
                                        {
                                            lonList.Add(data.ID);
                                            lonList.Add(data.ChangeEventID);
                                            lonList.Add("tChgCAddr");
                                            lonList.Add("hcaOID");
                                            lonList.Add("hcaEventID");
                                            lonList.Add("hcaID");
                                        }
                                    }
                                }
                                oldMasterKey = update.masterKey;
                            }
                        }
                        col.Update(data);
                        col.Dispose();
                    }
                    else if (SECURITY_ORDER_DATA.Equals(attr))
                    {
                        SecurityDataChangeCollection col = tm.SecurityDataChangeCollection;
                        SecurityDataChange data = new SecurityDataChange();
                        if (!newIdMap.ContainsKey(key))
                        {
                            col.Dispose();
                            continue;
                        }
                        List<long> idlst = newIdMap[key];
                        allEvntId = idlst[0];
                        long changeId = idlst[1];
                        data = col.GetByPrimaryKey(changeId);
                        for (int i = 0; i < lst.Count; i++)
                        {
                            UpdatedValuesEntity update = (UpdatedValuesEntity)lst[i];
                            if ("Expired".Equals(update.item))
                            {
                                if (string.IsNullOrEmpty(update.beforeValue))
                                {
                                    data.IsExpiredBeforeNull = true;
                                }
                                else
                                {

                                    data.ExpiredBefore = bool.Parse(update.beforeValue);
                                }
                            }
                            else if ("ExpiredDate".Equals(update.item))
                            {
                                if (string.IsNullOrEmpty(update.beforeValue))
                                {

                                    data.IsExpiredDateBeforeNull = true;
                                }
                                else
                                {

                                    data.ExpiredDateBefore = DateTime.Parse(update.beforeValue);
                                }
                            }
                            else if ("ExpiredReason".Equals(update.item))
                            {
                                data.ExpiredReasonBefore = update.beforeValue;
                            }
                            else if ("Default Value".Equals(update.item))
                            {
                                if (string.IsNullOrEmpty(update.beforeValue))
                                {

                                    data.IsDefaultValueBeforeNull = true;
                                }
                                else
                                {

                                    data.DefaultValueBefore = bool.Parse(update.beforeValue);
                                }
                            }
                            else if ("Security Class".Equals(update.item))
                            {
                                data.ClassBefore = update.beforeValue;
                            }
                            else if ("Security Type".Equals(update.item))
                            {
                                data.TypeBefore = update.beforeValue;
                            }

                            if (validateIds.ContainsKey(SECURITY_ORDER_DATA))
                            {
                                List<List<object>> list = validateIds[SECURITY_ORDER_DATA];
                                for (int x = 0; x < list.Count; x++)
                                {
                                    List<object> lonList = list[x];
                                    //if (lonList[0].Equals(update.gmfId))
                                    //{
                                    lonList.Add("Sec");
                                    //    break;
                                    //}
                                }
                            }
                        }
                        col.Update(data);
                        col.Dispose();
                    }
                    else if (RELATIONSHIP_ORDER_DATA.Equals(attr))
                    {
                        RelationshipDataChangeCollection cncc = tm.RelationshipDataChangeCollection;
                        RelationshipDataChange cnc = new RelationshipDataChange();
                        if (!newIdMap.ContainsKey(key))
                        {
                            cncc.Dispose();
                            continue;
                        }
                        List<long> idlst = newIdMap[key];
                        allEvntId = idlst[0];
                        long changeId = idlst[1];
                        cnc = cncc.GetByPrimaryKey(changeId);
                        for (int i = 0; i < lst.Count; i++)
                        {
                            UpdatedValuesEntity update = (UpdatedValuesEntity)lst[i];
                            if (string.IsNullOrEmpty(update.beforeValue))
                            {
                                cnc.IsPercentBeforeNull = true;
                            }
                            else
                            {
                                cnc.PercentBefore = decimal.Parse(update.beforeValue);
                            }

                            if (validateIds.ContainsKey(RELATIONSHIP_ORDER_DATA))
                            {
                                List<List<object>> list = validateIds[RELATIONSHIP_ORDER_DATA];
                                for (int x = 0; x < list.Count; x++)
                                {
                                    List<object> lonList = list[x];
                                    //if (lonList[0].Equals(update.gmfId))
                                    //{
                                    lonList.Add("Rel");
                                    //    break;
                                    //}
                                }
                            }
                        }
                        cncc.Update(cnc);
                        cncc.Dispose();
                    }
                }
            }

            // create source reference and urls
            if (ifChanged)
            {
                if (!ValidateSubsequentChangeForEditNewValue(validateIds, tm, companyDataCnt))
                {
                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be saved due to subsequent change.');", true);
                    return false;
                }
                ChangeEventCollection ceCol = tm.ChangeEventCollection;
                ChangeEvent oldCE = ceCol.GetByPrimaryKey(long.Parse(EventID.Value));
                // inactive the old event
                InactivateChangeRecord(oldCE, tm);
                updateSourceRefAndUrls(allEvntId, tm);
            }
            else if (!ifChanged && ifChangeReasonChanged(ChangeRSRevjson, ChangeRSBefJson))
            {
                if (!ValidateSubsequentChangeForEditNewValue(validateIds, tm, companyDataCnt))
                {
                    ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be saved due to subsequent change.');", true);
                    return false;
                }
                ChangeEventCollection ceCol = tm.ChangeEventCollection;
                ChangeEvent oldCE = ceCol.GetByPrimaryKey(long.Parse(EventID.Value));
                // inactive the old event
                InactivateChangeRecord(oldCE, tm);
                // insert the new event
                ceCol.Insert(ce);
                updateSourceRefAndUrls(ce.ID, tm);
                // update event id in other sub-tables
                List<DataRow> rowLst = null;
                if (allEventsData.Rows.Count > 0)
                {
                    rowLst = new List<DataRow>();
                    foreach (DataRow workRec in allEventsData.Rows)
                    {
                        rowLst.Add(workRec);
                    }
                    saveAllChanges(rowLst, ce.ID, tm);
                }
                ceCol.Dispose();
            }
            else
            {
                ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "debugger;alert('There is not any change!');", true);
                return false;
            }

            return true;
        }

        private void copyComChangeToNewEventID(long id, long eventID, TransactionManager tm)
        {
            CompanyTypeChangeCollection ctcc = tm.CompanyTypeChangeCollection;
            CompanyTypeChange ctc = ctcc.GetByPrimaryKey(id);
            ctc.ChangeEventID = eventID;
            ctcc.Insert(ctc);
            ctcc.Dispose();
        }

        /// <summary>
        /// Process attributes not in change but are compulsively displayed if parent is in current change in edit new popup.
        /// </summary>
        /// <param name="ce"></param>
        /// <param name="tm"></param>
        /// <param name="skipRevertThenEditAlertType"></param>
        /// <returns>bool</returns>
        private bool processMandatoryChildTypes(ChangeEvent ce, TransactionManager tm, List<string> skipRevertThenEditAlertType)
        {
            JavaScriptSerializer jsSer = new JavaScriptSerializer();
            Dictionary<string, bool> dynamicNaicssubMap = jsSer.Deserialize<Dictionary<string, bool>>(this.DynamicNaicssub.Value);
            long currentEventNaicsID = 0;

            //If change has industry type and can't be displayed,then other attributes cannot be saved neither.
            if (!bool.Parse(IsIndustryTypeNoSeq.Value))
            {
                ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Change could not be saved due to subsequent change.');", true);
                return false;
            }
            //For industry types that are not in tComType but will not set to new value.Skip alert and move to new eventID.
            foreach (DataRow rec in comTypeRecordList)
            {
                string type = rec["subType"].ToString();
                string id = rec["subId"].ToString();
                if ("NAICS".Equals(type))
                {
                    currentEventNaicsID = long.Parse(id);
                }
                foreach (string skipType in skipRevertThenEditAlertType)
                {
                    if (skipType.Equals(type))
                    {
                        copyComChangeToNewEventID(long.Parse(id), ce.ID, tm);
                        break;
                    }
                }
            }
            foreach (DataRow rec in comAddrRecordList)
            {
                string type = rec["subType"].ToString();
                string id = rec["subId"].ToString();
                foreach (string skipType in skipRevertThenEditAlertType)
                {
                    if (skipType.Equals(type))
                    {
                        copyComChangeToNewEventID(long.Parse(id), ce.ID, tm);
                        break;
                    }
                }
            }
            //For industry type not in change record but since NAICS change,child type would change and should be saved.
            if (bool.Parse(IsIndustryTypeNoSeq.Value))
            {
                foreach (DataGridItem eachRow in revchgEvents.Items)
                {
                    if (eachRow.Cells[1].Text.Equals(eachRow.Cells[2].Text) && eachRow.Cells[0].Text.Contains("Industry Type"))
                    {
                        if (ChangeReasonEditor.IND_NAICS_SUB.Equals(eachRow.Cells[0].Text) && !dynamicNaicssubMap["isNaicssubVisible"])
                        {
                            continue;
                        }
                        ListDataDropDown psList = (ListDataDropDown)eachRow.FindControl("updatedValueLst");
                        string value = psList.SelectedValue;
                        string type = eachRow.Cells[0].Text.Substring(16);
                        if ("Legal Structure".Equals(type))
                        {
                            type = "STRUCTURE";
                        }
                        int count = 0;
                        if (DEFAULT_STRING.Equals(value))
                        {
                            value = "";
                        }
                        CompanyTypeCollection updc = tm.CompanyTypeCollection;
                        updc.GetByCompanyGMFID_Type(ce.GMFID, type);
                        ArrayList list = updc.CompanyTypeList;
                        CompanyType data;
                        if (list.Count == 0)
                        {
                            data = new CompanyType();
                            data.CompanyGMFID = ce.GMFID;
                            data.FeedID = ce.FeedID;
                            data.Type = type;
                            data.TypeIncrement = 1;
                            data.Code = value;
                        }
                        else
                        {
                            data = (CompanyType)list[list.Count - 1];
                            data.Code = value;
                            currentEventNaicsID = data.ID;
                        }

                        if (string.IsNullOrEmpty(value))
                        {
                            if (string.IsNullOrEmpty(eachRow.Cells[1].Text))
                            {
                                // For case GICS/NAICSSUB is null in tComType and select null in edit new value.
                                // null null null
                                updc.Dispose();
                                continue;
                            }
                            else
                            {
                                // Delete it for case GICS/NAICSSUB is not null in tComType and select null in edit new value.
                                // A A null
                                count = updc.DeleteIndustry(data, ce, tm);
                            }
                        }
                        else if (!value.Equals(eachRow.Cells[1].Text))
                        {
                            count = updc.UpdateOrInsertIndustry(data, ce, tm);
                        }
                        updc.Dispose();
                        if (currentEventNaicsID > 0)
                        {
                            if (ValidateSubsequentChange("tChgCType", "hctOID", "hctEventID", "hctID", data.ID, long.Parse(this.EventID.Value), currentEventNaicsID, tm) > count)
                            {
                                ScriptManager.RegisterStartupScript(CrrUpdate, CrrUpdate.GetType(), "Warning", "alert('Please revert the change and then edit the existing value.');", true);
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// ValidateEditNewValue
        /// </summary>
        /// <param name="validateIds"></param>
        /// <param name="tm"></param>
        /// <returns></returns>
        private bool ValidateSubsequentChangeForEditNewValue(Dictionary<string, List<List<object>>> validateIds, TransactionManager tm, int companyDataCnt)
        {

            long changeEventId = long.Parse(this.EventID.Value);
            foreach (string key in validateIds.Keys)
            {
                List<List<object>> list = validateIds[key];
                for (int j = 0; j < list.Count; j++)
                {
                    List<object> needToV = list[j];

                    if (COMPANY_ORDER_ADDR.Equals(key))
                    {
                        CompanyAddressChangeCollection addrchange = tm.CompanyAddressChangeCollection;
                        addrchange.GetByChangeEventID(changeEventId);
                        ArrayList addrchangeList = addrchange.CompanyAddressChangeList;

                        for (int i = 0; i < addrchangeList.Count; i++)
                        {
                            CompanyAddressChange data = (CompanyAddressChange)addrchangeList[i];
                            if (ValidateSubsequentChange(needToV[3].ToString(), needToV[4].ToString(),
                                needToV[5].ToString(), needToV[6].ToString(), long.Parse(needToV[0].ToString()),
                                changeEventId, data.ID, tm) > 1)
                            {
                                return false;
                            }
                        }

                    }
                    else if (COMPANY_ORDER_NAME.Equals(key))
                    {
                        CompanyNameChangeCollection addrchange = tm.CompanyNameChangeCollection;
                        addrchange.GetByChangeEventID(changeEventId);
                        ArrayList addrchangeList = addrchange.CompanyNameChangeList;

                        for (int i = 0; i < addrchangeList.Count; i++)
                        {
                            CompanyNameChange data = (CompanyNameChange)addrchangeList[i];
                            if (ValidateSubsequentChange(needToV[3].ToString(), needToV[4].ToString(),
                                needToV[5].ToString(), needToV[6].ToString(), long.Parse(needToV[0].ToString()),
                                changeEventId, data.ID, tm) > 1)
                            {
                                return false;
                            }
                        }

                    }
                    else if (COMPANY_ORDER_CONT.Equals(key))
                    {
                        CompanyContactChangeCollection addrchange = tm.CompanyContactChangeCollection;
                        addrchange.GetByChangeEventID(changeEventId);
                        ArrayList addrchangeList = addrchange.CompanyContactChangeList;

                        for (int i = 0; i < addrchangeList.Count; i++)
                        {
                            CompanyContactChange data = (CompanyContactChange)addrchangeList[i];
                            if (ValidateSubsequentChange(needToV[3].ToString(), needToV[4].ToString(),
                                needToV[5].ToString(), needToV[6].ToString(), long.Parse(needToV[0].ToString()),
                                changeEventId, data.ID, tm) > 1)
                            {
                                return false;
                            }
                        }

                    }
                    else if (COMPANY_ORDER_DATA.Equals(key))
                    {
                        CompanyDataChangeCollection addrchange = tm.CompanyDataChangeCollection;
                        addrchange.GetByChangeEventID(changeEventId);
                        ArrayList addrchangeList = addrchange.CompanyDataChangeList;

                        for (int i = 0; i < addrchangeList.Count; i++)
                        {
                            CompanyDataChange data = (CompanyDataChange)addrchangeList[i];
                            if (ValidateSubsequentChangeForComData(needToV[2].ToString(), long.Parse(needToV[0].ToString()),
                                int.Parse(needToV[1].ToString()),
                                changeEventId, data.ID, tm) > companyDataCnt)
                            {
                                return false;
                            }
                        }

                    }
                    else if (COMPANY_ORDER_SVC.Equals(key))
                    {
                        CompanyServiceChangeCollection addrchange = tm.CompanyServiceChangeCollection;
                        addrchange.GetByChangeEventID(changeEventId);
                        ArrayList addrchangeList = addrchange.CompanyServiceChangeList;

                        for (int i = 0; i < addrchangeList.Count; i++)
                        {
                            CompanyServiceChange data = (CompanyServiceChange)addrchangeList[i];
                            if (ValidateSubsequentChange(needToV[3].ToString(), needToV[4].ToString(),
                                needToV[5].ToString(), needToV[6].ToString(), long.Parse(needToV[0].ToString()),
                                changeEventId, data.ID, tm) > 1)
                            {
                                return false;
                            }
                        }

                    }
                    else if (COMPANY_ORDER_TYPE.Equals(key))
                    {
                        CompanyTypeChangeCollection addrchange = tm.CompanyTypeChangeCollection;
                        addrchange.GetByChangeEventID(changeEventId);
                        ArrayList addrchangeList = addrchange.CompanyTypeChangeList;

                        for (int i = 0; i < addrchangeList.Count; i++)
                        {
                            CompanyTypeChange data = (CompanyTypeChange)addrchangeList[i];
                            if (ValidateSubsequentChange(needToV[3].ToString(), needToV[4].ToString(),
                                needToV[5].ToString(), needToV[6].ToString(), long.Parse(needToV[0].ToString()),
                                changeEventId, data.ID, tm) > 1)
                            {
                                return false;
                            }
                        }

                    }
                    else if (COMPANY_ORDER_UNQI.Equals(key))
                    {
                        CompanyUniqueIDChangeCollection addrchange = tm.CompanyUniqueIDChangeCollection;
                        addrchange.GetByChangeEventID(changeEventId);
                        ArrayList addrchangeList = addrchange.CompanyUniqueIDChangeList;

                        for (int i = 0; i < addrchangeList.Count; i++)
                        {
                            CompanyUniqueIDChange data = (CompanyUniqueIDChange)addrchangeList[i];
                            if (ValidateSubsequentChange(needToV[3].ToString(), needToV[4].ToString(),
                                needToV[5].ToString(), needToV[6].ToString(), long.Parse(needToV[0].ToString()),
                                changeEventId, data.ID, tm) > 1)
                            {
                                return false;
                            }
                        }

                    }
                    else if (SECURITY_ORDER_DATA.Equals(key))
                    {
                        SecurityDataChangeCollection addrchange = tm.SecurityDataChangeCollection;
                        addrchange.GetByChangeEventID(changeEventId);
                        ArrayList addrchangeList = addrchange.SecurityDataChangeList;

                        for (int i = 0; i < addrchangeList.Count; i++)
                        {
                            SecurityDataChange data = (SecurityDataChange)addrchangeList[i];
                            if (ValidateSubsequentChangeForComData(needToV[2].ToString(), long.Parse(needToV[0].ToString()),
                                int.Parse(needToV[1].ToString()),
                                changeEventId, data.ID, tm) > 1)
                            {
                                return false;
                            }
                        }

                    }
                    else if (RELATIONSHIP_ORDER_DATA.Equals(key))
                    {
                        RelationshipDataChangeCollection addrchange = tm.RelationshipDataChangeCollection;
                        addrchange.GetByChangeEventID(changeEventId);
                        ArrayList addrchangeList = addrchange.RelationshipDataChangeList;

                        for (int i = 0; i < addrchangeList.Count; i++)
                        {
                            RelationshipDataChange data = (RelationshipDataChange)addrchangeList[i];
                            if (ValidateSubsequentChangeForComData(needToV[2].ToString(), long.Parse(needToV[0].ToString()),
                                int.Parse(needToV[1].ToString()),
                                changeEventId, data.ID, tm) > 1)
                            {
                                return false;
                            }
                        }

                    }
                }

            }

            return true;
        }
        /// <summary>
        /// return true if there are changes in change reason.
        /// </summary>
        /// <param name="ChangeRSRevjson"></param>
        /// <param name="ChangeRSBefJson"></param>
        /// <returns></returns>
        private bool ifChangeReasonChanged(HiddenField ChangeRSRevjson, HiddenField ChangeRSBefJson)
        {
            JavaScriptSerializer jsSer = new JavaScriptSerializer();
            ChangeResonSource beforeCrsObj = jsSer.Deserialize<ChangeResonSource>(ChangeRSBefJson.Value);
            List<string> bSourceTypes = beforeCrsObj.changeSourceTypes;
            List<string> bSourceUrls = beforeCrsObj.changeSourceUrls;
            string bChangeReason = beforeCrsObj.changeReason;
            string bChangeComments = beforeCrsObj.changeComments;
            //string bShowComments = beforeCrsObj.showComment;

            ChangeResonSource crsObj = jsSer.Deserialize<ChangeResonSource>(ChangeRSRevjson.Value);
            List<string> sourceTypes = crsObj.changeSourceTypes;
            List<string> sourceUrls = crsObj.changeSourceUrls;
            string changeReason = crsObj.changeReason;
            string changeComments = crsObj.changeComments;
            //string showComments = crsObj.showComment;

            if (bSourceTypes.All(sourceTypes.Contains) && bSourceTypes.Count == sourceTypes.Count
                && bSourceUrls.All(sourceUrls.Contains) && bSourceUrls.Count == sourceUrls.Count
                && bChangeComments.Equals(changeComments) && bChangeReason.Equals(changeReason))
            {
                return false;
            }

            return true;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void DPDownList_indexChange(object sender, EventArgs e)
        {
            List<List<string>> input = getInputValues();
            JavaScriptSerializer jsSer = new JavaScriptSerializer();
            List<UpdatedValuesEntity> updateValuesList = jsSer.Deserialize<List<UpdatedValuesEntity>>(this.UpdateValuesListHidden.Value);
            Dictionary<string, bool> dynamicNaicssubMap = jsSer.Deserialize<Dictionary<string, bool>>(this.DynamicNaicssub.Value);
            Button b = (Button)sender;
            string senderName = b.Text;
            //List<bool> addressChangedLst = jsSer.Deserialize<List<bool>>(this.AddressChanged.Value);
            //bool ifAddressChanged = addressChangedLst[0];
            //bool ifResAddressChanged = addressChangedLst[1];

            for (int i = 0; i < updateValuesList.Count; i++)
            {
                if (updateValuesList[i].listName.Equals(senderName) ||
                    ("Sec Is Expired".Equals(senderName) && "Is Expired".Equals(updateValuesList[i].listName)))
                {
                    DataGridItem countryRow = revchgEvents.Items[updateValuesList[i].listRow];
                    ListDataDropDown dpList = (ListDataDropDown)countryRow.FindControl("updatedValueLst");
                    string value = dpList.SelectedValue;
                    string listName = "";

                    for (int j = 0; j < updateValuesList.Count; j++)
                    {
                        if (senderName.Equals(PHSICAL_ADDRESS_COUNTRY))
                        {
                            if (updateValuesList[j].listName.Equals(PHSICAL_ADDRESS_STATE))
                            {
                                listName = (!string.IsNullOrEmpty(value)) ? List.TranslateToSupplement2(List.ISOCOUNTRY_LISTNAME, value) : "EmptyList";
                                listName = listName ?? "EmptyList";
                                DataGridItem psRow = revchgEvents.Items[updateValuesList[j].listRow];
                                ListDataDropDown psList = (ListDataDropDown)psRow.FindControl("updatedValueLst");
                                psList.ClearSelection();
                                if ("EmptyList".Equals(listName) || "".Equals(listName))
                                {
                                    psList.Enabled = false;
                                }
                                else
                                {
                                    psList.Enabled = true;
                                    psList.EmptyRowText = EMPTY_ROW;
                                    psList.ListName = listName;
                                    psList.forceRebind = true;
                                }
                            }

                            //ifAddressChanged = true;
                        }
                        else if (senderName.Equals(PHSICAL_ADDRESS_STATE)
                            || senderName.Equals(PHSICAL_ADDRESS_LINE_ONE)
                            || senderName.Equals(PHSICAL_ADDRESS_LINE_TWO)
                            || senderName.Equals(PHSICAL_ADDRESS_CITY)
                            || senderName.Equals(PHSICAL_ADDRESS_PC)
                            || senderName.Equals(REGISTRY_ADDRESS_PC))
                        {
                            //ifAddressChanged = true;
                        }
                        else if (senderName.Equals(REGISTRY_ADDRESS_STATE)
                            || senderName.Equals(REGISTRY_ADDRESS_LINE_ONE)
                            || senderName.Equals(REGISTRY_ADDRESS_LINE_TWO)
                            || senderName.Equals(REGISTRY_ADDRESS_CITY)
                            || senderName.Equals(LEGAL_STRUCTURE))
                        {
                            //ifResAddressChanged = true;
                        }
                        else if (senderName.Equals(REGISTRY_ADDRESS_COUNTRY))
                        {
                            if (updateValuesList[j].listName.Equals(REGISTRY_ADDRESS_STATE))
                            {
                                listName = (!string.IsNullOrEmpty(value)) ? List.TranslateToSupplement2(List.ISOCOUNTRY_LISTNAME, value) : "EmptyList";
                                listName = listName ?? "EmptyList";
                                DataGridItem psRow = revchgEvents.Items[updateValuesList[j].listRow];
                                ListDataDropDown psList = (ListDataDropDown)psRow.FindControl("updatedValueLst");
                                psList.ClearSelection();
                                if ("EmptyList".Equals(listName) || "".Equals(listName))
                                {
                                    psList.Enabled = false;
                                }
                                else
                                {
                                    psList.Enabled = true;
                                    psList.EmptyRowText = EMPTY_ROW;
                                    psList.ListName = listName;
                                    psList.forceRebind = true;
                                }
                            }
                            else if (updateValuesList[j].listName.Equals(LEGAL_STRUCTURE))
                            {
                                listName = getSubListDataForCountry(value, ChangeReasonEditor.LEGAL_STRUCTURE);
                                DataGridItem psRow = revchgEvents.Items[updateValuesList[j].listRow];
                                ListDataDropDown psList = (ListDataDropDown)psRow.FindControl("updatedValueLst");
                                psList.ClearSelection();
                                if (null == listName || "EmptyList".Equals(listName))
                                {
                                    psList.Enabled = false;
                                }
                                else
                                {
                                    psList.Enabled = true;
                                    psList.EmptyRowText = EMPTY_ROW;
                                    psList.ListName = listName;
                                    psList.forceRebind = true;
                                }
                            }

                            //ifResAddressChanged = true;
                        }
                        else if (senderName.Equals(NAICS))
                        {
                            if (value == List.CLEAR_KSC)
                            {
                                value = DEFAULT_STRING;
                                dpList.SelectedValue = DEFAULT_STRING;
                            }
                            string nAICSSub = getSubListDataForNAICS(value);
                            if (value == DEFAULT_STRING || "EmptyList".Equals(nAICSSub))
                            {
                                dynamicNaicssubMap["isNaicssubVisible"] = false;
                            }
                            else
                            {
                                dynamicNaicssubMap["isNaicssubVisible"] = true;
                            }
                            if (!dynamicNaicssubMap["isNaicssubInChange"])
                            {
                                for (int k = 0; k < revchgEvents.Items.Count; k++)
                                {
                                    if (ChangeReasonEditor.IND_NAICS_SUB.Equals(revchgEvents.Items[k].Cells[0].Text))
                                    {
                                        revchgEvents.Items[k].Visible = dynamicNaicssubMap["isNaicssubVisible"];
                                        break;
                                    }
                                }
                            }

                            if (updateValuesList[j].listName.Equals(NACE))
                            {
                                listName = "fakedataNACECodes";
                                DataGridItem psRow = revchgEvents.Items[updateValuesList[j].listRow];
                                ListDataDropDown psList = (ListDataDropDown)psRow.FindControl("updatedValueLst");
                                //if (long.Parse(this.GMFID.Value) < 0)
                                //{
                                //    psList.forceRebind = false;
                                //}
                                //else
                                //{
                                psList.ClearSelection();
                                psList.EmptyRowText = EMPTY_ROW;
                                psList.ListName = listName;
                                psList.SelectedNaicsValue = value;
                                psList.forceRebind = true;
                                //}

                            }
                            else if (updateValuesList[j].listName.Equals(SIC))
                            {
                                listName = "fakedataSICCodes";
                                DataGridItem psRow = revchgEvents.Items[updateValuesList[j].listRow];
                                ListDataDropDown psList = (ListDataDropDown)psRow.FindControl("updatedValueLst");
                                psList.ClearSelection();
                                psList.EmptyRowText = EMPTY_ROW;
                                psList.ListName = listName;
                                psList.SelectedNaicsValue = value;
                                psList.forceRebind = true;

                            }
                            else if (updateValuesList[j].listName.Equals(NAICSSUB))
                            {
                                DataGridItem psRow = revchgEvents.Items[updateValuesList[j].listRow];
                                ListDataDropDown psList = (ListDataDropDown)psRow.FindControl("updatedValueLst");
                                ((CompareValidator)psRow.FindControl("CompareValidator_updatedValueLst")).Enabled = dynamicNaicssubMap["isNaicssubVisible"];
                                psList.ClearSelection();
                                psList.EmptyRowText = EMPTY_ROW;
                                psList.ListName = nAICSSub;
                                psList.SelectedNaicsValue = value;
                                psList.forceRebind = true;
                                psList.Visible = dynamicNaicssubMap["isNaicssubVisible"];

                            }
                            else if (updateValuesList[j].listName.Equals(GICS))
                            {
                                listName = "fakedataGICSCodes";
                                DataGridItem psRow = revchgEvents.Items[updateValuesList[j].listRow];
                                ListDataDropDown psList = (ListDataDropDown)psRow.FindControl("updatedValueLst");
                                psList.ClearSelection();
                                psList.EmptyRowText = EMPTY_ROW;
                                psList.ListName = listName;
                                psList.SelectedNaicsValue = value;
                                psList.forceRebind = true;
                            }
                        }
                        else if (senderName.Equals("Is Expired"))
                        {
                            if (updateValuesList[j].listName.Equals("Reason for expiration"))
                            {
                                DataGridItem psRow = revchgEvents.Items[updateValuesList[j].listRow];
                                ListDataDropDown psList = (ListDataDropDown)psRow.FindControl("updatedValueLst");
                                psList.ClearSelection();
                                if (value.Equals("False"))
                                {
                                    psList.SelectedIndex = 1;
                                }
                                else if (value.Equals("True"))
                                {
                                    psList.SelectedIndex = 2;
                                }
                                else if (value.Equals("Clear KSC Status"))
                                {
                                    psList.SelectedIndex = 3;
                                }
                                psList.ListName = "Reason for expiration";
                                psList.forceRebind = true;
                                psList.Enabled = false;
                            }
                            else if (updateValuesList[j].listName.Equals("Date of expiration"))
                            {
                                DataGridItem psRow = revchgEvents.Items[updateValuesList[j].listRow];
                                TextBox psList = (TextBox)psRow.FindControl("updatedValue");
                                if (value.Equals("False") || value.Equals("Clear KSC Status"))
                                {
                                    psList.Text = string.Empty;
                                }
                                else if (value.Equals("True"))
                                {
                                    if (string.IsNullOrEmpty(psRow.Cells[2].Text) || "&nbsp;".Equals(psRow.Cells[2].Text))
                                    {
                                        psList.Text = DateTime.Now.ToString();
                                    }
                                    else
                                    {
                                        psList.Text = psRow.Cells[2].Text;
                                    }
                                }
                                psList.Enabled = false;
                            }
                        }
                        else if (senderName.Equals("Sec Is Expired"))
                        {
                            if (updateValuesList[j].listName.Equals("Reason for expiration"))
                            {
                                DataGridItem psRow = revchgEvents.Items[updateValuesList[j].listRow];
                                ListDataDropDown psList = (ListDataDropDown)psRow.FindControl("updatedValueLst");
                                psList.ClearSelection();
                                if (value.Equals("False"))
                                {
                                    psList.SelectedIndex = 1;
                                }
                                else if (value.Equals("True"))
                                {
                                    psList.SelectedIndex = 4;
                                }
                                else if (value.Equals("Clear KSC Status"))
                                {
                                    psList.SelectedIndex = 3;
                                }
                                psList.ListName = "Reason for expiration";
                                psList.forceRebind = true;
                                psList.Enabled = false;
                            }
                            else if (updateValuesList[j].listName.Equals("Date of expiration"))
                            {
                                DataGridItem psRow = revchgEvents.Items[updateValuesList[j].listRow];
                                TextBox psList = (TextBox)psRow.FindControl("updatedValue");
                                if (value.Equals("False") || value.Equals("Clear KSC Status"))
                                {
                                    psList.Text = string.Empty;
                                }
                                else if (value.Equals("True"))
                                {
                                    if (string.IsNullOrEmpty(psRow.Cells[2].Text) || "&nbsp;".Equals(psRow.Cells[2].Text))
                                    {
                                        psList.Text = DateTime.Now.ToString();
                                    }
                                    else
                                    {
                                        psList.Text = psRow.Cells[2].Text;
                                    }
                                }
                                psList.Enabled = false;
                            }
                        }
                        else if (senderName.Equals("Security Class"))
                        {
                            if (updateValuesList[j].listName.Equals("Security Type"))
                            {
                                value = dpList.SelectedItem.Text;
                                DataGridItem psRow = revchgEvents.Items[updateValuesList[j].listRow];
                                ListDataDropDown psList = (ListDataDropDown)psRow.FindControl("updatedValueLst");
                                psList.ClearSelection();
                                int selectIndex = 0;
                                for (int x = 0; x < psList.Items.Count; x++)
                                {
                                    ListItem item = psList.Items[x];
                                    if (item.Text.Equals(value))
                                    {
                                        selectIndex = x;
                                        break;
                                    }
                                }
                                psList.SelectedIndex = selectIndex;
                            }
                        }

                    }
                }
            }
            DynamicNaicssub.Value = jsSer.Serialize(dynamicNaicssubMap);

            //List<bool> addlst = new List<bool>();
            //addlst.Add(ifAddressChanged);
            //addlst.Add(ifResAddressChanged);
            //AddressChanged.Value = jsSer.Serialize(addlst);
        }

        private long getComAddrOID(string Type, string gmfID)
        {
            if (long.Parse(gmfID) > 0)
            {
                KSC.GMF.Data.GMFDB db = new KSC.GMF.Data.GMFDB();
                string sqlStr = "select caOID from tComAddr where caGMFID = @GMFID and caFID =@FID and caType = @TYPE";

                SqlCommand cmd = new SqlCommand(sqlStr, (SqlConnection)db.Connection);
                cmd.Parameters.Add("@GMFID", SqlDbType.BigInt);
                cmd.Parameters["@GMFID"].Value = long.Parse(gmfID);
                cmd.Parameters.Add("@FID", SqlDbType.BigInt);
                cmd.Parameters["@FID"].Value = 2;
                cmd.Parameters.Add("@TYPE", SqlDbType.VarChar);
                cmd.Parameters["@TYPE"].Value = Type;
                object o = cmd.ExecuteScalar();
                db.Dispose();
                if (null == o)
                {
                    return 0;
                }
                return long.Parse(o.ToString());
            }
            return 0;
        }

        private long getComTypeOID(string Type, string gmfID)
        {
            if (long.Parse(gmfID) > 0)
            {
                KSC.GMF.Data.GMFDB db = new KSC.GMF.Data.GMFDB();
                string sqlStr = "select ctOID from tComType where ctGMFID = @GMFID and ctFID =@FID and ctType = @TYPE";

                SqlCommand cmd = new SqlCommand(sqlStr, (SqlConnection)db.Connection);
                cmd.Parameters.Add("@GMFID", SqlDbType.BigInt);
                cmd.Parameters["@GMFID"].Value = long.Parse(gmfID);
                cmd.Parameters.Add("@FID", SqlDbType.BigInt);
                cmd.Parameters["@FID"].Value = 2;
                cmd.Parameters.Add("@TYPE", SqlDbType.VarChar);
                cmd.Parameters["@TYPE"].Value = Type;
                object o = cmd.ExecuteScalar();
                db.Dispose();
                if (null == o)
                {
                    return 0;
                }
                return long.Parse(o.ToString());
            }
            return 0;
        }

        private string getComTypeCode(string Type)
        {
            string returnCode = null;
            if (long.Parse(this.GMFID.Value) > 0)
            {
                KSC.GMF.Data.GMFDB db = new KSC.GMF.Data.GMFDB();
                string sqlStr = "select ctCode from tComType where ctGMFID=@GMFID and ctFID =@FID and ctType = @TYPE";

                SqlCommand cmd = new SqlCommand(sqlStr, (SqlConnection)db.Connection);
                cmd.Parameters.Add("@GMFID", SqlDbType.BigInt);
                cmd.Parameters["@GMFID"].Value = long.Parse(this.GMFID.Value);
                cmd.Parameters.Add("@FID", SqlDbType.BigInt);
                cmd.Parameters["@FID"].Value = 2;
                cmd.Parameters.Add("@TYPE", SqlDbType.VarChar);
                cmd.Parameters["@TYPE"].Value = Type;
                object o = cmd.ExecuteScalar();
                db.Dispose();
                if (null == o)
                {
                    return null;
                }
                return o.ToString();
            }
            else
            {
                if (null != graphToAddCha && null != graphToAddCha.CreatedEntities && graphToAddCha.CreatedEntities.Count > 0)
                {
                    Dictionary<long, ProtoCompany> proComs = graphToAddCha.CreatedEntities;
                    if (null != proComs)
                    {
                        if (proComs.ContainsKey(long.Parse(this.GMFID.Value)))
                        {
                            ProtoCompany currCom = proComs[long.Parse(this.GMFID.Value)].History[historyIndexForProto];
                            if (NAICS.Equals(Type))
                            {
                                returnCode = currCom.NAICS;
                            }
                            else if (NAICSSUB.Equals(Type))
                            {
                                returnCode = currCom.NAICSSub;
                            }
                            else if (NACE.Equals(Type))
                            {
                                returnCode = currCom.NACE;
                            }
                            else if ("SIC1".Equals(Type))
                            {
                                returnCode = currCom.SIC;
                            }
                            else if (GICS.Equals(Type))
                            {
                                returnCode = currCom.GICS;
                            }
                        }
                    }
                }

                return returnCode;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="evntId"></param>
        /// <param name="tm"></param>
        private void updateSourceRefAndUrls(long evntId, TransactionManager tm)
        {
            JavaScriptSerializer jsSer = new JavaScriptSerializer();
            string chgRSjon = ChangeRSRevjson.Value;
            string chgRSjsonBef = ChangeRSBefJson.Value;
            // insert other change references
            ChangeResonSource crsObj = jsSer.Deserialize<ChangeResonSource>(chgRSjon);
            List<string> sourceTypes = crsObj.changeSourceTypes;
            List<string> sourceUrls = crsObj.changeSourceUrls;

            if (null != sourceTypes && sourceTypes.Count > 0)
            {
                ChangeSourceReferencesCollection srCol = tm.ChangeSourceReferencesCollection;
                ChangeSourceReferences csr = null;
                for (int inxSr = 0; inxSr < sourceTypes.Count; inxSr++)
                {
                    if (!String.IsNullOrEmpty(sourceTypes[inxSr]))
                    {
                        csr = new ChangeSourceReferences();
                        csr.ChangeEventID = evntId;
                        csr.Source = sourceTypes[inxSr];
                        csr.ReferenceURL = sourceUrls[inxSr];
                        srCol.Insert(csr);
                    }
                }
                srCol.Dispose();
            }
        }
    }
}