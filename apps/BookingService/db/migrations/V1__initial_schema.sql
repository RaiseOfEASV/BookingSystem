-- V1__initial_schema.sql

CREATE TABLE bookings
(
    id          UUID            PRIMARY KEY     DEFAULT gen_random_uuid(),
    customer_id UUID            NOT NULL,
    event_id    UUID            NOT NULL,
    seat_id     UUID            NOT NULL,
    amount      NUMERIC(18,2)   NOT NULL,
    currency    VARCHAR(10)     NOT NULL,
    status      VARCHAR(20)     NOT NULL,
    code        VARCHAR(30)     NOT NULL,
    notes       VARCHAR(500)    NULL,
    created_at  TIMESTAMPTZ     NOT NULL        DEFAULT NOW(),
    updated_at  TIMESTAMPTZ     NOT NULL        DEFAULT NOW(),

    CONSTRAINT uq_bookings_code
        UNIQUE (code),

    -- One active booking per seat per event
    CONSTRAINT uq_bookings_event_seat
        UNIQUE (event_id, seat_id)
);

CREATE INDEX ix_bookings_customer_status
    ON bookings (customer_id, status);

CREATE INDEX ix_bookings_event_seat
    ON bookings (event_id, seat_id);

CREATE INDEX ix_bookings_status
    ON bookings (status);

-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE booking_sagas
(
    id              UUID            PRIMARY KEY     DEFAULT gen_random_uuid(),
    event_id        UUID            NOT NULL,
    seat_id         UUID     NOT NULL,
    booking_id      UUID            NULL,
    correlation_id  UUID            NOT NULL        UNIQUE, 
    payment_id      VARCHAR(100)    NULL,
    status          VARCHAR(50)     NOT NULL,
    failure_reason  VARCHAR(500)    NULL,
    created_at      TIMESTAMPTZ     NOT NULL        DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL        DEFAULT NOW()
);

-- For  retry/fulfillment polling engines
-- exclude 'Finalized', 'Failed', and 'Compensated' because those rows are static.
CREATE INDEX ix_booking_sagas_active_status
    ON booking_sagas (status)
    WHERE status NOT IN ('Finalized', 'Failed', 'Compensated');