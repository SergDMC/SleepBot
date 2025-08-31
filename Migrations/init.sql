CREATE TABLE IF NOT EXISTS sleep_sessions (
    id SERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    sleep_time TIMESTAMPTZ NOT NULL,
    wake_time TIMESTAMPTZ NOT NULL,
    duration INTERVAL NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_sleep_sessions_user_time
    ON sleep_sessions(user_id, sleep_time);
