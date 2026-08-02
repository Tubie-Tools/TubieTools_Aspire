-- SQL Server Database Schema for TubieTools Aspire Multi-Tenant System
-- Execute this script to create all tables

-- Drop existing tables if they exist (in reverse order of dependencies)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TenantBillingRecords') DROP TABLE dbo.TenantBillingRecords;
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TenantUsage') DROP TABLE dbo.TenantUsage;
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TenantTeamMembers') DROP TABLE dbo.TenantTeamMembers;
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TenantCustomAgents') DROP TABLE dbo.TenantCustomAgents;
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TenantQuotas') DROP TABLE dbo.TenantQuotas;
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TenantSubscriptions') DROP TABLE dbo.TenantSubscriptions;
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Tenants') DROP TABLE dbo.Tenants;

-- Create Tenants Table
CREATE TABLE dbo.Tenants (
    TenantId VARCHAR(100) PRIMARY KEY NOT NULL,
    TenantName VARCHAR(255) NOT NULL,
    Description VARCHAR(500),
    ApiKey VARCHAR(255) NOT NULL UNIQUE,
    SecretKey VARCHAR(255) NOT NULL,
    CurrentTier VARCHAR(50) NOT NULL DEFAULT 'Free',
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT CHK_Tier CHECK (CurrentTier IN ('Free', 'Starter', 'Professional', 'Enterprise'))
);

-- Create indexes for Tenants
CREATE UNIQUE INDEX IX_Tenants_ApiKey ON dbo.Tenants(ApiKey);
CREATE INDEX IX_Tenants_IsActive ON dbo.Tenants(IsActive);
CREATE INDEX IX_Tenants_CreatedDate ON dbo.Tenants(CreatedDate);

-- Create Subscriptions Table
CREATE TABLE dbo.TenantSubscriptions (
    SubscriptionId VARCHAR(100) PRIMARY KEY NOT NULL,
    TenantId VARCHAR(100) NOT NULL,
    Tier VARCHAR(50) NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'active',
    StartDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    EndDate DATETIME2 NULL,
    RenewalDate DATETIME2 NULL,
    BillingInterval VARCHAR(50) NOT NULL DEFAULT 'monthly',
    AutoRenew BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Subscription_Tenant FOREIGN KEY (TenantId) 
        REFERENCES dbo.Tenants(TenantId) ON DELETE CASCADE,
    CONSTRAINT CHK_Tier_Sub CHECK (Tier IN ('Free', 'Starter', 'Professional', 'Enterprise')),
    CONSTRAINT CHK_Status CHECK (Status IN ('active', 'cancelled', 'pending', 'expired'))
);

-- Create indexes for Subscriptions
CREATE INDEX IX_Subscriptions_TenantId ON dbo.TenantSubscriptions(TenantId);
CREATE INDEX IX_Subscriptions_Status ON dbo.TenantSubscriptions(Status);

-- Create Quotas Table
CREATE TABLE dbo.TenantQuotas (
    TenantId VARCHAR(100) PRIMARY KEY NOT NULL,
    MonthlyApiCallLimit INT NOT NULL DEFAULT 1000,
    MonthlyApiCallsUsed INT NOT NULL DEFAULT 0,
    DailyApiCallLimit INT NOT NULL DEFAULT 100,
    DailyApiCallsUsed INT NOT NULL DEFAULT 0,
    QuotaExceeded BIT NOT NULL DEFAULT 0,
    ResetDate DATETIME2 NOT NULL DEFAULT DATEADD(MONTH, 1, GETUTCDATE()),
    CONSTRAINT FK_Quota_Tenant FOREIGN KEY (TenantId) 
        REFERENCES dbo.Tenants(TenantId) ON DELETE CASCADE
);

-- Create indexes for Quotas
CREATE INDEX IX_Quotas_QuotaExceeded ON dbo.TenantQuotas(QuotaExceeded);
CREATE INDEX IX_Quotas_ResetDate ON dbo.TenantQuotas(ResetDate);

-- Create Custom Agents Table
CREATE TABLE dbo.TenantCustomAgents (
    AgentId VARCHAR(100) PRIMARY KEY NOT NULL,
    TenantId VARCHAR(100) NOT NULL,
    AgentName VARCHAR(255) NOT NULL,
    SystemPrompt VARCHAR(2000),
    AssignedTools NVARCHAR(MAX),
    PreferredModel VARCHAR(100) NOT NULL DEFAULT 'gpt-4',
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Agent_Tenant FOREIGN KEY (TenantId) 
        REFERENCES dbo.Tenants(TenantId) ON DELETE CASCADE
);

