using System;
using System.Text;

using TotalBase;
using TotalBase.Enums;
using TotalModel.Models;

namespace TotalDAL.Helpers.SqlProgrammability.Productions
{
    public class PlannedOrder
    {
        private readonly TotalSmartPortalEntities totalSmartPortalEntities;

        public PlannedOrder(TotalSmartPortalEntities totalSmartPortalEntities)
        {
            this.totalSmartPortalEntities = totalSmartPortalEntities;
        }

        public void RestoreProcedure()
        {
            this.GetPlannedOrderIndexes();

            this.GetPlannedOrderViewDetails();
            this.PlannedOrderSaveRelative();
            this.PlannedOrderPostSaveValidate();

            this.PlannedOrderApproved();
            this.PlannedOrderEditable();
            this.PlannedOrderVoidable();

            this.PlannedOrderToggleApproved();
            this.PlannedOrderToggleVoid();
            this.PlannedOrderToggleVoidDetail();

            this.PlannedOrderInitReference();

            this.GetPlannedOrderLogs();
        }


        private void GetPlannedOrderIndexes()
        {
            string queryString;

            queryString = " @AspUserID nvarchar(128), @FromDate DateTime, @ToDate DateTime " + "\r\n";
            queryString = queryString + " WITH ENCRYPTION " + "\r\n";
            queryString = queryString + " AS " + "\r\n";
            queryString = queryString + "    BEGIN " + "\r\n";

            queryString = queryString + "       SELECT      PlannedOrders.PlannedOrderID, CAST(PlannedOrders.EntryDate AS DATE) AS EntryDate, PlannedOrders.Reference, PlannedOrders.Code, Customers.Name AS CustomerName, PlannedOrders.DeliveryDate, PlannedOrders.Caption, Locations.Code AS LocationCode, ISNULL(VoidTypes.Name, CASE PlannedOrders.InActivePartial WHEN 1 THEN N'Hủy một phần đh' ELSE N'' END) AS VoidTypeName, PlannedOrders.Description, PlannedOrders.TotalQuantity, PlannedOrders.Approved, PlannedOrders.InActive, PlannedOrders.InActivePartial " + "\r\n";
            queryString = queryString + "       FROM        PlannedOrders " + "\r\n";
            queryString = queryString + "                   INNER JOIN Locations ON PlannedOrders.EntryDate >= @FromDate AND PlannedOrders.EntryDate <= @ToDate AND PlannedOrders.OrganizationalUnitID IN (SELECT AccessControls.OrganizationalUnitID FROM AccessControls INNER JOIN AspNetUsers ON AccessControls.UserID = AspNetUsers.UserID WHERE AspNetUsers.Id = @AspUserID AND AccessControls.NMVNTaskID = " + (int)TotalBase.Enums.GlobalEnums.NmvnTaskID.PlannedOrder + " AND AccessControls.AccessLevel > 0) AND Locations.LocationID = PlannedOrders.LocationID " + "\r\n";
            queryString = queryString + "                   LEFT JOIN VoidTypes ON PlannedOrders.VoidTypeID = VoidTypes.VoidTypeID" + "\r\n";
            queryString = queryString + "                   LEFT JOIN Customers ON PlannedOrders.CustomerID = Customers.CustomerID " + "\r\n";
            queryString = queryString + "       " + "\r\n";

            queryString = queryString + "    END " + "\r\n";

            this.totalSmartPortalEntities.CreateStoredProcedure("GetPlannedOrderIndexes", queryString);
        }

        #region X


