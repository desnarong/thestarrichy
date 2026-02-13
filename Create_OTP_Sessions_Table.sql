-- Create OTP_Sessions table for storing OTP verification data
-- This table stores OTP codes with expiration for secure verification

CREATE TABLE [dbo].[OTP_Sessions] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [ReferenceId] NVARCHAR(50) NOT NULL UNIQUE, -- Unique reference for each OTP session
    [PhoneNumber] NVARCHAR(20) NOT NULL, -- Phone number the OTP was sent to
    [OTP] NVARCHAR(10) NOT NULL, -- The OTP code (encrypted in production)
    [ExpiryTime] DATETIME2 NOT NULL, -- When the OTP expires
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(), -- When the OTP was created
    [IsUsed] BIT DEFAULT 0, -- Whether the OTP has been used
    [UsedAt] DATETIME2 NULL, -- When the OTP was used
    [Attempts] INT DEFAULT 0 -- Number of verification attempts
);

-- Create index for faster lookups by ReferenceId
CREATE INDEX IX_OTP_Sessions_ReferenceId ON [OTP_Sessions] ([ReferenceId]);

-- Create index for faster lookups by PhoneNumber
CREATE INDEX IX_OTP_Sessions_PhoneNumber ON [OTP_Sessions] ([PhoneNumber]);

-- Create index for cleanup of expired OTPs
CREATE INDEX IX_OTP_Sessions_ExpiryTime ON [OTP_Sessions] ([ExpiryTime]);

-- Create index for unused OTPs
CREATE INDEX IX_OTP_Sessions_IsUsed ON [OTP_Sessions] ([IsUsed]);