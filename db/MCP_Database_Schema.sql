-- =============================================================================
-- ABC Alliant - Agentic AI MCP Demo
-- Target: SQL Server Express LocalDB  --  (localdb)\MSSQLLocalDB
-- Run via:
--   sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i MCP_Database_Schema.sql
-- or in SSMS / Azure Data Studio connected to (localdb)\MSSQLLocalDB
--
-- All PK, FK, UQ, CK, DF constraints are explicitly named.
-- =============================================================================

IF DB_ID('BenefitsIntelligenceDb') IS NULL
    CREATE DATABASE BenefitsIntelligenceDb;
GO
USE BenefitsIntelligenceDb;
GO

-- ---------- Schemas (one per bounded context = one MCP server) ----------
IF SCHEMA_ID('member')  IS NULL EXEC('CREATE SCHEMA member');
IF SCHEMA_ID('elig')    IS NULL EXEC('CREATE SCHEMA elig');
IF SCHEMA_ID('enroll')  IS NULL EXEC('CREATE SCHEMA enroll');
IF SCHEMA_ID('billing') IS NULL EXEC('CREATE SCHEMA billing');
IF SCHEMA_ID('payroll') IS NULL EXEC('CREATE SCHEMA payroll');
IF SCHEMA_ID('iam')     IS NULL EXEC('CREATE SCHEMA iam');
IF SCHEMA_ID('support') IS NULL EXEC('CREATE SCHEMA support');
GO

-- =============================================================================
-- A. SHARED / MEMBER MASTER
-- =============================================================================

CREATE TABLE member.EmployerGroup (
    GroupPolicyId   VARCHAR(32)   NOT NULL,
    GroupName       NVARCHAR(200) NOT NULL,
    PlanYearStart   DATE          NOT NULL,
    PlanYearEnd     DATE          NOT NULL,
    TpaId           VARCHAR(32)   NULL,
    Status          VARCHAR(20)   NOT NULL CONSTRAINT DF_member_EmployerGroup_Status DEFAULT ('Active'),
    CreatedUtc      DATETIME2     NOT NULL CONSTRAINT DF_member_EmployerGroup_Created DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_member_EmployerGroup PRIMARY KEY (GroupPolicyId),
    CONSTRAINT CK_member_EmployerGroup_Status
        CHECK (Status IN ('Active','Inactive','Suspended','Terminated')),
    CONSTRAINT CK_member_EmployerGroup_PlanYear
        CHECK (PlanYearEnd > PlanYearStart)
);
GO

CREATE TABLE member.Member (
    MemberId         VARCHAR(32)   NOT NULL,
    GroupPolicyId    VARCHAR(32)   NOT NULL,
    FirstName        NVARCHAR(100) NOT NULL,
    LastName         NVARCHAR(100) NOT NULL,
    DOB              DATE          NOT NULL,
    SsnLast4         CHAR(4)       NULL,
    Email            NVARCHAR(200) NULL,
    Phone            VARCHAR(20)   NULL,
    AddressLine1     NVARCHAR(200) NULL,
    City             NVARCHAR(100) NULL,
    [State]          CHAR(2)       NULL,
    Zip              VARCHAR(10)   NULL,
    EmploymentStatus VARCHAR(20)   NOT NULL CONSTRAINT DF_member_Member_EmpStatus DEFAULT ('Active'),
    HireDate         DATE          NULL,
    TerminationDate  DATE          NULL,
    SalaryBand       VARCHAR(10)   NULL,
    CreatedUtc       DATETIME2     NOT NULL CONSTRAINT DF_member_Member_Created DEFAULT (SYSUTCDATETIME()),
    UpdatedUtc       DATETIME2     NOT NULL CONSTRAINT DF_member_Member_Updated DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_member_Member PRIMARY KEY (MemberId),
    CONSTRAINT FK_member_Member_Group
        FOREIGN KEY (GroupPolicyId) REFERENCES member.EmployerGroup(GroupPolicyId),
    CONSTRAINT CK_member_Member_EmploymentStatus
        CHECK (EmploymentStatus IN ('Active','Terminated','COBRA','Retired','LeaveOfAbsence')),
    CONSTRAINT CK_member_Member_SsnLast4
        CHECK (SsnLast4 IS NULL OR SsnLast4 LIKE '[0-9][0-9][0-9][0-9]'),
    CONSTRAINT CK_member_Member_Email
        CHECK (Email IS NULL OR Email LIKE '%_@_%.__%')
);
GO
CREATE INDEX IX_member_Member_Group ON member.Member(GroupPolicyId);
GO

CREATE TABLE member.Dependent (
    DependentId  VARCHAR(32)   NOT NULL,
    MemberId     VARCHAR(32)   NOT NULL,
    FirstName    NVARCHAR(100) NOT NULL,
    LastName     NVARCHAR(100) NOT NULL,
    Relationship VARCHAR(20)   NOT NULL,
    DOB          DATE          NOT NULL,
    Gender       CHAR(1)       NULL,
    IsActive     BIT           NOT NULL CONSTRAINT DF_member_Dependent_IsActive DEFAULT (1),
    CONSTRAINT PK_member_Dependent PRIMARY KEY (DependentId),
    CONSTRAINT FK_member_Dependent_Member
        FOREIGN KEY (MemberId) REFERENCES member.Member(MemberId),
    CONSTRAINT CK_member_Dependent_Relationship
        CHECK (Relationship IN ('Spouse','Child','DomesticPartner','StepChild','Other')),
    CONSTRAINT CK_member_Dependent_Gender
        CHECK (Gender IS NULL OR Gender IN ('M','F','U'))
);
GO

-- =============================================================================
-- B. ELIGIBILITY MCP
-- =============================================================================