        private void GetPlannedOrderViewDetails()
        {
            string queryString;
            SqlProgrammability.Inventories.Inventories inventories = new Inventories.Inventories(this.totalSmartPortalEntities);

            queryString = " @PlannedOrderID Int " + "\r\n";
            queryString = queryString + " WITH ENCRYPTION " + "\r\n";
            queryString = queryString + " AS " + "\r\n";
            queryString = queryString + "    BEGIN " + "\r\n";

            queryString = queryString + "       SELECT      PlannedOrderDetails.PlannedOrderDetailID, PlannedOrderDetails.PlannedOrderID, " + "\r\n";
            queryString = queryString + "                   Commodities.CommodityID, Commodities.Code AS CommodityCode, Commodities.Name AS CommodityName, PlannedOrderDetails.CommodityTypeID, PlannedOrderDetails.PiecePerPack, " + "\r\n";
            queryString = queryString + "                   PlannedOrderDetails.CombineIndex, PlannedOrderDetails.MoldID, Molds.Code AS MoldCode, PlannedOrderDetails.MoldQuantity, PlannedOrderDetails.BomID, Boms.Code AS BomCode, Boms.Name AS BomName, PlannedOrderDetails.BlockUnit, PlannedOrderDetails.BlockQuantity, " + "\r\n";
            queryString = queryString + "                   VoidTypes.VoidTypeID, VoidTypes.Code AS VoidTypeCode, VoidTypes.Name AS VoidTypeName, VoidTypes.VoidClassID, " + "\r\n";
            queryString = queryString + "                   PlannedOrderDetails.QuantityRequested, PlannedOrderDetails.QuantityOnhand, PlannedOrderDetails.Quantity, PlannedOrderDetails.InActivePartial, PlannedOrderDetails.InActivePartialDate, PlannedOrderDetails.Specs, PlannedOrderDetails.Remarks " + "\r\n";
            queryString = queryString + "       FROM        PlannedOrderDetails " + "\r\n";
            queryString = queryString + "                   INNER JOIN Commodities ON PlannedOrderDetails.PlannedOrderID = @PlannedOrderID AND PlannedOrderDetails.CommodityID = Commodities.CommodityID " + "\r\n";
            queryString = queryString + "                   INNER JOIN Molds ON PlannedOrderDetails.MoldID = Molds.MoldID " + "\r\n";
            queryString = queryString + "                   INNER JOIN Boms ON PlannedOrderDetails.BomID = Boms.BomID " + "\r\n";
            queryString = queryString + "                   LEFT JOIN VoidTypes ON PlannedOrderDetails.VoidTypeID = VoidTypes.VoidTypeID " + "\r\n";

            queryString = queryString + "    END " + "\r\n";

            this.totalSmartPortalEntities.CreateStoredProcedure("GetPlannedOrderViewDetails", queryString);
        }

        private void PlannedOrderSaveRelative()
        {
            string queryString = " @EntityID int, @SaveRelativeOption int " + "\r\n"; //SaveRelativeOption: 1: Update, -1:Undo
            queryString = queryString + " WITH ENCRYPTION " + "\r\n";
            queryString = queryString + " AS " + "\r\n";
            queryString = queryString + "    BEGIN " + "\r\n";
            queryString = queryString + "       IF ((SELECT Approved FROM PlannedOrders WHERE PlannedOrderID = @EntityID AND Approved = 1) = 1) " + "\r\n";
            queryString = queryString + "           BEGIN " + "\r\n";
            queryString = queryString + "               UPDATE      PlannedOrders  SET Approved = 0 WHERE PlannedOrderID = @EntityID AND Approved = 1" + "\r\n"; //CLEAR APPROVE BEFORE CALL PlannedOrderToggleApproved
            queryString = queryString + "               IF @@ROWCOUNT = 1 " + "\r\n";
            queryString = queryString + "                   EXEC        PlannedOrderToggleApproved @EntityID, 1 " + "\r\n";
            queryString = queryString + "               ELSE " + "\r\n";
            queryString = queryString + "                   BEGIN " + "\r\n";
            queryString = queryString + "                       DECLARE     @msg NVARCHAR(300) = N'Dữ liệu không tồn tại hoặc đã duyệt'; " + "\r\n";
            queryString = queryString + "                       THROW       61001,  @msg, 1; " + "\r\n";
            queryString = queryString + "                   END " + "\r\n";
            queryString = queryString + "           END " + "\r\n";
            queryString = queryString + "    END " + "\r\n";

            this.totalSmartPortalEntities.CreateStoredProcedure("PlannedOrderSaveRelative", queryString);
        }

