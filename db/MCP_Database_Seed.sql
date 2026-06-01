-- =============================================================================
-- BenefitsIntelligenceDb - Seed Data
-- Target: (localdb)\MSSQLLocalDB
-- Run AFTER MCP_Database_Schema.sql against an empty database.
-- Order respects all FK constraints. All values satisfy CHECK constraints.
-- =============================================================================

USE BenefitsIntelligenceDb;
GO

SET NOCOUNT ON;
GO

-- =============================================================================
-- Cleanup (re-runnable). Delete in reverse FK order.
-- =============================================================================

DELETE FROM support.Complaint;
DELETE FROM support.Escalation;
DELETE FROM support.Interaction;
DELETE FROM support.CaseNote;
DELETE FROM support.[Case];
DELETE FROM iam.UserRole;
DELETE FROM iam.AccessRole;
DELETE FROM iam.PasswordResetRequest;
DELETE FROM iam.LoginAttempt;
DELETE FROM iam.UserAccount;
DELETE FROM payroll.PayrollFile;
DELETE FROM payroll.PayrollDeduction;
DELETE FROM billing.PremiumRate;
DELETE FROM billing.Payment;
DELETE FROM billing.InvoiceLine;
DELETE FROM billing.Invoice;
DELETE FROM enroll.OutboxEvent;
DELETE FROM enroll.QualifyingLifeEvent;
DELETE FROM enroll.PlanElection;
DELETE FROM enroll.EnrollmentTransaction;
DELETE FROM elig.CoverageHistory;
DELETE FROM elig.Accumulator;
DELETE FROM elig.MemberCoverage;
DELETE FROM elig.BenefitPlan;
DELETE FROM member.Dependent;
DELETE FROM member.Member;
DELETE FROM member.EmployerGroup;
GO

-- =============================================================================
-- A. MEMBER MASTER
-- =============================================================================

INSERT INTO member.EmployerGroup (GroupPolicyId, GroupName, PlanYearStart, PlanYearEnd, TpaId, Status) VALUES
('GP-ACME-001',   N'ACME Corporation',         '2026-01-01','2026-12-31','TPA-ABC','Active'),
('GP-NORTH-002',  N'Northwind Traders',        '2026-01-01','2026-12-31','TPA-ABC','Active'),
('GP-CONTOSO-03', N'Contoso Ltd',              '2026-01-01','2026-12-31','TPA-ABC','Active');
GO

INSERT INTO member.Member (MemberId, GroupPolicyId, FirstName, LastName, DOB, SsnLast4, Email, Phone, AddressLine1, City, [State], Zip, EmploymentStatus, HireDate, SalaryBand) VALUES
('MEM-10001','GP-ACME-001',  N'John',     N'Doe',       '1985-03-15','6789','john.doe@acme.com',    '312-555-0101','123 Main St',     N'Chicago',     'IL','60601','Active',     '2020-06-15','Band-4'),
('MEM-10002','GP-ACME-001',  N'Jane',     N'Smith',     '1990-07-22','3456','jane.smith@acme.com',  '312-555-0102','456 Oak Ave',     N'Chicago',     'IL','60602','Active',     '2021-03-01','Band-3'),
('MEM-10003','GP-ACME-001',  N'Carlos',   N'Garcia',    '1978-11-09','1122','carlos.g@acme.com',    '312-555-0103','789 Pine Rd',     N'Evanston',    'IL','60201','Active',     '2018-09-12','Band-5'),
('MEM-10004','GP-ACME-001',  N'Priya',    N'Patel',     '1992-02-28','9988','priya.p@acme.com',     '312-555-0104','321 Lake Dr',     N'Chicago',     'IL','60603','Active',     '2022-01-10','Band-2'),
('MEM-10005','GP-ACME-001',  N'Mike',     N'Johnson',   '1980-05-17','4455','mike.j@acme.com',      '312-555-0105','654 Elm St',     N'Naperville',  'IL','60540','Terminated', '2019-04-20','Band-3'),
('MEM-20001','GP-NORTH-002', N'Alice',    N'Brown',     '1988-08-30','7777','alice.b@northwind.com','206-555-0201','111 Pike Pl',     N'Seattle',     'WA','98101','Active',     '2020-11-01','Band-4'),
('MEM-20002','GP-NORTH-002', N'Bob',      N'Williams',  '1975-12-12','2222','bob.w@northwind.com',  '206-555-0202','222 Cherry St',  N'Seattle',     'WA','98102','Active',     '2017-05-15','Band-6'),
('MEM-20003','GP-NORTH-002', N'Sofia',    N'Lopez',     '1995-04-04','3333','sofia.l@northwind.com','206-555-0203','333 Spring Ave', N'Bellevue',    'WA','98004','Active',     '2023-02-20','Band-2'),
('MEM-20004','GP-NORTH-002', N'David',    N'Kim',       '1983-09-09','4444','david.k@northwind.com','206-555-0204','444 Olive Way',  N'Seattle',     'WA','98103','COBRA',      '2018-08-08','Band-4'),
('MEM-30001','GP-CONTOSO-03',N'Emily',    N'Chen',      '1991-06-25','5555','emily.c@contoso.com',  '617-555-0301','555 Beacon St',  N'Boston',      'MA','02108','Active',     '2021-07-12','Band-3'),
('MEM-30002','GP-CONTOSO-03',N'Raj',      N'Singh',     '1986-01-18','6666','raj.s@contoso.com',    '617-555-0302','666 Newbury St', N'Boston',      'MA','02116','Active',     '2019-10-05','Band-5'),
('MEM-30003','GP-CONTOSO-03',N'Hannah',   N'Murphy',    '1993-10-31','8888','hannah.m@contoso.com', '617-555-0303','777 Boylston St',N'Boston',      'MA','02199','LeaveOfAbsence','2022-04-18','Band-3');
GO

INSERT INTO member.Dependent (DependentId, MemberId, FirstName, LastName, Relationship, DOB, Gender, IsActive) VALUES
('DEP-10001-1','MEM-10001',N'Sarah',    N'Doe',     'Spouse',         '1986-08-22','F',1),
('DEP-10001-2','MEM-10001',N'Alex',     N'Doe',     'Child',          '2018-03-10','M',1),
('DEP-10001-3','MEM-10001',N'Lily',     N'Doe',     'Child',          '2020-11-04','F',1),
('DEP-10002-1','MEM-10002',N'Tom',      N'Smith',   'DomesticPartner','1989-04-14','M',1),
('DEP-10003-1','MEM-10003',N'Maria',    N'Garcia',  'Spouse',         '1979-02-02','F',1),
('DEP-10003-2','MEM-10003',N'Diego',    N'Garcia',  'Child',          '2010-05-19','M',1),
('DEP-10003-3','MEM-10003',N'Elena',    N'Garcia',  'Child',          '2013-09-30','F',1),
('DEP-10004-1','MEM-10004',N'Arjun',    N'Patel',   'Spouse',         '1991-12-01','M',1),
('DEP-20001-1','MEM-20001',N'Mark',     N'Brown',   'Spouse',         '1986-06-06','M',1),
('DEP-20001-2','MEM-20001',N'Ella',     N'Brown',   'Child',          '2019-07-15','F',1),
('DEP-20002-1','MEM-20002',N'Susan',    N'Williams','Spouse',         '1977-03-25','F',1),
('DEP-20002-2','MEM-20002',N'Jake',     N'Williams','Child',          '2008-12-20','M',1),
('DEP-20002-3','MEM-20002',N'Mia',      N'Williams','Child',          '2011-05-05','F',1),
('DEP-30001-1','MEM-30001',N'Kevin',    N'Chen',    'Spouse',         '1990-09-09','M',1),
('DEP-30002-1','MEM-30002',N'Aisha',    N'Singh',   'Spouse',         '1987-04-04','F',1),
('DEP-30002-2','MEM-30002',N'Vikram',   N'Singh',   'Child',          '2015-01-01','M',1),
('DEP-30003-1','MEM-30003',N'Liam',     N'Murphy',  'Child',          '2017-06-12','M',1);
GO

-- =============================================================================
-- B. ELIGIBILITY
-- =============================================================================

INSERT INTO elig.BenefitPlan (PlanId, PlanName, LineOfBusiness, CarrierId, PlanYear, NetworkType, DeductibleInd, DeductibleFam, OopMaxInd, OopMaxFam, Copay, Coinsurance) VALUES
('MED-GOLD-PPO-26',  N'BCBS Gold PPO',         'Medical','BCBS-IL', 2026,'PPO',  2000.00, 4000.00, 8000.00,16000.00, 30.00, 20.00),
('MED-SILVER-HMO-26',N'BCBS Silver HMO',       'Medical','BCBS-IL', 2026,'HMO',  3500.00, 7000.00,10000.00,20000.00, 25.00, 30.00),
('MED-BRONZE-HDHP-26',N'BCBS Bronze HDHP',     'Medical','BCBS-IL', 2026,'HDHP', 6000.00,12000.00,12000.00,24000.00,  0.00, 40.00),
('DEN-PPO-26',       N'Delta Dental PPO',      'Dental', 'DELTA-D', 2026,'PPO',    50.00,  150.00, 2000.00, 4000.00, 20.00, 20.00),
('VIS-CHOICE-26',    N'VSP Choice Vision',     'Vision', 'VSP',     2026,'PPO',     0.00,    0.00,  500.00, 1000.00, 10.00,  0.00),
('LIFE-BASIC-26',    N'MetLife Basic Life',    'Life',   'METLIFE', 2026,NULL,     NULL,    NULL,    NULL,    NULL,  NULL,  NULL);
GO

