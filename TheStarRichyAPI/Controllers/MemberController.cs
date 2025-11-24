using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TheStarRichyApi.Services;

namespace TheStarRichyApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _memberService;
        private readonly IMemberIncomeByPeriodService _memberIncomeByPeriodService;
        private readonly IMemberTeamBuyProductService _memberTeamBuyProductService;
        private readonly IMemberTeamByRegionBuyService _memberTeamByRegionBuyService;
        private readonly IMemberTeamByRegionService _memberTeamByRegionService;
        private readonly IMemberTeamNewBuyService _memberTeamNewBuyService;
        private readonly IMemberTeamNewRegisterService _memberTeamNewRegisterService;
        private readonly IMemberTeamTotalPositionPackageService _memberTeamTotalPositionPackageService;
        private readonly IMemberTeamTotalPositionRankingService _memberTeamTotalPositionRankingService;
        private readonly IMessagetoMemberService _messagetoMemberService;
        private readonly IEstimatePositionService _estimatePositionService;
        private readonly IMemberPermissionService _memberPermissionService;
        private readonly IReportMemberLeftSumPackageService _reportMemberLeftSumPackageService;
        private readonly IReportMemberLeftSumRankingService _reportMemberLeftSumRankingService;
        private readonly IReportMemberLeftTeamService _reportMemberLeftTeamService;
        private readonly IReportMemberRightSumPackageService _reportMemberRightSumPackageService;
        private readonly IReportMemberRightSumRankingService _reportMemberRightSumRankingService;
        private readonly IReportMemberRightTeamService _reportMemberRightTeamService;
        private readonly IReportMemberSponserTeamService _reportMemberSponserTeamService;

        /*==================== UPDATE 2025-09-07 ====================*/
        private readonly IMemberBinaryTeamService _memberBinaryTeamService;
        private readonly IFindUplineBinaryService _findUplineBinaryService;
        private readonly IFindRightBinaryService _findRightBinaryService;
        private readonly IFindMemberNameService _findMemberNameService;
        private readonly IFindMembercodeService _findMembercodeService;
        private readonly IFindLeftBinaryService _findLeftBinaryService;

        /*==================== UPDATE 2025-11-11 ====================*/
        private readonly IReportMemberSaleandExpainOrderService _reportMemberSaleandExpainOrderService;
        private readonly IReportMemberRightSourceOfPVService _reportMemberRightSourceOfPVService;
        private readonly IReportMemberPositionHistoryService _reportMemberPositionHistoryService;
        private readonly IReportMemberPOOrderService _reportMemberPOOrderService;
        private readonly IReportMemberLogService _reportMemberLogService;
        private readonly IReportMemberLeftSourceOfPVService _reportMemberLeftSourceOfPVService;
        private readonly IReportMemberDailyPointService _reportMemberDailyPointService;
        private readonly IReportMemberDaliyCutPointService _reportMemberDaliyCutPointService;
        private readonly IReportMemberBuyTopupOrderService _reportMemberBuyTopupOrderService;
        private readonly IReportMemberBuyHoldOrderService _reportMemberBuyHoldOrderService;
        private readonly IReportMemberBonusByPaymentPeriodService _reportMemberBonusByPaymentPeriodService;
        private readonly IReportMemberBonusByMonthService _reportMemberBonusByMonthService;
        private readonly IReportMemberBonusByDateService _reportMemberBonusByDateService;
        private readonly IReportMemberBonusByDateDetailSponserService _reportMemberBonusByDateDetailSponserService;
        private readonly IReportMemberBonusByDateDetailRebateService _reportMemberBonusByDateDetailRebateService;
        private readonly IReportMemberBonusByDateDetailMobileService _reportMemberBonusByDateDetailMobileService;
        private readonly IReportMemberBonusByDateDetailMatchingService _reportMemberBonusByDateDetailMatchingService;
        private readonly IReportMemberBonusByDateDetailBinaryService _reportMemberBonusByDateDetailBinaryService;
        private readonly IMemberDeliveryAddressService _memberDeliveryAddressService;
        private readonly IBranchService _branchService;
        private readonly IMemberFavoriteAddressService _memberFavoriteAddressService;
        public MemberController(
            IMemberService memberService,
            IMemberIncomeByPeriodService memberIncomeByPeriodService,
            IMemberTeamBuyProductService memberTeamBuyProductService,
            IMemberTeamByRegionBuyService memberTeamByRegionBuyService,
            IMemberTeamByRegionService memberTeamByRegionService,
            IMemberTeamNewBuyService memberTeamNewBuyService,
            IMemberTeamNewRegisterService memberTeamNewRegisterService,
            IMemberTeamTotalPositionPackageService memberTeamTotalPositionPackageService,
            IMemberTeamTotalPositionRankingService memberTeamTotalPositionRankingService,
            IMessagetoMemberService messagetoMemberService,
            IEstimatePositionService estimatePositionService,
            IMemberPermissionService memberPermissionService,
            IReportMemberLeftSumPackageService reportMemberLeftSumPackageService,
            IReportMemberLeftSumRankingService reportMemberLeftSumRankingService,
            IReportMemberLeftTeamService reportMemberLeftTeamService,
            IReportMemberRightSumPackageService reportMemberRightSumPackageService,
            IReportMemberRightSumRankingService reportMemberRightSumRankingService,
            IReportMemberRightTeamService reportMemberRightTeamService,
            IReportMemberSponserTeamService reportMemberSponserTeamService,
            /*==================== UPDATE 2025-09-07 ====================*/
            IMemberBinaryTeamService memberBinaryTeamService,
            IFindUplineBinaryService findUplineBinaryService,
            IFindRightBinaryService findRightBinaryService,
            IFindMemberNameService findMemberNameService,
            IFindMembercodeService findMembercodeService,
            IFindLeftBinaryService findLeftBinaryService,
            /*==================== UPDATE 2025-11-11 ====================*/
            IReportMemberSaleandExpainOrderService reportMemberSaleandExpainOrderService,
            IReportMemberRightSourceOfPVService reportMemberRightSourceOfPVService,
            IReportMemberPositionHistoryService reportMemberPositionHistoryService,
            IReportMemberPOOrderService reportMemberPOOrderService,
            IReportMemberLogService reportMemberLogService,
            IReportMemberLeftSourceOfPVService reportMemberLeftSourceOfPVService,
            IReportMemberDailyPointService reportMemberDailyPointService,
            IReportMemberDaliyCutPointService reportMemberDaliyCutPointService,
            IReportMemberBuyTopupOrderService reportMemberBuyTopupOrderService,
            IReportMemberBuyHoldOrderService reportMemberBuyHoldOrderService,
            IReportMemberBonusByPaymentPeriodService reportMemberBonusByPaymentPeriodService,
            IReportMemberBonusByMonthService reportMemberBonusByMonthService,
            IReportMemberBonusByDateService reportMemberBonusByDateService,
            IReportMemberBonusByDateDetailSponserService reportMemberBonusByDateDetailSponserService,
            IReportMemberBonusByDateDetailRebateService reportMemberBonusByDateDetailRebateService,
            IReportMemberBonusByDateDetailMobileService reportMemberBonusByDateDetailMobileService,
            IReportMemberBonusByDateDetailMatchingService reportMemberBonusByDateDetailMatchingService,
            IReportMemberBonusByDateDetailBinaryService reportMemberBonusByDateDetailBinaryService,
            IMemberDeliveryAddressService memberDeliveryAddressService,
            IBranchService branchService,
            IMemberFavoriteAddressService memberFavoriteAddressService
            )
        {
            _memberService = memberService;
            _memberIncomeByPeriodService = memberIncomeByPeriodService;
            _memberTeamBuyProductService = memberTeamBuyProductService;
            _memberTeamByRegionBuyService = memberTeamByRegionBuyService;
            _memberTeamByRegionService = memberTeamByRegionService;
            _memberTeamNewBuyService = memberTeamNewBuyService;
            _memberTeamNewRegisterService = memberTeamNewRegisterService;
            _memberTeamTotalPositionPackageService = memberTeamTotalPositionPackageService;
            _memberTeamTotalPositionRankingService = memberTeamTotalPositionRankingService;
            _messagetoMemberService = messagetoMemberService;
            _estimatePositionService = estimatePositionService;
            _memberPermissionService = memberPermissionService;
            _reportMemberLeftSumPackageService = reportMemberLeftSumPackageService;
            _reportMemberLeftSumRankingService = reportMemberLeftSumRankingService;
            _reportMemberLeftTeamService = reportMemberLeftTeamService;
            _reportMemberRightSumPackageService = reportMemberRightSumPackageService;
            _reportMemberRightSumRankingService = reportMemberRightSumRankingService;
            _reportMemberRightTeamService = reportMemberRightTeamService;
            _reportMemberSponserTeamService = reportMemberSponserTeamService;

            /*==================== UPDATE 2025-09-07 ====================*/
            _memberBinaryTeamService = memberBinaryTeamService;
            _findUplineBinaryService = findUplineBinaryService;
            _findRightBinaryService = findRightBinaryService;
            _findMemberNameService = findMemberNameService;
            _findMembercodeService = findMembercodeService;
            _findLeftBinaryService = findLeftBinaryService;

            /*==================== UPDATE 2025-11-11 ====================*/
            _reportMemberSaleandExpainOrderService = reportMemberSaleandExpainOrderService;
            _reportMemberRightSourceOfPVService = reportMemberRightSourceOfPVService;
            _reportMemberPositionHistoryService = reportMemberPositionHistoryService;
            _reportMemberPOOrderService = reportMemberPOOrderService;
            _reportMemberLogService = reportMemberLogService;
            _reportMemberLeftSourceOfPVService = reportMemberLeftSourceOfPVService;
            _reportMemberDailyPointService = reportMemberDailyPointService;
            _reportMemberDaliyCutPointService = reportMemberDaliyCutPointService;
            _reportMemberBuyTopupOrderService = reportMemberBuyTopupOrderService;
            _reportMemberBuyHoldOrderService = reportMemberBuyHoldOrderService;
            _reportMemberBonusByPaymentPeriodService = reportMemberBonusByPaymentPeriodService;
            _reportMemberBonusByMonthService = reportMemberBonusByMonthService;
            _reportMemberBonusByDateService = reportMemberBonusByDateService;
            _reportMemberBonusByDateDetailSponserService = reportMemberBonusByDateDetailSponserService;
            _reportMemberBonusByDateDetailRebateService = reportMemberBonusByDateDetailRebateService;
            _reportMemberBonusByDateDetailMobileService = reportMemberBonusByDateDetailMobileService;
            _reportMemberBonusByDateDetailMatchingService = reportMemberBonusByDateDetailMatchingService;
            _reportMemberBonusByDateDetailBinaryService = reportMemberBonusByDateDetailBinaryService;
            _memberDeliveryAddressService = memberDeliveryAddressService;
            _branchService = branchService;
            _memberFavoriteAddressService = memberFavoriteAddressService;
        }

        [HttpGet("hello")]
        public IActionResult HelloWorldAsync()
        {
            return Ok(new { message = "Hello World" });
        }
       

        [HttpGet("display")]
        public async Task<IActionResult> GetMember2Display()
        {
            try
            {
                string baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                var result = await _memberService.GetMember2DisplayAsync(baseUrl);
                if (result[0].MemberFlag == "N")
                {
                    return Unauthorized(new { message = "Invalid passkey or member" });
                }
                return Ok(result[0]);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("incomebyperiod")]
        public async Task<IActionResult> GetMemberIncomeByPeriod()
        {
            try
            {
                var result = await _memberIncomeByPeriodService.GetMember2DisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("teambuyproduct")]
        public async Task<IActionResult> GetMemberTeamBuyProduct()
        {
            try
            {
                var result = await _memberTeamBuyProductService.GetMember2DisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("teambyregionbuy")]
        public async Task<IActionResult> GetMemberTeamByRegionBuy()
        {
            try
            {
                var result = await _memberTeamByRegionBuyService.GetMember2DisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("teambyregion")]
        public async Task<IActionResult> GetMemberTeamByRegion()
        {
            try
            {
                var result = await _memberTeamByRegionService.GetMember2DisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("teamnewbuy")]
        public async Task<IActionResult> GetMemberTeamNewBuy()
        {
            try
            {
                var result = await _memberTeamNewBuyService.GetMember2DisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("teamnewregister")]
        public async Task<IActionResult> GetMemberTeamNewRegister()
        {
            try
            {
                var result = await _memberTeamNewRegisterService.GetMember2DisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("teamtotalpositionpackage")]
        public async Task<IActionResult> GetMemberTeamTotalPositionPackage()
        {
            try
            {
                var result = await _memberTeamTotalPositionPackageService.GetMember2DisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("teamtotalpositionranking")]
        public async Task<IActionResult> GetMemberTeamTotalPositionRanking()
        {
            try
            {
                var result = await _memberTeamTotalPositionRankingService.GetMember2DisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages()
        {
            try
            {
                var result = await _messagetoMemberService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("estimateposition")]
        public async Task<IActionResult> GetEstimatePosition()
        {
            try
            {
                var result = await _estimatePositionService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("memberpermission")]
        public async Task<IActionResult> GetMemberPermission()
        {
            try
            {
                var result = await _memberPermissionService.GetMember2DisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("reportmemberleftsumpackage")]
        public async Task<IActionResult> GetReportMemberLeftSumPackage()
        {
            try
            {
                var result = await _reportMemberLeftSumPackageService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("reportmemberleftsumranking")]
        public async Task<IActionResult> GetReportMemberLeftSumRanking()
        {
            try
            {
                var result = await _reportMemberLeftSumRankingService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("reportmemberleftteam")]
        public async Task<IActionResult> GetReportMemberLeftTeam()
        {
            try
            {
                var result = await _reportMemberLeftTeamService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("reportmemberrightsumpackage")]
        public async Task<IActionResult> GetReportMemberRightSumPackage()
        {
            try
            {
                var result = await _reportMemberRightSumPackageService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("reportmemberrightsumranking")]
        public async Task<IActionResult> GetReportMemberRightSumRanking()
        {
            try
            {
                var result = await _reportMemberRightSumRankingService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("reportmemberrightteam")]
        public async Task<IActionResult> GetReportMemberRightTeam()
        {
            try
            {
                var result = await _reportMemberRightTeamService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("reportmembersponserteam")]
        public async Task<IActionResult> GetReportMemberSponserTeam()
        {
            try
            {
                var result = await _reportMemberSponserTeamService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /*==================== UPDATE 2025-09-07 ====================*/
        [HttpGet("memberbinaryteam")]
        public async Task<IActionResult> GetMemberBinaryTeam()
        {
            try
            {
                var result = await _memberBinaryTeamService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("finduplinebinary")]
        public async Task<IActionResult> GetFindUplineBinary()
        {
            try
            {
                var result = await _findUplineBinaryService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("findrightbinary")]
        public async Task<IActionResult> GetFindRightBinary()
        {
            try
            {
                var result = await _findRightBinaryService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("findleftbinary")]
        public async Task<IActionResult> GetFindLeftBinary()
        {
            try
            {
                var result = await _findLeftBinaryService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("findmembername")]
        public async Task<IActionResult> GetFindMemberName()
        {
            try
            {
                var result = await _findMemberNameService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("findmembercode")]
        public async Task<IActionResult> GetFindMemberCode()
        {
            try
            {
                var result = await _findMembercodeService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /*==================== UPDATE 2025-11-11 ====================*/
        [HttpGet("reportsaleandexpainorder")]
        public async Task<IActionResult> GetreportMemberSaleandExpainOrder()
        {
            try
            {
                var result = await _reportMemberSaleandExpainOrderService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("reportrightsourceofpv")]
        public async Task<IActionResult> GetReportMemberRightSourceOfPV()
        {
            try
            {
                var result = await _reportMemberRightSourceOfPVService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("reportpositionhistory")]
        public async Task<IActionResult> GetReportMemberPositionHistory()
        {
            try
            {
                var result = await _reportMemberPositionHistoryService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("reportpoorder")]
        public async Task<IActionResult> GetReportMemberPOOrder()
        {
            try
            {
                var result = await _reportMemberPOOrderService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("reportlog")]
        public async Task<IActionResult> GetReportMemberLog()
        {
            try
            {
                var result = await _reportMemberLogService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("reportleftsourceofpv")]
        public async Task<IActionResult> GetReportMemberLeftSourceOfPV()
        {
            try
            {
                var result = await _reportMemberLeftSourceOfPVService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("reportdailypoint")]
        public async Task<IActionResult> GetReportMemberDailyPoint()
        {
            try
            {
                var result = await _reportMemberDailyPointService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("reportdailycutpoint")]
        public async Task<IActionResult> GetReportMemberDaliyCutPoint()
        {
            try
            {
                var result = await _reportMemberDaliyCutPointService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("reportbuytopuporder")]
        public async Task<IActionResult> GetReportMemberBuyTopupOrder()
        {
            try
            {
                var result = await _reportMemberBuyTopupOrderService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("reportbuyholdorder")]
        public async Task<IActionResult> GetReportMemberBuyHoldOrder()
        {
            try
            {
                var result = await _reportMemberBuyHoldOrderService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("reportbonusbypaymentperiod")]
        public async Task<IActionResult> GetReportMemberBonusByPaymentPeriod()
        {
            try
            {
                var result = await _reportMemberBonusByPaymentPeriodService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("reportbonusbymonth")]
        public async Task<IActionResult> GetReportMemberBonusByMonth()
        {
            try
            {
                var result = await _reportMemberBonusByMonthService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("reportbonusbydate")]
        public async Task<IActionResult> GetReportMemberBonusByDate()
        {
            try
            {
                var result = await _reportMemberBonusByDateService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("reportbonusbydatedetailsponser")]
        public async Task<IActionResult> GetReportMemberBonusByDateDetailSponser()
        {
            try
            {
                var result = await _reportMemberBonusByDateDetailSponserService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("reportbonusbydatedetailrebate")]
        public async Task<IActionResult> GetReportMemberBonusByDateDetailRebate()
        {
            try
            {
                var result = await _reportMemberBonusByDateDetailRebateService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("reportbonusbydatedetailmobile")]
        public async Task<IActionResult> GetReportMemberBonusByDateDetailMobile()
        {
            try
            {
                var result = await _reportMemberBonusByDateDetailMobileService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("reportbonusbydatedetailmatching")]
        public async Task<IActionResult> GetReportMemberBonusByDateDetailMatching()
        {
            try
            {
                var result = await _reportMemberBonusByDateDetailMatchingService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("reportbonusbydatedetailbinary")]
        public async Task<IActionResult> GetReportMemberBonusByDateDetailBinary()
        {
            try
            {
                var result = await _reportMemberBonusByDateDetailBinaryService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        //
        [HttpGet("member-addresses")]
        public async Task<IActionResult> GetMemberDeliveryAddress()
        {
            try
            {
                var result = await _memberDeliveryAddressService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("branchs")]
        public async Task<IActionResult> GetBranchs()
        {
            try
            {
                var result = await _branchService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("member-favorite-addresses")]
        public async Task<IActionResult> GetFavoriteAddresses()
        {
            try
            {
                var result = await _memberFavoriteAddressService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