        private void PlannedOrderPostSaveValidate()
        {
            string[] queryArray = new string[0];

            //queryArray[0] = " SELECT TOP 1 @FoundEntity = 'TEST Date: ' + CAST(EntryDate AS nvarchar) FROM PlannedOrders WHERE PlannedOrderID = @EntityID "; //FOR TEST TO BREAK OUT WHEN SAVE -> CHECK ROLL BACK OF TRANSACTION
            //queryArray[0] = " SELECT TOP 1 @FoundEntity = 'Service Date: ' + CAST(ServiceInvoices.EntryDate AS nvarchar) FROM PlannedOrders INNER JOIN PlannedOrders AS ServiceInvoices ON PlannedOrders.PlannedOrderID = @EntityID AND PlannedOrders.ServiceInvoiceID = ServiceInvoices.PlannedOrderID AND PlannedOrders.EntryDate < ServiceInvoices.EntryDate ";

            this.totalSmartPortalEntities.CreateProcedureToCheckExisting("PlannedOrderPostSaveValidate", queryArray);
        }




        private void PlannedOrderApproved()
        {
            string[] queryArray = new string[1];

            queryArray[0] = " SELECT TOP 1 @FoundEntity = PlannedOrderID FROM PlannedOrders WHERE PlannedOrderID = @EntityID AND Approved = 1";

            this.totalSmartPortalEntities.CreateProcedureToCheckExisting("PlannedOrderApproved", queryArray);
        }


        private void PlannedOrderEditable()
        {
            string[] queryArray = new string[3];

            queryArray[0] = " SELECT TOP 1 @FoundEntity = PlannedOrderID FROM PlannedOrders WHERE PlannedOrderID = @EntityID AND (InActive = 1 OR InActivePartial = 1)"; //Don't allow approve after void
            queryArray[1] = " SELECT TOP 1 @FoundEntity = PlannedOrderID FROM ProductionOrderDetails WHERE PlannedOrderID = @EntityID ";
            queryArray[2] = " SELECT TOP 1 @FoundEntity = PlannedOrderID FROM MaterialIssueDetails WHERE PlannedOrderID = @EntityID ";

            this.totalSmartPortalEntities.CreateProcedureToCheckExisting("PlannedOrderEditable", queryArray);
        }

        private void PlannedOrderVoidable()
        {
            string[] queryArray = new string[1];

            queryArray[0] = " SELECT TOP 1 @FoundEntity = PlannedOrderID FROM PlannedOrders WHERE PlannedOrderID = @EntityID AND Approved = 0"; //Must approve in order to allow void

            this.totalSmartPortalEntities.CreateProcedureToCheckExisting("PlannedOrderVoidable", queryArray);
        }