INSERT INTO elig.MemberCoverage (CoverageId, MemberId, PlanId, Tier, EffectiveDate, TermDate, Status, PrimaryCareProvNpi) VALUES
('COV-10001-MED','MEM-10001','MED-GOLD-PPO-26',  'Family',     '2026-01-01',NULL,        'Active',    '1234567890'),
('COV-10001-DEN','MEM-10001','DEN-PPO-26',       'Family',     '2026-01-01',NULL,        'Active',    NULL),
('COV-10001-VIS','MEM-10001','VIS-CHOICE-26',    'EE+Spouse',  '2026-01-01',NULL,        'Active',    NULL),
('COV-10001-LIFE','MEM-10001','LIFE-BASIC-26',   'EE',         '2026-01-01',NULL,        'Active',    NULL),
('COV-10002-MED','MEM-10002','MED-SILVER-HMO-26','EE+Spouse',  '2026-01-01',NULL,        'Active',    '2345678901'),
('COV-10002-DEN','MEM-10002','DEN-PPO-26',       'EE+Spouse',  '2026-01-01',NULL,        'Active',    NULL),
('COV-10003-MED','MEM-10003','MED-GOLD-PPO-26',  'Family',     '2026-01-01',NULL,        'Active',    '3456789012'),
('COV-10003-DEN','MEM-10003','DEN-PPO-26',       'Family',     '2026-01-01',NULL,        'Active',    NULL),
('COV-10003-VIS','MEM-10003','VIS-CHOICE-26',    'Family',     '2026-01-01',NULL,        'Active',    NULL),
('COV-10004-MED','MEM-10004','MED-BRONZE-HDHP-26','EE+Spouse', '2026-01-01',NULL,        'Active',    NULL),
('COV-10005-MED','MEM-10005','MED-SILVER-HMO-26','EE',         '2025-01-01','2026-02-28','Terminated','4567890123'),
('COV-20001-MED','MEM-20001','MED-GOLD-PPO-26',  'EE+Spouse',  '2026-01-01',NULL,        'Active',    '5678901234'),
('COV-20001-DEN','MEM-20001','DEN-PPO-26',       'EE+Spouse',  '2026-01-01',NULL,        'Active',    NULL),
('COV-20002-MED','MEM-20002','MED-GOLD-PPO-26',  'Family',     '2026-01-01',NULL,        'Active',    '6789012345'),
('COV-20002-DEN','MEM-20002','DEN-PPO-26',       'Family',     '2026-01-01',NULL,        'Active',    NULL),
('COV-20002-VIS','MEM-20002','VIS-CHOICE-26',    'Family',     '2026-01-01',NULL,        'Active',    NULL),
('COV-20003-MED','MEM-20003','MED-BRONZE-HDHP-26','EE',        '2026-01-01',NULL,        'Active',    NULL),
('COV-20004-MED','MEM-20004','MED-SILVER-HMO-26','EE',         '2025-09-01',NULL,        'COBRA',     '7890123456'),
('COV-30001-MED','MEM-30001','MED-SILVER-HMO-26','EE+Spouse',  '2026-01-01',NULL,        'Active',    '8901234567'),
('COV-30001-DEN','MEM-30001','DEN-PPO-26',       'EE+Spouse',  '2026-01-01',NULL,        'Active',    NULL),
('COV-30002-MED','MEM-30002','MED-GOLD-PPO-26',  'Family',     '2026-01-01',NULL,        'Active',    '9012345678'),
('COV-30002-DEN','MEM-30002','DEN-PPO-26',       'Family',     '2026-01-01',NULL,        'Active',    NULL),
('COV-30002-VIS','MEM-30002','VIS-CHOICE-26',    'Family',     '2026-01-01',NULL,        'Active',    NULL),
('COV-30003-MED','MEM-30003','MED-GOLD-PPO-26',  'EE+Children','2026-01-01',NULL,        'Suspended', '0123456789');
GO

INSERT INTO elig.Accumulator (MemberId, PlanId, PlanYear, AccumulatorType, LimitAmount, AppliedAmount) VALUES
('MEM-10001','MED-GOLD-PPO-26',  2026,'IndDeductible', 2000.00,  850.00),
('MEM-10001','MED-GOLD-PPO-26',  2026,'FamDeductible', 4000.00, 1200.00),
('MEM-10001','MED-GOLD-PPO-26',  2026,'IndOOP',        8000.00, 2100.00),
('MEM-10001','MED-GOLD-PPO-26',  2026,'FamOOP',       16000.00, 3500.00),
('MEM-10002','MED-SILVER-HMO-26',2026,'IndDeductible', 3500.00,  500.00),
('MEM-10002','MED-SILVER-HMO-26',2026,'IndOOP',       10000.00,  900.00),
('MEM-10003','MED-GOLD-PPO-26',  2026,'IndDeductible', 2000.00, 1850.00),
('MEM-10003','MED-GOLD-PPO-26',  2026,'FamDeductible', 4000.00, 3200.00),
('MEM-10003','MED-GOLD-PPO-26',  2026,'IndOOP',        8000.00, 4500.00),
('MEM-10003','MED-GOLD-PPO-26',  2026,'FamOOP',       16000.00, 7200.00),
('MEM-10004','MED-BRONZE-HDHP-26',2026,'IndDeductible',6000.00,  150.00),
('MEM-10004','MED-BRONZE-HDHP-26',2026,'IndOOP',     12000.00,  150.00),
('MEM-20001','MED-GOLD-PPO-26',  2026,'IndDeductible', 2000.00,  300.00),
('MEM-20001','MED-GOLD-PPO-26',  2026,'IndOOP',        8000.00,  450.00),
('MEM-20002','MED-GOLD-PPO-26',  2026,'IndDeductible', 2000.00, 2000.00),
('MEM-20002','MED-GOLD-PPO-26',  2026,'FamDeductible', 4000.00, 4000.00),
('MEM-20002','MED-GOLD-PPO-26',  2026,'IndOOP',        8000.00, 5600.00),
('MEM-20002','MED-GOLD-PPO-26',  2026,'FamOOP',       16000.00,11400.00),
('MEM-20003','MED-BRONZE-HDHP-26',2026,'IndDeductible',6000.00,  600.00),
('MEM-20004','MED-SILVER-HMO-26', 2026,'IndDeductible',3500.00, 1750.00),
('MEM-30001','MED-SILVER-HMO-26',2026,'IndDeductible', 3500.00,  200.00),
('MEM-30002','MED-GOLD-PPO-26',  2026,'IndDeductible', 2000.00, 1100.00),
('MEM-30002','MED-GOLD-PPO-26',  2026,'FamDeductible', 4000.00, 2400.00);
GO

INSERT INTO elig.CoverageHistory (CoverageId, MemberId, ChangeType, OldValueJson, NewValueJson, ChangedBy) VALUES
('COV-10001-MED','MEM-10001','Added',     NULL,                                           N'{"tier":"Family","plan":"MED-GOLD-PPO-26"}','sys'),
('COV-10001-VIS','MEM-10001','Changed',   N'{"tier":"EE"}',                               N'{"tier":"EE+Spouse"}',                      'csr-12'),
('COV-10005-MED','MEM-10005','Terminated',N'{"status":"Active","termDate":null}',         N'{"status":"Terminated","termDate":"2026-02-28"}','sys'),
('COV-20004-MED','MEM-20004','Changed',   N'{"status":"Active"}',                         N'{"status":"COBRA"}',                        'sys'),
('COV-30003-MED','MEM-30003','Changed',   N'{"status":"Active"}',                         N'{"status":"Suspended"}',                    'hr-44'),
('COV-10003-MED','MEM-10003','Added',     NULL,                                           N'{"tier":"Family","plan":"MED-GOLD-PPO-26"}','sys'),
('COV-20002-MED','MEM-20002','Added',     NULL,                                           N'{"tier":"Family","plan":"MED-GOLD-PPO-26"}','sys'),
('COV-30002-MED','MEM-30002','Added',     NULL,                                           N'{"tier":"Family","plan":"MED-GOLD-PPO-26"}','sys');
GO

-- =============================================================================
-- C. ENROLLMENT
-- =============================================================================

INSERT INTO enroll.EnrollmentTransaction (EnrollmentId, MemberId, GroupPolicyId, TransactionType, Status, EffectiveDate, SubmittedBy, SubmittedChannel, CorrelationId, RequestPayloadJson, ResultPayloadJson) VALUES
('ENR-10001-NH','MEM-10001','GP-ACME-001',  'NewHire',       'Completed','2020-06-15','sys',         '834','corr-001',N'{"plans":["MED-GOLD-PPO-26","DEN-PPO-26","VIS-CHOICE-26","LIFE-BASIC-26"],"tier":"Family"}',N'{"status":"completed","coverageIds":["COV-10001-MED","COV-10001-DEN","COV-10001-VIS","COV-10001-LIFE"]}'),
('ENR-10002-NH','MEM-10002','GP-ACME-001',  'NewHire',       'Completed','2021-03-01','sys',         '834','corr-002',N'{"plans":["MED-SILVER-HMO-26","DEN-PPO-26"],"tier":"EE+Spouse"}',N'{"status":"completed"}'),
('ENR-10003-NH','MEM-10003','GP-ACME-001',  'NewHire',       'Completed','2018-09-12','sys',         '834','corr-003',N'{"plans":["MED-GOLD-PPO-26"],"tier":"Family"}',N'{"status":"completed"}'),
('ENR-10001-OE','MEM-10001','GP-ACME-001',  'OpenEnrollment','Completed','2026-01-01','MEM-10001',   'Portal','corr-004',N'{"changes":[{"plan":"MED-GOLD-PPO-26","action":"continue"}]}',N'{"status":"completed"}'),
('ENR-10002-PC','MEM-10002','GP-ACME-001',  'PlanChange',    'Completed','2026-01-01','csr-12',      'CSR','corr-005',N'{"from":"MED-GOLD-PPO-25","to":"MED-SILVER-HMO-26"}',N'{"status":"completed"}'),
('ENR-10003-DA','MEM-10003','GP-ACME-001',  'DepAdd',        'Completed','2025-06-01','MEM-10003',   'Portal','corr-006',N'{"dependent":"DEP-10003-3"}',N'{"status":"completed"}'),
('ENR-10005-TR','MEM-10005','GP-ACME-001',  'Termination',   'Completed','2026-02-28','hr-44',       'Portal','corr-007',N'{"reason":"voluntary"}',N'{"status":"completed"}'),
('ENR-10001-QL','MEM-10001','GP-ACME-001',  'QLE',           'Completed','2025-11-04','MEM-10001',   'Portal','corr-008',N'{"qleType":"Birth","qleDate":"2020-11-04"}',N'{"status":"completed"}'),
('ENR-20001-NH','MEM-20001','GP-NORTH-002', 'NewHire',       'Completed','2020-11-01','sys',         '834','corr-009',N'{"plans":["MED-GOLD-PPO-26","DEN-PPO-26"],"tier":"EE+Spouse"}',N'{"status":"completed"}'),
('ENR-20003-NH','MEM-20003','GP-NORTH-002', 'NewHire',       'Completed','2023-02-20','sys',         '834','corr-010',N'{"plans":["MED-BRONZE-HDHP-26"],"tier":"EE"}',N'{"status":"completed"}'),
('ENR-20004-TR','MEM-20004','GP-NORTH-002', 'Termination',   'Completed','2025-08-31','hr-44',       'CSR','corr-011',N'{"reason":"layoff"}',N'{"status":"completed"}'),
('ENR-30001-NH','MEM-30001','GP-CONTOSO-03','NewHire',       'Completed','2021-07-12','sys',         '834','corr-012',N'{"plans":["MED-SILVER-HMO-26","DEN-PPO-26"],"tier":"EE+Spouse"}',N'{"status":"completed"}'),
('ENR-30002-OE','MEM-30002','GP-CONTOSO-03','OpenEnrollment','Completed','2026-01-01','MEM-30002',   'Portal','corr-013',N'{"changes":[]}',N'{"status":"completed"}'),
('ENR-30003-LE','MEM-30003','GP-CONTOSO-03','PlanChange',    'RequiresReview','2026-03-01','agent-run-99','Agent','corr-014',N'{"action":"reduceCoverage"}',NULL),
('ENR-10004-DA','MEM-10004','GP-ACME-001',  'DepAdd',        'Pending',  '2026-04-15','MEM-10004',   'Portal','corr-015',N'{"dependent":{"firstName":"Arjun","relationship":"Spouse"}}',NULL),
('ENR-10002-QL','MEM-10002','GP-ACME-001',  'QLE',           'InProgress','2026-05-01','MEM-10002',  'Portal','corr-016',N'{"qleType":"Marriage","qleDate":"2026-04-20"}',NULL),
('ENR-20002-DA','MEM-20002','GP-NORTH-002', 'DepAdd',        'Failed',   '2026-03-15','MEM-20002',   'Portal','corr-017',N'{"dependent":{"firstName":"Newborn"}}',N'{"error":"Missing documentation"}'),
('ENR-20001-OE','MEM-20001','GP-NORTH-002', 'OpenEnrollment','Completed','2026-01-01','MEM-20001',   'Portal','corr-018',N'{"changes":[]}',N'{"status":"completed"}'),
('ENR-30002-NH','MEM-30002','GP-CONTOSO-03','NewHire',       'Completed','2019-10-05','sys',         '834','corr-019',N'{"plans":["MED-GOLD-PPO-26"],"tier":"Family"}',N'{"status":"completed"}'),
('ENR-30001-DR','MEM-30001','GP-CONTOSO-03','DepRemove',     'Cancelled','2026-06-01','MEM-30001',   'Portal','corr-020',N'{"dependentId":"DEP-30001-1"}',N'{"reason":"user cancelled"}');
GO

