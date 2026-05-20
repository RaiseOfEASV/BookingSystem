CREATE TABLE venues (
                        id      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
                        name    NVARCHAR(200)    NOT NULL,
                        type    NVARCHAR(100)    NOT NULL,
                        address NVARCHAR(500)    NOT NULL,

                        CONSTRAINT PK_venues PRIMARY KEY (id)
);

CREATE TABLE seats (
                       id       UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
                       venue_id UNIQUEIDENTIFIER NOT NULL,
                       row      NVARCHAR(10)     NOT NULL,
                       number   INT              NOT NULL,
                       section  NVARCHAR(100)    NOT NULL,

                       CONSTRAINT PK_seats             PRIMARY KEY (id),
                       CONSTRAINT FK_seats_venue_id    FOREIGN KEY (venue_id) REFERENCES venues (id),
                       CONSTRAINT UQ_seats_venue_row_number
                           UNIQUE (venue_id, row, number, section)
);

CREATE TABLE events (
                        id         UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
                        venue_id   UNIQUEIDENTIFIER NOT NULL,
                        event_name NVARCHAR(300)    NOT NULL,
                        start_date DATETIME2        NOT NULL,
                        end_date DATETIME2        NOT NULL,

                        CONSTRAINT PK_events          PRIMARY KEY (id),
                        CONSTRAINT FK_events_venue_id FOREIGN KEY (venue_id) REFERENCES venues (id)
);

CREATE TABLE seats_inventory (
                                 id         UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
                                 event_id   UNIQUEIDENTIFIER NOT NULL,
                                 seat_id    UNIQUEIDENTIFIER NOT NULL,
                                 status     NVARCHAR(50)     NOT NULL,
                                 version    INT              NOT NULL DEFAULT 1,
        
                                 held_by    UNIQUEIDENTIFIER     NULL,
                                 held_at  DATETIME2 NULL,
                                     CONSTRAINT PK_seats_inventory
                                     PRIMARY KEY (id),
                                 CONSTRAINT UQ_seats_inventory_event_seat
                                     UNIQUE (event_id, seat_id),
                                 CONSTRAINT FK_seats_inventory_event_id
                                     FOREIGN KEY (event_id) REFERENCES events (id),
                                 CONSTRAINT FK_seats_inventory_seat_id
                                     FOREIGN KEY (seat_id)  REFERENCES seats  (id),
                                 CONSTRAINT CHK_seats_inventory_status
                                     CHECK (status IN ('available', 'held', 'taken', 'blocked'))
);


CREATE NONCLUSTERED INDEX IX_seats_venue_section_row_number
    ON seats (venue_id, section, row, number)
    WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_events_venue_id_start_date
    ON events (venue_id, start_date);

CREATE NONCLUSTERED INDEX IX_seats_inventory_event_status
    ON seats_inventory (event_id, status)
    INCLUDE (seat_id);
       