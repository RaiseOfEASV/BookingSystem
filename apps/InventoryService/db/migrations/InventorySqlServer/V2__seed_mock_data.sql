-- =============================================================================
-- V2__seed_mock_data.sql
-- Mock data for local / development testing.
-- Fixed GUIDs ensure the script is idempotent when re-run via IF NOT EXISTS guards.
-- =============================================================================

-- -------------------------------------------------------------------------
-- VENUE
-- -------------------------------------------------------------------------
DECLARE @venueId UNIQUEIDENTIFIER = 'A0000000-0000-0000-0000-000000000001';

IF NOT EXISTS (SELECT 1 FROM venues WHERE id = @venueId)
    INSERT INTO venues (id, name, type, address)
    VALUES (@venueId, 'The O2 Arena', 'Arena', 'Peninsula Square, London SE10 0DX');

-- -------------------------------------------------------------------------
-- SEATS  (3 sections × 5 seats = 15 seats)
-- -------------------------------------------------------------------------

-- VIP Section
DECLARE @seatVip1 UNIQUEIDENTIFIER = 'B0000000-0000-0000-0000-000000000001';
DECLARE @seatVip2 UNIQUEIDENTIFIER = 'B0000000-0000-0000-0000-000000000002';
DECLARE @seatVip3 UNIQUEIDENTIFIER = 'B0000000-0000-0000-0000-000000000003';
DECLARE @seatVip4 UNIQUEIDENTIFIER = 'B0000000-0000-0000-0000-000000000004';
DECLARE @seatVip5 UNIQUEIDENTIFIER = 'B0000000-0000-0000-0000-000000000005';

-- Floor Section
DECLARE @seatFloor1 UNIQUEIDENTIFIER = 'B0000000-0000-0000-0000-000000000011';
DECLARE @seatFloor2 UNIQUEIDENTIFIER = 'B0000000-0000-0000-0000-000000000012';
DECLARE @seatFloor3 UNIQUEIDENTIFIER = 'B0000000-0000-0000-0000-000000000013';
DECLARE @seatFloor4 UNIQUEIDENTIFIER = 'B0000000-0000-0000-0000-000000000014';
DECLARE @seatFloor5 UNIQUEIDENTIFIER = 'B0000000-0000-0000-0000-000000000015';

-- Balcony Section
DECLARE @seatBal1 UNIQUEIDENTIFIER = 'B0000000-0000-0000-0000-000000000021';
DECLARE @seatBal2 UNIQUEIDENTIFIER = 'B0000000-0000-0000-0000-000000000022';
DECLARE @seatBal3 UNIQUEIDENTIFIER = 'B0000000-0000-0000-0000-000000000023';
DECLARE @seatBal4 UNIQUEIDENTIFIER = 'B0000000-0000-0000-0000-000000000024';
DECLARE @seatBal5 UNIQUEIDENTIFIER = 'B0000000-0000-0000-0000-000000000025';

INSERT INTO seats (id, venue_id, row, number, section)
SELECT id, venue_id, row, number, section
FROM (VALUES
    (@seatVip1,   @venueId, 'A', 1, 'VIP'),
    (@seatVip2,   @venueId, 'A', 2, 'VIP'),
    (@seatVip3,   @venueId, 'A', 3, 'VIP'),
    (@seatVip4,   @venueId, 'A', 4, 'VIP'),
    (@seatVip5,   @venueId, 'A', 5, 'VIP'),
    (@seatFloor1, @venueId, 'B', 1, 'Floor'),
    (@seatFloor2, @venueId, 'B', 2, 'Floor'),
    (@seatFloor3, @venueId, 'B', 3, 'Floor'),
    (@seatFloor4, @venueId, 'B', 4, 'Floor'),
    (@seatFloor5, @venueId, 'B', 5, 'Floor'),
    (@seatBal1,   @venueId, 'C', 1, 'Balcony'),
    (@seatBal2,   @venueId, 'C', 2, 'Balcony'),
    (@seatBal3,   @venueId, 'C', 3, 'Balcony'),
    (@seatBal4,   @venueId, 'C', 4, 'Balcony'),
    (@seatBal5,   @venueId, 'C', 5, 'Balcony')
) AS src(id, venue_id, row, number, section)
WHERE NOT EXISTS (SELECT 1 FROM seats WHERE id = src.id);

-- -------------------------------------------------------------------------
-- EVENTS
-- -------------------------------------------------------------------------
DECLARE @eventId1 UNIQUEIDENTIFIER = 'C0000000-0000-0000-0000-000000000001';
DECLARE @eventId2 UNIQUEIDENTIFIER = 'C0000000-0000-0000-0000-000000000002';

INSERT INTO events (id, venue_id, event_name, start_date, end_date)
SELECT id, venue_id, event_name, start_date, end_date
FROM (VALUES
    (@eventId1, @venueId, 'Coldplay - Music of the Spheres World Tour', '2026-08-15 19:00:00', '2026-08-15 22:30:00'),
    (@eventId2, @venueId, 'Taylor Swift - The Eras Tour',               '2026-09-20 18:00:00', '2026-09-20 23:00:00')
) AS src(id, venue_id, event_name, start_date, end_date)
WHERE NOT EXISTS (SELECT 1 FROM events WHERE id = src.id);