INSERT INTO enroll.PlanElection (ElectionId, EnrollmentId, PlanId, Tier, EffectiveDate, [Action]) VALUES
('PE-001','ENR-10001-NH','MED-GOLD-PPO-26',  'Family',    '2020-06-15','Add'),
('PE-002','ENR-10001-NH','DEN-PPO-26',       'Family',    '2020-06-15','Add'),
('PE-003','ENR-10001-NH','VIS-CHOICE-26',    'EE+Spouse', '2020-06-15','Add'),
('PE-004','ENR-10001-NH','LIFE-BASIC-26',    'EE',        '2020-06-15','Add'),
('PE-005','ENR-10002-NH','MED-SILVER-HMO-26','EE+Spouse', '2021-03-01','Add'),
('PE-006','ENR-10002-NH','DEN-PPO-26',       'EE+Spouse', '2021-03-01','Add'),
('PE-007','ENR-10003-NH','MED-GOLD-PPO-26',  'Family',    '2018-09-12','Add'),
('PE-008','ENR-10002-PC','MED-SILVER-HMO-26','EE+Spouse', '2026-01-01','Change'),
('PE-009','ENR-10005-TR','MED-SILVER-HMO-26','EE',        '2026-02-28','Drop'),
('PE-010','ENR-20001-NH','MED-GOLD-PPO-26',  'EE+Spouse', '2020-11-01','Add'),
('PE-011','ENR-20001-NH','DEN-PPO-26',       'EE+Spouse', '2020-11-01','Add'),
('PE-012','ENR-20003-NH','MED-BRONZE-HDHP-26','EE',       '2023-02-20','Add'),
('PE-013','ENR-20004-TR','MED-SILVER-HMO-26','EE',        '2025-08-31','Drop'),
('PE-014','ENR-30001-NH','MED-SILVER-HMO-26','EE+Spouse', '2021-07-12','Add'),
('PE-015','ENR-30001-NH','DEN-PPO-26',       'EE+Spouse', '2021-07-12','Add'),
('PE-016','ENR-30002-NH','MED-GOLD-PPO-26',  'Family',    '2019-10-05','Add');
GO

INSERT INTO enroll.QualifyingLifeEvent (QleId, MemberId, QleType, QleDate, ElectionWindowEnd, Status) VALUES
('QLE-001','MEM-10001','Birth',          '2020-11-04','2020-12-04','Used'),
('QLE-002','MEM-10002','Marriage',       '2026-04-20','2026-05-20','Open'),
('QLE-003','MEM-10003','Adoption',       '2023-08-15','2023-09-15','Used'),
('QLE-004','MEM-10004','Newborn',        '2025-12-01','2026-01-15','Used'),
('QLE-005','MEM-20002','Newborn',        '2026-03-01','2026-03-31','Open'),
('QLE-006','MEM-20004','JobLoss',        '2025-08-31','2025-10-30','Used'),
('QLE-007','MEM-30001','Marriage',       '2024-06-10','2024-07-10','Used'),
('QLE-008','MEM-30003','SpouseJobChange','2025-11-15','2025-12-15','Expired');
GO

INSERT INTO enroll.OutboxEvent (AggregateId, EventType, PayloadJson, PublishedUtc) VALUES
('ENR-10001-NH','Enrollment.NewHire',         N'{"memberId":"MEM-10001","groupPolicyId":"GP-ACME-001"}', '2020-06-15T10:00:00'),
('ENR-10005-TR','Enrollment.Terminated',      N'{"memberId":"MEM-10005","terminationDate":"2026-02-28"}','2026-02-28T17:30:00'),
('ENR-10002-PC','Enrollment.PlanChanged',     N'{"memberId":"MEM-10002","oldPlan":"MED-GOLD-PPO-25","newPlan":"MED-SILVER-HMO-26"}','2026-01-01T00:01:00'),
('ENR-10003-DA','Enrollment.DependentAdded',  N'{"memberId":"MEM-10003","dependentId":"DEP-10003-3"}','2025-06-01T08:15:00'),
('ENR-20004-TR','Enrollment.Terminated',      N'{"memberId":"MEM-20004","terminationDate":"2025-08-31"}','2025-08-31T16:00:00'),
('ENR-10004-DA','Enrollment.DependentAdded',  N'{"memberId":"MEM-10004","dependentId":"DEP-10004-1"}',NULL),
('ENR-10002-QL','Enrollment.QualifyingLifeEvent',N'{"memberId":"MEM-10002","qleType":"Marriage"}',NULL),
('ENR-30003-LE','Enrollment.PlanChanged',     N'{"memberId":"MEM-30003","status":"RequiresReview"}',NULL),
('ENR-10001-OE','Enrollment.OpenEnrollment',  N'{"memberId":"MEM-10001","planYear":2026}','2026-01-01T00:00:30'),
('ENR-20001-OE','Enrollment.OpenEnrollment',  N'{"memberId":"MEM-20001","planYear":2026}','2026-01-01T00:00:45');
GO

-- =============================================================================
-- D. BILLING & PAYROLL
-- =============================================================================

INSERT INTO billing.PremiumRate (RateId, PlanId, Tier, AgeBand, SalaryBand, MonthlyPremium, EffectiveDate, EndDate) VALUES
('PR-001','MED-GOLD-PPO-26',  'EE',         '21-29',NULL,  550.00,'2026-01-01',NULL),
('PR-002','MED-GOLD-PPO-26',  'EE',         '30-39',NULL,  640.00,'2026-01-01',NULL),
('PR-003','MED-GOLD-PPO-26',  'EE',         '40-49',NULL,  720.00,'2026-01-01',NULL),
('PR-004','MED-GOLD-PPO-26',  'EE+Spouse',  NULL,   NULL, 1180.00,'2026-01-01',NULL),
('PR-005','MED-GOLD-PPO-26',  'EE+Children',NULL,   NULL, 1050.00,'2026-01-01',NULL),
('PR-006','MED-GOLD-PPO-26',  'Family',     NULL,   NULL, 1836.00,'2026-01-01',NULL),
('PR-007','MED-SILVER-HMO-26','EE',         NULL,   NULL,  420.00,'2026-01-01',NULL),
('PR-008','MED-SILVER-HMO-26','EE+Spouse',  NULL,   NULL,  890.00,'2026-01-01',NULL),
('PR-009','MED-SILVER-HMO-26','Family',     NULL,   NULL, 1420.00,'2026-01-01',NULL),
('PR-010','MED-BRONZE-HDHP-26','EE',        NULL,   NULL,  310.00,'2026-01-01',NULL),
('PR-011','MED-BRONZE-HDHP-26','EE+Spouse', NULL,   NULL,  640.00,'2026-01-01',NULL),
('PR-012','MED-BRONZE-HDHP-26','Family',    NULL,   NULL, 1080.00,'2026-01-01',NULL),
('PR-013','DEN-PPO-26',        'EE',        NULL,   NULL,   38.00,'2026-01-01',NULL),
('PR-014','DEN-PPO-26',        'EE+Spouse', NULL,   NULL,   75.00,'2026-01-01',NULL),
('PR-015','DEN-PPO-26',        'Family',    NULL,   NULL,  120.00,'2026-01-01',NULL),
('PR-016','VIS-CHOICE-26',     'EE',        NULL,   NULL,    9.00,'2026-01-01',NULL),
('PR-017','VIS-CHOICE-26',     'EE+Spouse', NULL,   NULL,   18.00,'2026-01-01',NULL),
('PR-018','VIS-CHOICE-26',     'Family',    NULL,   NULL,   28.00,'2026-01-01',NULL),
('PR-019','LIFE-BASIC-26',     'EE',        NULL,   NULL,    7.50,'2026-01-01',NULL);
GO