-- Create indexes for Custom Agents
CREATE INDEX IX_CustomAgents_TenantId ON dbo.TenantCustomAgents(TenantId);
CREATE INDEX IX_CustomAgents_IsActive ON dbo.TenantCustomAgents(IsActive);
CREATE INDEX IX_CustomAgents_CreatedDate ON dbo.TenantCustomAgents(CreatedDate);

-- Create Team Members Table
CREATE TABLE dbo.TenantTeamMembers (
    MemberId VARCHAR(100) PRIMARY KEY NOT NULL,
    TenantId VARCHAR(100) NOT NULL,
    Email VARCHAR(255) NOT NULL,
    Role VARCHAR(50) NOT NULL DEFAULT 'user',
    IsActive BIT NOT NULL DEFAULT 1,
    JoinedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Member_Tenant FOREIGN KEY (TenantId) 
        REFERENCES dbo.Tenants(TenantId) ON DELETE CASCADE,
    CONSTRAINT CHK_Role CHECK (Role IN ('admin', 'user', 'viewer')),
    CONSTRAINT UK_Tenant_Email UNIQUE(TenantId, Email)
);

-- Create indexes for Team Members
CREATE INDEX IX_TeamMembers_TenantId ON dbo.TenantTeamMembers(TenantId);
CREATE INDEX IX_TeamMembers_Email ON dbo.TenantTeamMembers(Email);
CREATE INDEX IX_TeamMembers_IsActive ON dbo.TenantTeamMembers(IsActive);

-- Create Usage Stats Table
CREATE TABLE dbo.TenantUsage (
    UsageId VARCHAR(100) PRIMARY KEY NOT NULL,
    TenantId VARCHAR(100) NOT NULL,
    Date DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ApiCallsCount INT NOT NULL DEFAULT 0,
    TokensUsed INT NOT NULL DEFAULT 0,
    CostInCents DECIMAL(10, 2) NOT NULL DEFAULT 0.00,
    CONSTRAINT FK_Usage_Tenant FOREIGN KEY (TenantId) 
        REFERENCES dbo.Tenants(TenantId) ON DELETE CASCADE
);

-- Create indexes for Usage
CREATE INDEX IX_Usage_TenantDate ON dbo.TenantUsage(TenantId, Date);
CREATE INDEX IX_Usage_Date ON dbo.TenantUsage(Date);

-- Create Billing Records Table
CREATE TABLE dbo.TenantBillingRecords (
    BillingRecordId VARCHAR(100) PRIMARY KEY NOT NULL,
    TenantId VARCHAR(100) NOT NULL,
    BillingPeriodStart DATETIME2 NOT NULL,
    BillingPeriodEnd DATETIME2 NOT NULL,
    TotalApiCalls INT NOT NULL DEFAULT 0,
    TotalTokensUsed INT NOT NULL DEFAULT 0,
    TotalCostInCents DECIMAL(10, 2) NOT NULL DEFAULT 0.00,
    Status VARCHAR(50) NOT NULL DEFAULT 'pending',
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Billing_Tenant FOREIGN KEY (TenantId) 
        REFERENCES dbo.Tenants(TenantId) ON DELETE CASCADE,
    CONSTRAINT CHK_BillingStatus CHECK (Status IN ('pending', 'sent', 'paid', 'overdue'))
);

-- Create indexes for Billing Records
CREATE INDEX IX_Billing_TenantId ON dbo.TenantBillingRecords(TenantId);
CREATE INDEX IX_Billing_Status ON dbo.TenantBillingRecords(Status);
CREATE INDEX IX_Billing_PeriodStart ON dbo.TenantBillingRecords(BillingPeriodStart);

-- Create Views for common queries
CREATE VIEW vw_ActiveTenants AS
SELECT 
    t.TenantId,
    t.TenantName,
    t.CurrentTier,
    ts.Status as SubscriptionStatus,
    tq.QuotaExceeded,
    t.CreatedDate
FROM dbo.Tenants t
LEFT JOIN dbo.TenantSubscriptions ts ON t.TenantId = ts.TenantId
LEFT JOIN dbo.TenantQuotas tq ON t.TenantId = tq.TenantId
WHERE t.IsActive = 1;

CREATE VIEW vw_TenantUsageSummary AS
SELECT 
    tu.TenantId,
    DATETRUNC(MONTH, tu.Date) as Month,
    SUM(tu.ApiCallsCount) as TotalApiCalls,
    SUM(tu.TokensUsed) as TotalTokens,
    SUM(tu.CostInCents) as TotalCost
FROM dbo.TenantUsage tu
GROUP BY tu.TenantId, DATETRUNC(MONTH, tu.Date);