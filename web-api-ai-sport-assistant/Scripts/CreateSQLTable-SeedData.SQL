USE DVSports

CREATE TABLE FootballGames (
    GameId INT IDENTITY(1,1) PRIMARY KEY,
    HomeTeam VARCHAR(50),
    AwayTeam VARCHAR(50),
    HomeScore INT,
    AwayScore INT,
    Final BIT,  -- Use BIT instead of BOOLEAN
    Penalties INT,
    Turnover INT,
    Winner VARCHAR(50),
    Loser VARCHAR(50)
);



INSERT INTO FootballGames (HomeTeam, AwayTeam, HomeScore, AwayScore, Final, Penalties, Turnover, Winner, Loser)
VALUES 
('OHIO ST',  'GEORGIA',   50, 14, 1, 4, 4, 'OHIO ST', 'GEORGIA'),
('ALABAMA',  'CLEMSON',   27, 24, 1, 6, 3, 'ALABAMA', 'CLEMSON'),
('MICHIGAN', 'LSU',       35, 28, 1, 5, 2, 'MICHIGAN', 'LSU'),
('OKLAHOMA', 'TEXAS',     42, 38, 1, 7, 5, 'OKLAHOMA', 'TEXAS'),
('FLORIDA',  'AUBURN',    31, 27, 1, 3, 1, 'FLORIDA', 'AUBURN'),
('PENN ST',  'NOTRE DAME',28, 21, 1, 8, 2, 'PENN ST', 'NOTRE DAME'),
('CLEMSON',  'USC',       24, 30, 1, 6, 4, 'USC', 'CLEMSON'),
('TEXAS',    'OHIO ST',   17, 42, 1, 5, 5, 'OHIO ST', 'TEXAS'),
('LSU',      'MICHIGAN',  21, 35, 1, 4, 3, 'MICHIGAN', 'LSU'),
('USC',      'ALABAMA',   20, 17, 1, 3, 4, 'USC', 'ALABAMA');