INSERT INTO billing.Invoice (InvoiceId, GroupPolicyId, MemberId, BillingPeriodStart, BillingPeriodEnd, InvoiceDate, DueDate, TotalAmount, BalanceDue, Status) VALUES
('INV-10001-2601','GP-ACME-001',  'MEM-10001','2026-01-01','2026-01-31','2026-01-05','2026-02-05', 1991.50,    0.00,'Paid'),
('INV-10001-2602','GP-ACME-001',  'MEM-10001','2026-02-01','2026-02-28','2026-02-05','2026-03-05', 1991.50,    0.00,'Paid'),
('INV-10001-2603','GP-ACME-001',  'MEM-10001','2026-03-01','2026-03-31','2026-03-05','2026-04-05', 1991.50, 1991.50,'Open'),
('INV-10002-2601','GP-ACME-001',  'MEM-10002','2026-01-01','2026-01-31','2026-01-05','2026-02-05',  965.00,    0.00,'Paid'),
('INV-10002-2602','GP-ACME-001',  'MEM-10002','2026-02-01','2026-02-28','2026-02-05','2026-03-05',  965.00,  500.00,'PartiallyPaid'),
('INV-10003-2601','GP-ACME-001',  'MEM-10003','2026-01-01','2026-01-31','2026-01-05','2026-02-05', 1984.00,    0.00,'Paid'),
('INV-10003-2602','GP-ACME-001',  'MEM-10003','2026-02-01','2026-02-28','2026-02-05','2026-03-05', 1984.00,    0.00,'Paid'),
('INV-10003-2603','GP-ACME-001',  'MEM-10003','2026-03-01','2026-03-31','2026-03-05','2026-04-05', 1984.00, 1984.00,'PastDue'),
('INV-10004-2601','GP-ACME-001',  'MEM-10004','2026-01-01','2026-01-31','2026-01-05','2026-02-05',  640.00,    0.00,'Paid'),
('INV-10005-2602','GP-ACME-001',  'MEM-10005','2026-02-01','2026-02-28','2026-02-05','2026-03-05',  420.00,  420.00,'Disputed'),
('INV-20001-2601','GP-NORTH-002', 'MEM-20001','2026-01-01','2026-01-31','2026-01-05','2026-02-05', 1255.00,    0.00,'Paid'),
('INV-20001-2602','GP-NORTH-002', 'MEM-20001','2026-02-01','2026-02-28','2026-02-05','2026-03-05', 1255.00,    0.00,'Paid'),
('INV-20002-2601','GP-NORTH-002', 'MEM-20002','2026-01-01','2026-01-31','2026-01-05','2026-02-05', 1984.00,    0.00,'Paid'),
('INV-20002-2602','GP-NORTH-002', 'MEM-20002','2026-02-01','2026-02-28','2026-02-05','2026-03-05', 1984.00, 1984.00,'Open'),
('INV-20003-2601','GP-NORTH-002', 'MEM-20003','2026-01-01','2026-01-31','2026-01-05','2026-02-05',  310.00,    0.00,'Paid'),
('INV-20004-2602','GP-NORTH-002', 'MEM-20004','2026-02-01','2026-02-28','2026-02-05','2026-03-05',  420.00,    0.00,'Paid'),
('INV-30001-2601','GP-CONTOSO-03','MEM-30001','2026-01-01','2026-01-31','2026-01-05','2026-02-05',  965.00,    0.00,'Paid'),
('INV-30001-2602','GP-CONTOSO-03','MEM-30001','2026-02-01','2026-02-28','2026-02-05','2026-03-05',  965.00,    0.00,'Paid'),
('INV-30002-2601','GP-CONTOSO-03','MEM-30002','2026-01-01','2026-01-31','2026-01-05','2026-02-05', 1984.00,    0.00,'Paid'),
('INV-30002-2602','GP-CONTOSO-03','MEM-30002','2026-02-01','2026-02-28','2026-02-05','2026-03-05', 1984.00,    0.00,'Paid'),
('INV-30003-2601','GP-CONTOSO-03','MEM-30003','2026-01-01','2026-01-31','2026-01-05','2026-02-05', 1050.00, 1050.00,'PastDue');
GO

-- InvoiceLines: each line EmployerPortion + EmployeePortion = PremiumAmount
INSERT INTO billing.InvoiceLine (InvoiceId, PlanId, LineDescription, PremiumAmount, EmployerPortion, EmployeePortion) VALUES
('INV-10001-2601','MED-GOLD-PPO-26',N'Medical Family',   1836.00, 1468.80,  367.20),
('INV-10001-2601','DEN-PPO-26',     N'Dental Family',     120.00,   96.00,   24.00),
('INV-10001-2601','VIS-CHOICE-26',  N'Vision EE+Spouse',   28.00,   22.40,    5.60),
('INV-10001-2601','LIFE-BASIC-26',  N'Life EE',             7.50,    7.50,    0.00),
('INV-10001-2602','MED-GOLD-PPO-26',N'Medical Family',   1836.00, 1468.80,  367.20),
('INV-10001-2602','DEN-PPO-26',     N'Dental Family',     120.00,   96.00,   24.00),
('INV-10001-2602','VIS-CHOICE-26',  N'Vision EE+Spouse',   28.00,   22.40,    5.60),
('INV-10001-2602','LIFE-BASIC-26',  N'Life EE',             7.50,    7.50,    0.00),
('INV-10001-2603','MED-GOLD-PPO-26',N'Medical Family',   1836.00, 1468.80,  367.20),
('INV-10001-2603','DEN-PPO-26',     N'Dental Family',     120.00,   96.00,   24.00),
('INV-10001-2603','VIS-CHOICE-26',  N'Vision EE+Spouse',   28.00,   22.40,    5.60),
('INV-10001-2603','LIFE-BASIC-26',  N'Life EE',             7.50,    7.50,    0.00),
('INV-10002-2601','MED-SILVER-HMO-26',N'Medical EE+Spouse',890.00,  712.00,  178.00),
('INV-10002-2601','DEN-PPO-26',     N'Dental EE+Spouse',   75.00,   60.00,   15.00),
('INV-10002-2602','MED-SILVER-HMO-26',N'Medical EE+Spouse',890.00,  712.00,  178.00),
('INV-10002-2602','DEN-PPO-26',     N'Dental EE+Spouse',   75.00,   60.00,   15.00),
('INV-10003-2601','MED-GOLD-PPO-26',N'Medical Family',   1836.00, 1468.80,  367.20),
('INV-10003-2601','DEN-PPO-26',     N'Dental Family',     120.00,   96.00,   24.00),
('INV-10003-2601','VIS-CHOICE-26',  N'Vision Family',      28.00,   22.40,    5.60),
('INV-10003-2602','MED-GOLD-PPO-26',N'Medical Family',   1836.00, 1468.80,  367.20),
('INV-10003-2602','DEN-PPO-26',     N'Dental Family',     120.00,   96.00,   24.00),
('INV-10003-2602','VIS-CHOICE-26',  N'Vision Family',      28.00,   22.40,    5.60),
('INV-10003-2603','MED-GOLD-PPO-26',N'Medical Family',   1836.00, 1468.80,  367.20),
('INV-10003-2603','DEN-PPO-26',     N'Dental Family',     120.00,   96.00,   24.00),
('INV-10003-2603','VIS-CHOICE-26',  N'Vision Family',      28.00,   22.40,    5.60),
('INV-10004-2601','MED-BRONZE-HDHP-26',N'Medical EE+Spouse',640.00,512.00,  128.00),
('INV-10005-2602','MED-SILVER-HMO-26',N'Medical EE',     420.00,  336.00,   84.00),
('INV-20001-2601','MED-GOLD-PPO-26',N'Medical EE+Spouse',1180.00,  944.00,  236.00),
('INV-20001-2601','DEN-PPO-26',     N'Dental EE+Spouse',   75.00,   60.00,   15.00),
('INV-20001-2602','MED-GOLD-PPO-26',N'Medical EE+Spouse',1180.00,  944.00,  236.00),
('INV-20001-2602','DEN-PPO-26',     N'Dental EE+Spouse',   75.00,   60.00,   15.00),
('INV-20002-2601','MED-GOLD-PPO-26',N'Medical Family',   1836.00, 1468.80,  367.20),
('INV-20002-2601','DEN-PPO-26',     N'Dental Family',     120.00,   96.00,   24.00),
('INV-20002-2601','VIS-CHOICE-26',  N'Vision Family',      28.00,   22.40,    5.60),
('INV-20002-2602','MED-GOLD-PPO-26',N'Medical Family',   1836.00, 1468.80,  367.20),
('INV-20002-2602','DEN-PPO-26',     N'Dental Family',     120.00,   96.00,   24.00),
('INV-20002-2602','VIS-CHOICE-26',  N'Vision Family',      28.00,   22.40,    5.60),
('INV-20003-2601','MED-BRONZE-HDHP-26',N'Medical EE',    310.00,  248.00,   62.00),
('INV-20004-2602','MED-SILVER-HMO-26',N'Medical EE COBRA',420.00,    0.00,  420.00),
('INV-30001-2601','MED-SILVER-HMO-26',N'Medical EE+Spouse',890.00, 712.00,  178.00),
('INV-30001-2601','DEN-PPO-26',     N'Dental EE+Spouse',   75.00,   60.00,   15.00),
('INV-30001-2602','MED-SILVER-HMO-26',N'Medical EE+Spouse',890.00, 712.00,  178.00),
('INV-30001-2602','DEN-PPO-26',     N'Dental EE+Spouse',   75.00,   60.00,   15.00),
('INV-30002-2601','MED-GOLD-PPO-26',N'Medical Family',   1836.00, 1468.80,  367.20),
('INV-30002-2601','DEN-PPO-26',     N'Dental Family',     120.00,   96.00,   24.00),
('INV-30002-2601','VIS-CHOICE-26',  N'Vision Family',      28.00,   22.40,    5.60),
('INV-30002-2602','MED-GOLD-PPO-26',N'Medical Family',   1836.00, 1468.80,  367.20),
('INV-30002-2602','DEN-PPO-26',     N'Dental Family',     120.00,   96.00,   24.00),
('INV-30002-2602','VIS-CHOICE-26',  N'Vision Family',      28.00,   22.40,    5.60),
('INV-30003-2601','MED-GOLD-PPO-26',N'Medical EE+Children',1050.00, 840.00,  210.00);
GO

