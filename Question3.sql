CREATE DATABASE RaceDay;


--============================================================
--   1. USERS
--============================================================ 

CREATE TABLE Users
(
    UserID INT IDENTITY(1,1) NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    Password VARCHAR(150) NOT NULL,
    Phone VARCHAR(150) NOT NULL,
    Role VARCHAR(150) NOT NULL
        CONSTRAINT DF_Users_Role DEFAULT ('Participant'),
    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Users PRIMARY KEY (UserID),
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);
GO


 --============================================================
 --ORGANISERS
 --============================================================ 

CREATE TABLE Organisers
(
    OrganiserID INT IDENTITY(1,1) NOT NULL,
    UserID INT NOT NULL,
    OrganisationName VARCHAR(20) NOT NULL,
    ContactNumber VARCHAR(100) NOT NULL,
    Website VARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Organisers_CreatedAt DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Organisers PRIMARY KEY (OrganiserID),

   
    CONSTRAINT UQ_Organisers_UserID UNIQUE (UserID),

    CONSTRAINT FK_Organisers_Users
        FOREIGN KEY (UserID)
        REFERENCES Users(UserID)
);
GO

 --============================================================
 --articipants
 --============================================================ 


CREATE TABLE Participants
(
    ParticipantID INT IDENTITY(1,1) NOT NULL,
    UserID INT NOT NULL,
    DateOfBirth DATE NOT NULL,
    Gender VARCHAR(10) NOT NULL,
    EmergencyContact VARCHAR(20) NOT NULL,
    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Participants_CreatedAt DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Participants PRIMARY KEY (ParticipantID),

    -- One user can have at most one participant profile
    CONSTRAINT UQ_Participants_UserID UNIQUE (UserID),

    CONSTRAINT FK_Participants_Users
        FOREIGN KEY (UserID)
        REFERENCES Users(UserID)
);
GO

 --============================================================
 --  4. EVENTS
 -- ============================================================ 

CREATE TABLE Events
(
    EventID INT IDENTITY(1,1) NOT NULL,
    OrganiserID INT NOT NULL,
    EventName VARCHAR(150) NOT NULL,
    Description VARCHAR(500) NULL,
    EventDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    Location VARCHAR(150) NOT NULL,
    Province VARCHAR(150) NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL
        CONSTRAINT DF_Events_EntryFee DEFAULT (0.00),
    MaxParticipants VARCHAR(150) NOT NULL,
    Status VARCHAR(150) NOT NULL
        CONSTRAINT DF_Events_Status DEFAULT ('Upcoming'),

    CONSTRAINT PK_Events PRIMARY KEY (EventID),

    CONSTRAINT FK_Events_Organisers
        FOREIGN KEY (OrganiserID)
        REFERENCES Organisers(OrganiserID),

    CONSTRAINT CK_Events_EntryFee
        CHECK (EntryFee >= 0)
);
GO


--============================================================
--   5. CATEGORIES
--============================================================ 

CREATE TABLE Categories
(
    CategoryID INT IDENTITY(1,1) NOT NULL,
    CategoryName VARCHAR(90) NOT NULL,
    Description VARCHAR(200) NULL,

    CONSTRAINT PK_Categories PRIMARY KEY (CategoryID),
    CONSTRAINT UQ_Categories_CategoryName UNIQUE (CategoryName)
);
GO


--============================================================
--   6. EVENT CATEGORIES
--============================================================ 

CREATE TABLE EventCategories
(
    CategoryID INT NOT NULL,
    EventID INT NOT NULL,

    CONSTRAINT PK_EventCategories
        PRIMARY KEY (CategoryID, EventID),

    CONSTRAINT FK_EventCategories_Categories
        FOREIGN KEY (CategoryID)
        REFERENCES Categories(CategoryID),

    CONSTRAINT FK_EventCategories_Events
        FOREIGN KEY (EventID)
        REFERENCES Events(EventID)
);
GO


--============================================================
--   7. ROUTES
--============================================================ 