        private void PlannedOrderToggleApproved()
        {
            string queryString = " @EntityID int, @Approved bit " + "\r\n";
            queryString = queryString + " WITH ENCRYPTION " + "\r\n";
            queryString = queryString + " AS " + "\r\n";

            queryString = queryString + "       UPDATE      PlannedOrders  SET Approved = @Approved, ApprovedDate = GetDate() WHERE PlannedOrderID = @EntityID AND Approved = ~@Approved" + "\r\n";

            queryString = queryString + "       IF @@ROWCOUNT = 1 " + "\r\n";
            queryString = queryString + "           BEGIN " + "\r\n";
            queryString = queryString + "               UPDATE          PlannedOrderDetails  SET Approved = @Approved WHERE PlannedOrderID = @EntityID ; " + "\r\n";

            #region INIT FirmOrders
            queryString = queryString + "               IF (@Approved = 1) " + "\r\n";
            queryString = queryString + "                   BEGIN " + "\r\n";

            queryString = queryString + "                       DECLARE         @CombineIndex int, @PriorCombineIndex int, @PlannedOrderDetailID int, @FirmOrderID int; " + "\r\n";

            queryString = queryString + "                       DECLARE         CURSORPlannedOrderDetails CURSOR LOCAL FOR SELECT CombineIndex, PlannedOrderDetailID FROM PlannedOrderDetails WHERE PlannedOrderID = @EntityID ORDER BY CombineIndex, PlannedOrderDetailID; " + "\r\n";
            queryString = queryString + "                       OPEN            CURSORPlannedOrderDetails; " + "\r\n";
            queryString = queryString + "                       FETCH NEXT FROM CURSORPlannedOrderDetails INTO @CombineIndex, @PlannedOrderDetailID; " + "\r\n";

            queryString = queryString + "                       WHILE @@FETCH_STATUS = 0   " + "\r\n";
            queryString = queryString + "                           BEGIN " + "\r\n";
            queryString = queryString + "                               IF (@PriorCombineIndex <> @CombineIndex OR @CombineIndex IS NULL OR (NOT @CombineIndex IS NULL AND @PriorCombineIndex IS NULL)) " + "\r\n";
            queryString = queryString + "                                   BEGIN " + "\r\n"; //SHOULD KEEP BomID = 1 IN BOM LIST (Boms): HERE: AT FIRST, WE SET BomID = 1 AS FIRST INIT (DEFAULT ONLY), THEN: IT WILL BE SET TO THE CORRECT BON LATER
            queryString = queryString + "                                       INSERT INTO     FirmOrders         (EntryDate, Reference, Code, VoucherDate, DeliveryDate, Purposes, PlannedOrderID,      BomID,      BlockUnit,      BlockQuantity, CustomerID, UserID, PreparedPersonID, OrganizationalUnitID, LocationID, ApproverID, TotalQuantity, TotalQuantitySemifinished,         Specs,         Specification, Description, Remarks, CreatedDate, EditedDate, Approved, ApprovedDate, VoidTypeID, InActive, InActivePartial, InActiveDate) " + "\r\n";
            queryString = queryString + "                                       SELECT                              EntryDate, Reference, Code, VoucherDate, DeliveryDate, Purposes, PlannedOrderID, 1 AS BomID, 0 AS BlockUnit, 0 AS BlockQuantity, CustomerID, UserID, PreparedPersonID, OrganizationalUnitID, LocationID, ApproverID, TotalQuantity, TotalQuantitySemifinished, NULL AS Specs, NULL AS Specification, Description, Remarks, CreatedDate, EditedDate, Approved, ApprovedDate, VoidTypeID, InActive, InActivePartial, InActiveDate FROM PlannedOrders WHERE PlannedOrderID = @EntityID; " + "\r\n";
            queryString = queryString + "                                       SELECT          @FirmOrderID    =   SCOPE_IDENTITY(); " + "\r\n";
            queryString = queryString + "                                       SET             @PriorCombineIndex = @CombineIndex; " + "\r\n";
            queryString = queryString + "                                   END " + "\r\n";

            queryString = queryString + "                               INSERT INTO     FirmOrderDetails   (FirmOrderID, EntryDate, PlannedOrderID, PlannedOrderDetailID, LocationID, CustomerID, CommodityID, CommodityTypeID, PiecePerPack, BomID, BlockUnit, BlockQuantity, MoldID, MoldQuantity, DeliveryDate, QuantityRequested, QuantityOnhand, Quantity, QuantitySemifinished, Specs, Description, Remarks, VoidTypeID, Approved, InActive, InActivePartial, InActivePartialDate) " + "\r\n";
            queryString = queryString + "                               SELECT                             @FirmOrderID, EntryDate, PlannedOrderID, PlannedOrderDetailID, LocationID, CustomerID, CommodityID, CommodityTypeID, PiecePerPack, BomID, BlockUnit, BlockQuantity, MoldID, MoldQuantity, DeliveryDate, QuantityRequested, QuantityOnhand, Quantity, QuantitySemifinished, Specs, Description, Remarks, VoidTypeID, Approved, InActive, InActivePartial, InActivePartialDate FROM PlannedOrderDetails WHERE PlannedOrderDetailID = @PlannedOrderDetailID; " + "\r\n";

            queryString = queryString + "                               FETCH NEXT FROM CURSORPlannedOrderDetails INTO @CombineIndex, @PlannedOrderDetailID; " + "\r\n";
            queryString = queryString + "                           END " + "\r\n";

            queryString = queryString + "                       UPDATE          FirmOrders SET FirmOrders.TotalQuantity = FirmOrderDetails.TotalQuantity, FirmOrders.TotalQuantitySemifinished = FirmOrderDetails.TotalQuantitySemifinished, FirmOrders.BomID = FirmOrderDetails.BomID, FirmOrders.BlockUnit = FirmOrderDetails.BlockUnit, FirmOrders.BlockQuantity = FirmOrderDetails.BlockQuantity, FirmOrders.Specs = FirmOrderDetails.Specs, FirmOrders.Specification = FirmOrderDetails.Description " + "\r\n";
            queryString = queryString + "                       FROM            FirmOrders " + "\r\n";
            queryString = queryString + "                                       INNER JOIN (SELECT FirmOrderID, SUM(Quantity) AS TotalQuantity, SUM(QuantitySemifinished) AS TotalQuantitySemifinished, MIN(BomID) AS BomID, MIN(BlockUnit) AS BlockUnit, MIN(BlockQuantity) AS BlockQuantity, MIN(Description) AS Description, MIN(Specs) AS Specs FROM FirmOrderDetails WHERE PlannedOrderID = @EntityID GROUP BY FirmOrderID) FirmOrderDetails ON FirmOrders.PlannedOrderID = @EntityID AND FirmOrders.FirmOrderID = FirmOrderDetails.FirmOrderID; " + "\r\n";


            queryString = queryString + "                       INSERT INTO     FirmOrderMaterials (FirmOrderID, PlannedOrderID, EntryDate, LocationID, CustomerID, BomID, BlockUnit, BlockQuantity, MaterialID, Quantity, QuantityIssued, VoidTypeID, Approved, InActive, InActivePartial, InActivePartialDate) " + "\r\n";
            queryString = queryString + "                       SELECT          FirmOrderDetails.FirmOrderID, FirmOrderDetails.PlannedOrderID, FirmOrderDetails.EntryDate, FirmOrderDetails.LocationID, FirmOrderDetails.CustomerID, FirmOrderDetails.BomID, FirmOrderDetails.BlockUnit, FirmOrderDetails.BlockQuantity, BomDetails.MaterialID, ROUND(SUM(FirmOrderDetails.Quantity * FirmOrderDetails.BlockQuantity / FirmOrderDetails.BlockUnit), " + (int)GlobalEnums.rndQuantity + ") AS Quantity, 0 AS QuantityIssued, FirmOrderDetails.VoidTypeID, FirmOrderDetails.Approved, FirmOrderDetails.InActive, FirmOrderDetails.InActivePartial, FirmOrderDetails.InActivePartialDate " + "\r\n";
            queryString = queryString + "                       FROM            FirmOrderDetails INNER JOIN BomDetails ON FirmOrderDetails.PlannedOrderID = @EntityID AND FirmOrderDetails.BomID = BomDetails.BomID " + "\r\n";
            queryString = queryString + "                       GROUP BY        FirmOrderDetails.FirmOrderID, FirmOrderDetails.PlannedOrderID, FirmOrderDetails.EntryDate, FirmOrderDetails.LocationID, FirmOrderDetails.CustomerID, FirmOrderDetails.BomID, FirmOrderDetails.BlockUnit, FirmOrderDetails.BlockQuantity, BomDetails.MaterialID, FirmOrderDetails.VoidTypeID, FirmOrderDetails.Approved, FirmOrderDetails.InActive, FirmOrderDetails.InActivePartial, FirmOrderDetails.InActivePartialDate; " + "\r\n";

            queryString = queryString + "                   END " + "\r\n";

            queryString = queryString + "               ELSE " + "\r\n";
            queryString = queryString + "                   BEGIN " + "\r\n";
            queryString = queryString + "                       DELETE FROM     FirmOrderMaterials WHERE PlannedOrderID = @EntityID ; " + "\r\n";
            queryString = queryString + "                       DELETE FROM     FirmOrderDetails WHERE PlannedOrderID = @EntityID ; " + "\r\n";
            queryString = queryString + "                       DELETE FROM     FirmOrders WHERE PlannedOrderID = @EntityID ; " + "\r\n";
            queryString = queryString + "                   END " + "\r\n";
            #endregion INIT FirmOrders

            queryString = queryString + "           END " + "\r\n";
            queryString = queryString + "       ELSE " + "\r\n";
            queryString = queryString + "           BEGIN " + "\r\n";
            queryString = queryString + "               DECLARE     @msg NVARCHAR(300) = N'Dữ liệu không tồn tại hoặc đã ' + iif(@Approved = 0, N'hủy', '')  + N' duyệt' ; " + "\r\n";
            queryString = queryString + "               THROW       61001,  @msg, 1; " + "\r\n";
            queryString = queryString + "           END " + "\r\n";

            this.totalSmartPortalEntities.CreateStoredProcedure("PlannedOrderToggleApproved", queryString);
        }