CREATE TABLE elig.BenefitPlan (
    PlanId         VARCHAR(40)   NOT NULL,
    PlanName       NVARCHAR(200) NOT NULL,
    LineOfBusiness VARCHAR(20)   NOT NULL,
    CarrierId      VARCHAR(40)   NOT NULL,
    PlanYear       INT           NOT NULL,
    NetworkType    VARCHAR(20)   NULL,
    DeductibleInd  DECIMAL(10,2) NULL,
    DeductibleFam  DECIMAL(10,2) NULL,
    OopMaxInd      DECIMAL(10,2) NULL,
    OopMaxFam      DECIMAL(10,2) NULL,
    Copay          DECIMAL(10,2) NULL,
    Coinsurance    DECIMAL(5,2)  NULL,
    CONSTRAINT PK_elig_BenefitPlan PRIMARY KEY (PlanId),
    CONSTRAINT UQ_elig_BenefitPlan_NameYear UNIQUE (PlanName, PlanYear),
    CONSTRAINT CK_elig_BenefitPlan_LOB
        CHECK (LineOfBusiness IN ('Medical','Dental','Vision','Life','Disability','Supplemental')),
    CONSTRAINT CK_elig_BenefitPlan_Network
        CHECK (NetworkType IS NULL OR NetworkType IN ('PPO','HMO','EPO','HDHP','POS','Indemnity')),
    CONSTRAINT CK_elig_BenefitPlan_PlanYear
        CHECK (PlanYear BETWEEN 2000 AND 2099),
    CONSTRAINT CK_elig_BenefitPlan_Coinsurance
        CHECK (Coinsurance IS NULL OR (Coinsurance >= 0 AND Coinsurance <= 100))
);
GO

CREATE TABLE elig.MemberCoverage (
    CoverageId         VARCHAR(40) NOT NULL,
    MemberId           VARCHAR(32) NOT NULL,
    PlanId             VARCHAR(40) NOT NULL,
    Tier               VARCHAR(40) NOT NULL,
    EffectiveDate      DATE        NOT NULL,
    TermDate           DATE        NULL,
    Status             VARCHAR(20) NOT NULL CONSTRAINT DF_elig_MemberCoverage_Status DEFAULT ('Active'),
    PrimaryCareProvNpi VARCHAR(20) NULL,
    CreatedUtc         DATETIME2   NOT NULL CONSTRAINT DF_elig_MemberCoverage_Created DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_elig_MemberCoverage PRIMARY KEY (CoverageId),
    CONSTRAINT FK_elig_MemberCoverage_Member
        FOREIGN KEY (MemberId) REFERENCES member.Member(MemberId),
    CONSTRAINT FK_elig_MemberCoverage_Plan
        FOREIGN KEY (PlanId) REFERENCES elig.BenefitPlan(PlanId),
    CONSTRAINT CK_elig_MemberCoverage_Tier
        CHECK (Tier IN ('EE','EE+Spouse','EE+Children','Family','Waived')),
    CONSTRAINT CK_elig_MemberCoverage_Status
        CHECK (Status IN ('Active','Terminated','Pending','COBRA','Suspended')),
    CONSTRAINT CK_elig_MemberCoverage_Dates
        CHECK (TermDate IS NULL OR TermDate >= EffectiveDate)
);
GO
CREATE INDEX IX_elig_MemberCoverage_Member ON elig.MemberCoverage(MemberId, Status);
GO

CREATE TABLE elig.Accumulator (
    AccumulatorId   BIGINT        IDENTITY(1,1) NOT NULL,
    MemberId        VARCHAR(32)   NOT NULL,
    PlanId          VARCHAR(40)   NOT NULL,
    PlanYear        INT           NOT NULL,
    AccumulatorType VARCHAR(40)   NOT NULL,
    LimitAmount     DECIMAL(12,2) NOT NULL,
    AppliedAmount   DECIMAL(12,2) NOT NULL CONSTRAINT DF_elig_Accumulator_Applied DEFAULT (0),
    LastUpdatedUtc  DATETIME2     NOT NULL CONSTRAINT DF_elig_Accumulator_Updated DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_elig_Accumulator PRIMARY KEY (AccumulatorId),
    CONSTRAINT FK_elig_Accumulator_Member
        FOREIGN KEY (MemberId) REFERENCES member.Member(MemberId),
    CONSTRAINT FK_elig_Accumulator_Plan
        FOREIGN KEY (PlanId) REFERENCES elig.BenefitPlan(PlanId),
    CONSTRAINT UQ_elig_Accumulator_MemberPlanYearType
        UNIQUE (MemberId, PlanId, PlanYear, AccumulatorType),
    CONSTRAINT CK_elig_Accumulator_Type
        CHECK (AccumulatorType IN
               ('IndDeductible','FamDeductible','IndOOP','FamOOP','VisitLimit','CopayCap')),
    CONSTRAINT CK_elig_Accumulator_Amounts
        CHECK (LimitAmount >= 0 AND AppliedAmount >= 0 AND AppliedAmount <= LimitAmount)
);
GO
CREATE INDEX IX_elig_Accumulator_Member ON elig.Accumulator(MemberId, PlanYear);
GO

CREATE TABLE elig.CoverageHistory (
    HistoryId    BIGINT        IDENTITY(1,1) NOT NULL,
    CoverageId   VARCHAR(40)   NOT NULL,
    MemberId     VARCHAR(32)   NOT NULL,
    ChangeType   VARCHAR(20)   NOT NULL,
    OldValueJson NVARCHAR(MAX) NULL,
    NewValueJson NVARCHAR(MAX) NULL,
    ChangedUtc   DATETIME2     NOT NULL CONSTRAINT DF_elig_CoverageHistory_Changed DEFAULT (SYSUTCDATETIME()),
    ChangedBy    VARCHAR(64)   NULL,
    CONSTRAINT PK_elig_CoverageHistory PRIMARY KEY (HistoryId),
    CONSTRAINT FK_elig_CoverageHistory_Member
        FOREIGN KEY (MemberId) REFERENCES member.Member(MemberId),
    CONSTRAINT CK_elig_CoverageHistory_ChangeType
        CHECK (ChangeType IN ('Added','Changed','Terminated','Reinstated'))
);
GO
CREATE INDEX IX_elig_CoverageHistory_Coverage ON elig.CoverageHistory(CoverageId);
GO

