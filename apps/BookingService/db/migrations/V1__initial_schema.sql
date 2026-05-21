-- V1__initial_schema.sql

CREATE TABLE bookings
(
    id             UUID            PRIMARY KEY     DEFAULT gen_random_uuid(),
    correlation_id UUID            NOT NULL,
    customer_id    UUID            NOT NULL,
    event_id       UUID            NOT NULL,
    seat_id        UUID            NOT NULL,
    amount         NUMERIC(18,2)   NOT NULL,
    currency       VARCHAR(10)     NOT NULL,
    status         VARCHAR(20)     NOT NULL, -- 'Pending', 'Confirmed', 'Cancelled'
    code           VARCHAR(30)     NOT NULL,
    notes          VARCHAR(500)    NULL,
    created_at     TIMESTAMPTZ     NOT NULL        DEFAULT NOW(),
    updated_at     TIMESTAMPTZ     NOT NULL        DEFAULT NOW(),

    CONSTRAINT uq_bookings_code
        UNIQUE (code),

    -- Bulletproof Saga Idempotency. One Saga can only have one row.
    CONSTRAINT uq_bookings_correlation_id
        UNIQUE (correlation_id)
);

-- PARTIAL INDEX: Protects the seat asset ONLY while it's Pending or Confirmed.
-- Once status becomes 'Cancelled', this rule stops applying to that row, allowing storing the canceled booking for audit or refound if refound fails!
CREATE UNIQUE INDEX uq_active_bookings_event_seat
    ON bookings (event_id, seat_id)
    WHERE status IN ('Pending', 'Confirmed');

-- Index for customer dashboard lookups
CREATE INDEX ix_bookings_customer_status
    ON bookings (customer_id, status);

-- Index for background workers monitoring status states
CREATE INDEX ix_bookings_status
    ON bookings (status);

-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE booking_sagas
(
    id              UUID            PRIMARY KEY     DEFAULT gen_random_uuid(),
    event_id        UUID            NOT NULL,
    seat_id         UUID            NOT NULL,
    customer_id     UUID            NOT NULL,
    amount          NUMERIC(18,2)   NOT NULL,
    currency        VARCHAR(10)     NOT NULL,
    notes           VARCHAR(500)    NULL,
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

-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE outbox_messages
(
    id           UUID         PRIMARY KEY  DEFAULT gen_random_uuid(),
    occurred_on  TIMESTAMPTZ  NOT NULL     DEFAULT NOW(),
    type         VARCHAR(500) NOT NULL,
    content      JSONB        NOT NULL,
    processed_on TIMESTAMPTZ  NULL
);

-- Background worker polls only unprocessed rows — partial index keeps it fast.
CREATE INDEX ix_outbox_messages_unprocessed
    ON outbox_messages (occurred_on)
    WHERE processed_on IS NULL;