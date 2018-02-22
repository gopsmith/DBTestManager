using System;
using System.Collections.Generic;
using System.Diagnostics;
using MyOrganization.Editorial.Site.Data.Models.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyOrganization.Editorial.Site.Data;
using MyOrganization.Editorial.App.Extensions;
using System.Linq;
using TestUtils.DbTest;

namespace SiteTests.DataClientTests
{
    [TestClass()]
    public class BulkImportDataClientTest
    {
        private IBulkImportDataClient _bulkImportDataClient;
        private string _adminConnectionString;

        private DbTestManager _dbTestManager;

        public BulkImportDataClientTest()
        {
            _adminConnectionString = MyOrganization.Core.ConfigSettings.GetInstance().AppSettings["AdminConnection"];
            _bulkImportDataClient = new BulkImportDataClient();
            _dbTestManager = new DbTestManager(_adminConnectionString);
        }


        [TestInitialize]
        public void StartUp()
        {
        }

        [TestCleanup]
        public void CleanUp()
        {
            _dbTestManager.Dispose();
        }


        #region Test Methods

        [TestCategory("BulkImportDataClientTest"), TestMethod]
	    public void BulkImportEvents_ImportsEvents()
	    {
		    var bulkImportId = _bulkImportDataClient.BulkImportEvents(TempEventsXml, "E", "gsmith", "C:\\EventsImport.xlsx");

			//add the SP-generated rows to our test-data cleanup list:
			_dbTestManager.AddToCleanup("BulkImportTb", bulkImportId);
			_dbTestManager.AddToCleanup("BulkEventItemTb", bulkImportId, "BulkImportId");	//for this table, use the foreign key

			//peeking at some values in the header:
			var tdHeader = _dbTestManager.Retrieve("BulkImportTb", bulkImportId);
			Assert.IsNotNull(tdHeader);
			Assert.IsTrue(tdHeader.Get<string>("OrigFileN") == "C:\\EventsImport.xlsx");
			Assert.IsTrue(tdHeader.Get<string>("CrtUsrN") == "gsmith");

			//peeking at some values in the line item:
			var tdLine1 = _dbTestManager.Retrieve("BulkEventItemTb", bulkImportId, keyColumnName: "BulkImportId");
			Assert.IsNotNull(tdLine1);
			Assert.IsTrue(tdLine1.Get<int>("ParntMEI") == 175679276);
			Assert.IsTrue(tdLine1.Get<string>("IptcCptnTmplt") == "[CELEBRITY] is photographed for [PUBLICATION] on [DATE] in [CITY], City.");
		}