INSERT INTO billing.Payment (PaymentId, InvoiceId, MemberId, GroupPolicyId, Amount, PaymentMethod, PaymentDate, ConfirmationNumber, Status) VALUES
('PMT-001','INV-10001-2601','MEM-10001','GP-ACME-001',  1991.50,'ACH',    '2026-01-15','CONF-AB1001','Posted'),
('PMT-002','INV-10001-2602','MEM-10001','GP-ACME-001',  1991.50,'ACH',    '2026-02-15','CONF-AB1002','Posted'),
('PMT-003','INV-10002-2601','MEM-10002','GP-ACME-001',   965.00,'Card',   '2026-01-20','CONF-AB1003','Posted'),
('PMT-004','INV-10002-2602','MEM-10002','GP-ACME-001',   500.00,'Card',   '2026-02-20','CONF-AB1004','Posted'),
('PMT-005','INV-10003-2601','MEM-10003','GP-ACME-001',  1984.00,'ACH',    '2026-01-12','CONF-AB1005','Posted'),
('PMT-006','INV-10003-2602','MEM-10003','GP-ACME-001',  1984.00,'ACH',    '2026-02-12','CONF-AB1006','Posted'),
('PMT-007','INV-10004-2601','MEM-10004','GP-ACME-001',   640.00,'ACH',    '2026-01-22','CONF-AB1007','Posted'),
('PMT-008','INV-20001-2601','MEM-20001','GP-NORTH-002', 1255.00,'Payroll','2026-01-15','CONF-NW2001','Posted'),
('PMT-009','INV-20001-2602','MEM-20001','GP-NORTH-002', 1255.00,'Payroll','2026-02-15','CONF-NW2002','Posted'),
('PMT-010','INV-20002-2601','MEM-20002','GP-NORTH-002', 1984.00,'Payroll','2026-01-15','CONF-NW2003','Posted'),
('PMT-011','INV-20003-2601','MEM-20003','GP-NORTH-002',  310.00,'Payroll','2026-01-15','CONF-NW2004','Posted'),
('PMT-012','INV-20004-2602','MEM-20004','GP-NORTH-002',  420.00,'Check',  '2026-02-25','CONF-NW2005','Posted'),
('PMT-013','INV-30001-2601','MEM-30001','GP-CONTOSO-03', 965.00,'ACH',    '2026-01-18','CONF-CT3001','Posted'),
('PMT-014','INV-30001-2602','MEM-30001','GP-CONTOSO-03', 965.00,'ACH',    '2026-02-18','CONF-CT3002','Posted'),
('PMT-015','INV-30002-2601','MEM-30002','GP-CONTOSO-03',1984.00,'ACH',    '2026-01-18','CONF-CT3003','Posted'),
('PMT-016','INV-30002-2602','MEM-30002','GP-CONTOSO-03',1984.00,'ACH',    '2026-02-18','CONF-CT3004','Posted'),
('PMT-017',NULL,             'MEM-10001','GP-ACME-001',    50.00,'Card',   '2026-03-01','CONF-AB1008','Refunded'),
('PMT-018','INV-10002-2602','MEM-10002','GP-ACME-001',   100.00,'Card',   '2026-02-22','CONF-AB1009','Failed'),
('PMT-019',NULL,             NULL,        'GP-NORTH-002',5000.00,'Wire',   '2026-02-01','CONF-NW2006','Posted');
GO

INSERT INTO payroll.PayrollFile (PayrollFileId, GroupPolicyId, PayDate, FileBlobUri, TotalDeductions, Status) VALUES
('PF-ACME-2601-1', 'GP-ACME-001', '2026-01-15','https://blob/acme/2026-01-15.csv', 1525.40,'Posted'),
('PF-ACME-2601-2', 'GP-ACME-001', '2026-01-31','https://blob/acme/2026-01-31.csv', 1525.40,'Posted'),
('PF-ACME-2602-1', 'GP-ACME-001', '2026-02-15','https://blob/acme/2026-02-15.csv', 1525.40,'Posted'),
('PF-NORTH-2601',  'GP-NORTH-002','2026-01-15','https://blob/north/2026-01-15.csv', 1820.00,'Reconciled'),
('PF-NORTH-2602',  'GP-NORTH-002','2026-02-15','https://blob/north/2026-02-15.csv', 1820.00,'Processing'),
('PF-CONTOSO-2601','GP-CONTOSO-03','2026-01-15','https://blob/contoso/2026-01-15.csv',2400.00,'Posted');
GO

INSERT INTO payroll.PayrollDeduction (DeductionId, MemberId, PlanId, PayPeriodStart, PayPeriodEnd, PreTaxAmount, PostTaxAmount, Status) VALUES
('PD-001','MEM-10001','MED-GOLD-PPO-26',  '2026-01-01','2026-01-15',183.60, 0.00,'Deducted'),
('PD-002','MEM-10001','DEN-PPO-26',       '2026-01-01','2026-01-15', 12.00, 0.00,'Deducted'),
('PD-003','MEM-10001','VIS-CHOICE-26',    '2026-01-01','2026-01-15',  2.80, 0.00,'Deducted'),
('PD-004','MEM-10001','LIFE-BASIC-26',    '2026-01-01','2026-01-15',  0.00, 3.75,'Deducted'),
('PD-005','MEM-10001','MED-GOLD-PPO-26',  '2026-01-16','2026-01-31',183.60, 0.00,'Deducted'),
('PD-006','MEM-10001','MED-GOLD-PPO-26',  '2026-02-01','2026-02-15',183.60, 0.00,'Deducted'),
('PD-007','MEM-10002','MED-SILVER-HMO-26','2026-01-01','2026-01-15', 89.00, 0.00,'Deducted'),
('PD-008','MEM-10002','DEN-PPO-26',       '2026-01-01','2026-01-15',  7.50, 0.00,'Deducted'),
('PD-009','MEM-10002','MED-SILVER-HMO-26','2026-02-01','2026-02-15', 89.00, 0.00,'Reversed'),
('PD-010','MEM-10003','MED-GOLD-PPO-26',  '2026-01-01','2026-01-15',183.60, 0.00,'Deducted'),
('PD-011','MEM-10003','DEN-PPO-26',       '2026-01-01','2026-01-15', 12.00, 0.00,'Deducted'),
('PD-012','MEM-10004','MED-BRONZE-HDHP-26','2026-01-01','2026-01-15',64.00, 0.00,'Deducted'),
('PD-013','MEM-20001','MED-GOLD-PPO-26',  '2026-01-01','2026-01-15',118.00, 0.00,'Deducted'),
('PD-014','MEM-20001','DEN-PPO-26',       '2026-01-01','2026-01-15',  7.50, 0.00,'Deducted'),
('PD-015','MEM-20002','MED-GOLD-PPO-26',  '2026-01-01','2026-01-15',183.60, 0.00,'Deducted'),
('PD-016','MEM-20003','MED-BRONZE-HDHP-26','2026-01-01','2026-01-15',31.00, 0.00,'Deducted'),
('PD-017','MEM-30001','MED-SILVER-HMO-26','2026-01-01','2026-01-15', 89.00, 0.00,'Deducted'),
('PD-018','MEM-30002','MED-GOLD-PPO-26',  '2026-01-01','2026-01-15',183.60, 0.00,'Deducted'),
('PD-019','MEM-30002','DEN-PPO-26',       '2026-01-01','2026-01-15', 12.00, 0.00,'Deducted'),
('PD-020','MEM-10001','MED-GOLD-PPO-26',  '2026-03-01','2026-03-15',183.60, 0.00,'Scheduled'),
('PD-021','MEM-10002','MED-SILVER-HMO-26','2026-03-01','2026-03-15', 89.00, 0.00,'Scheduled'),
('PD-022','MEM-10003','MED-GOLD-PPO-26',  '2026-03-01','2026-03-15',183.60, 0.00,'Scheduled'),
('PD-023','MEM-20002','MED-GOLD-PPO-26',  '2026-02-16','2026-02-28',183.60, 0.00,'Skipped');
GO

-- =============================================================================
-- E. IAM (Account & Access)
-- =============================================================================

INSERT INTO iam.AccessRole (RoleId, RoleName, Description, ScopesJson) VALUES
('ROLE-MEMBER',  'Member',          N'Member self-service portal access',           N'["benefits:read","claims:read","payments:manage","profile:write"]'),
('ROLE-CSR',     'CustomerCareRep', N'CSR tier 1 — read all, limited writes',       N'["member:read","coverage:read","invoice:read","case:write","escalate:write"]'),
('ROLE-CSR-T2',  'CustomerCareT2',  N'CSR tier 2 — escalations, complex changes',   N'["member:read","coverage:write","invoice:write","case:write","refund:write"]'),
('ROLE-HRADMIN', 'HRAdmin',         N'Employer HR admin — manage group enrollments',N'["group:manage","enrollment:write","payroll:read","report:read"]'),
('ROLE-TPAADMIN','TPAAdmin',        N'Third-party admin — full TPA scope',          N'["*"]');
GO

INSERT INTO iam.UserAccount (UserId, MemberId, UserType, Username, Email, OktaUserId, Status, MfaEnrolled, LastLoginUtc, FailedLoginCount, PasswordChangedUtc) VALUES
('USR-MEM-10001','MEM-10001','Member',  'john.doe',     'john.doe@acme.com',    'okta-001','Active',          1,'2026-03-10T14:23:00',0,'2025-12-15T10:00:00'),
('USR-MEM-10002','MEM-10002','Member',  'jane.smith',   'jane.smith@acme.com',  'okta-002','Active',          1,'2026-03-12T09:15:00',0,'2025-11-20T11:00:00'),
('USR-MEM-10003','MEM-10003','Member',  'carlos.g',     'carlos.g@acme.com',    'okta-003','Active',          1,'2026-03-08T16:45:00',1,'2025-10-05T08:30:00'),
('USR-MEM-10004','MEM-10004','Member',  'priya.p',      'priya.p@acme.com',     'okta-004','Locked',          1,'2026-03-13T07:30:00',5,'2025-09-10T09:00:00'),
('USR-MEM-10005','MEM-10005','Member',  'mike.j',       'mike.j@acme.com',      'okta-005','Disabled',        0,'2026-02-25T11:00:00',0,'2024-12-01T10:00:00'),
('USR-MEM-20001','MEM-20001','Member',  'alice.b',      'alice.b@northwind.com','okta-006','Active',          1,'2026-03-11T13:00:00',0,'2026-01-15T10:00:00'),
('USR-MEM-20002','MEM-20002','Member',  'bob.w',        'bob.w@northwind.com',  'okta-007','PasswordExpired', 1,'2026-02-28T08:00:00',0,'2025-08-10T10:00:00'),
('USR-MEM-20004','MEM-20004','Member',  'david.k',      'david.k@northwind.com','okta-008','Active',          1,'2026-03-09T15:30:00',0,'2025-12-01T10:00:00'),
('USR-MEM-30001','MEM-30001','Member',  'emily.c',      'emily.c@contoso.com',  'okta-009','Active',          1,'2026-03-13T10:00:00',0,'2026-02-01T10:00:00'),
('USR-CSR-001',   NULL,       'CSR',    'kate.lee',     'kate.lee@abc.com',     'okta-100','Active',          1,'2026-03-13T08:00:00',0,'2026-01-10T10:00:00'),
('USR-CSR-002',   NULL,       'CSR',    'omar.h',       'omar.h@abc.com',       'okta-101','Active',          1,'2026-03-13T08:00:00',0,'2026-01-10T10:00:00'),
('USR-CSR-003',   NULL,       'CSR',    'lisa.m',       'lisa.m@abc.com',       'okta-102','Active',          1,'2026-03-12T09:00:00',0,'2026-02-15T10:00:00'),
('USR-HR-001',    NULL,       'HRAdmin','hr.acme',      'hr@acme.com',          'okta-200','Active',          1,'2026-03-12T11:00:00',0,'2026-01-05T10:00:00'),
('USR-HR-002',    NULL,       'HRAdmin','hr.northwind', 'hr@northwind.com',     'okta-201','Active',          1,'2026-03-10T10:00:00',0,'2026-01-08T10:00:00'),
('USR-TPA-001',   NULL,       'TPAAdmin','admin.abc',   'admin@abc.com',        'okta-300','Active',          1,'2026-03-13T07:00:00',0,'2026-02-20T10:00:00');
GO

