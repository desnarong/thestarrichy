-- ============================================================
-- M06_Addresses
-- เก็บที่อยู่ของสมาชิก แยกตามประเภท:
--   Type 1 = ที่อยู่ตามบัตรประชาชน/พาสปอร์ต
--   Type 2 = ที่อยู่ปัจจุบัน
--   Type 3 = ที่อยู่ออกใบกำกับภาษี (รองรับทั้งบุคคลและนิติบุคคล)
-- FK: MemberCode → ตาราง Member หลัก
-- FK: TambonId  → TBL_TAMBONS.id
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'M06_Addresses'
)
BEGIN
    CREATE TABLE [dbo].[M06_Addresses] (
        -- Primary Key
        [Id]             INT            IDENTITY(1,1) NOT NULL,

        -- เจ้าของที่อยู่
        [MemberCode]     NVARCHAR(50)   NOT NULL,
        [AddressType]    TINYINT        NOT NULL,
            -- 1 = ที่อยู่ตามบัตรประชาชน/พาสปอร์ต
            -- 2 = ที่อยู่ปัจจุบัน
            -- 3 = ที่อยู่ออกใบกำกับภาษี

        -- ============ ที่อยู่แบบละเอียด ============
        [HouseNumber]    NVARCHAR(50)   NULL,  -- บ้านเลขที่
        [Moo]            NVARCHAR(20)   NULL,  -- หมู่ที่
        [Alley]          NVARCHAR(100)  NULL,  -- ซอย / ตรอก
        [Road]           NVARCHAR(100)  NULL,  -- ถนน
        [Building]       NVARCHAR(100)  NULL,  -- อาคาร / หมู่บ้าน
        [Floor]          NVARCHAR(20)   NULL,  -- ชั้น
        [Other]          NVARCHAR(300)  NULL,  -- อื่นๆ / หมายเหตุ

        -- ============ ตำแหน่งที่ตั้ง ============
        [TambonId]       INT            NULL,  -- FK → TBL_TAMBONS.id (ตำบล/แขวง)
        [Zipcode]        NVARCHAR(10)   NULL,  -- รหัสไปรษณีย์ (override ถ้าต่างจาก TBL_TAMBONS)

        -- ============ เฉพาะ Type 3 (ออกใบกำกับภาษี) ============
        [CompanyName]    NVARCHAR(200)  NULL,  -- ชื่อบริษัท / ชื่อในใบกำกับภาษี
        [CompanyTaxId]   NVARCHAR(20)   NULL,  -- เลขประจำตัวผู้เสียภาษี (13 หลัก บุคคล / นิติบุคคล)
        [BranchCode]     NVARCHAR(10)   NULL,  -- รหัสสาขา (00000 = สำนักงานใหญ่)
        [BranchName]     NVARCHAR(100)  NULL,  -- ชื่อสาขา

        -- ============ Audit ============
        [CreatedAt]      DATETIME       NOT NULL CONSTRAINT [DF_M06_Addresses_CreatedAt] DEFAULT GETDATE(),
        [UpdatedAt]      DATETIME       NULL,
        [UpdatedBy]      NVARCHAR(50)   NULL,
        [IsActive]       BIT            NOT NULL CONSTRAINT [DF_M06_Addresses_IsActive] DEFAULT 1,

        -- ============ Constraints ============
        CONSTRAINT [PK_M06_Addresses]
            PRIMARY KEY CLUSTERED ([Id] ASC),

        -- สมาชิกหนึ่งคนมีที่อยู่แต่ละประเภทได้เพียง 1 รายการ
        CONSTRAINT [UK_M06_Addresses_MemberCode_Type]
            UNIQUE ([MemberCode], [AddressType]),

        -- FK ไปยัง TBL_TAMBONS
        CONSTRAINT [FK_M06_Addresses_TAMBONS]
            FOREIGN KEY ([TambonId])
            REFERENCES [dbo].[TBL_TAMBONS] ([id])
    );

    PRINT 'Created table M06_Addresses';
END
ELSE
BEGIN
    PRINT 'Table M06_Addresses already exists — skipped.';
END
GO

-- Index เพื่อเพิ่มความเร็วในการ query ตาม MemberCode
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.M06_Addresses')
      AND name = 'IX_M06_Addresses_MemberCode'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_M06_Addresses_MemberCode]
        ON [dbo].[M06_Addresses] ([MemberCode] ASC)
        INCLUDE ([AddressType], [IsActive]);

    PRINT 'Created index IX_M06_Addresses_MemberCode';
END
GO

-- ============================================================
-- Optional: Add CHECK CONSTRAINT สำหรับ AddressType
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE parent_object_id = OBJECT_ID('dbo.M06_Addresses')
      AND name = 'CK_M06_Addresses_AddressType'
)
BEGIN
    ALTER TABLE [dbo].[M06_Addresses]
        ADD CONSTRAINT [CK_M06_Addresses_AddressType]
            CHECK ([AddressType] IN (1, 2, 3));

    PRINT 'Added CHECK constraint for AddressType (1=บัตร, 2=ปัจจุบัน, 3=ออกภาษี)';
END
GO

-- ============================================================
-- VIEW: เพื่อ JOIN กับ TBL_TAMBONS แสดงชื่อตำบล/อำเภอ/จังหวัด
-- ============================================================
IF OBJECT_ID('dbo.VW_MEMBER_ADDRESSES', 'V') IS NOT NULL
    DROP VIEW [dbo].[VW_MEMBER_ADDRESSES];
GO

CREATE VIEW [dbo].[VW_MEMBER_ADDRESSES]
AS
SELECT
    a.[Id],
    a.[MemberCode],
    a.[AddressType],
    CASE a.[AddressType]
        WHEN 1 THEN N'ที่อยู่ตามบัตรประชาชน'
        WHEN 2 THEN N'ที่อยู่ปัจจุบัน'
        WHEN 3 THEN N'ที่อยู่ออกใบกำกับภาษี'
        ELSE N'ไม่ทราบ'
    END AS [AddressTypeName],

    -- ที่อยู่ละเอียด
    a.[HouseNumber],
    a.[Moo],
    a.[Alley],
    a.[Road],
    a.[Building],
    a.[Floor],
    a.[Other],

    -- location
    a.[TambonId],
    a.[Zipcode],
    t.[tambon]           AS [Tambon],
    t.[amphoe]           AS [Amphoe],
    t.[province]         AS [Province],
    t.[tambon_code]      AS [TambonCode],
    t.[amphoe_code]      AS [AmphoeCode],
    t.[province_code]    AS [ProvinceCode],
    ISNULL(a.[Zipcode], t.[zipcode]) AS [EffectiveZipcode],

    -- นิติบุคคล (type 3)
    a.[CompanyName],
    a.[CompanyTaxId],
    a.[BranchCode],
    a.[BranchName],

    -- audit
    a.[CreatedAt],
    a.[UpdatedAt],
    a.[UpdatedBy],
    a.[IsActive]
FROM
    [dbo].[M06_Addresses] a
LEFT JOIN
    [dbo].[TBL_TAMBONS] t ON t.[id] = a.[TambonId];
GO

PRINT 'Created view VW_MEMBER_ADDRESSES';
GO
