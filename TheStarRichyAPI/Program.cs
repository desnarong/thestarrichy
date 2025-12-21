using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TheStarRichyApi.Models.Kbank;
using TheStarRichyApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ==================== SERVICE REGISTRATIONS ====================
builder.Services.AddMemoryCache();

// Authentication & Login
builder.Services.AddScoped<ILoginService, LoginService>();

// Member Management
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IMemberIncomeByPeriodService, MemberIncomeByPeriodService>();
builder.Services.AddScoped<IMemberPermissionService, MemberPermissionService>();
builder.Services.AddScoped<IMessagetoMemberService, MessagetoMemberService>();
builder.Services.AddScoped<IEstimatePositionService, EstimatePositionService>();
builder.Services.AddScoped<IMemberDeliveryAddressService, MemberDeliveryAddressService>();

// Team Management
builder.Services.AddScoped<IMemberTeamBuyProductService, MemberTeamBuyProductService>();
builder.Services.AddScoped<IMemberTeamByRegionBuyService, MemberTeamByRegionBuyService>();
builder.Services.AddScoped<IMemberTeamByRegionService, MemberTeamByRegionService>();
builder.Services.AddScoped<IMemberTeamNewBuyService, MemberTeamNewBuyService>();
builder.Services.AddScoped<IMemberTeamNewRegisterService, MemberTeamNewRegisterService>();
builder.Services.AddScoped<IMemberTeamTotalPositionPackageService, MemberTeamTotalPositionPackageService>();
builder.Services.AddScoped<IMemberTeamTotalPositionRankingService, MemberTeamTotalPositionRankingService>();
builder.Services.AddScoped<IMemberBinaryTeamService, MemberBinaryTeamService>();

// Search & Find
builder.Services.AddScoped<IFindLeftBinaryService, FindLeftBinaryService>();
builder.Services.AddScoped<IFindRightBinaryService, FindRightBinaryService>();
builder.Services.AddScoped<IFindUplineBinaryService, FindUplineBinaryService>();
builder.Services.AddScoped<IFindMemberNameService, FindMemberNameService>();
builder.Services.AddScoped<IFindMembercodeService, FindMembercodeService>();
builder.Services.AddScoped<IFindMembercodeForSaleService, FindMembercodeForSaleService>();

// Reports
builder.Services.AddScoped<IReportMemberDailyPointService, ReportMemberDailyPointService>();
builder.Services.AddScoped<IReportMemberLeftSumPackageService, ReportMemberLeftSumPackageService>();
builder.Services.AddScoped<IReportMemberLeftSumRankingService, ReportMemberLeftSumRankingService>();
builder.Services.AddScoped<IReportMemberLeftTeamService, ReportMemberLeftTeamService>();
builder.Services.AddScoped<IReportMemberRightSumPackageService, ReportMemberRightSumPackageService>();
builder.Services.AddScoped<IReportMemberRightSumRankingService, ReportMemberRightSumRankingService>();
builder.Services.AddScoped<IReportMemberRightTeamService, ReportMemberRightTeamService>();
builder.Services.AddScoped<IReportMemberSponserTeamService, ReportMemberSponserTeamService>();
builder.Services.AddScoped<IReportMemberSaleandExpainOrderService, ReportMemberSaleandExpainOrderService>();
builder.Services.AddScoped<IReportMemberRightSourceOfPVService, ReportMemberRightSourceOfPVService>();
builder.Services.AddScoped<IReportMemberPositionHistoryService, ReportMemberPositionHistoryService>();
builder.Services.AddScoped<IReportMemberPOOrderService, ReportMemberPOOrderService>();
builder.Services.AddScoped<IReportMemberLogService, ReportMemberLogService>();
builder.Services.AddScoped<IReportMemberLeftSourceOfPVService, ReportMemberLeftSourceOfPVService>();
builder.Services.AddScoped<IReportMemberDaliyCutPointService, ReportMemberDaliyCutPointService>();
builder.Services.AddScoped<IReportMemberBuyTopupOrderService, ReportMemberBuyTopupOrderService>();
builder.Services.AddScoped<IReportMemberBuyHoldOrderService, ReportMemberBuyHoldOrderService>();
builder.Services.AddScoped<IReportMemberBonusByPaymentPeriodService, ReportMemberBonusByPaymentPeriodService>();
builder.Services.AddScoped<IReportMemberBonusByMonthService, ReportMemberBonusByMonthService>();
builder.Services.AddScoped<IReportMemberBonusByDateService, ReportMemberBonusByDateService>();
builder.Services.AddScoped<IReportMemberBonusByDateDetailSponserService, ReportMemberBonusByDateDetailSponserService>();
builder.Services.AddScoped<IReportMemberBonusByDateDetailRebateService, ReportMemberBonusByDateDetailRebateService>();
builder.Services.AddScoped<IReportMemberBonusByDateDetailMobileService, ReportMemberBonusByDateDetailMobileService>();
builder.Services.AddScoped<IReportMemberBonusByDateDetailMatchingService, ReportMemberBonusByDateDetailMatchingService>();
builder.Services.AddScoped<IReportMemberBonusByDateDetailBinaryService, ReportMemberBonusByDateDetailBinaryService>();
builder.Services.AddScoped<IMemberFavoriteAddressService, MemberFavoriteAddressService>();
builder.Services.AddScoped<IFindCenterMobileService, FindCenterMobileService>();

// Products
builder.Services.AddScoped<IProductGroupService, ProductGroupService>();
builder.Services.AddScoped<IGroupofProductsService, GroupofProductsService>();
builder.Services.AddScoped<IProductListForTopupService, ProductListForTopupService>();
builder.Services.AddScoped<IProductListForHoldService, ProductListForHoldService>();
builder.Services.AddScoped<IProductListForHurryService, ProductListForHurryService>();

// Cart
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IKbankWebhookService, KbankWebhookService>();

// ✅ Payment (Use HttpClientFactory only)
builder.Services.AddHttpClient<IKbankAuthService, KbankAuthService>();
builder.Services.AddHttpClient<IKbankQrPaymentService, KbankQrPaymentService>();
builder.Services.Configure<KbankSettings>(builder.Configuration.GetSection("Kbank"));


// System
builder.Services.AddScoped<ICheckwebStatusService, CheckwebStatusService>();
builder.Services.AddScoped<IStaticService, StaticService>();
builder.Services.AddScoped<IBankService, BankService>();

// ================================================================

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