-- =============================================================================
-- C. ENROLLMENT MCP  (read + write)
-- =============================================================================

CREATE TABLE enroll.EnrollmentTransaction (
    EnrollmentId       VARCHAR(40)    NOT NULL,
    MemberId           VARCHAR(32)    NOT NULL,
    GroupPolicyId      VARCHAR(32)    NOT NULL,
    TransactionType    VARCHAR(30)    NOT NULL,
    Status             VARCHAR(20)    NOT NULL CONSTRAINT DF_enroll_Enrollment_Status DEFAULT ('Pending'),
    EffectiveDate      DATE           NOT NULL,
    SubmittedBy        VARCHAR(64)    NOT NULL,
    SubmittedChannel   VARCHAR(20)    NOT NULL,
    CorrelationId      VARCHAR(64)    NULL,
    RequestPayloadJson NVARCHAR(MAX)  NOT NULL,
    ResultPayloadJson  NVARCHAR(MAX)  NULL,
    ErrorMessage       NVARCHAR(2000) NULL,
    CreatedUtc         DATETIME2      NOT NULL CONSTRAINT DF_enroll_Enrollment_Created DEFAULT (SYSUTCDATETIME()),
    CompletedUtc       DATETIME2      NULL,
    CONSTRAINT PK_enroll_EnrollmentTransaction PRIMARY KEY (EnrollmentId),
    CONSTRAINT FK_enroll_Enrollment_Member
        FOREIGN KEY (MemberId) REFERENCES member.Member(MemberId),
    CONSTRAINT FK_enroll_Enrollment_Group
        FOREIGN KEY (GroupPolicyId) REFERENCES member.EmployerGroup(GroupPolicyId),
    CONSTRAINT CK_enroll_Enrollment_Type
        CHECK (TransactionType IN
               ('NewHire','Termination','QLE','OpenEnrollment','PlanChange','DepAdd','DepRemove','Reinstate')),
    CONSTRAINT CK_enroll_Enrollment_Status
        CHECK (Status IN ('Pending','InProgress','Completed','Failed','RequiresReview','Cancelled')),
    CONSTRAINT CK_enroll_Enrollment_Channel
        CHECK (SubmittedChannel IN ('Agent','Portal','API','834','CSR','Batch')),
    CONSTRAINT CK_enroll_Enrollment_PayloadJson
        CHECK (ISJSON(RequestPayloadJson) = 1)
);
GO
CREATE INDEX IX_enroll_Enrollment_Member ON enroll.EnrollmentTransaction(MemberId, Status);
CREATE INDEX IX_enroll_Enrollment_Correlation ON enroll.EnrollmentTransaction(CorrelationId);
GO

CREATE TABLE enroll.PlanElection (
    ElectionId    VARCHAR(40) NOT NULL,
    EnrollmentId  VARCHAR(40) NOT NULL,
    PlanId        VARCHAR(40) NOT NULL,
    Tier          VARCHAR(40) NOT NULL,
    EffectiveDate DATE        NOT NULL,
    [Action]      VARCHAR(10) NOT NULL,
    CONSTRAINT PK_enroll_PlanElection PRIMARY KEY (ElectionId),
    CONSTRAINT FK_enroll_PlanElection_Enrollment
        FOREIGN KEY (EnrollmentId) REFERENCES enroll.EnrollmentTransaction(EnrollmentId) ON DELETE CASCADE,
    CONSTRAINT FK_enroll_PlanElection_Plan
        FOREIGN KEY (PlanId) REFERENCES elig.BenefitPlan(PlanId),
    CONSTRAINT CK_enroll_PlanElection_Tier
        CHECK (Tier IN ('EE','EE+Spouse','EE+Children','Family','Waived')),
    CONSTRAINT CK_enroll_PlanElection_Action
        CHECK ([Action] IN ('Add','Drop','Change'))
);
GO

CREATE TABLE enroll.QualifyingLifeEvent (
    QleId             VARCHAR(40) NOT NULL,
    MemberId          VARCHAR(32) NOT NULL,
    QleType           VARCHAR(30) NOT NULL,
    QleDate           DATE        NOT NULL,
    ElectionWindowEnd DATE        NOT NULL,
    Status            VARCHAR(20) NOT NULL CONSTRAINT DF_enroll_QLE_Status DEFAULT ('Open'),
    CONSTRAINT PK_enroll_QualifyingLifeEvent PRIMARY KEY (QleId),
    CONSTRAINT FK_enroll_QLE_Member
        FOREIGN KEY (MemberId) REFERENCES member.Member(MemberId),
    CONSTRAINT CK_enroll_QLE_Type
        CHECK (QleType IN
               ('Marriage','Birth','Adoption','Divorce','Newborn','JobLoss','SpouseJobChange','Death','Other')),
    CONSTRAINT CK_enroll_QLE_Status
        CHECK (Status IN ('Open','Used','Expired')),
    CONSTRAINT CK_enroll_QLE_Window
        CHECK (ElectionWindowEnd >= QleDate)
);
GO
CREATE INDEX IX_enroll_QLE_Member ON enroll.QualifyingLifeEvent(MemberId, Status);
GO

CREATE TABLE enroll.OutboxEvent (
    OutboxId     BIGINT        IDENTITY(1,1) NOT NULL,
    AggregateId  VARCHAR(40)   NOT NULL,
    EventType    VARCHAR(80)   NOT NULL,
    PayloadJson  NVARCHAR(MAX) NOT NULL,
    CreatedUtc   DATETIME2     NOT NULL CONSTRAINT DF_enroll_OutboxEvent_Created DEFAULT (SYSUTCDATETIME()),
    PublishedUtc DATETIME2     NULL,
    CONSTRAINT PK_enroll_OutboxEvent PRIMARY KEY (OutboxId),
    CONSTRAINT CK_enroll_OutboxEvent_PayloadJson
        CHECK (ISJSON(PayloadJson) = 1)
);
GO
CREATE INDEX IX_enroll_Outbox_Unpublished ON enroll.OutboxEvent(PublishedUtc) WHERE PublishedUtc IS NULL;
GO

