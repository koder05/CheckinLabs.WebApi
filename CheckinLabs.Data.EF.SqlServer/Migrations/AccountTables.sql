create table [dbo].[Users]
(
	[Id] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED,
	[Name] nvarchar(128) COLLATE Cyrillic_General_CI_AS not null,
    [UID] uniqueidentifier not null,
    [SecretHash] varbinary(1024) not null,
    [SecretSalt]  varbinary(1024) not null,
    [AccountState] int not null,
    [CreateDate] datetime not null,
    [CreatedOver] nvarchar(128) COLLATE Cyrillic_General_CI_AS
);
GO
create table [dbo].[UserProfiles]
(
	[Id] [int] NOT NULL CONSTRAINT [PK_UserProfile] PRIMARY KEY CLUSTERED 
						CONSTRAINT [FK_User] FOREIGN KEY REFERENCES [dbo].[Users] ([Id]),
	[Email] nvarchar(128) COLLATE Cyrillic_General_CI_AS not null,
	[Phone] varchar(128),
	[DisplayName] nvarchar(260) COLLATE Cyrillic_General_CI_AS not null,
	[ManagerName] nvarchar(260) COLLATE Cyrillic_General_CI_AS,
	[CompanyName] nvarchar(260) COLLATE Cyrillic_General_CI_AS
);
GO
create table [dbo].[UserCheckins]
(
	[Id] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_UserCheckin] PRIMARY KEY CLUSTERED,
	[UserProfileId] [int] NOT NULL CONSTRAINT [FK_UserProfile] FOREIGN KEY REFERENCES [dbo].[UserProfiles] ([Id]),
    [CheckinAddr] nvarchar(128) COLLATE Cyrillic_General_CI_AS not null,
    [NotifyChannelType] int not null,
    [UserCheckinType] int not null,
    [CreateDate] datetime not null,
    [NotifyDate] datetime,
    [CheckinDate] datetime,
    [Code] varchar(1024) not null,
    [Msg] nvarchar(max) COLLATE Cyrillic_General_CI_AS not null,
    [IsUsed] bit not null
);
GO
create unique index UIDX_Name on [dbo].[Users]([Name]);
create unique index UIDX_UID on [dbo].[Users]([UID]);
create unique index UIDX_Code on [dbo].[UserCheckins]([Code]);