CREATE TABLE Routes
(
    RouteID INT IDENTITY(1,1) NOT NULL,
    EventID INT NOT NULL,
    RouteName VARCHAR(100) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL,
    ElevationGainM INT NOT NULL,
    StartPoint VARCHAR(150) NOT NULL,
    FinishPoint VARCHAR(150) NOT NULL,
    RouteURL VARCHAR(500) NULL,

    CONSTRAINT PK_Routes PRIMARY KEY (RouteID),

  
    CONSTRAINT UQ_Routes_EventID UNIQUE (EventID),

    CONSTRAINT FK_Routes_Events
        FOREIGN KEY (EventID)
        REFERENCES Events(EventID),

    CONSTRAINT CK_Routes_Distance
        CHECK (DistanceKm > 0),

    CONSTRAINT CK_Routes_Elevation
        CHECK (ElevationGainM >= 0)
);
GO


 --============================================================
 --  8. EVENT ENROLLMENTS
 --============================================================ 

CREATE TABLE EventEnrollments
(
    EnrollmentID INT IDENTITY(1,1) NOT NULL,
    EventID INT NOT NULL,
    ParticipantID INT NOT NULL,
    EnrollmentDate DATETIME2 NOT NULL
        CONSTRAINT DF_EventEnrollments_EnrollmentDate
        DEFAULT (SYSDATETIME()),
    RaceNumber VARCHAR(20) NOT NULL,
    Status VARCHAR(20) NOT NULL
        CONSTRAINT DF_EventEnrollments_Status
        DEFAULT ('Registered'),

    CONSTRAINT PK_EventEnrollments
        PRIMARY KEY (EnrollmentID),

    CONSTRAINT FK_EventEnrollments_Events
        FOREIGN KEY (EventID)
        REFERENCES Events(EventID),

    CONSTRAINT FK_EventEnrollments_Participants
        FOREIGN KEY (ParticipantID)
        REFERENCES Participants(ParticipantID),

 
    CONSTRAINT UQ_EventEnrollments_EventParticipant
        UNIQUE (EventID, ParticipantID)
);
GO


 --============================================================
 -- 9. RESULTS
 --============================================================ 

CREATE TABLE Results
(
    ResultID INT IDENTITY(1,1) NOT NULL,
    EnrollmentID INT NOT NULL,
    FinishTime TIME NULL,
    Position INT NULL,
    AveragePace VARCHAR(20) NULL,
    ResultStatus VARCHAR(20) NOT NULL
        CONSTRAINT DF_Results_ResultStatus
        DEFAULT ('Pending'),

    CONSTRAINT PK_Results PRIMARY KEY (ResultID),

  
    CONSTRAINT UQ_Results_EnrollmentID UNIQUE (EnrollmentID),

    CONSTRAINT FK_Results_EventEnrollments
        FOREIGN KEY (EnrollmentID)
        REFERENCES EventEnrollments(EnrollmentID),

    CONSTRAINT CK_Results_Position
        CHECK (Position IS NULL OR Position > 0)
);
GO


 --============================================================
 -- 10. EVENT WEATHER
 --============================================================ 

CREATE TABLE EventWeather
(
    WeatherID INT IDENTITY(1,1) NOT NULL,
    EventID INT NOT NULL,
    ForecastDate DATE NOT NULL,
    TemperatureC DECIMAL(5,2) NOT NULL,
    Condition VARCHAR(80) NOT NULL,
    WindSpeedKph DECIMAL(6,2) NOT NULL,
    RainChancePercent INT NOT NULL,
    RecordedAt DATETIME2 NOT NULL
        CONSTRAINT DF_EventWeather_RecordedAt
        DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_EventWeather PRIMARY KEY (WeatherID),

    CONSTRAINT FK_EventWeather_Events
        FOREIGN KEY (EventID)
        REFERENCES Events(EventID),

    CONSTRAINT CK_EventWeather_WindSpeed
        CHECK (WindSpeedKph >= 0),

    CONSTRAINT CK_EventWeather_RainChance
        CHECK (RainChancePercent BETWEEN 0 AND 100)
);
GO


 --============================================================
 --  SELECT
 -- ============================================================ 

INSERT INTO Users
    (FirstName, LastName, Email, Password, Phone, Role)