INSERT INTO iam.UserRole (UserId, RoleId, GrantedBy) VALUES
('USR-MEM-10001','ROLE-MEMBER',  'sys'),
('USR-MEM-10002','ROLE-MEMBER',  'sys'),
('USR-MEM-10003','ROLE-MEMBER',  'sys'),
('USR-MEM-10004','ROLE-MEMBER',  'sys'),
('USR-MEM-10005','ROLE-MEMBER',  'sys'),
('USR-MEM-20001','ROLE-MEMBER',  'sys'),
('USR-MEM-20002','ROLE-MEMBER',  'sys'),
('USR-MEM-20004','ROLE-MEMBER',  'sys'),
('USR-MEM-30001','ROLE-MEMBER',  'sys'),
('USR-CSR-001',  'ROLE-CSR',     'USR-TPA-001'),
('USR-CSR-002',  'ROLE-CSR',     'USR-TPA-001'),
('USR-CSR-003',  'ROLE-CSR-T2',  'USR-TPA-001'),
('USR-HR-001',   'ROLE-HRADMIN', 'USR-TPA-001'),
('USR-HR-002',   'ROLE-HRADMIN', 'USR-TPA-001'),
('USR-TPA-001',  'ROLE-TPAADMIN','sys');
GO

INSERT INTO iam.LoginAttempt (UserId, Username, AttemptUtc, Success, FailureReason, IpAddress, UserAgent, DeviceId) VALUES
('USR-MEM-10001','john.doe',     '2026-03-10T14:23:00',1,NULL,            '24.10.5.100',  'Mozilla/5.0 Chrome','dev-001'),
('USR-MEM-10001','john.doe',     '2026-03-12T09:00:00',1,NULL,            '24.10.5.100',  'Mozilla/5.0 Chrome','dev-001'),
('USR-MEM-10002','jane.smith',   '2026-03-12T09:15:00',1,NULL,            '24.10.5.150',  'Mozilla/5.0 Safari','dev-002'),
('USR-MEM-10003','carlos.g',     '2026-03-08T16:40:00',0,'BadPassword',   '24.11.6.200',  'Mozilla/5.0 Chrome','dev-003'),
('USR-MEM-10003','carlos.g',     '2026-03-08T16:45:00',1,NULL,            '24.11.6.200',  'Mozilla/5.0 Chrome','dev-003'),
('USR-MEM-10004','priya.p',      '2026-03-13T07:25:00',0,'BadPassword',   '24.12.7.50',   'Mozilla/5.0 Chrome','dev-004'),
('USR-MEM-10004','priya.p',      '2026-03-13T07:26:00',0,'BadPassword',   '24.12.7.50',   'Mozilla/5.0 Chrome','dev-004'),
('USR-MEM-10004','priya.p',      '2026-03-13T07:27:00',0,'BadPassword',   '24.12.7.50',   'Mozilla/5.0 Chrome','dev-004'),
('USR-MEM-10004','priya.p',      '2026-03-13T07:28:00',0,'BadPassword',   '24.12.7.50',   'Mozilla/5.0 Chrome','dev-004'),
('USR-MEM-10004','priya.p',      '2026-03-13T07:29:00',0,'BadPassword',   '24.12.7.50',   'Mozilla/5.0 Chrome','dev-004'),
('USR-MEM-10004','priya.p',      '2026-03-13T07:30:00',0,'Locked',        '24.12.7.50',   'Mozilla/5.0 Chrome','dev-004'),
('USR-MEM-10005','mike.j',       '2026-02-25T11:00:00',0,'Disabled',      '24.13.8.10',   'Mozilla/5.0 Chrome','dev-005'),
('USR-MEM-20001','alice.b',      '2026-03-11T13:00:00',1,NULL,            '45.20.10.5',   'Mozilla/5.0 Firefox','dev-101'),
('USR-MEM-20002','bob.w',        '2026-02-28T08:00:00',0,'Expired',       '45.20.10.20',  'Mozilla/5.0 Edge',   'dev-102'),
('USR-MEM-20004','david.k',      '2026-03-09T15:30:00',1,NULL,            '45.20.10.40',  'Mozilla/5.0 Chrome', 'dev-103'),
(NULL,            'unknownuser', '2026-03-13T03:15:00',0,'UnknownUser',   '185.244.1.99', 'curl/8.0',           NULL),
(NULL,            'jane.smith',  '2026-03-13T03:16:00',0,'BadPassword',   '185.244.1.99', 'curl/8.0',           NULL),
('USR-CSR-001',  'kate.lee',     '2026-03-13T08:00:00',1,NULL,            '10.1.1.5',     'Mozilla/5.0 Chrome', 'corp-001'),
('USR-CSR-002',  'omar.h',       '2026-03-13T08:00:00',1,NULL,            '10.1.1.6',     'Mozilla/5.0 Chrome', 'corp-002'),
('USR-HR-001',   'hr.acme',      '2026-03-12T11:00:00',1,NULL,            '50.30.5.10',   'Mozilla/5.0 Chrome', 'hr-acme-01'),
('USR-MEM-30001','emily.c',      '2026-03-13T10:00:00',1,NULL,            '70.40.20.5',   'Mozilla/5.0 Safari', 'iphone-001');
GO

INSERT INTO iam.PasswordResetRequest (ResetRequestId, UserId, RequestedUtc, Channel, VerificationMethod, Status, ExpiresUtc, CompletedUtc, InitiatedBy) VALUES
('PR-001','USR-MEM-10003','2026-03-08T16:35:00','Email','Email','Completed','2026-03-08T17:35:00','2026-03-08T16:43:00','self'),
('PR-002','USR-MEM-10004','2026-03-13T07:35:00','Agent','MFA',  'Pending',  '2026-03-13T08:35:00',NULL,                'agent-run-101'),
('PR-003','USR-MEM-20002','2026-02-28T08:05:00','Email','Email','Completed','2026-02-28T09:05:00','2026-02-28T08:25:00','self'),
('PR-004','USR-MEM-10002','2026-01-30T12:00:00','SMS',  'SMS',  'Expired',  '2026-01-30T13:00:00',NULL,                'self'),
('PR-005','USR-MEM-30001','2026-02-01T09:00:00','Portal','KBA', 'Completed','2026-02-01T10:00:00','2026-02-01T09:45:00','self'),
('PR-006','USR-MEM-10001','2025-12-15T09:00:00','Email','Email','Completed','2025-12-15T10:00:00','2025-12-15T09:55:00','self'),
('PR-007','USR-MEM-20004','2025-11-30T14:00:00','Agent','KBA',  'Completed','2025-11-30T15:00:00','2025-11-30T14:30:00','csr-001'),
('PR-008','USR-MEM-10003','2026-03-10T10:00:00','Email','Email','Failed',   '2026-03-10T11:00:00',NULL,                'self'),
('PR-009','USR-MEM-10004','2026-03-13T07:45:00','Email','MFA',  'Sent',     '2026-03-13T08:45:00',NULL,                'csr-001'),
('PR-010','USR-MEM-20001','2026-01-05T11:00:00','SMS',  'SMS',  'Cancelled','2026-01-05T12:00:00',NULL,                'self');
GO

-- =============================================================================
-- F. CUSTOMER SUPPORT
-- =============================================================================