-- -------------------------------------------------------------------------
-- SEAT INVENTORY
--
-- Event 1 (Coldplay):  busy night — mix of available / held / taken
--   VIP:     3 available, 1 held, 1 taken
--   Floor:   2 available, 2 held, 1 taken
--   Balcony: 5 available
--
-- Event 2 (Taylor Swift): all seats available (fresh event)
-- -------------------------------------------------------------------------

-- Saga correlation IDs used for the held seats
DECLARE @sagaId1 UNIQUEIDENTIFIER = 'EEEEEEEE-0000-0000-0000-000000000001';
DECLARE @sagaId2 UNIQUEIDENTIFIER = 'EEEEEEEE-0000-0000-0000-000000000002';
DECLARE @sagaId3 UNIQUEIDENTIFIER = 'EEEEEEEE-0000-0000-0000-000000000003';

-- Event 1 inventory
INSERT INTO seats_inventory (id, event_id, seat_id, status, version, held_by, held_at)
SELECT id, event_id, seat_id, status, version, held_by, held_at
FROM (VALUES
    -- VIP
    (NEWID(), @eventId1, @seatVip1,   'available', 1, NULL,     NULL),
    (NEWID(), @eventId1, @seatVip2,   'available', 1, NULL,     NULL),
    (NEWID(), @eventId1, @seatVip3,   'available', 1, NULL,     NULL),
    (NEWID(), @eventId1, @seatVip4,   'held',      2, @sagaId1, '2026-05-19 10:00:00'),
    (NEWID(), @eventId1, @seatVip5,   'taken',     2, NULL,     NULL),
    -- Floor
    (NEWID(), @eventId1, @seatFloor1, 'available', 1, NULL,     NULL),
    (NEWID(), @eventId1, @seatFloor2, 'available', 1, NULL,     NULL),
    (NEWID(), @eventId1, @seatFloor3, 'held',      2, @sagaId2, '2026-05-19 10:05:00'),
    (NEWID(), @eventId1, @seatFloor4, 'held',      2, @sagaId3, '2026-05-19 10:10:00'),
    (NEWID(), @eventId1, @seatFloor5, 'taken',     2, NULL,     NULL),
    -- Balcony
    (NEWID(), @eventId1, @seatBal1,   'available', 1, NULL,     NULL),
    (NEWID(), @eventId1, @seatBal2,   'available', 1, NULL,     NULL),
    (NEWID(), @eventId1, @seatBal3,   'available', 1, NULL,     NULL),
    (NEWID(), @eventId1, @seatBal4,   'available', 1, NULL,     NULL),
    (NEWID(), @eventId1, @seatBal5,   'available', 1, NULL,     NULL)
) AS src(id, event_id, seat_id, status, version, held_by, held_at)
WHERE NOT EXISTS (
    SELECT 1 FROM seats_inventory
    WHERE event_id = src.event_id AND seat_id = src.seat_id
);

-- Event 2 inventory (all available)
INSERT INTO seats_inventory (id, event_id, seat_id, status, version, held_by, held_at)
SELECT id, event_id, seat_id, status, version, held_by, held_at
FROM (VALUES
    (NEWID(), @eventId2, @seatVip1,   'available', 1, NULL, NULL),
    (NEWID(), @eventId2, @seatVip2,   'available', 1, NULL, NULL),
    (NEWID(), @eventId2, @seatVip3,   'available', 1, NULL, NULL),
    (NEWID(), @eventId2, @seatVip4,   'available', 1, NULL, NULL),
    (NEWID(), @eventId2, @seatVip5,   'available', 1, NULL, NULL),
    (NEWID(), @eventId2, @seatFloor1, 'available', 1, NULL, NULL),
    (NEWID(), @eventId2, @seatFloor2, 'available', 1, NULL, NULL),
    (NEWID(), @eventId2, @seatFloor3, 'available', 1, NULL, NULL),
    (NEWID(), @eventId2, @seatFloor4, 'available', 1, NULL, NULL),
    (NEWID(), @eventId2, @seatFloor5, 'available', 1, NULL, NULL),
    (NEWID(), @eventId2, @seatBal1,   'available', 1, NULL, NULL),
    (NEWID(), @eventId2, @seatBal2,   'available', 1, NULL, NULL),
    (NEWID(), @eventId2, @seatBal3,   'available', 1, NULL, NULL),
    (NEWID(), @eventId2, @seatBal4,   'available', 1, NULL, NULL),
    (NEWID(), @eventId2, @seatBal5,   'available', 1, NULL, NULL)
) AS src(id, event_id, seat_id, status, version, held_by, held_at)
WHERE NOT EXISTS (
    SELECT 1 FROM seats_inventory
    WHERE event_id = src.event_id AND seat_id = src.seat_id
);
