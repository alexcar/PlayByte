/* ============================================================================
   PlayByte — Seed adicional de catálogo (SQL Server Management Studio)
   Insere mais 14 bandas (com álbuns e faixas) para permitir testar a paginação
   do catálogo (5 bandas por página).

   Como usar:
     1. Abra o SSMS conectado à instância do SQL Server do PlayByte.
     2. Selecione o banco de dados da aplicação (ex.: USE PlayByte;).
     3. Execute este script (F5).

   Observações:
     - Idempotente: cada banda só é inserida se ainda não existir (pelo nome).
     - Ids gerados com NEWID() (uniqueidentifier), compatível com a coluna id.
     - Esquema em snake_case (bands/albums/tracks), conforme o EF Core.
   ============================================================================ */

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @now           datetimeoffset = SYSDATETIMEOFFSET();
DECLARE @bandId        uniqueidentifier;
DECLARE @albumId       uniqueidentifier;

/* Tabela de trabalho: define as 14 bandas, um álbum e suas faixas (m:ss em segundos). */
DECLARE @catalog TABLE (
    band_name     nvarchar(200),
    album_title   nvarchar(300),
    release_year  int,
    track_title   nvarchar(300),
    duration_secs int,
    track_order   int
);

INSERT INTO @catalog (band_name, album_title, release_year, track_title, duration_secs, track_order) VALUES
-- 1
(N'The Beatles', N'Abbey Road', 1969, N'Come Together', 259, 1),
(N'The Beatles', N'Abbey Road', 1969, N'Something', 182, 2),
(N'The Beatles', N'Abbey Road', 1969, N'Here Comes the Sun', 185, 3),
-- 2
(N'Led Zeppelin', N'Led Zeppelin IV', 1971, N'Black Dog', 296, 1),
(N'Led Zeppelin', N'Led Zeppelin IV', 1971, N'Rock and Roll', 220, 2),
(N'Led Zeppelin', N'Led Zeppelin IV', 1971, N'Stairway to Heaven', 482, 3),
-- 3
(N'The Rolling Stones', N'Let It Bleed', 1969, N'Gimme Shelter', 271, 1),
(N'The Rolling Stones', N'Let It Bleed', 1969, N'You Can''t Always Get What You Want', 450, 2),
-- 4
(N'Radiohead', N'OK Computer', 1997, N'Paranoid Android', 383, 1),
(N'Radiohead', N'OK Computer', 1997, N'Karma Police', 261, 2),
(N'Radiohead', N'OK Computer', 1997, N'No Surprises', 229, 3),
-- 5
(N'U2', N'The Joshua Tree', 1987, N'With or Without You', 296, 1),
(N'U2', N'The Joshua Tree', 1987, N'Where the Streets Have No Name', 338, 2),
-- 6
(N'AC/DC', N'Back in Black', 1980, N'Hells Bells', 312, 1),
(N'AC/DC', N'Back in Black', 1980, N'Back in Black', 255, 2),
(N'AC/DC', N'Back in Black', 1980, N'You Shook Me All Night Long', 210, 3),
-- 7
(N'Red Hot Chili Peppers', N'Californication', 1999, N'Around the World', 238, 1),
(N'Red Hot Chili Peppers', N'Californication', 1999, N'Otherside', 255, 2),
(N'Red Hot Chili Peppers', N'Californication', 1999, N'Californication', 321, 3),
-- 8
(N'Guns N'' Roses', N'Appetite for Destruction', 1987, N'Welcome to the Jungle', 274, 1),
(N'Guns N'' Roses', N'Appetite for Destruction', 1987, N'Sweet Child o'' Mine', 356, 2),
(N'Guns N'' Roses', N'Appetite for Destruction', 1987, N'Paradise City', 406, 3),
-- 9
(N'The Police', N'Synchronicity', 1983, N'Every Breath You Take', 253, 1),
(N'The Police', N'Synchronicity', 1983, N'King of Pain', 299, 2),
-- 10
(N'Daft Punk', N'Discovery', 2001, N'One More Time', 320, 1),
(N'Daft Punk', N'Discovery', 2001, N'Harder, Better, Faster, Stronger', 224, 2),
(N'Daft Punk', N'Discovery', 2001, N'Digital Love', 301, 3),
-- 11
(N'Arctic Monkeys', N'AM', 2013, N'Do I Wanna Know?', 272, 1),
(N'Arctic Monkeys', N'AM', 2013, N'R U Mine?', 201, 2),
(N'Arctic Monkeys', N'AM', 2013, N'Why''d You Only Call Me When You''re High?', 161, 3),
-- 12
(N'David Bowie', N'The Rise and Fall of Ziggy Stardust', 1972, N'Starman', 254, 1),
(N'David Bowie', N'The Rise and Fall of Ziggy Stardust', 1972, N'Ziggy Stardust', 193, 2),
-- 13
(N'Muse', N'Black Holes and Revelations', 2006, N'Supermassive Black Hole', 212, 1),
(N'Muse', N'Black Holes and Revelations', 2006, N'Starlight', 240, 2),
(N'Muse', N'Black Holes and Revelations', 2006, N'Knights of Cydonia', 366, 3),
-- 14
(N'Black Sabbath', N'Paranoid', 1970, N'War Pigs', 478, 1),
(N'Black Sabbath', N'Paranoid', 1970, N'Paranoid', 168, 2),
(N'Black Sabbath', N'Paranoid', 1970, N'Iron Man', 356, 3);

/* Cursor sobre as bandas distintas que ainda não existem. */
DECLARE @bandName    nvarchar(200);
DECLARE @albumTitle  nvarchar(300);
DECLARE @releaseYear int;

DECLARE band_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT DISTINCT band_name, album_title, release_year
    FROM @catalog c
    WHERE NOT EXISTS (SELECT 1 FROM bands b WHERE b.name = c.band_name);

OPEN band_cursor;
FETCH NEXT FROM band_cursor INTO @bandName, @albumTitle, @releaseYear;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @bandId = NEWID();
    SET @albumId = NEWID();

    INSERT INTO bands (id, name, cover_image_url, created_at_utc, updated_at_utc)
    VALUES (@bandId, @bandName, NULL, @now, NULL);

    INSERT INTO albums (id, band_id, title, release_year)
    VALUES (@albumId, @bandId, @albumTitle, @releaseYear);

    INSERT INTO tracks (id, album_id, title, duration_seconds)
    SELECT NEWID(), @albumId, c.track_title, c.duration_secs
    FROM @catalog c
    WHERE c.band_name = @bandName
    ORDER BY c.track_order;

    FETCH NEXT FROM band_cursor INTO @bandName, @albumTitle, @releaseYear;
END

CLOSE band_cursor;
DEALLOCATE band_cursor;

COMMIT TRANSACTION;

PRINT 'Seed adicional concluído. Total de bandas no catálogo:';
SELECT COUNT(*) AS total_bandas FROM bands;
