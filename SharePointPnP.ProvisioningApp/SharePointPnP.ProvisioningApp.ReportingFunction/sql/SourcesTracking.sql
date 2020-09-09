USE [sppnpprovisioningreporting]
GO

/****** Object:  Table [dbo].[Sources]    Script Date: 5/11/2020 2:40:27 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Sources](
	[SourceId] [nvarchar](50) NOT NULL,
	[SourceDisplayName] [nvarchar](200) NOT NULL,
 CONSTRAINT [PK_Sources_SourceId] PRIMARY KEY CLUSTERED 
(
	[SourceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO

/****** Object:  Table [dbo].[SourcesTracking]    Script Date: 5/11/2020 2:40:29 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SourcesTracking](
	[SourceTrackingId] [bigint] IDENTITY(1,1) NOT NULL,
	[SourceId] [nvarchar](50) NOT NULL,
	[SourceTrackingDateTime] [datetime] NOT NULL,
	[SourceTrackingAction] [tinyint] NOT NULL,
	[SourceTrackingUrl] [nvarchar](500) NULL,
	[SourceTrackingFromProduction] bit NOT NULL,
	[TemplateId] [uniqueidentifier] NULL,
	[TenantId] [uniqueidentifier] NULL,
	[SiteId] [uniqueidentifier] NULL
)

GO

ALTER TABLE dbo.SourcesTracking
   ADD CONSTRAINT PK_SourcesTracking_SourceTrackingId PRIMARY KEY CLUSTERED (SourceTrackingId)
GO

ALTER TABLE [dbo].[SourcesTracking]  WITH CHECK ADD  CONSTRAINT [FK_SourcesTracking_Sources] FOREIGN KEY([SourceId])
REFERENCES [dbo].[Sources] ([SourceId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[SourcesTracking] CHECK CONSTRAINT [FK_SourcesTracking_Sources]
GO

USE [sppnpprovisioningreporting]
GO

/****** Object:  StoredProcedure [dbo].[InsertSourceTracking]    Script Date: 5/11/2020 2:44:53 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[InsertSourceTracking] 
(
@SourceId nvarchar(50),
@SourceTrackingDateTime datetime,
@SourceTrackingAction tinyint,
@SourceTrackingUrl nvarchar(500) = NULL,
@SourceTrackingFromProduction bit,
@TemplateId uniqueidentifier = NULL,
@TenantId uniqueidentifier = NULL,
@SiteId uniqueidentifier = NULL
)
AS

INSERT INTO dbo.SourcesTracking VALUES
(
	@SourceId,
	@SourceTrackingDateTime,
	@SourceTrackingAction,
	@SourceTrackingUrl,
	@SourceTrackingFromProduction,
	@TemplateId,
	@TenantId,
	@SiteId
)

GO