        private void PlannedOrderToggleVoid()
        {
            string queryString = " @EntityID int, @InActive bit, @VoidTypeID int " + "\r\n";
            queryString = queryString + " WITH ENCRYPTION " + "\r\n";
            queryString = queryString + " AS " + "\r\n";

            queryString = queryString + "       UPDATE      PlannedOrders  SET InActive = @InActive, InActiveDate = GetDate(), VoidTypeID = IIF(@InActive = 1, @VoidTypeID, NULL) WHERE PlannedOrderID = @EntityID AND InActive = ~@InActive" + "\r\n";

            queryString = queryString + "       IF @@ROWCOUNT = 1 " + "\r\n";
            queryString = queryString + "           BEGIN " + "\r\n";
            queryString = queryString + "               UPDATE          FirmOrders              SET InActive = @InActive WHERE PlannedOrderID = @EntityID ; " + "\r\n";
            queryString = queryString + "               UPDATE          FirmOrderDetails        SET InActive = @InActive WHERE PlannedOrderID = @EntityID ; " + "\r\n";
            queryString = queryString + "               UPDATE          FirmOrderMaterials      SET InActive = @InActive WHERE PlannedOrderID = @EntityID ; " + "\r\n";

            queryString = queryString + "               UPDATE          PlannedOrderDetails     SET InActive = @InActive WHERE PlannedOrderID = @EntityID ; " + "\r\n";            
            queryString = queryString + "           END " + "\r\n";
            queryString = queryString + "       ELSE " + "\r\n";
            queryString = queryString + "           BEGIN " + "\r\n";
            queryString = queryString + "               DECLARE     @msg NVARCHAR(300) = N'Dữ liệu không tồn tại hoặc đã ' + iif(@InActive = 0, 'phục hồi lệnh', '')  + ' hủy' ; " + "\r\n";
            queryString = queryString + "               THROW       61001,  @msg, 1; " + "\r\n";
            queryString = queryString + "           END " + "\r\n";


            this.totalSmartPortalEntities.CreateStoredProcedure("PlannedOrderToggleVoid", queryString);
        }