-- =============================================================================
-- D. BILLING & PAYROLL MCP
-- =============================================================================

CREATE TABLE billing.Invoice (
    InvoiceId          VARCHAR(40)   NOT NULL,
    GroupPolicyId      VARCHAR(32)   NOT NULL,
    MemberId           VARCHAR(32)   NULL,
    BillingPeriodStart DATE          NOT NULL,
    BillingPeriodEnd   DATE          NOT NULL,
    InvoiceDate        DATE          NOT NULL,
    DueDate            DATE          NOT NULL,
    TotalAmount        DECIMAL(12,2) NOT NULL,
    BalanceDue         DECIMAL(12,2) NOT NULL,
    Status             VARCHAR(20)   NOT NULL CONSTRAINT DF_billing_Invoice_Status DEFAULT ('Open'),
    CreatedUtc         DATETIME2     NOT NULL CONSTRAINT DF_billing_Invoice_Created DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_billing_Invoice PRIMARY KEY (InvoiceId),
    CONSTRAINT FK_billing_Invoice_Group
        FOREIGN KEY (GroupPolicyId) REFERENCES member.EmployerGroup(GroupPolicyId),
    CONSTRAINT FK_billing_Invoice_Member
        FOREIGN KEY (MemberId) REFERENCES member.Member(MemberId),
    CONSTRAINT CK_billing_Invoice_Status
        CHECK (Status IN ('Open','Paid','PastDue','Disputed','Void','PartiallyPaid')),
    CONSTRAINT CK_billing_Invoice_Amounts
        CHECK (TotalAmount >= 0 AND BalanceDue >= 0 AND BalanceDue <= TotalAmount),
    CONSTRAINT CK_billing_Invoice_Period
        CHECK (BillingPeriodEnd >= BillingPeriodStart AND DueDate >= InvoiceDate)
);
GO
CREATE INDEX IX_billing_Invoice_Member ON billing.Invoice(MemberId, Status);
CREATE INDEX IX_billing_Invoice_Group  ON billing.Invoice(GroupPolicyId, Status);
GO

CREATE TABLE billing.InvoiceLine (
    InvoiceLineId   BIGINT        IDENTITY(1,1) NOT NULL,
    InvoiceId       VARCHAR(40)   NOT NULL,
    PlanId          VARCHAR(40)   NOT NULL,
    LineDescription NVARCHAR(200) NOT NULL,
    PremiumAmount   DECIMAL(12,2) NOT NULL,
    EmployerPortion DECIMAL(12,2) NOT NULL,
    EmployeePortion DECIMAL(12,2) NOT NULL,
    CONSTRAINT PK_billing_InvoiceLine PRIMARY KEY (InvoiceLineId),
    CONSTRAINT FK_billing_InvoiceLine_Invoice
        FOREIGN KEY (InvoiceId) REFERENCES billing.Invoice(InvoiceId) ON DELETE CASCADE,
    CONSTRAINT FK_billing_InvoiceLine_Plan
        FOREIGN KEY (PlanId) REFERENCES elig.BenefitPlan(PlanId),
    CONSTRAINT CK_billing_InvoiceLine_Amounts
        CHECK (PremiumAmount >= 0 AND EmployerPortion >= 0 AND EmployeePortion >= 0),
    CONSTRAINT CK_billing_InvoiceLine_Sum
        CHECK (EmployerPortion + EmployeePortion = PremiumAmount)
);
GO

CREATE TABLE billing.Payment (
    PaymentId          VARCHAR(40)   NOT NULL,
    InvoiceId          VARCHAR(40)   NULL,
    MemberId           VARCHAR(32)   NULL,
    GroupPolicyId      VARCHAR(32)   NULL,
    Amount             DECIMAL(12,2) NOT NULL,
    PaymentMethod      VARCHAR(20)   NOT NULL,
    PaymentDate        DATE          NOT NULL,
    ConfirmationNumber VARCHAR(40)   NOT NULL,
    Status             VARCHAR(20)   NOT NULL CONSTRAINT DF_billing_Payment_Status DEFAULT ('Posted'),
    CONSTRAINT PK_billing_Payment PRIMARY KEY (PaymentId),
    CONSTRAINT FK_billing_Payment_Invoice
        FOREIGN KEY (InvoiceId) REFERENCES billing.Invoice(InvoiceId),
    CONSTRAINT FK_billing_Payment_Member
        FOREIGN KEY (MemberId) REFERENCES member.Member(MemberId),
    CONSTRAINT FK_billing_Payment_Group
        FOREIGN KEY (GroupPolicyId) REFERENCES member.EmployerGroup(GroupPolicyId),
    CONSTRAINT UQ_billing_Payment_Confirmation UNIQUE (ConfirmationNumber),
    CONSTRAINT CK_billing_Payment_Method
        CHECK (PaymentMethod IN ('ACH','Card','Check','Wire','Cash','Payroll')),
    CONSTRAINT CK_billing_Payment_Status
        CHECK (Status IN ('Posted','Pending','Failed','Refunded','Reversed')),
    CONSTRAINT CK_billing_Payment_Amount
        CHECK (Amount > 0)
);
GO