INSERT INTO support.[Case] (CaseId, MemberId, GroupPolicyId, CaseType, Subject, [Description], Status, Priority, Channel, AssignedToUserId, AssignedQueue, SlaDueUtc, SlaBreached, CorrelationId, CreatedByUserId, CreatedBySource, CreatedUtc, UpdatedUtc, ResolvedUtc, ClosedUtc, ResolutionNotes) VALUES
('CASE-001','MEM-10001','GP-ACME-001',  'BillingInquiry',     N'Question about March invoice',          N'Member calling to ask why March premium is higher than February.',                                'Resolved',  'Normal','Phone','USR-CSR-001','CSRTier2',     '2026-03-10T12:00:00',0,'corr-call-001','USR-CSR-001','CSR',   '2026-03-10T11:30:00','2026-03-10T11:50:00','2026-03-10T11:45:00','2026-03-10T11:50:00',N'Explained age band increase. Member satisfied.'),
('CASE-002','MEM-10004','GP-ACME-001',  'LoginIssue',         N'Cannot login - account locked',         N'Member locked out after 5 failed attempts. Wants password reset.',                                'InProgress','High',  'Phone','USR-CSR-002',NULL,           '2026-03-13T11:30:00',0,'agent-run-101','USR-CSR-002','Agent', '2026-03-13T07:30:00','2026-03-13T07:42:00',NULL,                  NULL,                  NULL),
('CASE-003','MEM-10005','GP-ACME-001',  'BillingInquiry',     N'Disputing February invoice',            N'Member terminated coverage 2/28 but received invoice. Disputing charge.',                          'Escalated', 'High',  'Email','USR-CSR-003','BillingOps',   '2026-03-12T17:00:00',1,NULL,           'USR-MEM-10005','Member','2026-03-09T14:00:00','2026-03-09T15:30:00',NULL,                  NULL,                  NULL),
('CASE-004','MEM-10002','GP-ACME-001',  'QLEProcessing',      N'Marriage QLE - add spouse',             N'Recently married, wants to add spouse Tom Smith to coverage.',                                     'Open',      'Normal','Portal','USR-CSR-001','EnrollmentOps','2026-03-15T18:00:00',0,'corr-016',    'USR-MEM-10002','Member','2026-04-22T10:00:00','2026-04-22T10:05:00',NULL,                  NULL,                  NULL),
('CASE-005','MEM-20002','GP-NORTH-002', 'EnrollmentChange',   N'Add newborn',                            N'Member had a newborn 3/1/2026, wants to add to medical and dental coverage.',                      'Pending',   'High',  'Phone','USR-CSR-002','EnrollmentOps','2026-03-08T17:00:00',1,NULL,           'USR-CSR-002','CSR',   '2026-03-04T13:00:00','2026-03-08T17:00:00',NULL,                  NULL,                  NULL),
('CASE-006','MEM-20004','GP-NORTH-002', 'COBRAInquiry',       N'COBRA premium amount question',         N'Asking why COBRA premium is so high. Wants explanation of 102% calculation.',                      'Closed',    'Normal','Phone','USR-CSR-001',NULL,           '2026-03-05T17:00:00',0,NULL,           'USR-CSR-001','CSR',   '2026-03-04T15:24:00','2026-03-04T15:35:00','2026-03-04T15:30:00','2026-03-04T15:35:00',N'Explained COBRA admin fee. Provided written breakdown.'),
('CASE-007','MEM-30001','GP-CONTOSO-03','PlanQuestion',       N'In-network providers near me',          N'Wants list of in-network PCPs in Boston area.',                                                    'Resolved',  'Low',   'Chat', 'USR-CSR-001',NULL,           '2026-03-12T15:00:00',0,NULL,           'USR-MEM-30001','Member','2026-03-12T14:36:00','2026-03-12T14:42:00','2026-03-12T14:40:00','2026-03-12T14:42:00',N'Provided provider directory link and 5 PCP names.'),
('CASE-008','MEM-30002','GP-CONTOSO-03','EligibilityVerification',N'Provider needs eligibility',         N'Northeast Imaging called for eligibility verification.',                                            'Resolved',  'Normal','Phone','USR-CSR-002',NULL,           '2026-03-11T11:00:00',0,NULL,           'USR-CSR-002','CSR',   '2026-03-11T10:30:00','2026-03-11T10:36:00','2026-03-11T10:35:00','2026-03-11T10:36:00',N'Verified active coverage, faxed proof.'),
('CASE-009','MEM-30003','GP-CONTOSO-03','EnrollmentChange',   N'Reduce coverage during LOA',            N'Member on leave wants to reduce to EE only.',                                                      'InProgress','High',  'Email','USR-CSR-003','EnrollmentOps','2026-03-15T18:00:00',0,'corr-014',    'USR-MEM-30003','Member','2026-03-13T09:00:00','2026-03-13T10:00:00',NULL,                  NULL,                  NULL),
('CASE-010','MEM-10003','GP-ACME-001',  'PayrollInquiry',     N'Pre-tax deduction wrong',               N'Member says March paycheck deduction is off by $20.',                                              'Open',      'Normal','Phone','USR-CSR-001','BillingOps',   '2026-03-15T18:00:00',0,NULL,           'USR-CSR-001','CSR',   '2026-03-12T14:00:00','2026-03-12T14:10:00',NULL,                  NULL,                  NULL),
('CASE-011','MEM-10001','GP-ACME-001',  'IDCardRequest',      N'Replacement ID card',                   N'Lost ID card, requesting replacement (digital + mail).',                                           'Resolved',  'Low',   'Portal','USR-CSR-002',NULL,          '2026-03-08T18:00:00',0,NULL,           'USR-MEM-10001','Member','2026-03-08T13:15:00','2026-03-08T13:25:00','2026-03-08T13:20:00','2026-03-08T13:25:00',N'Digital ID sent via email; physical mail in 3 days.'),
('CASE-012','MEM-20001','GP-NORTH-002', 'GeneralInquiry',     N'How to use HSA',                        N'New to HDHP plan, wants to know how to contribute to HSA.',                                        'Resolved',  'Normal','Chat', 'USR-CSR-001',NULL,           '2026-03-09T18:00:00',0,NULL,           'USR-MEM-20001','Member','2026-03-09T17:00:00','2026-03-09T17:15:00','2026-03-09T17:10:00','2026-03-09T17:15:00',N'Explained HSA, sent enrollment link.'),
('CASE-013','MEM-10003','GP-ACME-001',  'Complaint',          N'Pharmacy denial complaint',             N'Member complaining that BCBS denied tier-3 drug. Wants formal review.',                            'Escalated', 'High',  'Email','USR-CSR-003','Compliance',   '2026-03-15T18:00:00',0,NULL,           'USR-MEM-10003','Member','2026-03-12T08:00:00','2026-03-12T08:30:00',NULL,                  NULL,                  NULL),
('CASE-014','MEM-10002','GP-ACME-001',  'PasswordReset',      N'Forgot password',                       N'Member forgot password, requesting reset.',                                                        'Cancelled', 'Low',   'Portal','USR-CSR-001',NULL,           '2026-01-30T13:00:00',0,'PR-004',       'USR-MEM-10002','Member','2026-01-30T12:00:00','2026-01-30T13:01:00',NULL,                  '2026-01-30T13:01:00',N'Member cancelled before completing.'),
('CASE-015','MEM-30001','GP-CONTOSO-03','DocumentRequest',    N'Need SBC for tax filing',               N'Requesting Summary of Benefits Coverage PDF for 2025 plan.',                                       'Resolved',  'Normal','Portal','USR-CSR-002',NULL,           '2026-03-02T18:00:00',0,NULL,           'USR-MEM-30001','Member','2026-03-01T14:55:00','2026-03-01T15:02:00','2026-03-01T15:00:00','2026-03-01T15:02:00',N'SBC PDF sent via secure portal.'),
('CASE-016','MEM-20002','GP-NORTH-002', 'Complaint',          N'Long hold time during newborn call',    N'Member complaining about 30+ min hold during newborn enrollment call.',                            'Open',      'Low',   'Phone','USR-CSR-001',NULL,           '2026-04-22T17:00:00',0,NULL,           'USR-MEM-20002','Member','2026-03-08T11:00:00','2026-03-08T11:00:00',NULL,                  NULL,                  NULL),
('CASE-017','MEM-10001','GP-ACME-001',  'Complaint',          N'Privacy concern - transcript exposure', N'Member alleges PHI visible in transcript shared with spouse.',                                     'Escalated', 'High',  'Email','USR-CSR-003','Compliance',   '2026-08-15T17:00:00',0,NULL,           'USR-MEM-10001','Member','2026-02-15T10:00:00','2026-02-15T11:00:00',NULL,                  NULL,                  NULL),
('CASE-018','MEM-20004','GP-NORTH-002', 'Complaint',          N'COBRA premium dispute',                 N'Disagreement with COBRA premium calculation. Same root cause as CASE-006.',                       'Closed',    'Normal','Phone','USR-CSR-001',NULL,           '2026-04-18T17:00:00',0,NULL,           'USR-CSR-001','CSR',   '2026-03-04T14:00:00','2026-03-04T16:00:00','2026-03-04T15:30:00','2026-03-04T16:00:00',N'Dismissed after written explanation.');
GO

INSERT INTO support.CaseNote (CaseId, AuthorUserId, AuthorType, NoteText, IsInternal) VALUES
('CASE-001','USR-CSR-001','CSR',   N'Member called at 11:30 AM. Pulled invoice INV-10001-2603. Member age 41 — moved into 40-49 band on Jan 1.',1),
('CASE-001','USR-CSR-001','CSR',   N'Walked member through age-band pricing in plan SBC. Sent rate sheet PDF.',1),
('CASE-001','USR-CSR-001','CSR',   N'Member acknowledged understanding. Case resolved.',1),
('CASE-002','USR-CSR-002','Agent', N'Agent attempt failed login analysis: 5 BadPassword attempts in 5 min → automatic lock.',1),
('CASE-002','USR-CSR-002','CSR',   N'Member verified identity via DOB + last4 SSN. Initiated MFA-verified reset PR-002.',1),
('CASE-003','USR-CSR-003','CSR',   N'Invoice was generated 2/5, before termination posted on 2/28. Need to credit balance.',1),
('CASE-003','USR-CSR-003','CSR',   N'Escalating to Billing Ops — pro-rated refund required.',1),
('CASE-004','USR-MEM-10002','Member',N'Married 4/20/2026. Need to add Tom Smith effective 5/1.',0),
('CASE-004','USR-CSR-001','CSR',   N'Created enrollment task ENR-10002-QL. Awaiting marriage certificate upload.',1),
('CASE-005','USR-CSR-002','CSR',   N'Member submitted birth certificate via secure upload. Awaiting EnrollmentOps to process.',1),
('CASE-005','USR-CSR-002','CSR',   N'SLA breached — escalated priority.',1),
('CASE-006','USR-CSR-001','CSR',   N'Sent COBRA fee breakdown email. Member said thanks.',1),
('CASE-007','USR-CSR-001','CSR',   N'Used provider lookup tool, filtered by zip 02108, PCP only, in-network.',1),
('CASE-008','USR-CSR-002','CSR',   N'Provider Northeast Imaging called via secure line. Confirmed coverage active for MEM-30002 family tier.',1),
('CASE-008','USR-CSR-002','CSR',   N'Faxed eligibility letter to 617-555-9999.',1),
('CASE-009','USR-MEM-30003','Member',N'On LOA since April. Cannot afford family tier premium. Wants EE only.',0),
('CASE-009','USR-CSR-003','CSR',   N'Confirmed LOA documentation in file. Processing reduction to EE tier effective next pay period.',1),
('CASE-010','USR-MEM-10003','Member',N'Expected $183.60 pre-tax for medical. Got $203.60. Where is extra $20?',0),
('CASE-010','USR-CSR-001','CSR',   N'Looking into payroll file PF-ACME-2602-1. May be FSA contribution mixed in.',1),
('CASE-011','USR-CSR-002','CSR',   N'Sent digital ID via email. Initiated physical mail request.',1),
('CASE-012','USR-CSR-001','CSR',   N'Provided HSA setup link. Member can contribute up to IRS family limit.',0),
('CASE-013','USR-MEM-10003','Member',N'BCBS refuses to cover drug XYZ-100. Need formal complaint process.',0),
('CASE-013','USR-CSR-003','CSR',   N'Routed to Compliance for formal grievance. Filed complaint CMP-001.',1),
('CASE-014','USR-MEM-10002','Member',N'Actually I remembered my password. Cancel.',0),
('CASE-015','USR-CSR-002','CSR',   N'Pulled 2025 SBC from archive. Sent via portal secure download.',1);
GO