		[TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkImportEvents_ImportsShoots()
		{
			var bulkImportId = _bulkImportDataClient.BulkImportEvents(TempShootsXml, "S", "gsmith", "C:\\ShootsImport.xlsx");

			//add the SP-generated rows to our test-data cleanup list:
			_dbTestManager.AddToCleanup("BulkImportTb", bulkImportId);
			_dbTestManager.AddToCleanup("BulkEventItemTb", bulkImportId, "BulkImportId");	//for this table, use the foreign key

			var tdHeader = _dbTestManager.Retrieve("BulkImportTb", bulkImportId);
			Assert.IsNotNull(tdHeader);
			Assert.IsTrue(tdHeader.Get<string>("OrigFileN") == "C:\\ShootsImport.xlsx");
			Assert.IsTrue(tdHeader.Get<string>("CrtUsrN") == "gsmith");

			var tdLine1 = _dbTestManager.Retrieve("BulkEventItemTb", bulkImportId, keyColumnName: "BulkImportId");
			Assert.IsNotNull(tdLine1);
			Assert.IsTrue(tdLine1.Get<string>("PhgrApvlF") == "Y");
			Assert.IsTrue(tdLine1.Get<string>("IptcCptnTmplt") == "[CELEBRITY] is photographed for [PUBLICATION] on [DATE] in [CITY], City.");
		}

		[TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkImportContractItems_ImportsAgreements()
		{
			var bulkImportId = _bulkImportDataClient.BulkImportContractItems(TempAgreementsXml, BulkType.Agreement, BulkEditType.Add, "gsmith", "C:\\AgreementImport.xlsx");

			//add the SP-generated rows to our test-data cleanup list:
			_dbTestManager.AddToCleanup("BulkImportTb", bulkImportId);
			_dbTestManager.AddToCleanup("BulkAgreementItemTb", bulkImportId, "BulkImportId");	//for this table, use the foreign key

			//peeking at some values in the header:
			var tdHeader = _dbTestManager.Retrieve("BulkImportTb", bulkImportId);
			Assert.IsNotNull(tdHeader);
			Assert.IsTrue(tdHeader.Get<string>("OrigFileN") == "C:\\AgreementImport.xlsx");
			Assert.IsTrue(tdHeader.Get<string>("CrtUsrN") == "gsmith");
			Assert.IsTrue(tdHeader.Get<string>("ImportTypeC") == "A");
			Assert.IsTrue(tdHeader.Get<string>("EditType") == "A");

			//peeking at some values in the line item:
			var tdLine1 = _dbTestManager.Retrieve("BulkAgreementItemTb", bulkImportId, keyColumnName: "BulkImportId");
			Assert.IsNotNull(tdLine1);
			Assert.IsTrue(tdLine1.Get<string>("Term") == "One Year");
			Assert.IsTrue(tdLine1.Get<string>("TerminationReason").IsNullOrEmpty());
		}

        [TestCategory("BulkImportDataClientTest"), TestMethod]
        public void BulkImportContractItems_ImportsAgreementsInTwoPasses()
        {
            var bulkImportId = _bulkImportDataClient.BulkImportContractItems(TempAgreementsXml, BulkType.Agreement, BulkEditType.Add, "gsmith", "C:\\AgreementImport.xlsx");

            //add a second line item (same as the first):
            _bulkImportDataClient.BulkImportContractItems(TempAgreementsXml, BulkType.Agreement, BulkEditType.Add, "gsmith", "C:\\AgreementImport.xlsx", bulkImportId);

            //add the SP-generated rows to our test-data cleanup list:
            _dbTestManager.AddToCleanup("BulkImportTb", bulkImportId);
            _dbTestManager.AddToCleanup("BulkAgreementItemTb", bulkImportId, "BulkImportId");	//for this table, use the foreign key

            //peeking at some values in the header:
            var tdHeader = _dbTestManager.Retrieve("BulkImportTb", bulkImportId);
            Assert.IsNotNull(tdHeader);
            Assert.IsTrue(tdHeader.Get<string>("OrigFileN") == "C:\\AgreementImport.xlsx");
            Assert.IsTrue(tdHeader.Get<string>("CrtUsrN") == "gsmith");
            Assert.IsTrue(tdHeader.Get<string>("ImportTypeC") == "A");
            Assert.IsTrue(tdHeader.Get<string>("EditType") == "A");

            //peeking at some values in the line item:
            var tdItemKeys = _dbTestManager.Retrieve("BulkAgreementItemTb", bulkImportId, keyColumnName: "BulkImportId")
                                           .Where(c => c.ColumnName == "BulkAgreementItemId").Select(f => f.Value).ToArray();
            Assert.IsTrue(tdItemKeys.Length == 2);
            var tdLine1 = _dbTestManager.Retrieve("BulkAgreementItemTb", tdItemKeys[0], keyColumnName: "BulkAgreementItemId");
            var tdLine2 = _dbTestManager.Retrieve("BulkAgreementItemTb", tdItemKeys[1], keyColumnName: "BulkAgreementItemId");
            Assert.IsNotNull(tdLine1);
            Assert.IsNotNull(tdLine2);
            Assert.IsTrue(tdLine1.Get<string>("Term") == "One Year");
            Assert.IsTrue(tdLine2.Get<string>("Term") == "One Year");
            Assert.IsTrue(tdLine1.Get<string>("TerminationReason").IsNullOrEmpty());
            Assert.IsTrue(tdLine2.Get<string>("TerminationReason").IsNullOrEmpty());
        }

        [TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkImportContractItems_ImportsContentProviders()
		{
			var bulkImportId = _bulkImportDataClient.BulkImportContractItems(TempContentProvidersXml, BulkType.ContentProvider, BulkEditType.Add, "gsmith", "C:\\ContentProviderImport.xlsx");

			//add the SP-generated rows to our test-data cleanup list:
			_dbTestManager.AddToCleanup("BulkImportTb", bulkImportId);
			_dbTestManager.AddToCleanup("BulkContentProviderItemTb", bulkImportId, "BulkImportId");	//for this table, use the foreign key

			//peeking at some values in the header:
			var tdHeader = _dbTestManager.Retrieve("BulkImportTb", bulkImportId);
			Assert.IsNotNull(tdHeader);
			Assert.IsTrue(tdHeader.Get<string>("OrigFileN") == "C:\\ContentProviderImport.xlsx");
			Assert.IsTrue(tdHeader.Get<string>("CrtUsrN") == "gsmith");
			Assert.IsTrue(tdHeader.Get<string>("ImportTypeC") == "P");
			Assert.IsTrue(tdHeader.Get<string>("EditType") == "A");

			//peeking at some values in the line item:
			var tdLine1 = _dbTestManager.Retrieve("BulkContentProviderItemTb", bulkImportId, keyColumnName: "BulkImportId");
			Assert.IsNotNull(tdLine1);
			Assert.IsTrue(tdLine1.Get<string>("AttributionLine") == "Stanley K. | John A. | Jerry la V.");
			Assert.IsTrue(tdLine1.Get<string>("TaxpayerID") == "5554154848");
		}

        [TestCategory("BulkImportDataClientTest"), TestMethod]
        public void BulkImportContractItems_ImportsContentProvidersInTwoPasses()
        {
            var bulkImportId = _bulkImportDataClient.BulkImportContractItems(TempContentProvidersXml, BulkType.ContentProvider, BulkEditType.Add, "gsmith", "C:\\ContentProviderImport.xlsx");

            //add a second line item (same as the first):
            _bulkImportDataClient.BulkImportContractItems(TempContentProvidersXml, BulkType.ContentProvider, BulkEditType.Add, "gsmith", "C:\\ContentProviderImport.xlsx", bulkImportId);

            //add the SP-generated rows to our test-data cleanup list:
            _dbTestManager.AddToCleanup("BulkImportTb", bulkImportId);
            _dbTestManager.AddToCleanup("BulkContentProviderItemTb", bulkImportId, "BulkImportId");	//for this table, use the foreign key

            //peeking at some values in the two line items:
            var tdHeader = _dbTestManager.Retrieve("BulkImportTb", bulkImportId);
            Assert.IsNotNull(tdHeader);
            Assert.IsTrue(tdHeader.Get<string>("OrigFileN") == "C:\\ContentProviderImport.xlsx");
            Assert.IsTrue(tdHeader.Get<string>("CrtUsrN") == "gsmith");
            Assert.IsTrue(tdHeader.Get<string>("ImportTypeC") == "P");
            Assert.IsTrue(tdHeader.Get<string>("EditType") == "A");

            //peeking at some values in the line item:
            var tdItemKeys = _dbTestManager.Retrieve("BulkContentProviderItemTb", bulkImportId, keyColumnName: "BulkImportId")
                                           .Where(c => c.ColumnName == "BulkContentProviderItemId").Select(f => f.Value).ToArray();
            Assert.IsTrue(tdItemKeys.Length == 2);
            var tdLine1 = _dbTestManager.Retrieve("BulkContentProviderItemTb", tdItemKeys[0], keyColumnName: "BulkContentProviderItemId");
            var tdLine2 = _dbTestManager.Retrieve("BulkContentProviderItemTb", tdItemKeys[1], keyColumnName: "BulkContentProviderItemId");
            Assert.IsNotNull(tdLine1);
            Assert.IsNotNull(tdLine2);
            Assert.IsTrue(tdLine1.Get<string>("AttributionLine") == "Stanley K. | John A. | Jerry la V. ");
            Assert.IsTrue(tdLine2.Get<string>("AttributionLine") == "Stanley K. | John A. | Jerry la V. ");
            Assert.IsTrue(tdLine1.Get<string>("TaxpayerID") == "5554154848");
            Assert.IsTrue(tdLine2.Get<string>("TaxpayerID") == "5554154848");
        }

		[TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkImportContractItems_ImportsContracts()
		{
			var bulkImportId = _bulkImportDataClient.BulkImportContractItems(TempContractsXml, BulkType.Contract, BulkEditType.Update, "gsmith", "C:\\ContractImport.xlsx");

			//add the SP-generated rows to our test-data cleanup list:
			_dbTestManager.AddToCleanup("BulkImportTb", bulkImportId);
			_dbTestManager.AddToCleanup("BulkContractItemTb", bulkImportId, "BulkImportId");	//for this table, use the foreign key

			//peeking at some values in the header:
			var tdHeader = _dbTestManager.Retrieve("BulkImportTb", bulkImportId);
			Assert.IsNotNull(tdHeader);
			Assert.IsTrue(tdHeader.Get<string>("OrigFileN") == "C:\\ContractImport.xlsx");
			Assert.IsTrue(tdHeader.Get<string>("CrtUsrN") == "gsmith");
			Assert.IsTrue(tdHeader.Get<string>("ImportTypeC") == "C");
			Assert.IsTrue(tdHeader.Get<string>("EditType") == "U");

			//peeking at some values in the line item:
			var tdLine1 = _dbTestManager.Retrieve("BulkContractItemTb", bulkImportId, keyColumnName: "BulkImportId");
			Assert.IsNotNull(tdLine1);
			Assert.IsTrue(tdLine1.Get<string>("TerritoryRestrictions") == "<APPEND>Brazil Country Hide");
			Assert.IsTrue(tdLine1.Get<long>("ContractID") == 8235000642);
		}

        [TestCategory("BulkImportDataClientTest"), TestMethod]
        public void BulkImportContractItems_ImportsContractsInTwoPasses()
        {
            var bulkImportId = _bulkImportDataClient.BulkImportContractItems(TempContractsXml, BulkType.Contract, BulkEditType.Update, "gsmith", "C:\\ContractImport.xlsx");

            //add a second line item (same as the first):
            _bulkImportDataClient.BulkImportContractItems(TempContractsXml, BulkType.Contract, BulkEditType.Update, "gsmith", "C:\\ContractImport.xlsx", bulkImportId);

            //add the SP-generated rows to our test-data cleanup list:
            _dbTestManager.AddToCleanup("BulkImportTb", bulkImportId);
            _dbTestManager.AddToCleanup("BulkContractItemTb", bulkImportId, "BulkImportId");	//for this table, use the foreign key

            //peeking at some values in the header:
            var tdHeader = _dbTestManager.Retrieve("BulkImportTb", bulkImportId);
            Assert.IsNotNull(tdHeader);
            Assert.IsTrue(tdHeader.Get<string>("OrigFileN") == "C:\\ContractImport.xlsx");
            Assert.IsTrue(tdHeader.Get<string>("CrtUsrN") == "gsmith");
            Assert.IsTrue(tdHeader.Get<string>("ImportTypeC") == "C");
            Assert.IsTrue(tdHeader.Get<string>("EditType") == "U");

            //peeking at some values in the two line items:
            var tdItemKeys = _dbTestManager.Retrieve("BulkContractItemTb", bulkImportId, keyColumnName: "BulkImportId")
                                           .Where(c => c.ColumnName == "BulkContractItemId").Select(f => f.Value).ToArray();
            Assert.IsTrue(tdItemKeys.Length == 2);
            var tdLine1 = _dbTestManager.Retrieve("BulkContractItemTb", tdItemKeys[0], keyColumnName: "BulkContractItemId");
            var tdLine2 = _dbTestManager.Retrieve("BulkContractItemTb", tdItemKeys[1], keyColumnName: "BulkContractItemId");
            Assert.IsNotNull(tdLine1);
            Assert.IsNotNull(tdLine2);
            Assert.IsTrue(tdLine1.Get<string>("TerritoryRestrictions") == "<APPEND>Brazil Country Hide");
            Assert.IsTrue(tdLine2.Get<string>("TerritoryRestrictions") == "<APPEND>Brazil Country Hide");
            Assert.IsTrue(tdLine1.Get<long>("ContractID") == 8235000642);
            Assert.IsTrue(tdLine2.Get<long>("ContractID") == 8235000642);
        }

	    [TestCategory("ExcludeFromBuild"), TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkEventImportGetNextItem_Generates_OneErrorAndOneSuccess()
	    {
		    var tries = 3;
		    List<decimal> bulkImportItemIds = null;
		    BulkImportItem blkItem = null;

			while (tries > 0)
			{
				var bulkImportId = InsertRowIntoBulkImportTb();				//Event Bulk Add
				var eventItem1 = TempInvalidBulkEventItem(bulkImportId);	//this line item should fail, since it's missing all the required fields
				var eventItem2 = TempValidBulkEventItem(bulkImportId, 2);	//this line item should succeed
				bulkImportItemIds = _dbTestManager.InsertRange(eventItem1, eventItem2).ToList();	//create in BulkEventItemTb

				//The following should return the one valid item, LineNum=2 (#1 fails and doesn't get returned),
				//UNLESS the scheduled task service grabbed it before we got to it.
				//ALSO, we might instead grab someone else's valid item; in that case we don't care what it looks like, just that we get a non-null bulk item back)
				if ((blkItem = _bulkImportDataClient.BulkEventImportGetNextItem()) != null)
					break;
				
				tries--;
			}
			Assert.IsNotNull(blkItem, "BulkEventItem is NULL");

			//we can only check the resulting values if we happened to get back OUR valid item, LineNum=2:
			if (blkItem.ItemId == bulkImportItemIds[1])
			{
				//peeking at what the DB call did to line item 1: it should have the expected MsgTxt value:
				var tdrLine1 = _dbTestManager.Retrieve("BulkEventItemTb", bulkImportItemIds[0]);
				Assert.IsTrue(tdrLine1.Get<string>("MsgTxt") == "Start Date must be provided; Publish To Customer must be \"Y\" or \"N\"; Add To Bulletin must be \"Y\" or \"N\"; City not found; Country must be provided; City must be provided; Source must be provided; ");

				//line item 2 should have the expected values:
				var tdrLine2 = _dbTestManager.Retrieve("BulkEventItemTb", bulkImportItemIds[1], new List<string> { "LineNum", "StatC", "MsgTxt" });
				Assert.IsTrue(tdrLine2.Get<int>("LineNum") == 2);
				Assert.IsTrue(tdrLine2.Get<string>("MsgTxt").IsNullOrEmpty());
				Assert.IsTrue(tdrLine2.Get<string>("StatC") == "P");
			}
		}

	    [TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkAgreementItemGetNextSp_BulkCreate_ReturnsSuccess()
	    {
		    var tries = 3;
		    decimal bulkImportItemId = 0;
			BulkContractsImportItem<BulkAgreementItem> blkItem = null;

		    while (tries > 0)
		    {
			    var bulkImportId = InsertRowIntoBulkImportTb("A");		//Agreement Bulk Add
			    var importItem = TempBulkAgreementItem(bulkImportId);	//this line item should succeed (SP does no validations)
			    bulkImportItemId = _dbTestManager.Insert(importItem);	//create in BulkAgreementItemTb

				//the following should return the one valid item, LineNum=1
				//UNLESS the scheduled task service grabbed it before we got to it.
				//ALSO, we might instead grab someone else's valid Agreement item, so we don't care what it looks like, just that we get a non-null bulk item back)
			    if ((blkItem = _bulkImportDataClient.BulkContractsImportGetNextItem<BulkAgreementItem>()) != null)
				    break;

			    tries--;
		    }
			Assert.IsNotNull(blkItem, "BulkAgreementItem is NULL");
			Assert.IsTrue((blkItem.GetType().FullName).Contains("MyOrganization.Editorial.Site.Data.BulkAgreementItem"));

			//we can only check the resulting values if we happened to get back OUR valid item, LineNum=1:
			if (blkItem.ItemId == bulkImportItemId)
			{
				//peeking at what the DB call did to line item 1: it should have the expected StatC and MsgTxt values, plus initial values:
				var tdrLine1 = _dbTestManager.Retrieve("BulkAgreementItemTb", bulkImportItemId);
				Assert.IsTrue(tdrLine1.Get<int>("LineNum") == 1);
				Assert.IsTrue(tdrLine1.Get<string>("MsgTxt").IsNullOrEmpty());
				Assert.IsTrue(tdrLine1.Get<string>("AgreementDocumentFullFilename") == "C:\\agreement.docx");
				Assert.IsTrue(tdrLine1.Get<string>("StatC") == "P");
			}
		}

		[TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkAgreementItemGetNextSp_BulkEdit_ReturnsSuccess()
		{
		    var tries = 3;
		    decimal bulkImportItemId = 0;
			BulkContractsImportItem<BulkAgreementItem> blkItem = null;

			while (tries > 0)
			{
				var bulkImportId = InsertRowIntoBulkImportTb("A", "U");	//Agreement Bulk Update
				var importItem = TempBulkAgreementItem(bulkImportId);	//this line item should succeed (SP does no validations)
				bulkImportItemId = _dbTestManager.Insert(importItem);	//create in BulkAgreementItemTb

				//the following should return the one valid item, LineNum=1
				//UNLESS the scheduled task service grabbed it before we got to it.
				//ALSO, we might instead grab someone else's valid Agreement item, so we don't care what it looks like, just that we get a non-null bulk item back)
				if ((blkItem = _bulkImportDataClient.BulkContractsImportGetNextItem<BulkAgreementItem>()) != null)
					break;

				tries--;
			}
			Assert.IsNotNull(blkItem, "BulkAgreementItem is NULL");
			Assert.IsTrue((blkItem.GetType().FullName).Contains("MyOrganization.Editorial.Site.Data.BulkAgreementItem"));

			//we can only check the resulting values if we happened to get back OUR valid item, LineNum=1:
			if (blkItem.ItemId == bulkImportItemId)
			{
				//peeking at what the DB call did to line item 1: it should have the expected StatC and MsgTxt values, plus initial values:
				var tdrLine1 = _dbTestManager.Retrieve("BulkAgreementItemTb", bulkImportItemId);
				Assert.IsTrue(tdrLine1.Get<int>("LineNum") == 1);
				Assert.IsTrue(tdrLine1.Get<string>("MsgTxt").IsNullOrEmpty());
				Assert.IsTrue(tdrLine1.Get<string>("AgreementDocumentFullFilename") == "C:\\agreement.docx");
				Assert.IsTrue(tdrLine1.Get<string>("StatC") == "P");
			}
		}

		[TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkContentProviderItemGetNextSp_BulkCreate_ReturnsSuccess()
		{
		    var tries = 3;
		    decimal bulkImportItemId = 0;
			BulkContractsImportItem<BulkContentProviderItem> blkItem = null;

			while (tries > 0)
			{
				var bulkImportId = InsertRowIntoBulkImportTb("P");			//ContentProvider Bulk Add
				var importItem = TempBulkContentProviderItem(bulkImportId);	//this line item should succeed (SP does no validations)
				bulkImportItemId = _dbTestManager.Insert(importItem);		//create in BulkContentProviderItemTb

				//the following should return the one valid item, LineNum=1
				//UNLESS the scheduled task service grabbed it before we got to it.
				//ALSO, we might instead grab someone else's valid ContentProvider item, so we don't care what it looks like, just that we get a non-null bulk item back)
				if ((blkItem = _bulkImportDataClient.BulkContractsImportGetNextItem<BulkContentProviderItem>()) != null)
					break;

				tries--;
			}
			Assert.IsNotNull(blkItem, "BulkContentProviderItem is NULL");
			Assert.IsTrue((blkItem.GetType().FullName).Contains("MyOrganization.Editorial.Site.Data.BulkContentProviderItem"));

			//we can only check the resulting values if we happened to get back OUR valid item, LineNum=1:
			if (blkItem.ItemId == bulkImportItemId)
			{
				//peeking at what the DB call did to line item 1: it should have the expected StatC and MsgTxt values, plus initial values:
				var tdrLine1 = _dbTestManager.Retrieve("BulkContentProviderItemTb", bulkImportItemId);
				Assert.IsTrue(tdrLine1.Get<int>("LineNum") == 1);
				Assert.IsTrue(tdrLine1.Get<string>("MsgTxt").IsNullOrEmpty());
				Assert.IsTrue(tdrLine1.Get<string>("ContentProviderName") == "G. Smith");
				Assert.IsTrue(tdrLine1.Get<string>("StatC") == "P");
			}
		}

		[TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkContentProviderItemGetNextSp_BulkEdit_ReturnsSuccess()
		{
		    var tries = 3;
		    decimal bulkImportItemId = 0;
			BulkContractsImportItem<BulkContentProviderItem> blkItem = null;

			while (tries > 0)
			{
				var bulkImportId = InsertRowIntoBulkImportTb("P", "U");		//ContentProvider Bulk Update
				var importItem = TempBulkContentProviderItem(bulkImportId);	//this line item should succeed (SP does no validations)
				bulkImportItemId = _dbTestManager.Insert(importItem);		//create in BulkContentProviderItemTb

				//the following should return the one valid item, LineNum=1
				//UNLESS the scheduled task service grabbed it before we got to it.
				//ALSO, we might instead grab someone else's valid ContentProvider item, so we don't care what it looks like, just that we get a non-null bulk item back)
				if ((blkItem = _bulkImportDataClient.BulkContractsImportGetNextItem<BulkContentProviderItem>()) != null)
					break;

				tries--;
			}
			Assert.IsNotNull(blkItem, "BulkContentProviderItem is NULL");
			Assert.IsTrue((blkItem.GetType().FullName).Contains("MyOrganization.Editorial.Site.Data.BulkContentProviderItem"));

			//we can only check the resulting values if we happened to get back OUR valid item, LineNum=1:
			if (blkItem.ItemId == bulkImportItemId)
			{
				var tdrLine1 = _dbTestManager.Retrieve("BulkContentProviderItemTb", bulkImportItemId);
				Assert.IsTrue(tdrLine1.Get<int>("LineNum") == 1);
				Assert.IsTrue(tdrLine1.Get<string>("MsgTxt").IsNullOrEmpty());
				Assert.IsTrue(tdrLine1.Get<string>("ContentProviderName") == "G. Smith");
				Assert.IsTrue(tdrLine1.Get<string>("StatC") == "P");
			}
		}

        [TestCategory("ExcludeFromBuild"), TestMethod]
		public void BulkContractItemGetNextSp_BulkCreate_ReturnsSuccess()
		{
		    var tries = 3;
		    decimal bulkImportItemId = 0;
			BulkContractsImportItem<BulkContractItem> blkItem = null;

			while (tries > 0)
			{
				var bulkImportId = InsertRowIntoBulkImportTb("C");		//Contract Bulk Add
				var importItem = TempBulkContractItem(bulkImportId);	//this line item should succeed (SP does no validations)
				bulkImportItemId = _dbTestManager.Insert(importItem);	//create in BulkContractItemTb

				//the following should return the one valid item, LineNum=1
				//UNLESS the scheduled task service grabbed it before we got to it.
				//ALSO, we might instead grab someone else's valid Contract item, so we don't care what it looks like, just that we get a non-null bulk item back)
				if ((blkItem = _bulkImportDataClient.BulkContractsImportGetNextItem<BulkContractItem>()) != null)
					break;

				tries--;
			}
			Assert.IsNotNull(blkItem, "BulkContractItem is NULL");
			Assert.IsTrue((blkItem.GetType().FullName).Contains("MyOrganization.Editorial.Site.Data.BulkContractItem"));

			//we can only check the resulting values if we happened to get back OUR valid item, LineNum=1:
			if (blkItem.ItemId == bulkImportItemId)
			{
				//peeking at what the DB call did to line item 1: it should have the expected StatC and MsgTxt values, plus initial values:
				var tdrLine1 = _dbTestManager.Retrieve("BulkContractItemTb", bulkImportItemId);
				Assert.IsTrue(tdrLine1.Get<int>("LineNum") == 1);
				Assert.IsTrue(tdrLine1.Get<string>("MsgTxt").IsNullOrEmpty());
				Assert.IsTrue(tdrLine1.Get<string>("ContractName") == "Awesome Unique Creative Person Contract");
				Assert.IsTrue(tdrLine1.Get<string>("StatC") == "P");
			}
		}

        [TestCategory("ExcludeFromBuild"), TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkContractItemGetNextSp_BulkEdit_ReturnsSuccess()
		{
		    var tries = 3;
		    decimal bulkImportItemId = 0;
			BulkContractsImportItem<BulkContractItem> blkItem = null;

			while (tries > 0)
			{
				var bulkImportId = InsertRowIntoBulkImportTb("C", "U");		//Contract Bulk Update
				var importItem = TempBulkContractItem(bulkImportId);		//this line item should succeed (SP does no validations)
				bulkImportItemId = _dbTestManager.Insert(importItem);	//create in BulkContractItemTb

				//the following should return the one valid item, LineNum=1
				//UNLESS the scheduled task service grabbed it before we got to it.
				//ALSO, we might instead grab someone else's valid Contract item, so we don't care what it looks like, just that we get a non-null bulk item back)
				if ((blkItem = _bulkImportDataClient.BulkContractsImportGetNextItem<BulkContractItem>()) != null)
					break;

				tries--;
			}
			Assert.IsNotNull(blkItem, "BulkContractItem is NULL");
			Assert.IsTrue((blkItem.GetType().FullName).Contains("MyOrganization.Editorial.Site.Data.BulkContractItem"));

			//we can only check the resulting values if we happened to get back OUR valid item, LineNum=1:
			if (blkItem.ItemId == bulkImportItemId)
			{
				//peeking at what the DB call did to line item 1: it should have the expected StatC and MsgTxt values, plus initial values:
				var tdrLine1 = _dbTestManager.Retrieve("BulkContractItemTb", bulkImportItemId);
				Assert.IsTrue(tdrLine1.Get<int>("LineNum") == 1);
				Assert.IsTrue(tdrLine1.Get<string>("MsgTxt").IsNullOrEmpty());
				Assert.IsTrue(tdrLine1.Get<string>("ContractName") == "Awesome Unique Creative Person Contract");
				Assert.IsTrue(tdrLine1.Get<string>("StatC") == "P");
			}
		}

        [TestCategory("ExcludeFromBuild"), TestCategory("BulkImportDataClientTest"), TestMethod]
        public void BulkEventImportUpdateItem_UpdatesEventItem()
        {
			//seed the DB with test data
			var bulkImportId = InsertRowIntoBulkImportTb();				//Event Bulk Add
			var eventItem = TempValidBulkEventItem(bulkImportId);
			var bulkImportItemId = (int) _dbTestManager.Insert(eventItem);

			var blkItem = new BulkImportItem()
            {
				ItemId = bulkImportItemId,
                Event = new Event()
	            {
		            EventId = 21,
					Assignments = new List<Assignment>() {new Assignment() { AsgnI = 22 }}
	            },
                ImportType = BulkImportType.Event
            };
			
            try
            {
                _bulkImportDataClient.BulkEventImportUpdateItem("S", blkItem, "No errors");
            }
            catch (Exception ex)
            {
                Assert.Fail("BulkEventImportUpdateItem failed to update EventItem");
            }

			//peeking at what the DB did to line item 1: it should have our updated values:
			var bulkEventLine = _dbTestManager.Retrieve("BulkEventItemTb", bulkImportItemId);
			Assert.IsTrue(bulkEventLine.Get<string>("StatC") == "S");
			Assert.IsTrue(bulkEventLine.Get<string>("MsgTxt") == "No errors");
			Assert.IsTrue(bulkEventLine.Get<int>("EvntI") == 21);
			Assert.IsTrue(bulkEventLine.Get<int>("AsgnI") == 22);

			//the header row should be of the correct type, and its status should now be "S":
	        var bulkEventHdr = _dbTestManager.Retrieve("BulkImportTb", bulkImportId);
			Assert.IsTrue(bulkEventHdr.Get<string>("ImportTypeC") == "E");
			Assert.IsTrue(bulkEventHdr.Get<string>("StatC") == "S");
		}

        [TestCategory("ExcludeFromBuild"), TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkEventImportUpdateItem_UpdatesShootItem()
		{
			//seed the DB with test data
			var bulkImportId = InsertRowIntoBulkImportTb("S");			//Shoot Bulk Add
			var shootItem = TempValidBulkShootItem(bulkImportId);
			var bulkImportItemId = (int) _dbTestManager.Insert(shootItem);

			var blkItem = new BulkImportItem()
			{
				ItemId = bulkImportItemId,
				Shoot = new Shoot()
				{
					ShootI = 31,
					ShootAssignments = new List<ShootAssignment>() {new ShootAssignment() { AsgnI = 32 }}
				},
				ImportType = BulkImportType.Contour
			};

			try
			{
				_bulkImportDataClient.BulkEventImportUpdateItem("S", blkItem, "No errors");
			}
			catch (Exception ex)
			{
				Assert.Fail("BulkEventImportUpdateItem failed to update ShootItem");
			}

			//peeking at what the DB did to line item 1: it should have our updated values:
			var bulkEventLine = _dbTestManager.Retrieve("BulkEventItemTb", bulkImportItemId);
			Assert.IsTrue(bulkEventLine.Get<string>("StatC") == "S");
			Assert.IsTrue(bulkEventLine.Get<string>("MsgTxt") == "No errors");
			Assert.IsTrue(bulkEventLine.Get<int>("EvntI") == 31);
			Assert.IsTrue(bulkEventLine.Get<int>("AsgnI") == 32);

			//the header row should be of the correct type, and its status should now be "S":
			var bulkEventHdr = _dbTestManager.Retrieve("BulkImportTb", bulkImportId);
			Assert.IsTrue(bulkEventHdr.Get<string>("ImportTypeC") == "S");
			Assert.IsTrue(bulkEventHdr.Get<string>("StatC") == "S");
		}

        [TestCategory("ExcludeFromBuild"), TestCategory("BulkImportDataClientTest"), TestMethod]
	    public void BulkAgreementImportUpdateItem_BulkCreate_ReturnsInvalidStatCValue()
	    {
			var bulkImportId = InsertRowIntoBulkImportTb("A");			//Agreement Bulk Add
			var importItem = TempBulkAgreementItem(bulkImportId);		//this line item should succeed (SP does no validations)
			var bulkImportItemId = (int) _dbTestManager.Insert(importItem);	//create in BulkAgreementItemTb

			try
			{
				//changing the status to "P" is an invalid update for a Bulk Add:
				_bulkImportDataClient.BulkContractsImportUpdateItem("P", BulkType.Agreement, bulkImportItemId, "No errors", null);
			}
			catch (Exception ex)
			{
				Assert.IsTrue(ex.InnerException.Message == "The stored procedure returned a negative return value of -1020");
				return;
			}
			Assert.Fail("BulkAgreementImportUpdateItem did not return error -1020, 'Invalid StatC Value'");
		}

        [TestCategory("ExcludeFromBuild"), TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkAgreementImportUpdateItem_BulkEdit_AllowsStatCUpdateToP()
		{
			var bulkImportId = InsertRowIntoBulkImportTb("A", "U");		//Agreement Bulk Update
			var importItem = TempBulkAgreementItem(bulkImportId);		//this line item should succeed (SP does no validations)
			var bulkImportItemId = (int) _dbTestManager.Insert(importItem);	//create in BulkAgreementItemTb

			try
			{
				//changing the status to "P" is a VALID update for a Bulk Update:
				_bulkImportDataClient.BulkContractsImportUpdateItem("P", BulkType.Agreement, bulkImportItemId, "No errors", null);
			}
			catch (Exception ex)
			{
				Assert.Fail("BulkAgreementImportUpdateItem failed to update StatC to 'P'");
			}
		}

        [TestCategory("ExcludeFromBuild"), TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkAgreementImportUpdateItem_BulkEdit_ReturnsInvalidStatCValue()
		{
			var bulkImportId = InsertRowIntoBulkImportTb("A", "U");		//Agreement Bulk Update
			var importItem = TempBulkAgreementItem(bulkImportId);		//this line item should succeed (SP does no validations)
			var bulkImportItemId = (int) _dbTestManager.Insert(importItem);	//create in BulkAgreementItemTb

			try
			{
				//changing the status to "X" is an invalid update for a Bulk Update:
				_bulkImportDataClient.BulkContractsImportUpdateItem("X", BulkType.Agreement, bulkImportItemId, "No errors", null);
			}
			catch (Exception ex)
			{
				Assert.IsTrue(ex.InnerException.Message == "The stored procedure returned a negative return value of -1020");
				return;
			}
			Assert.Fail("BulkAgreementImportUpdateItem did not return error -1020, 'Invalid StatC Value'");
		}

        [TestCategory("ExcludeFromBuild"), TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkAgreementImportUpdateItem_UpdatesAgreementItem()
		{
			var bulkImportId = InsertRowIntoBulkImportTb("A");			//Agreement Bulk Add
			var importItem = TempBulkAgreementItem(bulkImportId);		//this line item should succeed (SP does no validations)
			var bulkImportItemId = (int) _dbTestManager.Insert(importItem);	//create in BulkAgreementItemTb

			var serviceValues = new Dictionary<string, string>()
				{
					{"AgreementID", "9988"},
					{"ContentProviderName", "R. Linda"},
					{"OracleVendorID", "13579"},
					{"OldValues", "<AgreementDetail/>"},
					{"CreateDate", "10/9/13 2:13 PM"}		//TODO: rename it "CreateUpdateDate" to match DB
				};

			try
			{
				_bulkImportDataClient.BulkContractsImportUpdateItem("S", BulkType.Agreement, bulkImportItemId, "No errors", serviceValues);
			}
			catch (Exception ex)
			{
				Assert.Fail("BulkAgreementImportUpdateItem failed to update AgreementItem");
			}

			//peeking at what the DB did to the line item: it should have our updated values:
			var bulkEventLine = _dbTestManager.Retrieve("BulkAgreementItemTb", bulkImportItemId);
			Assert.IsTrue(bulkEventLine.Get<string>("StatC") == "S");
			Assert.IsTrue(bulkEventLine.Get<string>("MsgTxt") == "No errors");
			Assert.IsTrue(bulkEventLine.Get<long>("AgreementID") == 9988);
			Assert.IsTrue(bulkEventLine.Get<string>("ContentProviderName") == "R. Linda");
			Assert.IsTrue(bulkEventLine.Get<long>("OracleVendorID") == 13579);
			Assert.IsTrue(bulkEventLine.Get<string>("OldValues") == "<AgreementDetail />");
			Assert.IsTrue(bulkEventLine.Get<DateTime>("CreateUpdateDate") == DateTime.Parse("10/9/13 2:13 PM"));

			//the header row should be of the correct type, and its status should now be "S":
			var bulkEventHdr = _dbTestManager.Retrieve("BulkImportTb", bulkImportId);
			Assert.IsTrue(bulkEventHdr.Get<string>("ImportTypeC") == "A");
			Assert.IsTrue(bulkEventHdr.Get<string>("StatC") == "S");
		}

        [TestCategory("ExcludeFromBuild"), TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkContentProviderImportUpdateItem_BulkCreate_ReturnsInvalidStatCValue()
		{
			var bulkImportId = InsertRowIntoBulkImportTb("P");			//ContentProvider Bulk Add
			var importItem = TempBulkContentProviderItem(bulkImportId);		//this line item should succeed (SP does no validations)
			var bulkImportItemId = (int) _dbTestManager.Insert(importItem);	//create in BulkContentProviderItemTb

			try
			{
				//changing the status to "P" is an invalid update for a Bulk Add:
				_bulkImportDataClient.BulkContractsImportUpdateItem("P", BulkType.ContentProvider, bulkImportItemId, "No errors", null);
			}
			catch (Exception ex)
			{
				Assert.IsTrue(ex.InnerException.Message == "The stored procedure returned a negative return value of -1020");
				return;
			}
			Assert.Fail("BulkContentProviderImportUpdateItem did not return error -1020, 'Invalid StatC Value'");
		}

        [TestCategory("ExcludeFromBuild"), TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkContentProviderImportUpdateItem_BulkEdit_AllowsStatCUpdateToP()
		{
			var bulkImportId = InsertRowIntoBulkImportTb("P", "U");		//ContentProvider Bulk Update
			var importItem = TempBulkContentProviderItem(bulkImportId);		//this line item should succeed (SP does no validations)
			var bulkImportItemId = (int) _dbTestManager.Insert(importItem);	//create in BulkContentProviderItemTb

			try
			{
				//changing the status to "P" is a VALID update for a Bulk Update:
				_bulkImportDataClient.BulkContractsImportUpdateItem("P", BulkType.ContentProvider, bulkImportItemId, "No errors", null);
			}
			catch (Exception ex)
			{
				Assert.Fail("BulkContentProviderImportUpdateItem failed to update StatC to 'P'");
			}
		}

        [TestCategory("ExcludeFromBuild"), TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkContentProviderImportUpdateItem_BulkEdit_ReturnsInvalidStatCValue()
		{
			var bulkImportId = InsertRowIntoBulkImportTb("P", "U");		//ContentProvider Bulk Update
			var importItem = TempBulkContentProviderItem(bulkImportId);		//this line item should succeed (SP does no validations)
			var bulkImportItemId = (int) _dbTestManager.Insert(importItem);	//create in BulkContentProviderItemTb

			try
			{
				//changing the status to "X" is an invalid update for a Bulk Update:
				_bulkImportDataClient.BulkContractsImportUpdateItem("X", BulkType.ContentProvider, bulkImportItemId, "No errors", null);
			}
			catch (Exception ex)
			{
				Assert.IsTrue(ex.InnerException.Message == "The stored procedure returned a negative return value of -1020");
				return;
			}
			Assert.Fail("BulkContentProviderImportUpdateItem did not return error -1020, 'Invalid StatC Value'");
		}

        [TestCategory("ExcludeFromBuild"), TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkContentProviderImportUpdateItem_UpdatesContentProviderItem()
		{
			var bulkImportId = InsertRowIntoBulkImportTb("P");				//ContentProvider Bulk Add
			var importItem = TempBulkContentProviderItem(bulkImportId);		//this line item should succeed (SP does no validations)
			var bulkImportItemId = (int) _dbTestManager.Insert(importItem);	//create in BulkContentProviderItemTb

			var serviceValues = new Dictionary<string, string>()
				{
					{"ContentProviderID", "9988"},
					{"OracleVendorID", "13579"},
					{"OldValues", "<ContentProviderDetail/>"},
					{"CreateDate", "10/9/13 2:13 PM"}		//TODO: rename it "CreateUpdateDate" to match DB
				};

			try
			{
				_bulkImportDataClient.BulkContractsImportUpdateItem("S", BulkType.ContentProvider, bulkImportItemId, "No errors", serviceValues);
			}
			catch (Exception ex)
			{
				Assert.Fail("BulkContentProviderImportUpdateItem failed to update ContentProviderItem");
			}

			//peeking at what the DB did to the line item: it should have our updated values:
			var bulkEventLine = _dbTestManager.Retrieve("BulkContentProviderItemTb", bulkImportItemId);
			Assert.IsTrue(bulkEventLine.Get<string>("StatC") == "S");
			Assert.IsTrue(bulkEventLine.Get<string>("MsgTxt") == "No errors");
			Assert.IsTrue(bulkEventLine.Get<long>("ContentProviderID") == 9988);
			Assert.IsTrue(bulkEventLine.Get<long>("OracleVendorID") == 13579);
			Assert.IsTrue(bulkEventLine.Get<string>("OldValues") == "<ContentProviderDetail />");
			Assert.IsTrue(bulkEventLine.Get<DateTime>("CreateUpdateDate") == DateTime.Parse("10/9/13 2:13 PM"));

			//the header row should be of the correct type, and its status should now be "S":
			var bulkEventHdr = _dbTestManager.Retrieve("BulkImportTb", bulkImportId);
			Assert.IsTrue(bulkEventHdr.Get<string>("ImportTypeC") == "P");
			Assert.IsTrue(bulkEventHdr.Get<string>("StatC") == "S");
		}

        [TestCategory("ExcludeFromBuild"), TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkContractImportUpdateItem_BulkCreate_ReturnsInvalidStatCValue()
		{
			var bulkImportId = InsertRowIntoBulkImportTb("C");			//Contract Bulk Add
			var importItem = TempBulkContractItem(bulkImportId);		//this line item should succeed (SP does no validations)
			var bulkImportItemId = (int) _dbTestManager.Insert(importItem);	//create in BulkContractItemTb

			try
			{
				//changing the status to "P" is an invalid update for a Bulk Add:
				_bulkImportDataClient.BulkContractsImportUpdateItem("P", BulkType.Contract, bulkImportItemId, "No errors", null);
			}
			catch (Exception ex)
			{
				Assert.IsTrue(ex.InnerException.Message == "The stored procedure returned a negative return value of -1020");
				return;
			}
			Assert.Fail("BulkContractImportUpdateItem did not return error -1020, 'Invalid StatC Value'");
		}

        [TestCategory("ExcludeFromBuild"), TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkContractImportUpdateItem_BulkEdit_AllowsStatCUpdateToP()
		{
			var bulkImportId = InsertRowIntoBulkImportTb("C", "U");		//Contract Bulk Update
			var importItem = TempBulkContractItem(bulkImportId);		//this line item should succeed (SP does no validations)
			var bulkImportItemId = (int) _dbTestManager.Insert(importItem);	//create in BulkContractItemTb

			try
			{
				//changing the status to "P" is a VALID update for a Bulk Update:
				_bulkImportDataClient.BulkContractsImportUpdateItem("P", BulkType.Contract, bulkImportItemId, "No errors", null);
			}
			catch (Exception ex)
			{
				Assert.Fail("BulkContractImportUpdateItem failed to update StatC to 'P'");
			}
		}

        [TestCategory("ExcludeFromBuild"), TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkContractImportUpdateItem_BulkEdit_ReturnsInvalidStatCValue()
		{
			var bulkImportId = InsertRowIntoBulkImportTb("C", "U");		//Contract Bulk Update
			var importItem = TempBulkContractItem(bulkImportId);		//this line item should succeed (SP does no validations)
			var bulkImportItemId = (int) _dbTestManager.Insert(importItem);	//create in BulkContractItemTb

			try
			{
				//changing the status to "X" is an invalid update for a Bulk Update:
				_bulkImportDataClient.BulkContractsImportUpdateItem("X", BulkType.Contract, bulkImportItemId, "No errors", null);
			}
			catch (Exception ex)
			{
				Assert.IsTrue(ex.InnerException.Message == "The stored procedure returned a negative return value of -1020");
				return;
			}
			Assert.Fail("BulkContractImportUpdateItem did not return error -1020, 'Invalid StatC Value'");
		}

        [TestCategory("ExcludeFromBuild"), TestCategory("BulkImportDataClientTest"), TestMethod]
		public void BulkContractImportUpdateItem_UpdatesContractItem()
		{
			var bulkImportId = InsertRowIntoBulkImportTb("C");			//Contract Bulk Add
			var importItem = TempBulkContractItem(bulkImportId);		//this line item should succeed (SP does no validations)
			var bulkImportItemId = (int) _dbTestManager.Insert(importItem);	//create in BulkContractItemTb

			var serviceValues = new Dictionary<string, string>()
				{
					{"ContractID", "9988"},
					{"ContentProviderName", "R. Linda"},
					{"OracleVendorID", "13579"},
					{"OldValues", "<ContractDetail/>"},
					{"CreateDate", "10/9/13 2:13 PM"}		//TODO: rename it "CreateUpdateDate" to match DB
				};

			try
			{
				_bulkImportDataClient.BulkContractsImportUpdateItem("S", BulkType.Contract, bulkImportItemId, "No errors", serviceValues);
			}
			catch (Exception ex)
			{
				Assert.Fail("BulkContractImportUpdateItem failed to update ContractItem");
			}

			//peeking at what the DB did to the line item: it should have our updated values:
			var bulkEventLine = _dbTestManager.Retrieve("BulkContractItemTb", bulkImportItemId);
			Assert.IsTrue(bulkEventLine.Get<string>("StatC") == "S");
			Assert.IsTrue(bulkEventLine.Get<string>("MsgTxt") == "No errors");
			Assert.IsTrue(bulkEventLine.Get<long>("ContractID") == 9988);
			Assert.IsTrue(bulkEventLine.Get<string>("ContentProviderName") == "R. Linda");
			Assert.IsTrue(bulkEventLine.Get<long>("OracleVendorID") == 13579);
			Assert.IsTrue(bulkEventLine.Get<string>("OldValues") == "<ContractDetail />");
			Assert.IsTrue(bulkEventLine.Get<DateTime>("CreateUpdateDate") == DateTime.Parse("10/9/13 2:13 PM"));

			//the header row should be of the correct type, and its status should now be "S":
			var bulkEventHdr = _dbTestManager.Retrieve("BulkImportTb", bulkImportId);
			Assert.IsTrue(bulkEventHdr.Get<string>("ImportTypeC") == "C");
			Assert.IsTrue(bulkEventHdr.Get<string>("StatC") == "S");
		}

        [TestCategory("ExcludeFromBuild"), TestCategory("BulkImportDataClientTest"), TestMethod]
		public void TestDbTestManager()
        {
			//proof of concept test for DbTestManager

            var tdata1 = new DbTestTableData("BulkImportTb")
				{
					{ "ImportTypeC", 'C' },
					{ "StatC", "X" },
                    { "OrigFileN", "C:\\test.xls" },
                    { "CrtUsrN", "gsmith" },
					{ "CrtD", new DateTime(2013, 9, 5, 9, 0, 0) },
				};            
            var bulkImportId = _dbTestManager.Insert(tdata1); // we'll use the returned ID in the following inserts
            
            var tdata2 = new DbTestTableData("BulkContractItemTb")
                {
                    { "BulkImportId", bulkImportId },
                    { "LineNum", 1 },
                    { "StatC", "F" },
                    { "MsgTxt",	"this one failed" },
                    { "AgreementId", 99 },
                };
            
            var tdata3 = new DbTestTableData("BulkContractItemTb")
                {
                    { "BulkImportId", bulkImportId },
                    { "LineNum", 2 },
                    { "StatC", "S" },
                    { "AgreementId", 100 },
                };

			var bulkImportItemIds = _dbTestManager.InsertRange(tdata2, tdata3).ToList(); // This works too.      
			Debug.Print(bulkImportItemIds.Count().ToString());
        }

		#endregion


		#region Test Helpers

		public const string TempEventsXml = @"
			<BulkItems>
				<BulkItem>
					<EventName>Halloween 2012</EventName>
					<AddtoBulletinY_N>Y</AddtoBulletinY_N>
					<StartDate_mm_dd_yyyy_>01/30/2013</StartDate_mm_dd_yyyy_>
					<EndDate_mm_dd_yyyy_>02/01/2013</EndDate_mm_dd_yyyy_>
					<StartTime_hh_mmxm_>09:10am</StartTime_hh_mmxm_>
					<EndTime_hh_mmxm_>08:30pm</EndTime_hh_mmxm_>
					<EventNotes>NO NOTES HERE</EventNotes>
					<VenueName>Madison Square Garden</VenueName>
					<City>New York</City>
					<State_Province>New York</State_Province>
					<Country>United States</Country>
					<SupplementalCategory1>ACE</SupplementalCategory1>
					<SupplementalCategory2>ENT</SupplementalCategory2>
					<SupplementalCategory3>MUS</SupplementalCategory3>
					<Classification1>Entertainment, Culture and Arts > Entertainment (General)</Classification1>
					<Classification2>Sports > Ice Hockey > NHL</Classification2>
					<Classification3>Sports > Basketball > NBA Pro</Classification3>
					<Keyword1>Concert</Keyword1>
					<Keyword2>Interview</Keyword2>
					<Keyword3>Music</Keyword3>
					<Source>NBAE</Source>
					<ParentEventMEID>175679276</ParentEventMEID>
					<DefaultIPTCCaption>[CELEBRITY] is photographed for [PUBLICATION] on [DATE] in [CITY], City.</DefaultIPTCCaption>
					<InclusionRouting>USA-IN</InclusionRouting>
					<ExclusionRouting>CHINA-OUT</ExclusionRouting>
					<EventRestrictions>evnt rstrcns</EventRestrictions>
					<ContentType>Stills</ContentType>
					<BylineName>S. Grant</BylineName>
					<BylineTitle>Contributor</BylineTitle>
					<RequiresClientApprovalY_N>N</RequiresClientApprovalY_N>
					<ExclusiveCoverageY_N>Y</ExclusiveCoverageY_N>
					<CallForImageY_N>N</CallForImageY_N>
					<EventRestrictionsSpecialInstructions>evnt restrcn spcl instrs</EventRestrictionsSpecialInstructions>
					<DisplayMyOrganizationProductPagesY_N>Y</DisplayMyOrganizationProductPagesY_N>
					<DisplayWireImageHomepageY_N>Y</DisplayWireImageHomepageY_N>
					<DisplayFilmMagicHomepageY_N>N</DisplayFilmMagicHomepageY_N>
					<DisplayDate_mm_dd_yyyy_>09/19/2013</DisplayDate_mm_dd_yyyy_>
					<MyOrganizationWebSiteDestinationY_N>Y</MyOrganizationWebSiteDestinationY_N>
					<WireImageWebSiteDestinationY_N>Y</WireImageWebSiteDestinationY_N>
					<FilmMagicWebSiteDestinationY_N>N</FilmMagicWebSiteDestinationY_N>
					<CredentialIssuedbyMyOrganizationY_N>Y</CredentialIssuedbyMyOrganizationY_N>
				</BulkItem>
			</BulkItems>
		";

		public const string TempShootsXml = @"
			<BulkItems>
				<BulkItem>
					<ShootName>Halloween 2012</ShootName>
					<ShootDateFrom_mm_dd_yyyy_>01/30/2013</ShootDateFrom_mm_dd_yyyy_>
					<ShootDateTo_mm_dd_yyyy_>02/01/2013</ShootDateTo_mm_dd_yyyy_>
					<City>West New York</City>
					<State>New Jersey</State>
					<Country>United States</Country>
					<IPTCCaption>[CELEBRITY] is photographed for [PUBLICATION] on [DATE] in [CITY], City.</IPTCCaption>
					<BylineName>S. Grant</BylineName>
					<BylineTitle>Contributor</BylineTitle>
					<Publication>New York Times</Publication>
					<DatePublished_mm_dd_yyyy_>09/02/2013</DatePublished_mm_dd_yyyy_>
					<Celebrity1>Michael Jackson</Celebrity1>
					<Celebrity2>Madonna</Celebrity2>
					<Celebrity3>Jerry Springer</Celebrity3>
					<Keyword1>Concert</Keyword1>
					<Keyword2>Interview</Keyword2>
					<Keyword3>Music</Keyword3>
					<Source>North America</Source>
					<ShootEditor>Parker</ShootEditor>
					<PhotographerApprovalFlagY_N>Y</PhotographerApprovalFlagY_N>
					<PublicistApprovalFlagY_N>Y</PublicistApprovalFlagY_N>
					<PhotographerRestrictions>phgr restrcns</PhotographerRestrictions>
					<ShootRestrictions>the shoot restrictinos</ShootRestrictions>
					<DomesticEmbargoDate_mm_dd_yyyy_>10-03-2013</DomesticEmbargoDate_mm_dd_yyyy_>
					<IntlEmbargoDate_mm_dd_yyyy_>10-18-2013</IntlEmbargoDate_mm_dd_yyyy_>
					<WorkflowStatus>Pending Art</WorkflowStatus>
					<QtyRcvd>14</QtyRcvd>
					<Format>Digital</Format>
					<ArtTypes>Digital Upload</ArtTypes>
					<DateRcvd_mm_dd_yyyy_>09/19/2013</DateRcvd_mm_dd_yyyy_>
					<B_WorC>Color</B_WorC>
					<Num_Selected>14</Num_Selected>
					<UploadDate_mm_dd_yyyy_>09/20/2013</UploadDate_mm_dd_yyyy_>
					<ModelApprovalFlagY_N>Y</ModelApprovalFlagY_N>
					<CoverApprovalFlagY_N>N</CoverApprovalFlagY_N>
				</BulkItem>
			</BulkItems>
		";

	    public const string TempAgreementsXml = @"
			<BulkItems>
				<BulkItem>
					<AgreementDocumentFullFilename>C:\agreement.docx</AgreementDocumentFullFilename>
					<SignatoryID>1</SignatoryID>
					<SigningDate>10/09/2013</SigningDate>
					<AgreementType>UNI 4.2</AgreementType>
					<Term>One Year</Term>
					<CountryofTaxResidency>India</CountryofTaxResidency>
					<AutoGenerateContracts>Yes</AutoGenerateContracts>
					<CMId></CMId>
					<HomeTerritory></HomeTerritory>
					<TerminationDate></TerminationDate>
					<TerminationReason></TerminationReason>
				</BulkItem>
			</BulkItems>
		";

		//this one is used for a Bulk EDIT:
	    public const string TempContractsXml = @"
			<BulkItems>
				<BulkItem>
					<ContractModel>Standard Phase2</ContractModel>
					<ContractName>Revised Contract Name</ContractName>
					<ContractTitle></ContractTitle>
					<ContractType>Celebrity</ContractType>
					<ContractStatus>Inactive</ContractStatus>
					<ParentSource>ABC</ParentSource>
					<BonusEligibleY_N>N</BonusEligibleY_N>
					<TerritoryRestrictionsRollingEmbargo>123</TerritoryRestrictionsRollingEmbargo>
					<TerritoryRestrictionsEmbargoEndDate_mm_dd_yyyy_>12/30/2013</TerritoryRestrictionsEmbargoEndDate_mm_dd_yyyy_>
					<TerritoryRestrictions>&lt;APPEND&gt;Brazil Country Hide</TerritoryRestrictions>
					<PublicationRestrictionsRollingEmbargo>24</PublicationRestrictionsRollingEmbargo>
					<PublicationRestrictionsEmbargoEndDate_mm_dd_yyyy_>12/31/2013</PublicationRestrictionsEmbargoEndDate_mm_dd_yyyy_>
					<PublicationRestrictions>&lt;REPLACE&gt;Spain Country Hide</PublicationRestrictions>
					<Notes>[CLEAR_DATA]</Notes>
					<CMId>8235000642</CMId>
				</BulkItem>
			</BulkItems>
		";
	    //Territory, Publication and Notes values (for EDIT updates) must start with "[APPEND]" or "[REPLACE]", or contain only "[CLEAR_DATA]"
	    /* additional Bulk CREATE fields;
				<AgreementID></AgreementID>
				<Agent></Agent>
				<AgentStartDate_mm_dd_yyyy_></AgentStartDate_mm_dd_yyyy_>
				<AgentEndDate_mm_dd_yyyy_></AgentEndDate_mm_dd_yyyy_>
		*/

	    public const string TempContentProvidersXml = @"
			<BulkItems>
				<BulkItem>
					<ContentProviderName>Jerry la V.</ContentProviderName>
					<FirstName>Jerry</FirstName>
					<LastName>La V.</LastName>
					<Email>JLaV@kmail.com</Email>
					<StreetAddress>125 Main Street</StreetAddress>
					<StreetAddressLine2>Suite 1999</StreetAddressLine2>
					<City_Town>San Luis Obispo</City_Town>
					<State_Province>California</State_Province>
					<MailingCountry>United States</MailingCountry>
					<Zip_PostalCode>94999</Zip_PostalCode>
					<TelephoneNumber>123-456-7890</TelephoneNumber>
					<FaxNumber>098-765-4321</FaxNumber>
					<WebsiteURL>www.vlaj.com</WebsiteURL>
					<AttributionLine>Stanley K. | John A. | Jerry la V.</AttributionLine>
					<PaymentDocumentName>C:\PaymentDocument.pdf</PaymentDocumentName>
					<BankCountry>France</BankCountry>
					<Currency>Euro (EUR)</Currency>
					<PaymentType>International Wire Transfer</PaymentType>
					<BankName>Bank Leumi</BankName>
					<BankCity_Town>Arles</BankCity_Town>
					<BankState_Province>Provence</BankState_Province>
					<BankABARoutingNumber>135-792-4680</BankABARoutingNumber>
					<BankAccountNumber_IBAN>4994797979</BankAccountNumber_IBAN>
					<BankIDNumber_SWIFT_BSBetc__>325325325325</BankIDNumber_SWIFT_BSBetc__>
					<OracleBankType>United States Requirements</OracleBankType>
					<PaymentNotificationEmail>JLaV@gmail.com</PaymentNotificationEmail>
					<TaxDocumentName>C:\TaxDocument.docx</TaxDocumentName>
					<Paypal_PayoneerEmailAddress>JLaV@ppmail.com</Paypal_PayoneerEmailAddress>
					<TaxCountry>Chile</TaxCountry>
					<TaxBusinessType>S Corporation</TaxBusinessType>
					<TaxpayerID>5554154848</TaxpayerID>
					<PaymentPayeeName>Jerry la V.</PaymentPayeeName>
					<CMId></CMId>
				</BulkItem>
			</BulkItems>
		";

		public decimal InsertRowIntoBulkImportTb(string importTypeC = "E", string editType = "A")
		{
			var tdHdr = new DbTestTableData("BulkImportTb")
				{
					{ "ImportTypeC", importTypeC },
					{ "EditType", editType },
					{ "StatC", "X" },
                    { "OrigFileN", "C:\\test.xls" },
                    { "CrtUsrN", "gsmith" },
					{ "CrtD", DateTime.Now },
				};
			var bulkImportId = _dbTestManager.Insert(tdHdr);
			return bulkImportId;
		}

		public DbTestTableData TempInvalidBulkEventItem(decimal bulkImportId, int lineNum = 1)
		{
			var tdLine = new DbTestTableData("BulkEventItemTb")
                {
                    { "BulkImportId", bulkImportId },
                    { "LineNum", lineNum },
                    { "StatC", "X" },
                    { "EvntN", "Bill & Ted's Bogus Event" },
                };
			return tdLine;
		}

		public DbTestTableData TempValidBulkEventItem(decimal bulkImportId, int lineNum = 1)
		{
			var tdLine = new DbTestTableData("BulkEventItemTb")
                {
                    { "BulkImportId", bulkImportId },
                    { "LineNum", lineNum },
                    { "StatC", "X" },
                    { "EvntN", "Bob & Carol Event" },
					{ "EvntStrtDt", "9/13/2013" },
					{ "PubToCustF", "N" },
					{ "BltnDispF", "Y" },
					{ "EvntCtyN", "Chicago" },
					{ "EvntCntryN", "United States" },
					{ "EvntAsgnSrcN", "AsiaPac" }
                };
			return tdLine;
		}

		public DbTestTableData TempValidBulkShootItem(decimal bulkImportId, int lineNum = 1)
		{
			var tdLine = new DbTestTableData("BulkEventItemTb")
                {
                    { "BulkImportId", bulkImportId },
                    { "LineNum", lineNum },
                    { "StatC", "X" },
                    { "EvntN", "Bob & Carol's Shoot" },
					{ "EvntStrtDt", "10/7/2013" },
					{ "PubToCustF", "N" },
					{ "BltnDispF", "Y" },
					{ "EvntCtyN", "Chicago" },
					{ "EvntCntryN", "United States" },
					{ "EvntAsgnSrcN", "AsiaPac" }
                };
			return tdLine;
		}

		public DbTestTableData TempBulkAgreementItem(decimal bulkImportId, int lineNum = 1)
		{
			var tdLine = new DbTestTableData("BulkAgreementItemTb")
                {
                    { "BulkImportId", bulkImportId },
                    { "LineNum", lineNum },
                    { "StatC", "X" },
                    { "AgreementDocumentFullFilename", "C:\\agreement.docx" },
                    { "ContentProviderID", "100" },
                    { "SigningDate", "9/1/13" },
                    { "AgreementType", "UNI 4.2" },
                    { "Term", "Three months" },
                    { "TaxCountry", "Australia" },
                    { "AutogenerateContracts", "No" }
				};
			return tdLine;
		}

		public DbTestTableData TempBulkContentProviderItem(decimal bulkImportId, int lineNum = 1)
		{
			var tdLine = new DbTestTableData("BulkContentProviderItemTb")
                {
                    { "BulkImportId", bulkImportId },
                    { "LineNum", lineNum },
                    { "StatC", "X" },
                    { "ContentProviderName", "G. Smith" },
                    { "FirstName", "Gordon" },
                    { "LastName", "Smith" },
                    { "Email", "gsmith@MyOrganization.com" },
                    { "StreetAddress", "500 Main Street" },
                    { "AddressLine2", "Apt. 1" },
                    { "CityTown", "West New York" },
                    { "StateProvince", "NJ" },
                    { "MailingCountry", "United States" },
                    { "PostalCode", "04047" },
                    { "TelephoneNumber", "201-555-1234" },
                    { "FaxNumber", "201-555-2345" },
                    { "WebsiteURL", "www.westnewyork.com" },
                    { "AttributionLine", "Gordon Smith | G. Smith" },
                    { "PaymentDocumentFullFilename", "C:\\PaymentDocument.docx" },
                    { "BankCountry", "Cayman Islands" },
                    { "Currency", "US Dollar (USD)" },
                    { "PaymentType", "International Wire Transfer" },
                    { "BankName", "JP Morgan hase" },
                    { "BankCityTown", "Newark" },
                    { "BankStateProvince", "DE" },
                    { "BankABARoutingNumber", "222-3-4445555" },
                    { "BankAccountNumber", "12345" },
                    { "BankIDNumber", "987654000" },
                    { "OracleBankType", "United States Requirements" },
                    { "PaymentNotificationEmail", "gsmith@totallywrong.com" },
                    { "TaxDocumentFullFilename", "C:\\TaxDocument.xlsx" },
                    { "PaypalEmail", "gordon@paypal.com" },
                    { "TaxCountry", "Cayman Islands" },
                    { "TaxBusinessType", "Limited Liability Company" },
                    { "TaxpayerID", "543678912" },
                    { "PaymentPayeeName", "G. Smith" }
				};
			return tdLine;
		}

		public DbTestTableData TempBulkContractItem(decimal bulkImportId, int lineNum = 1)
		{
			var tdLine = new DbTestTableData("BulkContractItemTb")
                {
                    { "BulkImportId", bulkImportId },
                    { "LineNum", lineNum },
                    { "StatC", "X" },
                    { "AgreementID", DBNull.Value },
                    { "ContractModel", "Flickr" },
                    { "ContractName", "Awesome Unique Creative Person Contract" },
                    { "ContractTitle", "Contributor" },
                    { "ContractType", "Photographer-Shoot" },
                    { "ContractStatus", "Active" },
                    { "ParentSource", "My Organization" },
                    { "BonusEligibleY_N", "Y" },
                    { "Agent", "567" },
                    { "AgentStartDate", "8/15/13" },
                    { "AgentEndDate", "9/15/13" },
                    { "TerritoryRestrictionsRollingEmbargo", "123" },
                    { "TerritoryRestrictionsEmbargoEndDate", "12/31/13" },
                    { "TerritoryRestrictions", "Russian Federation Country Hide|France Country Hide" },
                    { "PublicationRestrictionsRollingEmbargo", "123" },
                    { "PublicationRestrictionsEmbargoEndDate", "11/30/13" },
                    { "PublicationRestrictions", "20/20 Publication Block|20th Century Fox Studios Publication Hide" },
                    { "Notes", "this is a run-on note, going everywhere yet nowhere" }
				};
			return tdLine;
		}
		#endregion
	}
}