        private void PlannedOrderToggleVoidDetail()
        {
            string queryString = " @EntityID int, @EntityDetailID int, @InActivePartial bit, @VoidTypeID int " + "\r\n";
            queryString = queryString + " WITH ENCRYPTION " + "\r\n";
            queryString = queryString + " AS " + "\r\n";

            queryString = queryString + "       UPDATE      FirmOrderDetails        SET InActivePartial = @InActivePartial, InActivePartialDate = GetDate(), VoidTypeID = IIF(@InActivePartial = 1, @VoidTypeID, NULL) WHERE PlannedOrderID = @EntityID AND PlannedOrderDetailID = @EntityDetailID AND InActivePartial = ~@InActivePartial ; " + "\r\n";
            queryString = queryString + "       UPDATE      FirmOrderMaterials      SET InActivePartial = @InActivePartial, InActivePartialDate = GetDate(), VoidTypeID = IIF(@InActivePartial = 1, @VoidTypeID, NULL) WHERE FirmOrderID = (SELECT FirmOrderID FROM FirmOrderDetails WHERE PlannedOrderID = @EntityID AND PlannedOrderDetailID = @EntityDetailID) AND InActivePartial = ~@InActivePartial ; " + "\r\n";

            queryString = queryString + "       UPDATE      PlannedOrderDetails     SET InActivePartial = @InActivePartial, InActivePartialDate = GetDate(), VoidTypeID = IIF(@InActivePartial = 1, @VoidTypeID, NULL) WHERE PlannedOrderID = @EntityID AND PlannedOrderDetailID = @EntityDetailID AND InActivePartial = ~@InActivePartial ; " + "\r\n";            

            queryString = queryString + "       IF @@ROWCOUNT = 1 " + "\r\n";
            queryString = queryString + "           BEGIN " + "\r\n";
            queryString = queryString + "               DECLARE         @MaxInActivePartial bit     SET @MaxInActivePartial = (SELECT MAX(CAST(InActivePartial AS int)) FROM PlannedOrderDetails WHERE PlannedOrderID = @EntityID) ;" + "\r\n";
            queryString = queryString + "               UPDATE          PlannedOrders  SET InActivePartial = @MaxInActivePartial WHERE PlannedOrderID = @EntityID ; " + "\r\n";
            queryString = queryString + "           END " + "\r\n";
            queryString = queryString + "       ELSE " + "\r\n";
            queryString = queryString + "           BEGIN " + "\r\n";
            queryString = queryString + "               DECLARE     @msg NVARCHAR(300) = N'Dữ liệu không tồn tại hoặc đã ' + iif(@InActivePartial = 0, 'phục hồi lệnh', '')  + ' hủy' ; " + "\r\n";
            queryString = queryString + "               THROW       61001,  @msg, 1; " + "\r\n";
            queryString = queryString + "           END " + "\r\n";
            this.totalSmartPortalEntities.CreateStoredProcedure("PlannedOrderToggleVoidDetail", queryString);
        }