CREATE TABLE billing.PremiumRate (
    RateId         VARCHAR(40)   NOT NULL,
    PlanId         VARCHAR(40)   NOT NULL,
    Tier           VARCHAR(40)   NOT NULL,
    AgeBand        VARCHAR(20)   NULL,
    SalaryBand     VARCHAR(10)   NULL,
    MonthlyPremium DECIMAL(10,2) NOT NULL,
    EffectiveDate  DATE          NOT NULL,
    EndDate        DATE          NULL,
    CONSTRAINT PK_billing_PremiumRate PRIMARY KEY (RateId),
    CONSTRAINT FK_billing_PremiumRate_Plan
        FOREIGN KEY (PlanId) REFERENCES elig.BenefitPlan(PlanId),
    CONSTRAINT CK_billing_PremiumRate_Tier
        CHECK (Tier IN ('EE','EE+Spouse','EE+Children','Family')),
    CONSTRAINT CK_billing_PremiumRate_Amount
        CHECK (MonthlyPremium >= 0),
    CONSTRAINT CK_billing_PremiumRate_Dates
        CHECK (EndDate IS NULL OR EndDate >= EffectiveDate)
);
GO

CREATE TABLE payroll.PayrollDeduction (
    DeductionId    VARCHAR(40)   NOT NULL,
    MemberId       VARCHAR(32)   NOT NULL,
    PlanId         VARCHAR(40)   NOT NULL,
    PayPeriodStart DATE          NOT NULL,
    PayPeriodEnd   DATE          NOT NULL,
    PreTaxAmount   DECIMAL(10,2) NOT NULL CONSTRAINT DF_payroll_Deduction_PreTax DEFAULT (0),
    PostTaxAmount  DECIMAL(10,2) NOT NULL CONSTRAINT DF_payroll_Deduction_PostTax DEFAULT (0),
    Status         VARCHAR(20)   NOT NULL CONSTRAINT DF_payroll_Deduction_Status DEFAULT ('Scheduled'),
    CreatedUtc     DATETIME2     NOT NULL CONSTRAINT DF_payroll_Deduction_Created DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_payroll_PayrollDeduction PRIMARY KEY (DeductionId),
    CONSTRAINT FK_payroll_Deduction_Member
        FOREIGN KEY (MemberId) REFERENCES member.Member(MemberId),
    CONSTRAINT FK_payroll_Deduction_Plan
        FOREIGN KEY (PlanId) REFERENCES elig.BenefitPlan(PlanId),
    CONSTRAINT CK_payroll_Deduction_Status
        CHECK (Status IN ('Scheduled','Deducted','Reversed','Skipped')),
    CONSTRAINT CK_payroll_Deduction_Amounts
        CHECK (PreTaxAmount >= 0 AND PostTaxAmount >= 0),
    CONSTRAINT CK_payroll_Deduction_Period
        CHECK (PayPeriodEnd >= PayPeriodStart)
);
GO
CREATE INDEX IX_payroll_Deduction_Member ON payroll.PayrollDeduction(MemberId, PayPeriodEnd);
GO

CREATE TABLE payroll.PayrollFile (
    PayrollFileId   VARCHAR(40)   NOT NULL,
    GroupPolicyId   VARCHAR(32)   NOT NULL,
    PayDate         DATE          NOT NULL,
    FileBlobUri     NVARCHAR(500) NULL,
    TotalDeductions DECIMAL(14,2) NOT NULL,
    Status          VARCHAR(20)   NOT NULL CONSTRAINT DF_payroll_File_Status DEFAULT ('Received'),
    CONSTRAINT PK_payroll_PayrollFile PRIMARY KEY (PayrollFileId),
    CONSTRAINT FK_payroll_File_Group
        FOREIGN KEY (GroupPolicyId) REFERENCES member.EmployerGroup(GroupPolicyId),
    CONSTRAINT CK_payroll_File_Status
        CHECK (Status IN ('Received','Processing','Posted','Failed','Reconciled')),
    CONSTRAINT CK_payroll_File_Total
        CHECK (TotalDeductions >= 0)
);
GO

-- =============================================================================
-- E. ACCOUNT & ACCESS MCP
-- =============================================================================

CREATE TABLE iam.UserAccount (
    UserId             VARCHAR(64)   NOT NULL,
    MemberId           VARCHAR(32)   NULL,
    UserType           VARCHAR(20)   NOT NULL,
    Username           NVARCHAR(100) NOT NULL,
    Email              NVARCHAR(200) NOT NULL,
    OktaUserId         VARCHAR(64)   NULL,
    Status             VARCHAR(20)   NOT NULL CONSTRAINT DF_iam_UserAccount_Status DEFAULT ('Active'),
    MfaEnrolled        BIT           NOT NULL CONSTRAINT DF_iam_UserAccount_Mfa DEFAULT (0),
    LastLoginUtc       DATETIME2     NULL,
    FailedLoginCount   INT           NOT NULL CONSTRAINT DF_iam_UserAccount_FailedCount DEFAULT (0),
    LockedUntilUtc     DATETIME2     NULL,
    PasswordChangedUtc DATETIME2     NULL,
    CreatedUtc         DATETIME2     NOT NULL CONSTRAINT DF_iam_UserAccount_Created DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_iam_UserAccount PRIMARY KEY (UserId),
    CONSTRAINT FK_iam_UserAccount_Member
        FOREIGN KEY (MemberId) REFERENCES member.Member(MemberId),
    CONSTRAINT UQ_iam_UserAccount_Username UNIQUE (Username),
    CONSTRAINT UQ_iam_UserAccount_Email    UNIQUE (Email),
    CONSTRAINT UQ_iam_UserAccount_OktaId   UNIQUE (OktaUserId),
    CONSTRAINT CK_iam_UserAccount_Type
        CHECK (UserType IN ('Member','CSR','HRAdmin','TPAAdmin','Broker','Vendor')),
    CONSTRAINT CK_iam_UserAccount_Status
        CHECK (Status IN ('Active','Locked','Disabled','Suspended','PasswordExpired','PendingActivation')),
    CONSTRAINT CK_iam_UserAccount_FailedCount
        CHECK (FailedLoginCount >= 0),
    CONSTRAINT CK_iam_UserAccount_Email
        CHECK (Email LIKE '%_@_%.__%')
);
GO