INSERT INTO support.Interaction (InteractionId, MemberId, CaseId, Channel, Direction, HandledByUserId, HandledByAgent, SessionId, Intent, Summary, TranscriptUri, SentimentScore, DurationSeconds, StartedUtc, EndedUtc) VALUES
('INT-001','MEM-10001','CASE-001','Phone','Inbound','USR-CSR-001',0,'sess-001','Billing',N'Inquiry about March premium increase. Resolved via SBC walkthrough.',NULL,        0.300,420,'2026-03-10T11:30:00','2026-03-10T11:37:00'),
('INT-002','MEM-10004','CASE-002','Phone','Inbound','USR-CSR-002',0,'sess-002','LoginIssue',N'Account locked, agent diagnosed via IAM tools and initiated reset.','https://blob/transcripts/INT-002.txt',-0.200,720,'2026-03-13T07:30:00','2026-03-13T07:42:00'),
('INT-003','MEM-10005','CASE-003','Email','Inbound',NULL,           0,'sess-003','Billing',N'Email dispute of post-termination invoice. Escalated to BillingOps.',NULL,        -0.500,NULL,'2026-03-09T14:00:00','2026-03-09T14:00:00'),
('INT-004','MEM-10002','CASE-004','Portal','Inbound',NULL,          0,'sess-004','Enrollment',N'Portal submission of QLE marriage event.',NULL,                                     0.100,180,'2026-04-22T10:00:00','2026-04-22T10:03:00'),
('INT-005','MEM-20002','CASE-005','Phone','Inbound','USR-CSR-002',0,'sess-005','Enrollment',N'Add newborn to coverage. Birth cert uploaded.',NULL,                                  0.400,540,'2026-03-04T13:00:00','2026-03-04T13:09:00'),
('INT-006','MEM-20004','CASE-006','Phone','Inbound','USR-CSR-001',0,'sess-006','PlanQA',N'COBRA premium explanation. Member satisfied.',NULL,                                       0.250,360,'2026-03-04T15:24:00','2026-03-04T15:30:00'),
('INT-007','MEM-30001','CASE-007','Chat','Inbound','USR-CSR-001',0,'sess-007','PlanQA',N'PCP lookup in Boston area.',NULL,                                                            0.500,240,'2026-03-12T14:36:00','2026-03-12T14:40:00'),
('INT-008','MEM-30001',NULL,      'Portal','Inbound',NULL,          1,'sess-008','PlanQA',N'Agent answered "what does my dental plan cover?" via RAG knowledge base.','https://blob/transcripts/INT-008.txt',0.600,45,'2026-03-13T10:05:00','2026-03-13T10:06:00'),
('INT-009','MEM-30002','CASE-008','Phone','Inbound','USR-CSR-002',0,'sess-009','Eligibility',N'Provider called for verification.',NULL,                                              0.400,300,'2026-03-11T10:30:00','2026-03-11T10:35:00'),
('INT-010','MEM-30003','CASE-009','Email','Inbound',NULL,           0,'sess-010','Enrollment',N'Email request to reduce coverage during LOA.',NULL,                                  -0.100,NULL,'2026-03-13T09:00:00','2026-03-13T09:00:00'),
('INT-011','MEM-10003','CASE-010','Phone','Inbound','USR-CSR-001',0,'sess-011','Payroll',N'Payroll deduction discrepancy. Investigating.',NULL,                                     -0.050,360,'2026-03-12T14:00:00','2026-03-12T14:06:00'),
('INT-012','MEM-10001','CASE-011','Portal','Inbound',NULL,          1,'sess-012','Other',N'Agent processed ID card replacement via portal action.',NULL,                              0.500,60,'2026-03-08T13:15:00','2026-03-08T13:16:00'),
('INT-013','MEM-20001','CASE-012','Chat','Inbound','USR-CSR-001',0,'sess-013','PlanQA',N'HSA Q&A.',NULL,                                                                                0.700,420,'2026-03-09T17:00:00','2026-03-09T17:07:00'),
('INT-014','MEM-10003','CASE-013','Email','Inbound',NULL,           0,'sess-014','Complaint',N'Formal complaint about pharmacy denial.',NULL,                                       -0.800,NULL,'2026-03-12T08:00:00','2026-03-12T08:00:00'),
('INT-015','MEM-10002','CASE-014','Portal','Inbound',NULL,          1,'sess-015','PasswordReset',N'Started reset, then cancelled.',NULL,                                              0.100,90,'2026-01-30T12:00:00','2026-01-30T12:01:30'),
('INT-016','MEM-30001','CASE-015','Portal','Inbound',NULL,          1,'sess-016','Other',N'Agent retrieved SBC and delivered via portal.',NULL,                                        0.600,30,'2026-03-01T14:55:00','2026-03-01T14:55:30'),
('INT-017','MEM-10001',NULL,      'Phone','Outbound','USR-CSR-001',0,'sess-017','Other',N'Outbound courtesy callback re: invoice resolution.',NULL,                                  0.500,120,'2026-03-11T10:00:00','2026-03-11T10:02:00'),
('INT-018','MEM-20002',NULL,      'Phone','Outbound','USR-CSR-002',0,'sess-018','Enrollment',N'Outbound to confirm newborn coverage effective date.',NULL,                          0.400,180,'2026-03-05T11:00:00','2026-03-05T11:03:00');
GO

INSERT INTO support.Escalation (EscalationId, CaseId, FromUserId, ToQueue, ToUserId, Reason, ReasonDetail, Status, EscalatedUtc, AcknowledgedUtc, ResolvedUtc) VALUES
('ESC-001','CASE-003','USR-CSR-001','BillingOps',   'USR-CSR-003','ActionRequired',  N'Pro-rated refund processing required',                  'InProgress','2026-03-09T15:00:00','2026-03-09T15:30:00',NULL),
('ESC-002','CASE-005','USR-CSR-002','EnrollmentOps',NULL,         'SLABreach',       N'SLA breached — newborn enrollment pending > 5 days',     'Open',      '2026-03-08T17:00:00',NULL,                 NULL),
('ESC-003','CASE-013','USR-CSR-003','Compliance',   NULL,         'Complaint',       N'Formal grievance regarding tier-3 pharmacy denial',     'Open',      '2026-03-12T08:30:00',NULL,                 NULL),
('ESC-004','CASE-009','USR-CSR-001','EnrollmentOps','USR-CSR-003','MemberRequest',   N'Member request to reduce coverage during LOA',          'Acknowledged','2026-03-13T09:30:00','2026-03-13T10:00:00',NULL),
('ESC-005','CASE-002',NULL,          'SecurityOps', NULL,         'GuardrailTriggered',N'Agent flagged repeated failed logins as suspicious',   'Resolved',  '2026-03-13T07:35:00','2026-03-13T07:40:00','2026-03-13T07:50:00'),
('ESC-006','CASE-010','USR-CSR-001','BillingOps',   NULL,         'ActionRequired',  N'Payroll discrepancy needs reconciliation',              'Open',      '2026-03-12T14:10:00',NULL,                 NULL),
('ESC-007','CASE-005','USR-CSR-002','CSRTier3',     NULL,         'Supervisor',      N'Supervisor review requested by member',                 'Resolved',  '2026-03-06T10:00:00','2026-03-06T10:15:00','2026-03-06T11:00:00');
GO

INSERT INTO support.Complaint (ComplaintId, CaseId, MemberId, ComplaintType, Severity, RegulatoryFlag, RegulatorAgency, [Description], RootCause, CorrectiveAction, Status, FiledUtc, DueUtc, ResolvedUtc) VALUES
('CMP-001','CASE-013','MEM-10003','Coverage',    'High',    1,'IL Dept of Insurance',N'Member alleges improper denial of medically necessary tier-3 drug XYZ-100.',NULL,                                            NULL,                                            'UnderReview','2026-03-12T09:00:00','2026-04-26T17:00:00',NULL),
('CMP-002','CASE-003','MEM-10005','Billing',     'Medium',  0,NULL,                  N'Disputed post-termination invoice.',                                          N'Invoice generated before termination posted',  N'Refund issued; aging billing cycle moved up.',  'Resolved',   '2026-03-09T14:30:00','2026-04-23T17:00:00','2026-03-15T16:00:00'),
('CMP-003','CASE-016','MEM-20002','Service',     'Low',     0,NULL,                  N'Long hold time during newborn enrollment call.',                              NULL,                                            NULL,                                            'Filed',      '2026-03-08T11:00:00','2026-04-22T17:00:00',NULL),
('CMP-004','CASE-017','MEM-10001','Privacy',     'High',    1,'OCR (HHS)',           N'Member alleges PHI was visible in agent transcript shared with spouse.',     N'Transcript redaction filter missed PHI string',N'Strengthened agent PHI redaction guardrail',   'UnderReview','2026-02-15T10:00:00','2026-08-15T17:00:00',NULL),
('CMP-005','CASE-018','MEM-20004','Coverage',    'Medium',  0,NULL,                  N'Disagreement with COBRA premium calculation.',                                N'Member did not understand 102% admin fee',     N'Sent written explanation; case closed.',       'Dismissed',  '2026-03-04T14:00:00','2026-04-18T17:00:00','2026-03-04T16:00:00');
GO

-- =============================================================================
-- Verify counts
-- =============================================================================
SELECT 'member.EmployerGroup' AS [Table], COUNT(*) AS Rows FROM member.EmployerGroup
UNION ALL SELECT 'member.Member',                COUNT(*) FROM member.Member
UNION ALL SELECT 'member.Dependent',             COUNT(*) FROM member.Dependent
UNION ALL SELECT 'elig.BenefitPlan',             COUNT(*) FROM elig.BenefitPlan
UNION ALL SELECT 'elig.MemberCoverage',          COUNT(*) FROM elig.MemberCoverage
UNION ALL SELECT 'elig.Accumulator',             COUNT(*) FROM elig.Accumulator
UNION ALL SELECT 'elig.CoverageHistory',         COUNT(*) FROM elig.CoverageHistory
UNION ALL SELECT 'enroll.EnrollmentTransaction', COUNT(*) FROM enroll.EnrollmentTransaction
UNION ALL SELECT 'enroll.PlanElection',          COUNT(*) FROM enroll.PlanElection
UNION ALL SELECT 'enroll.QualifyingLifeEvent',   COUNT(*) FROM enroll.QualifyingLifeEvent
UNION ALL SELECT 'enroll.OutboxEvent',           COUNT(*) FROM enroll.OutboxEvent
UNION ALL SELECT 'billing.Invoice',              COUNT(*) FROM billing.Invoice
UNION ALL SELECT 'billing.InvoiceLine',          COUNT(*) FROM billing.InvoiceLine
UNION ALL SELECT 'billing.Payment',              COUNT(*) FROM billing.Payment
UNION ALL SELECT 'billing.PremiumRate',          COUNT(*) FROM billing.PremiumRate
UNION ALL SELECT 'payroll.PayrollDeduction',     COUNT(*) FROM payroll.PayrollDeduction
UNION ALL SELECT 'payroll.PayrollFile',          COUNT(*) FROM payroll.PayrollFile
UNION ALL SELECT 'iam.UserAccount',              COUNT(*) FROM iam.UserAccount
UNION ALL SELECT 'iam.LoginAttempt',             COUNT(*) FROM iam.LoginAttempt
UNION ALL SELECT 'iam.PasswordResetRequest',     COUNT(*) FROM iam.PasswordResetRequest
UNION ALL SELECT 'iam.AccessRole',               COUNT(*) FROM iam.AccessRole
UNION ALL SELECT 'iam.UserRole',                 COUNT(*) FROM iam.UserRole
UNION ALL SELECT 'support.Case',                 COUNT(*) FROM support.[Case]
UNION ALL SELECT 'support.CaseNote',             COUNT(*) FROM support.CaseNote
UNION ALL SELECT 'support.Interaction',          COUNT(*) FROM support.Interaction
UNION ALL SELECT 'support.Escalation',           COUNT(*) FROM support.Escalation
UNION ALL SELECT 'support.Complaint',            COUNT(*) FROM support.Complaint
ORDER BY 1;
GO
