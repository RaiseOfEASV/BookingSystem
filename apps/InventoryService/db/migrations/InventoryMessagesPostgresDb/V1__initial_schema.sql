-- V1__create_messages_table.sql

CREATE TABLE messages
(
    id              UUID            PRIMARY KEY     DEFAULT gen_random_uuid(),
    event_id        UUID            NOT NULL,
    seat_id         UUID            NOT NULL,
    correlation_id  UUID            NOT NULL,
    status          VARCHAR(50)     NOT NULL,
    retry_count     INT             NOT NULL        DEFAULT 0,
    last_error      VARCHAR(2000)   NULL,
    received_at     TIMESTAMPTZ     NOT NULL        DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL        DEFAULT NOW(),
    processed_at    TIMESTAMPTZ     NULL
);

CREATE INDEX IX_messages_status_updated_at
    ON messages (status, updated_at);