        private void PlannedOrderInitReference()
        {
            SimpleInitReference simpleInitReference = new SimpleInitReference("PlannedOrders", "PlannedOrderID", "Reference", ModelSettingManager.ReferenceLength, ModelSettingManager.ReferencePrefix(GlobalEnums.NmvnTaskID.PlannedOrder));
            this.totalSmartPortalEntities.CreateTrigger("PlannedOrderInitReference", simpleInitReference.CreateQuery());
        }

        private void GetPlannedOrderLogs()
        {
            string queryString = " @PlannedOrderID int, @FirmOrderID int " + "\r\n";
            queryString = queryString + " WITH ENCRYPTION " + "\r\n";
            queryString = queryString + " AS " + "\r\n";
            queryString = queryString + "    BEGIN " + "\r\n";

            queryString = queryString + "       DECLARE         @FirmOrders TABLE (FirmOrderID int NOT NULL PRIMARY KEY)" + "\r\n";
            queryString = queryString + "       IF (NOT @FirmOrderID IS NULL) " + "\r\n";
            queryString = queryString + "           INSERT INTO @FirmOrders (FirmOrderID) VALUES (@FirmOrderID) " + "\r\n";
            queryString = queryString + "       ELSE " + "\r\n";
            queryString = queryString + "           INSERT INTO @FirmOrders (FirmOrderID) SELECT FirmOrderID FROM FirmOrders WHERE PlannedOrderID = @PlannedOrderID " + "\r\n";

            queryString = queryString + "       SELECT          MaterialIssueDetails.MaterialIssueDetailID, MaterialIssueDetails.EntryDate, ProductionLines.Code AS ProductionLineCode, IIF(Workshifts.EntryDate IS NULL, NULL, MaterialIssueDetails.EntryDate) AS MaterialIssueWorkshiftEntryDate, Workshifts.Code AS MaterialIssueWorkshiftCode, GoodsReceiptDetails.Reference AS GoodsReceiptReference, GoodsReceiptDetails.BatchEntryDate, MaterialIssueDetails.Quantity AS MaterialIssueDetailQuantity, NULL AS SemifinishedProductWorkshiftEntryDate, NULL AS SemifinishedProductWorkshiftCode, NULL AS CommoditiyCode, NULL AS Quantity, NULL AS Weights " + "\r\n";
            queryString = queryString + "       FROM            MaterialIssueDetails " + "\r\n";
            queryString = queryString + "                       INNER JOIN Workshifts ON MaterialIssueDetails.FirmOrderID IN (SELECT FirmOrderID FROM @FirmOrders) AND MaterialIssueDetails.WorkshiftID = Workshifts.WorkshiftID " + "\r\n";
            queryString = queryString + "                       INNER JOIN ProductionLines ON MaterialIssueDetails.ProductionLineID = ProductionLines.ProductionLineID " + "\r\n";
            queryString = queryString + "                       INNER JOIN GoodsReceiptDetails ON MaterialIssueDetails.GoodsReceiptDetailID = GoodsReceiptDetails.GoodsReceiptDetailID " + "\r\n";

            queryString = queryString + "                       UNION ALL " + "\r\n";

            queryString = queryString + "       SELECT          SemifinishedProductDetails.MaterialIssueDetailID, MaterialIssueDetails.EntryDate, NULL AS ProductionLineCode, NULL AS MaterialIssueWorkshiftEntryDate, NULL AS MaterialIssueWorkshiftCode, NULL AS GoodsReceiptReference, NULL AS BatchEntryDate, NULL AS MaterialIssueDetailQuantity, IIF(Workshifts.EntryDate IS NULL, NULL, SemifinishedProductDetails.EntryDate) AS SemifinishedProductWorkshiftEntryDate, Workshifts.Code AS SemifinishedProductWorkshiftCode, Commodities.Code AS CommoditiyCode, SemifinishedProductDetails.Quantity, 0.0 AS Weights " + "\r\n";
            queryString = queryString + "       FROM            SemifinishedProductDetails " + "\r\n";
            queryString = queryString + "                       INNER JOIN Workshifts ON SemifinishedProductDetails.FirmOrderID IN (SELECT FirmOrderID FROM @FirmOrders) AND SemifinishedProductDetails.WorkshiftID = Workshifts.WorkshiftID " + "\r\n";
            queryString = queryString + "                       INNER JOIN Commodities ON SemifinishedProductDetails.CommodityID = Commodities.CommodityID " + "\r\n";
            queryString = queryString + "                       INNER JOIN MaterialIssueDetails ON SemifinishedProductDetails.MaterialIssueDetailID = MaterialIssueDetails.MaterialIssueDetailID " + "\r\n";

            queryString = queryString + "       ORDER BY        MaterialIssueDetailID, EntryDate, SemifinishedProductWorkshiftEntryDate " + "\r\n";

            queryString = queryString + "    END " + "\r\n";

            this.totalSmartPortalEntities.CreateStoredProcedure("GetPlannedOrderLogs", queryString);
        }

        #endregion
    }
}
