﻿using System.Net;
using System.Web.Mvc;
using System.Text;

using AutoMapper;
using RequireJsNet;

using TotalBase.Enums;

using TotalModel.Models;

using TotalCore.Services.Productions;
using TotalCore.Repositories.Commons;

using TotalDTO.Productions;

using TotalPortal.Controllers;
using TotalPortal.Areas.Productions.ViewModels;
using TotalPortal.Areas.Productions.Builders;
using System.Collections.Generic;

namespace TotalPortal.Areas.Productions.Controllers
{
    public class SemifinishedProductsController : GenericViewDetailController<SemifinishedProduct, SemifinishedProductDetail, SemifinishedProductViewDetail, SemifinishedProductDTO, SemifinishedProductPrimitiveDTO, SemifinishedProductDetailDTO, SemifinishedProductViewModel>
    {
        private readonly ISemifinishedProductService semifinishedProductService;

        public SemifinishedProductsController(ISemifinishedProductService semifinishedProductService, ISemifinishedProductViewModelSelectListBuilder semifinishedProductViewModelSelectListBuilder)
            : base(semifinishedProductService, semifinishedProductViewModelSelectListBuilder, true)
        {
            this.semifinishedProductService = semifinishedProductService;
        }

        protected override ICollection<SemifinishedProductViewDetail> GetEntityViewDetails(SemifinishedProductViewModel semifinishedProductViewModel)
        {
            ICollection<SemifinishedProductViewDetail> semifinishedProductViewDetails = this.semifinishedProductService.GetSemifinishedProductViewDetails(semifinishedProductViewModel.SemifinishedProductID, semifinishedProductViewModel.FirmOrderID);

            return semifinishedProductViewDetails;
        }

        protected override SemifinishedProductViewModel InitViewModelByCopy(SemifinishedProductViewModel simpleViewModel)
        {
            return new SemifinishedProductViewModel()
            {
                MaterialIssueID = simpleViewModel.MaterialIssueID,
                MaterialIssueDetailID = simpleViewModel.MaterialIssueDetailID,
                
                GoodsReceiptID = simpleViewModel.GoodsReceiptID,
                GoodsReceiptDetailID = simpleViewModel.GoodsReceiptDetailID,
                GoodsReceiptReference = simpleViewModel.GoodsReceiptReference,
                GoodsReceiptEntryDate = simpleViewModel.GoodsReceiptEntryDate,
                
                FirmOrderID = simpleViewModel.FirmOrderID,
                FirmOrderCode = simpleViewModel.FirmOrderCode,
                FirmOrderReference = simpleViewModel.FirmOrderReference,
                FirmOrderEntryDate = simpleViewModel.FirmOrderEntryDate,
                FirmOrderSpecification = simpleViewModel.FirmOrderSpecification,
                
                MaterialIssueDetailWorkshiftID = simpleViewModel.MaterialIssueDetailWorkshiftID,
                MaterialIssueDetailWorkshiftCode = simpleViewModel.MaterialIssueDetailWorkshiftCode,
                MaterialIssueDetailWorkshiftEntryDate = simpleViewModel.MaterialIssueDetailWorkshiftEntryDate,

                ProductionLineID = simpleViewModel.ProductionLineID,
                ProductionLineCode = simpleViewModel.ProductionLineCode,

                MaterialCode = simpleViewModel.MaterialCode,
                MaterialName = simpleViewModel.MaterialName,
                MaterialQuantity = simpleViewModel.MaterialQuantity,
                MaterialQuantityRemains = this.semifinishedProductService.GetMaterialQuantityRemains(simpleViewModel.MaterialIssueDetailID),
                
                Customer = simpleViewModel.Customer,

                ShiftID = simpleViewModel.ShiftID,
                StartSequenceNo = simpleViewModel.StopSequenceNo + 1,
                FoilUnitCounts = simpleViewModel.FoilUnitCounts,
                FoilUnitWeights = simpleViewModel.FoilUnitWeights,

                CrucialWorker = simpleViewModel.CrucialWorker
            };
        }

        public virtual ActionResult GetPendingFirmOrderMaterials()
        {
            this.AddRequireJsOptions();
            return View();
        }
    }
}