CREATE TABLE iam.LoginAttempt (
    AttemptId     BIGINT        IDENTITY(1,1) NOT NULL,
    UserId        VARCHAR(64)   NULL,
    Username      NVARCHAR(100) NOT NULL,
    AttemptUtc    DATETIME2     NOT NULL CONSTRAINT DF_iam_LoginAttempt_Utc DEFAULT (SYSUTCDATETIME()),
    Success       BIT           NOT NULL,
    FailureReason VARCHAR(80)   NULL,
    IpAddress     VARCHAR(45)   NULL,
    UserAgent     NVARCHAR(400) NULL,
    DeviceId      VARCHAR(64)   NULL,
    CONSTRAINT PK_iam_LoginAttempt PRIMARY KEY (AttemptId),
    CONSTRAINT FK_iam_LoginAttempt_User
        FOREIGN KEY (UserId) REFERENCES iam.UserAccount(UserId),
    CONSTRAINT CK_iam_LoginAttempt_Reason
        CHECK (FailureReason IS NULL OR FailureReason IN
               ('BadPassword','Locked','MFAFail','Disabled','Expired','UnknownUser','RateLimit','Other')),
    CONSTRAINT CK_iam_LoginAttempt_SuccessReason
        CHECK ((Success = 1 AND FailureReason IS NULL) OR (Success = 0 AND FailureReason IS NOT NULL))
);
GO
CREATE INDEX IX_iam_LoginAttempt_User ON iam.LoginAttempt(UserId, AttemptUtc DESC);
GO

CREATE TABLE iam.PasswordResetRequest (
    ResetRequestId     VARCHAR(64) NOT NULL,
    UserId             VARCHAR(64) NOT NULL,
    RequestedUtc       DATETIME2   NOT NULL CONSTRAINT DF_iam_PasswordReset_Requested DEFAULT (SYSUTCDATETIME()),
    Channel            VARCHAR(20) NOT NULL,
    VerificationMethod VARCHAR(20) NOT NULL,
    Status             VARCHAR(20) NOT NULL CONSTRAINT DF_iam_PasswordReset_Status DEFAULT ('Pending'),
    ExpiresUtc         DATETIME2   NOT NULL,
    CompletedUtc       DATETIME2   NULL,
    InitiatedBy        VARCHAR(64) NOT NULL,
    CONSTRAINT PK_iam_PasswordResetRequest PRIMARY KEY (ResetRequestId),
    CONSTRAINT FK_iam_PasswordReset_User
        FOREIGN KEY (UserId) REFERENCES iam.UserAccount(UserId),
    CONSTRAINT CK_iam_PasswordReset_Channel
        CHECK (Channel IN ('Email','SMS','Agent','Portal')),
    CONSTRAINT CK_iam_PasswordReset_Verification
        CHECK (VerificationMethod IN ('Email','SMS','KBA','MFA','InPerson')),
    CONSTRAINT CK_iam_PasswordReset_Status
        CHECK (Status IN ('Pending','Sent','Completed','Failed','Expired','Cancelled')),
    CONSTRAINT CK_iam_PasswordReset_Expiry
        CHECK (ExpiresUtc > RequestedUtc)
);
GO
CREATE INDEX IX_iam_PasswordReset_User ON iam.PasswordResetRequest(UserId, Status);
GO

CREATE TABLE iam.AccessRole (
    RoleId      VARCHAR(40)   NOT NULL,
    RoleName    VARCHAR(80)   NOT NULL,
    Description NVARCHAR(400) NULL,
    ScopesJson  NVARCHAR(MAX) NOT NULL,
    CONSTRAINT PK_iam_AccessRole PRIMARY KEY (RoleId),
    CONSTRAINT UQ_iam_AccessRole_Name UNIQUE (RoleName),
    CONSTRAINT CK_iam_AccessRole_ScopesJson CHECK (ISJSON(ScopesJson) = 1)
);
GO