VALUES
    ('Thabo', 'Mokoena',
     'thabo.mokoena@raceday.co.za',
     'Password123!',
     '0825551001',
     'Organiser'),

    ('Naledi', 'Dlamini',
     'naledi.dlamini@raceday.co.za',
     'Password123!',
     '0835551002',
     'Organiser'),

    ('lad', 'chant',
     'ladchant@email.com',
     'Password123!',
     '0845551003',
     'Participant'),

    ('Lerato', 'Molefe',
     'lerato.molefe@email.com',
     'Password123!',
     '0855551004',
     'Participant');





INSERT INTO Organisers
    (UserID, OrganisationName, ContactNumber, Website)
VALUES
    (1,
     'RunSA',
     '0115551001',
     'https://www.runsa.co.za'),

    (2,
     'ActiveRace',
     '0115551002',
     'https://www.activerace.co.za');





INSERT INTO Participants
    (UserID, DateOfBirth, Gender, EmergencyContact)
VALUES
    (3,
     '2000-05-15',
     'Male',
     '0795552001'),

    (4,
     '1999-08-22',
     'Female',
     '0795552002');





INSERT INTO Events
    (OrganiserID, EventName, Description, EventDate, StartTime,
     Location, Province, EntryFee, MaxParticipants, Status)
VALUES
    (1,
     'Johannesburg City Run',
     'A competitive road running event through central Johannesburg.',
     '2026-10-10',
     '07:00:00',
     'Johannesburg',
     'Gauteng',
     150.00,
     '500',
     'Upcoming'),

    (1,
     'Pretoria Spring Race',
     'A community road race suitable for beginner and experienced runners.',
     '2026-10-24',
     '06:30:00',
     'Pretoria',
     'Gauteng',
     120.00,
     '400',
     'Upcoming'),

    (2,
     'Cape Town Coastal Run',
     'A scenic coastal running event with ocean views.',
     '2026-11-07',
     '06:00:00',
     'Cape Town',
     'Western Cape',
     200.00,
     '600',
     'Upcoming');




INSERT INTO Categories
    (CategoryName, Description)
VALUES
    ('5 KM',
     'Five kilometre running category.'),

    ('10 KM',
     'Ten kilometre running category.'),

    ('21 KM',
     'Half marathon category.');



INSERT INTO EventCategories
    (CategoryID, EventID)
VALUES
    (1, 1),
    (2, 1),
    (3, 1),
    (1, 2),
    (2, 2),
    (3, 2),
    (1, 3),
    (2, 3),
    (3, 3);


INSERT INTO Routes
    (EventID, RouteName, DistanceKm, ElevationGainM,
     StartPoint, FinishPoint, RouteURL)
VALUES
    (1,
     'Johannesburg Central Route',
     10.00,
     120,
     'Nelson Mandela Square',
     'Sandton Sports Club',
     'https://www.runsa.co.za/routes/jhb'),

    (2,
     'Pretoria Union Buildings Route',
     10.00,
     150,
     'Union Buildings',
     'Pretoria National Botanical Garden',
     'https://www.runsa.co.za/routes/pta'),

    (3,
     'Cape Town Coastal Route',
     21.10,
     180,
     'Sea Point',
     'Camps Bay',
     'https://www.activerace.co.za/routes/cpt');




INSERT INTO EventEnrollments
    (EventID, ParticipantID, RaceNumber, Status)
VALUES
    (1, 1, 'JHB001', 'Registered'),

    (1, 2, 'JHB002', 'Registered'),

    (2, 1, 'PTA001', 'Registered'),

    (3, 2, 'CPT001', 'Registered');



INSERT INTO Results
    (EnrollmentID, FinishTime, Position, AveragePace, ResultStatus)
VALUES
    (1,
     '00:52:35',
     12,
     '05:15 min/km',
     'Completed'),

    (2,
     '00:58:42',
     25,
     '05:52 min/km',
     'Completed');
GO



INSERT INTO EventWeather
    (EventID, ForecastDate, TemperatureC, Condition,
     WindSpeedKph, RainChancePercent)
VALUES
    (1,
     '2026-10-10',
     18.50,
     'Sunny',
     12.50,
     10),

    (2,
     '2026-10-24',
     20.00,
     'Partly Cloudy',
     15.20,
     20),

    (3,
     '2026-11-07',
     19.50,
     'Clear',
     18.00,
     15);
GO


