CREATE TABLE [dbo].[BackgroundChecks]
(
[ID] [int] NOT NULL IDENTITY(1, 1),
[Created] [datetime] NOT NULL CONSTRAINT [DF_BackgroundChecks_Created] DEFAULT (getdate()),
[Updated] [datetime] NOT NULL CONSTRAINT [DF_BackgroundChecks_Updated] DEFAULT (getdate()),
[UserID] [int] NOT NULL CONSTRAINT [DF_Table_1_Status] DEFAULT ((1)),
[StatusID] [int] NOT NULL CONSTRAINT [DF_Table_1_UserID] DEFAULT ((1)),
[PeopleID] [int] NOT NULL CONSTRAINT [DF_Table_1_ReportID] DEFAULT ((1)),
[ServiceCode] [nvarchar] (25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL CONSTRAINT [DF_BackgroundChecks_ServiceCode] DEFAULT (''),
[ReportID] [int] NOT NULL CONSTRAINT [DF_BackgroundChecks_ReportID] DEFAULT ((0)),
[ReportLink] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL CONSTRAINT [DF_BackgroundChecks_ReportLink] DEFAULT (''),
[IssueCount] [int] NOT NULL CONSTRAINT [DF_Table_1_AlertCount] DEFAULT ((0)),
[ErrorMessages] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ReportTypeID] [int] NOT NULL CONSTRAINT [DF_BackgroundChecks_ReportTypeID] DEFAULT ((0)),
[ReportLabelID] [int] NOT NULL CONSTRAINT [DF_BackgroundChecks_ReportLabelID] DEFAULT ((0))
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