CREATE TABLE iam.UserRole (
    UserId     VARCHAR(64) NOT NULL,
    RoleId     VARCHAR(40) NOT NULL,
    GrantedUtc DATETIME2   NOT NULL CONSTRAINT DF_iam_UserRole_Granted DEFAULT (SYSUTCDATETIME()),
    GrantedBy  VARCHAR(64) NOT NULL,
    CONSTRAINT PK_iam_UserRole PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_iam_UserRole_User
        FOREIGN KEY (UserId) REFERENCES iam.UserAccount(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_iam_UserRole_Role
        FOREIGN KEY (RoleId) REFERENCES iam.AccessRole(RoleId) ON DELETE CASCADE
);
GO

-- =============================================================================
-- F. CUSTOMER SUPPORT MCP  (case management, interactions, escalations)
-- =============================================================================

CREATE TABLE support.[Case] (
    CaseId          VARCHAR(40)    NOT NULL,
    MemberId        VARCHAR(32)    NULL,
    GroupPolicyId   VARCHAR(32)    NULL,
    CaseType        VARCHAR(40)    NOT NULL,
    Subject         NVARCHAR(300)  NOT NULL,
    Description     NVARCHAR(MAX)  NULL,
    Status          VARCHAR(20)    NOT NULL CONSTRAINT DF_support_Case_Status DEFAULT ('New'),
    Priority        VARCHAR(10)    NOT NULL CONSTRAINT DF_support_Case_Priority DEFAULT ('Normal'),
    Channel         VARCHAR(20)    NOT NULL,
    AssignedToUserId VARCHAR(64)   NULL,
    AssignedQueue   VARCHAR(60)    NULL,
    SlaDueUtc       DATETIME2      NULL,
    SlaBreached     BIT            NOT NULL CONSTRAINT DF_support_Case_Breached DEFAULT (0),
    CorrelationId   VARCHAR(64)    NULL,
    CreatedByUserId VARCHAR(64)    NOT NULL,
    CreatedBySource VARCHAR(20)    NOT NULL,
    CreatedUtc      DATETIME2      NOT NULL CONSTRAINT DF_support_Case_Created DEFAULT (SYSUTCDATETIME()),
    UpdatedUtc      DATETIME2      NOT NULL CONSTRAINT DF_support_Case_Updated DEFAULT (SYSUTCDATETIME()),
    ResolvedUtc     DATETIME2      NULL,
    ClosedUtc       DATETIME2      NULL,
    ResolutionNotes NVARCHAR(MAX)  NULL,
    CONSTRAINT PK_support_Case PRIMARY KEY (CaseId),
    CONSTRAINT FK_support_Case_Member
        FOREIGN KEY (MemberId) REFERENCES member.Member(MemberId),
    CONSTRAINT FK_support_Case_Group
        FOREIGN KEY (GroupPolicyId) REFERENCES member.EmployerGroup(GroupPolicyId),
    CONSTRAINT FK_support_Case_AssignedTo
        FOREIGN KEY (AssignedToUserId) REFERENCES iam.UserAccount(UserId),
    CONSTRAINT CK_support_Case_Type
        CHECK (CaseType IN
               ('GeneralInquiry','EnrollmentChange','ClaimsDispute','BillingInquiry',
                'COBRAInquiry','ProviderIssue','Complaint','Appeal','IDCardRequest',
                'DocumentRequest','EligibilityVerification','QLEProcessing',
                'PasswordReset','LoginIssue','PayrollInquiry','PlanQuestion','Other')),
    CONSTRAINT CK_support_Case_Status
        CHECK (Status IN ('New','Open','InProgress','Pending','Escalated','Resolved','Closed','Cancelled')),
    CONSTRAINT CK_support_Case_Priority
        CHECK (Priority IN ('Low','Normal','High','Critical')),
    CONSTRAINT CK_support_Case_Channel
        CHECK (Channel IN ('Phone','Email','Chat','Portal','Agent','Teams','API')),
    CONSTRAINT CK_support_Case_Source
        CHECK (CreatedBySource IN ('Agent','CSR','Member','HRAdmin','TPAAdmin','System')),
    CONSTRAINT CK_support_Case_Dates
        CHECK (ResolvedUtc IS NULL OR ResolvedUtc >= CreatedUtc)
);
GO
CREATE INDEX IX_support_Case_Member   ON support.[Case](MemberId, Status);
CREATE INDEX IX_support_Case_Assigned ON support.[Case](AssignedToUserId, Status);
CREATE INDEX IX_support_Case_Queue    ON support.[Case](AssignedQueue, Status);
CREATE INDEX IX_support_Case_Sla      ON support.[Case](SlaDueUtc) WHERE Status <> 'Resolved' AND Status <> 'Closed' AND Status <> 'Cancelled';
GO

CREATE TABLE support.CaseNote (
    CaseNoteId    BIGINT         IDENTITY(1,1) NOT NULL,
    CaseId        VARCHAR(40)    NOT NULL,
    AuthorUserId  VARCHAR(64)    NOT NULL,
    AuthorType    VARCHAR(20)    NOT NULL,
    NoteText      NVARCHAR(MAX)  NOT NULL,
    IsInternal    BIT            NOT NULL CONSTRAINT DF_support_CaseNote_Internal DEFAULT (1),
    CreatedUtc    DATETIME2      NOT NULL CONSTRAINT DF_support_CaseNote_Created DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_support_CaseNote PRIMARY KEY (CaseNoteId),
    CONSTRAINT FK_support_CaseNote_Case
        FOREIGN KEY (CaseId) REFERENCES support.[Case](CaseId) ON DELETE CASCADE,
    CONSTRAINT CK_support_CaseNote_AuthorType
        CHECK (AuthorType IN ('Agent','CSR','Member','HRAdmin','TPAAdmin','System'))
);
GO
CREATE INDEX IX_support_CaseNote_Case ON support.CaseNote(CaseId, CreatedUtc DESC);
GO

CREATE TABLE support.Interaction (
    InteractionId   VARCHAR(40)    NOT NULL,
    MemberId        VARCHAR(32)    NULL,
    CaseId          VARCHAR(40)    NULL,
    Channel         VARCHAR(20)    NOT NULL,
    Direction       VARCHAR(10)    NOT NULL,
    HandledByUserId VARCHAR(64)    NULL,
    HandledByAgent  BIT            NOT NULL CONSTRAINT DF_support_Interaction_ByAgent DEFAULT (0),
    SessionId       VARCHAR(64)    NULL,
    Intent          VARCHAR(60)    NULL,
    Summary         NVARCHAR(MAX)  NULL,
    TranscriptUri   NVARCHAR(500)  NULL,
    SentimentScore  DECIMAL(4,3)   NULL,
    DurationSeconds INT            NULL,
    StartedUtc      DATETIME2      NOT NULL CONSTRAINT DF_support_Interaction_Started DEFAULT (SYSUTCDATETIME()),
    EndedUtc        DATETIME2      NULL,
    CONSTRAINT PK_support_Interaction PRIMARY KEY (InteractionId),
    CONSTRAINT FK_support_Interaction_Member
        FOREIGN KEY (MemberId) REFERENCES member.Member(MemberId),
    CONSTRAINT FK_support_Interaction_Case
        FOREIGN KEY (CaseId) REFERENCES support.[Case](CaseId),
    CONSTRAINT FK_support_Interaction_HandledBy
        FOREIGN KEY (HandledByUserId) REFERENCES iam.UserAccount(UserId),
    CONSTRAINT CK_support_Interaction_Channel
        CHECK (Channel IN ('Phone','Email','Chat','Portal','SMS','Teams','API')),
    CONSTRAINT CK_support_Interaction_Direction
        CHECK (Direction IN ('Inbound','Outbound')),
    CONSTRAINT CK_support_Interaction_Intent
        CHECK (Intent IS NULL OR Intent IN
               ('Eligibility','Enrollment','Billing','Payroll','PlanQA',
                'PasswordReset','LoginIssue','Complaint','Other')),
    CONSTRAINT CK_support_Interaction_Sentiment
        CHECK (SentimentScore IS NULL OR (SentimentScore >= -1 AND SentimentScore <= 1)),
    CONSTRAINT CK_support_Interaction_Duration
        CHECK (DurationSeconds IS NULL OR DurationSeconds >= 0),
    CONSTRAINT CK_support_Interaction_Dates
        CHECK (EndedUtc IS NULL OR EndedUtc >= StartedUtc)
);
GO
CREATE INDEX IX_support_Interaction_Member  ON support.Interaction(MemberId, StartedUtc DESC);
CREATE INDEX IX_support_Interaction_Case    ON support.Interaction(CaseId);
CREATE INDEX IX_support_Interaction_Session ON support.Interaction(SessionId);
GO

CREATE TABLE support.Escalation (
    EscalationId    VARCHAR(40)    NOT NULL,
    CaseId          VARCHAR(40)    NOT NULL,
    FromUserId      VARCHAR(64)    NULL,
    ToQueue         VARCHAR(60)    NOT NULL,
    ToUserId        VARCHAR(64)    NULL,
    Reason          VARCHAR(40)    NOT NULL,
    ReasonDetail    NVARCHAR(1000) NULL,
    Status          VARCHAR(20)    NOT NULL CONSTRAINT DF_support_Escalation_Status DEFAULT ('Open'),
    EscalatedUtc    DATETIME2      NOT NULL CONSTRAINT DF_support_Escalation_Escalated DEFAULT (SYSUTCDATETIME()),
    AcknowledgedUtc DATETIME2      NULL,
    ResolvedUtc     DATETIME2      NULL,
    CONSTRAINT PK_support_Escalation PRIMARY KEY (EscalationId),
    CONSTRAINT FK_support_Escalation_Case
        FOREIGN KEY (CaseId) REFERENCES support.[Case](CaseId) ON DELETE CASCADE,
    CONSTRAINT FK_support_Escalation_From
        FOREIGN KEY (FromUserId) REFERENCES iam.UserAccount(UserId),
    CONSTRAINT FK_support_Escalation_To
        FOREIGN KEY (ToUserId) REFERENCES iam.UserAccount(UserId),
    CONSTRAINT CK_support_Escalation_Reason
        CHECK (Reason IN
               ('LowConfidence','Complaint','ActionRequired','GuardrailTriggered',
                'SLABreach','MemberRequest','Compliance','Supervisor','Other')),
    CONSTRAINT CK_support_Escalation_Queue
        CHECK (ToQueue IN
               ('CSRTier2','CSRTier3','BillingOps','EnrollmentOps','Compliance',
                'HRAdmin','TPAAdmin','SecurityOps','LegalReview')),
    CONSTRAINT CK_support_Escalation_Status
        CHECK (Status IN ('Open','Acknowledged','InProgress','Resolved','Cancelled'))
);
GO
CREATE INDEX IX_support_Escalation_Case  ON support.Escalation(CaseId);
CREATE INDEX IX_support_Escalation_Queue ON support.Escalation(ToQueue, Status);
GO

CREATE TABLE support.Complaint (
    ComplaintId       VARCHAR(40)    NOT NULL,
    CaseId            VARCHAR(40)    NOT NULL,
    MemberId          VARCHAR(32)    NOT NULL,
    ComplaintType     VARCHAR(40)    NOT NULL,
    Severity          VARCHAR(20)    NOT NULL,
    RegulatoryFlag    BIT            NOT NULL CONSTRAINT DF_support_Complaint_Regulatory DEFAULT (0),
    RegulatorAgency   VARCHAR(60)    NULL,
    Description       NVARCHAR(MAX)  NOT NULL,
    RootCause         NVARCHAR(MAX)  NULL,
    CorrectiveAction  NVARCHAR(MAX)  NULL,
    Status            VARCHAR(20)    NOT NULL CONSTRAINT DF_support_Complaint_Status DEFAULT ('Filed'),
    FiledUtc          DATETIME2      NOT NULL CONSTRAINT DF_support_Complaint_Filed DEFAULT (SYSUTCDATETIME()),
    DueUtc            DATETIME2      NOT NULL,
    ResolvedUtc       DATETIME2      NULL,
    CONSTRAINT PK_support_Complaint PRIMARY KEY (ComplaintId),
    CONSTRAINT FK_support_Complaint_Case
        FOREIGN KEY (CaseId) REFERENCES support.[Case](CaseId),
    CONSTRAINT FK_support_Complaint_Member
        FOREIGN KEY (MemberId) REFERENCES member.Member(MemberId),
    CONSTRAINT CK_support_Complaint_Type
        CHECK (ComplaintType IN
               ('Service','Billing','Claims','Coverage','Access','Privacy','Discrimination','Other')),
    CONSTRAINT CK_support_Complaint_Severity
        CHECK (Severity IN ('Low','Medium','High','Critical')),
    CONSTRAINT CK_support_Complaint_Status
        CHECK (Status IN ('Filed','UnderReview','Resolved','Dismissed','Escalated')),
    CONSTRAINT CK_support_Complaint_Due
        CHECK (DueUtc >= FiledUtc)
);
GO
CREATE INDEX IX_support_Complaint_Member ON support.Complaint(MemberId, Status);
CREATE INDEX IX_support_Complaint_Due    ON support.Complaint(DueUtc) WHERE Status <> 'Resolved' AND Status <> 'Dismissed';
GO

-- =============================================================================
-- End of schema
--   Verify:
--     SELECT s.name AS [schema], t.name AS [table]
--     FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id
--     ORDER BY s.name, t.name;
-- ===============