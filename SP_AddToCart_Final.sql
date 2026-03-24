USE [STARRICHY_MB]
GO
/****** Object:  StoredProcedure [dbo].[SP_AddToCart]    Script Date: 24/03/2569 21:48:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ============================================
-- SP_AddToCart - เพิ่ม Logic คำนวณค่าส่ง
-- ============================================

ALTER PROCEDURE [dbo].[SP_AddToCart]
    @MemberCode NVARCHAR(20),
    @ProductID NVARCHAR(20),
    @ProductCode NVARCHAR(20),
    @ProductName NVARCHAR(200),
    @ProductImage NVARCHAR(500),
    @Price DECIMAL(18,2),
    @PV DECIMAL(18,2),
    @Quantity INT,
    @MakerBy NVARCHAR(20),
	@BillType NVARCHAR(2),
	@Limit NVARCHAR(1),
    @DLCode NVARCHAR(20) = NULL,
    @DLName NVARCHAR(200) = NULL,
    @RegisterDate DATETIME = NULL,
    @CenterCode NVARCHAR(20) = NULL,
    @CenterName NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CartID INT;
	DECLARE @ExistingBillType INT;
    DECLARE @ExpiryDate DATETIME;
    DECLARE @ExistingDLCode NVARCHAR(20);
    DECLARE @ExistingCenterCode NVARCHAR(20);
    DECLARE @NeedClear BIT = 0;
    DECLARE @ShippingFee DECIMAL(18,2) = 0;
    
    -- ⭐ ตัวแปรสำหรับคำนวณค่าส่ง
    DECLARE @TypeofFee INT;
    DECLARE @CondFee DECIMAL(18,2);
    DECLARE @DeliveryFee1 DECIMAL(18,2);
    DECLARE @DeliveryFee2 DECIMAL(18,2);
    DECLARE @TotalAmount DECIMAL(18,2);
    DECLARE @TotalPV DECIMAL(18,2);
    
    -- ⭐ ตัวแปรตรวจสอบ flag สินค้า
    DECLARE @HasPickupOnly BIT = 0;
    DECLARE @HasFreeShipping BIT = 0;
    
    -- กำหนดวันหมดอายุ (วันถัดไป เวลา 23:59:59)
    SET @ExpiryDate = DATEADD(DAY, DATEDIFF(DAY, 0, GETDATE()) + 1, 0);
    SET @ExpiryDate = DATEADD(SECOND, -1, @ExpiryDate);
    
    -- ⭐ ดึงตะกร้าที่ Active พร้อมเช็ค DL และ Center
    SELECT 
        @CartID = CartID,
        @ExistingDLCode = DLCode,
        @ExistingCenterCode = CenterCode,
		@ExistingBillType = BillType
    FROM ShoppingCarts 
    WHERE MemberCode = @MemberCode 
        AND Status = 'Active' 
        AND ExpiryDate > GETDATE();
    
    -- ⭐ เช็คว่าต้องล้างตะกร้าหรือไม่
    IF @CartID IS NOT NULL
    BEGIN
        IF (
            (@DLCode IS NOT NULL AND @ExistingDLCode IS NOT NULL AND @DLCode <> @ExistingDLCode) OR
            (@CenterCode IS NOT NULL AND @ExistingCenterCode IS NOT NULL AND @CenterCode <> @ExistingCenterCode) OR
			(@BillType IS NOT NULL AND @ExistingBillType IS NOT NULL AND @BillType <> @ExistingBillType)
        )
        BEGIN
            DELETE FROM ShoppingCartItems WHERE CartID = @CartID;
            DELETE FROM ShoppingCarts WHERE CartID = @CartID;
            
            SET @CartID = NULL;
            SET @NeedClear = 1;
            
            PRINT '⚠️ ล้างตะกร้าเดิมเนื่องจาก DL/Center ไม่ตรงกัน';
        END
    END


    -- ถ้าไม่มีตะกร้า หรือถูกล้างไปแล้ว → สร้างใหม่
    IF @CartID IS NULL
    BEGIN
        INSERT INTO ShoppingCarts (
            MemberCode, 
            CreatedDate, 
            ExpiryDate, 
            Makerby,
            CenterCode,
            BillType, 
            PaymentType, 
            DeliveryType,
            ShippingFee,
            DLCode,
            DLName,
            RegisterDate,
            CenterName
        )
        VALUES (
            @MemberCode, 
            GETDATE(), 
            @ExpiryDate, 
            @MakerBy,
            ISNULL(@CenterCode, '01'),
            @BillType,
            0,
            0,
            0,
            @DLCode,
            @DLName,
            @RegisterDate,
            @CenterName
        );
        
        SET @CartID = SCOPE_IDENTITY();
        PRINT '✅ สร้างตะกร้าใหม่';
    END
    ELSE
    BEGIN
        UPDATE ShoppingCarts
        SET 
            DLCode = ISNULL(DLCode, @DLCode),
            DLName = ISNULL(DLName, @DLName),
            RegisterDate = ISNULL(RegisterDate, @RegisterDate),
            CenterCode = ISNULL(CenterCode, @CenterCode),
            CenterName = ISNULL(CenterName, @CenterName)
        WHERE CartID = @CartID;
    END
    
    -- เช็คว่ามีสินค้าอยู่แล้วหรือไม่
    IF EXISTS (SELECT 1 FROM ShoppingCartItems 
               WHERE CartID = @CartID AND ProductID = @ProductID)
    BEGIN
        UPDATE ShoppingCartItems
        SET Quantity = @Quantity,
            SubTotal = @Price * @Quantity
        WHERE CartID = @CartID AND ProductID = @ProductID;
        
        PRINT '✅ อัพเดทจำนวนสินค้า';
    END
    ELSE
    BEGIN
        INSERT INTO ShoppingCartItems (
            CartID, 
            ProductID, 
            ProductCode, 
            ProductName, 
            ProductImage, 
            Price, 
            PV, 
            Quantity, 
            SubTotal,
			Limit
        )
        VALUES (
            @CartID, 
            @ProductID, 
            @ProductCode, 
            @ProductName,
            @ProductImage, 
            @Price, 
            @PV, 
            @Quantity, 
            @Price * @Quantity,
			@Limit
        );
        
        PRINT '✅ เพิ่มสินค้าใหม่';
    END
    
    -- ⭐⭐⭐ คำนวณยอดรวมก่อน
    SELECT 
        @TotalAmount = ISNULL(SUM(SubTotal), 0),
        @TotalPV = ISNULL(SUM(PV * Quantity), 0)
    FROM ShoppingCartItems 
    WHERE CartID = @CartID;
    
    -- ⭐⭐⭐ ตรวจสอบ flag สินค้าในตะกร้า (M01_X46, M01_X50)
    -- เช็คว่ามีสินค้า M01_X46='1' (ต้องรับเอง) ในตะกร้าหรือไม่
    SELECT @HasPickupOnly = MAX(CASE WHEN ISNULL(P.M01_X46, '0') = '1' THEN 1 ELSE 0 END)
    FROM ShoppingCartItems SCI
    INNER JOIN [000_Product] P ON SCI.ProductID = P.ProductID
    WHERE SCI.CartID = @CartID;
    
    -- เช็คว่ามีสินค้า M01_X50='1' (ส่งฟรี) ในตะกร้าหรือไม่
    SELECT @HasFreeShipping = MAX(CASE WHEN ISNULL(P.M01_X50, '0') = '1' THEN 1 ELSE 0 END)
    FROM ShoppingCartItems SCI
    INNER JOIN [000_Product] P ON SCI.ProductID = P.ProductID
    WHERE SCI.CartID = @CartID;
    
    -- ⭐⭐⭐ คำนวณค่าส่งตามเงื่อนไข
    IF @HasPickupOnly = 1
    BEGIN
        -- M01_X46='1': ต้องรับเองที่ศูนย์ ไม่มีค่าส่ง
        SET @ShippingFee = 0;
        PRINT '📦 สินค้าต้องรับเองที่ศูนย์ - ค่าส่ง = 0';
    END
    ELSE IF @HasFreeShipping = 1
    BEGIN
        -- M01_X50='1': ส่งฟรี
        SET @ShippingFee = 0;
        PRINT '🎁 สินค้าส่งฟรี - ค่าส่ง = 0';
    END
    ELSE
    BEGIN
        -- ดึงเงื่อนไขค่าส่งจาก S02 (เฉพาะเมื่อไม่มีสินค้าพิเศษ)
        SELECT TOP 1
            @TypeofFee = S02_X109,
            @CondFee = S02_X111,
            @DeliveryFee1 = S02_X110,
            @DeliveryFee2 = S02_X123
        FROM S02;
        
        -- คำนวณค่าส่งตามเงื่อนไข
        IF @TypeofFee = 0
        BEGIN
            -- เช็คจากยอดเงิน (TotalAmount)
            IF @TotalAmount >= @CondFee
                SET @ShippingFee = ISNULL(@DeliveryFee2, 0);
            ELSE
                SET @ShippingFee = @DeliveryFee1;
        END
        ELSE IF @TypeofFee = 1
        BEGIN
            -- เช็คจากยอด PV (TotalPV)
            IF @TotalPV >= @CondFee
                SET @ShippingFee = ISNULL(@DeliveryFee2, 0);
            ELSE
                SET @ShippingFee = @DeliveryFee1;
        END
        ELSE
        BEGIN
            -- Default: ไม่มีเงื่อนไข = ส่งฟรี
            SET @ShippingFee = 0;
        END
        
        PRINT '💰 คำนวณค่าส่งตามเงื่อนไข S02';
    END
    
    -- ⭐⭐⭐ อัพเดทยอดรวมและค่าส่งในตะกร้า
    UPDATE ShoppingCarts
    SET TotalAmount = @TotalAmount,
        TotalPV = @TotalPV,
        ShippingFee = @ShippingFee
    WHERE CartID = @CartID;
    
    -- Return CartID
    SELECT @CartID AS CartID;
END;
