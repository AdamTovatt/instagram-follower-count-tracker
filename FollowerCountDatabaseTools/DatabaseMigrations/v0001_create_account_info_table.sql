CREATE TABLE account_info (
    id SERIAL PRIMARY KEY,          -- Unique identifier for each account
    name VARCHAR NOT NULL,          -- Account name (corresponding to the Name property)
    followers INT NOT NULL,         -- Follower count
    following INT NOT NULL,         -- Following count
    posts INT NOT NULL,             -- Post count
    record_time TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP  -- Time of this record
);
